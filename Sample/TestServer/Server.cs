using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;
using TestHelpers;
using TestHelpers.Domain;

namespace TestServer
{
    //README: set TestClient and TestServer both as startup projects, then hit F5
    class Server
    {
        static void Main(string[] args)
        {
            using (IConnection conn = new ConnectionFactory().CreateConnection())
            {
                using (var ch = conn.CreateModel())
                {
                    ch.ExchangeDeclare(Helper.ExchangeName, "direct");
                    var queuename = ch.EnsureQueue();
                   
                    var subscription = new Subscription(ch, queuename);

                    new MySimpleRpcServerSubclass(subscription).MainLoop();
                }
            }
        }
    }
    
    internal class MySimpleRpcServerSubclass : SimpleRpcServer
    {
        public MySimpleRpcServerSubclass(Subscription subscription) 
            : base(subscription) { }

        public override byte[] HandleSimpleCall(bool isRedelivered, IBasicProperties requestProperties, byte[] body, out IBasicProperties replyProperties)
        {
            replyProperties = requestProperties;
            replyProperties.MessageId = Guid.NewGuid().ToString();

            //var m = WMessage.Deserialize(body);
            //var r = new WResponse
            //            {
            //                Message = String.Format("Got message {0} with body-text {1}", m.Name, m.Body)
            //            };
          //  Console.WriteLine("workload arrived {0}", m.Body);

            return body;
        }
    }

}
