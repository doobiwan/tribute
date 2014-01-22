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
    public class ExchangeRequest : IExchange
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

        public async Task<byte[]> SubmitRequestAsync(byte[] requestData, string exchangeName = null)
        {
            byte[] response = null;
            var queue = exchangeName + "_queue";

            await connection.Open();

            using (var sub = connection.GetOpenSubscriberChannel())
            {
                try
                {
                    sub.Error += OnErrorHandler;

                    await sub.Subscribe(GetRequestHash(requestData), (s, bytes) =>
                    {
                        response = bytes;
                    });

                    await connection.Lists.AddLast(db, queue, requestData);

                    SpinWait.SpinUntil(() => response != null, TimeSpan.FromSeconds(requestTimeout));
                    return response;
                }
                finally
                {
                    sub.Error -= OnErrorHandler;
                }
            }
        }

        private void OnErrorHandler(object sender, ErrorEventArgs args)
        {
            Debugger.Break();
            throw (args.Exception);
        }

        private string GetRequestHash(byte[] requestData)
        {
            string hash;
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                hash = Convert.ToBase64String(sha1.ComputeHash(requestData));
            }
            return hash;
        }
    }
}
