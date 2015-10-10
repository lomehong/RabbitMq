using Conejo;
using RabbitMq.Common;
using RabbitMq.Common.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Service
{
    class Program
    {
        /// <summary>
        /// 日志
        /// </summary>
        private static ILogger logger = null;

        static void Main(string[] args)
        {
            logger = new EmptyLogger();

            var connection =
                Connection.Create(x => x
                    .ConnectTo("localhost", "/")
                    .WithCredentials("guest", "guest"));

            var server =
                Channel.Create(connection, x => x
                    .ThroughDirectExchange("rpc")
                        .InQueue("ping")
                            .WithRoutingKeyAsQueueName());


            server.Serve<Request, Response>(Handler);
        }

        static Response Handler(Request request)
        {
            Response response = new Response();
            string result = string.Format("{0}-{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"), request.Text);
            //logger.DebugFormat("{0}", result);
            response.Text = result;
            return response;
        }
    }
}
