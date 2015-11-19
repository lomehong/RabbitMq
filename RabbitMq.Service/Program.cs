using RabbitMq.Common;
using RabbitMq.Common.Logger;
using RabbitMq.Common.Rpc;
using RabbitMq.Rpc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMq.Service
{
    class Program
    {
        /// <summary>
        /// 日志
        /// </summary>
        private static ILogger logger = null;

        static IConnectionFactory _factory;
        static IConnection _connection = null;

        static void Main(string[] args)
        {
            logger = new EmptyLogger();

            IRpcService service = new RpcService();
            service.StartService();

            Console.ReadKey();
            logger.Debug("q");
        }
    }
}
