using System;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BookSleeve;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using ErrorEventArgs = BookSleeve.ErrorEventArgs;

namespace tribute
{
    public class ExchangeRequest
    {
        private readonly RedisConnection connection;
        private int db = 0;
        private int requestTimeout = 20;

        public ExchangeRequest(RedisConnection connection)
        {
            this.connection = connection;
        }

        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(TRequest request, string exchangeName = null)
        {
            exchangeName = exchangeName ?? typeof(TRequest).Name + "_" + typeof(TResponse).Name;

            var requestData = Utility.Serialize(request);
            var responseData = await SubmitRequestAsync(requestData, exchangeName);
            return Utility.Deserialize<TResponse>(responseData);
        }

        private async Task<byte[]> SubmitRequestAsync(byte[] requestData, string exchangeName = null)
        {
            byte[] response = null;
            var queue = exchangeName + "_queue";

            await connection.Open();

            using (var sub = connection.GetOpenSubscriberChannel())
            {
                try
                {
                    sub.Error += Utility.OnErrorHandler;
                    var responseSignal = new ManualResetEventSlim();
                    var requestId = Utility.GetObjectHash(requestData);

                    await sub.Subscribe(requestId, (s, bytes) =>
                    {
                        response = bytes;
                        responseSignal.Set();
                    });

                    await connection.Strings.Set(db, requestId, requestData);
                    await connection.Lists.AddLast(db, queue, requestId);

                    responseSignal.Wait(TimeSpan.FromSeconds(requestTimeout));
                    return response;
                }
                finally
                {
                    sub.Error -= Utility.OnErrorHandler;
                }
            }
        }
    }
}
