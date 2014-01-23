using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookSleeve;

namespace tribute
{
    public class ExchangeResponse<TRequest, TResponse> : IStartable
    {
        private readonly RedisConnection subConnection;
        private readonly RedisConnection pubConnection;
        private RedisSubscriberConnection sub;
        private ManualResetEventSlim stopSignal;
        private readonly Func<TRequest, TResponse> execute;
        
        public readonly string ExchangeName;
        private string exchangeQueue { get { return ExchangeName + "_queue"; } }
        private string exchangeChannel { get { return ExchangeName + "_channel"; } }

        public ExchangeResponse(RedisConnection subConnection, RedisConnection pubConnection, Func<TRequest, TResponse> execute, string exchangeName)
        {
            this.subConnection = subConnection;
            this.pubConnection = pubConnection;
            this.execute = execute;

            this.ExchangeName = exchangeName ?? typeof(TRequest).Name + "_" + typeof(TResponse).Name;
        }

        private async Task Listen()
        {
            await subConnection.Open();

            using (sub = subConnection.GetOpenSubscriberChannel())
            {
                try
                {
                    sub.Error += Utility.OnErrorHandler;
                    stopSignal = new ManualResetEventSlim();

                    await sub.Subscribe(exchangeChannel, async (s, bytes) => { await RespondTo(bytes); });

                    stopSignal.Wait();
                    sub.Unsubscribe(exchangeChannel);
                }
                finally
                {
                    sub.Error -= Utility.OnErrorHandler;
                    subConnection.Close(false);
                }
            }
        }
        private async Task RespondTo(byte[] bytes)
        {
            var request = Utility.Deserialize<TRequest>(bytes);
            var response = execute(request);
            await Publish(Utility.Serialize(response));
        }
        private async Task Publish(byte[] response)
        {
            await pubConnection.Open();
            await pubConnection.Publish(exchangeChannel, response);
            pubConnection.Close(false);
        }

        public async Task StartAsync()
        {
            await Listen();
        }
        public async Task StopAsync()
        {
            stopSignal.Set();
        }
    }
}
