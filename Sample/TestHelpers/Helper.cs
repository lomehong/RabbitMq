using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace TestHelpers
{
    /// <summary>
    /// Just a simple class to make the demo client and server more clean
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Setup for console applicaiton, hence use of Console.WriteLine
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>

        public const string QueueId = "test.request.reply.queue";
        public const string ExchangeName = "test.workqueue";

        public static string EnsureQueue(this IModel ch)
        {
          //  Console.WriteLine("Creating a queue");

            var queueName = ch.QueueDeclare(QueueId, false, false, false, null);

            ch.QueueBind(queueName, ExchangeName, "", null);

           // Console.WriteLine("Done. Created queue {0}.\n", queueName);

            return queueName;
        }
    }
}
