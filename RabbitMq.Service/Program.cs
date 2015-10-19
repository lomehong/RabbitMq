using RabbitMq.Common;
using RabbitMq.Common.Logger;
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
       // static IModel _channel;

        static void Main(string[] args)
        {
            logger = new EmptyLogger();

            var rabbitMqConfig = Utility.GetRabbitMqConfig();
            _factory = new ConnectionFactory()
            {
                HostName = rabbitMqConfig.HostName,
                UserName = rabbitMqConfig.UserName,
                Password = rabbitMqConfig.Password,
                Port = rabbitMqConfig.Port,
                VirtualHost = rabbitMqConfig.VirtualHost
            };
             _connection = _factory.CreateConnection();

            for (var i = 0; i < 5; i++)
            {
               new Thread(new ThreadStart(Recevice)).Start();
            }
        }

        static void Recevice()
        {
           // IConnection _connection = _factory.CreateConnection();
            IModel _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "rpc_queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _channel.BasicQos(0, 1, false);
            var eventConsumer = new EventingBasicConsumer(_channel);
            eventConsumer.Received += (c, request) =>
            {
               // Task.Factory.StartNew(() =>
                //{
                    //Console.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "]do:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"));
                    var body = request.Body;
                    var props = request.BasicProperties;
                    var replyProps = c.Model.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    //Console.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "]" + props.CorrelationId);
                    byte[] responseBytes = new byte[1024 * 1];
                    c.Model.BasicPublish(exchange: "",
                                         routingKey: props.ReplyTo,
                                         basicProperties: replyProps,
                                         body: responseBytes);
                    c.Model.BasicAck(deliveryTag: request.DeliveryTag,
                                     multiple: false);
               // });
            };
            // var consumer = new QueueingBasicConsumer(_channel);
            //_channel.BasicConsume(queue: "rpc_queue",
            //                     noAck: false,
            //                     consumer: consumer);
            _channel.BasicConsume(queue: "rpc_queue",
                     noAck: false,
                     consumer: eventConsumer);
            Console.WriteLine(" [x] Awaiting RPC requests[" + Thread.CurrentThread.ManagedThreadId +"]");

            //while (true)
            //{
            //    string response = null;
            //    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

            //    var body = ea.Body;
            //    var props = ea.BasicProperties;
            //    var replyProps = _channel.CreateBasicProperties();
            //    replyProps.CorrelationId = props.CorrelationId;

            //    try
            //    {
            //        var message = Encoding.UTF8.GetString(body);
            //        response = message;
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(" [.] " + e.Message);
            //        response = "";
            //    }
            //    finally
            //    {
            //        byte[] responseBytes = new byte[1024*1];
            //        _channel.BasicPublish(exchange: "",
            //                             routingKey: props.ReplyTo,
            //                             basicProperties: replyProps,
            //                             body: responseBytes);
            //        _channel.BasicAck(deliveryTag: ea.DeliveryTag,
            //                         multiple: false);
            //    }
           //}
        }

        private static void EventConsumer_Received(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            EventingBasicConsumer eb = sender as EventingBasicConsumer;
            var body = args.Body;
            var props = args.BasicProperties;
            var replyProps = eb.Model.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            Console.WriteLine("["+Thread.CurrentThread.ManagedThreadId+"]" + props.CorrelationId);
            byte[] responseBytes = new byte[1024 * 1];
            eb.Model.BasicPublish(exchange: "",
                                 routingKey: props.ReplyTo,
                                 basicProperties: replyProps,
                                 body: responseBytes);
            eb.Model.BasicAck(deliveryTag: args.DeliveryTag,
                             multiple: false);
        }
    }
}
