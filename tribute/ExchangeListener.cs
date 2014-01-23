using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookSleeve;

namespace tribute
{
    public class ExchangeListener : IStartable
    {
        private int db = 0;
        private int requestTimeout = 20;
        private List<IStartable> exchanges = new List<IStartable>();
        
        public void AddExchange<TRequest, TResponse>(ExchangeResponse<TRequest, TResponse> exchange, string exchangeName = null)
        {
            exchanges.Add(exchange);
        }

        public async Task StartAsync()
        {
            foreach (var exchange in exchanges)
            {
                try
                {
                    await exchange.StartAsync();
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                }
            }
        }
        public async Task StopAsync()
        {
            foreach (var exchange in exchanges)
            {
                try
                {
                    await exchange.StopAsync();
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                }
            }
        }
    }
}
