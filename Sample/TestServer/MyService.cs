using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestServer
{
    public class MyService
    {
        static IConnection _connection;

        static IConnection GetConnection
        {
            get
            {
                if (null == _connection)
                {
                    _connection = new ConnectionFactory
                    {
                        HostName = "localhost",
                        UserName = "guest",
                        Password = "guest",
                        Port = 5672,
                        VirtualHost = "/"
                    }.CreateConnection();
                }

                return _connection;
            }
        }

        public void CreateService(string queue)
        {
            if(string.IsNullOrWhiteSpace(queue))
            {
                return;
            }

            for(var i=0;i<10;i++)
            {
                new Thread(new ThreadStart(() =>
               {
                   using (IConnection conn = GetConnection)
                   {
                       using (var ch = conn.CreateModel())
                       {
                           var queuename = ch.QueueDeclare(queue, false, false, false, null);

                           var subscription = new Subscription(ch, queuename);

                           new MyRpcService(subscription).MainLoop();
                       }
                   }
               })).Start();

            }

        }
    }

    public class MyRpcService: SimpleRpcServer
    {
        public MyRpcService(Subscription subscription)
            : base(subscription)
        { }

        public override byte[] HandleCall(bool isRedelivered, IBasicProperties requestProperties, byte[] body, out IBasicProperties replyProperties)
        {
            return base.HandleCall(isRedelivered, requestProperties, body, out replyProperties);
        }

        public override byte[] HandleSimpleCall(bool isRedelivered, IBasicProperties requestProperties, byte[] body, out IBasicProperties replyProperties)
        {
            replyProperties = requestProperties;
            replyProperties.MessageId = Guid.NewGuid().ToString();
            return Encoding.UTF8.GetBytes("{result:\"test\",exception:\"\",status:\"00000\"}");
        }
    }
}
