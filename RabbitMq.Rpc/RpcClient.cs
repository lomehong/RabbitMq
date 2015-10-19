using RabbitMq.Common;
using RabbitMq.Common.Logger;
using RabbitMq.Common.Rpc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.Impl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMq.Rpc
{
    public class RpcClient : IRpcClient
    {
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger logger = null;

        private static IConnection _connection = null;

        private static object lok = new object();

        /// <summary>
        /// 线程控制
        /// </summary>
        readonly CancellationTokenSource tokenSource;


        readonly ConcurrentDictionary<string, BlockingCollection<string>> _callBackQueue = new ConcurrentDictionary<string, BlockingCollection<string>>();


        /// <summary>
        /// 接收回调消息的Channel
        /// </summary>
        //IModel _receivechannel;

        /// <summary>
        /// Publish消息的Channel
        /// </summary>
        IModel _publishChannel;

        string replyTo;

        public RpcClient()
        {
            logger = new EmptyLogger();
            this.tokenSource = new CancellationTokenSource();

            var rabbitMqConfig = Utility.GetRabbitMqConfig();
            var factory = new ConnectionFactory()
            {
                HostName = rabbitMqConfig.HostName,
                UserName = rabbitMqConfig.UserName,
                Password = rabbitMqConfig.Password,
                Port = rabbitMqConfig.Port,
                VirtualHost = rabbitMqConfig.VirtualHost
            };
            _connection = factory.CreateConnection();
            

            replyTo = "RpcCallBack_" + Utility.GetFriendlyApplicationName();


            ReceiveCallBack();
        }

        /// <summary>
        /// 接收回调消息，并写入本地队列
        /// </summary>
        private void ReceiveCallBack()
        {
            for (var i = 0; i < 10; i++)
            {
                new Thread(new ThreadStart(StartReceive)).Start();
            }
        }

        private void StartReceive()
        {
            IModel _receivechannel = _connection.CreateModel();
            QueueingBasicConsumer consumer = new QueueingBasicConsumer(_receivechannel);
            _receivechannel.QueueDeclare(replyTo, false, false, false, null);
            _receivechannel.BasicQos(0, 1, false);
            _receivechannel.BasicConsume(replyTo, false, consumer);
            while (!this.tokenSource.IsCancellationRequested)
            {
                BasicDeliverEventArgs message = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                if (null == message)
                {
                    continue;
                }

                var bodyString = Encoding.UTF8.GetString(message.Body);
                var props = message.BasicProperties;
                if (_callBackQueue.ContainsKey(props.CorrelationId))
                    _callBackQueue[props.CorrelationId].Add(bodyString);
                _receivechannel.BasicAck(message.DeliveryTag, false);
            }
        }

        public TMessage Call<TMessage>(string serviceId, string interfaceId, string method, params object[] p)
        {
            var result = default(TMessage);
            string queue = serviceId;
            if (null == _publishChannel)
            {
                _publishChannel = _connection.CreateModel();
            }
            if (null != _publishChannel)
            {
                //Stopwatch st = new Stopwatch();
                //st.Start();
                string corrId = string.Empty;
                try
                {
                    var localmessage = "hello";
                    byte[] body = new byte[1024 * 2];// Encoding.UTF8.GetBytes(localmessage);

                    var properties = _publishChannel.CreateBasicProperties();
                    corrId = Guid.NewGuid().ToString();
                    _callBackQueue.TryAdd(corrId, new BlockingCollection<string>(1));
                    //st.Stop();
                    //this.logger.Debug("Add" + st.ElapsedMilliseconds);
                    //st.Start();
                    properties.CorrelationId = corrId;
                    properties.ContentEncoding = "UTF-8";
                    properties.ReplyTo = replyTo;

#if DEBUG
                    //  Stopwatch st = new Stopwatch();
                    // st.Start();
#endif
                    _publishChannel.BasicPublish(string.Empty, queue, properties, body);
                    //st.Stop();
                    //this.logger.Debug("Publish" + st.ElapsedMilliseconds);
                    //st.Start();
                    var stringResult = _callBackQueue[corrId].Take();
                    //st.Stop();
                    //this.logger.Debug("Take" + st.ElapsedMilliseconds);
                    //this.logger.DebugFormat(stringResult);
#if DEBUG
                    //  st.Stop();
                    // logger.DebugFormat("Call完成，耗时：" + st.ElapsedMilliseconds);
#endif
                }
                catch (Exception ex)
                {

                }
            }

            return result;
        }

        public void Excute(string serviceId, string interfaceId, string method, params object[] p)
        {
            throw new NotImplementedException();
        }
    }
}
