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

        /// <summary>
        /// RabbitMq连接
        /// </summary>
        private static IConnection _connection = null;

        /// <summary>
        /// 线程控制
        /// </summary>
        readonly CancellationTokenSource tokenSource;

        /// <summary>
        /// 存放回调消息的本地非阻塞队列
        /// </summary>
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
                VirtualHost = rabbitMqConfig.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
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

        /// <summary>
        /// 开始接收回调消息
        /// </summary>
        private void StartReceive()
        {
            IModel _receivechannel = _connection.CreateModel();

            _receivechannel.QueueDeclare(replyTo, false, false, false, null);
            _receivechannel.BasicQos(0, 10, false);
            QueueingBasicConsumer consumer = new QueueingBasicConsumer(_receivechannel);
            _receivechannel.BasicConsume(replyTo, false, consumer);
            while (!this.tokenSource.IsCancellationRequested)
            {
                BasicDeliverEventArgs message = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                if (null == message)
                {
                    continue;
                }

                string corrdId = string.Empty;

                try
                {
                    var bodyString = Encoding.UTF8.GetString(message.Body);
                    var props = message.BasicProperties;

                    corrdId = props.CorrelationId;

                    if (_callBackQueue.ContainsKey(corrdId))
                        _callBackQueue[corrdId].Add(bodyString);
                }
                catch (Exception ex)
                {
                    this.logger.Error("StartReceive:" + ex.Message);

                    BlockingCollection<string> bq = null;
                    if (!string.IsNullOrEmpty(corrdId))
                    {
                        _callBackQueue.TryRemove(corrdId, out bq);
                    }
                }
                finally
                {
                    try
                    {
                        _receivechannel.BasicAck(message.DeliveryTag, false);
                    }
                    catch (Exception e1)
                    {
                        this.logger.Error(e1.Message);
                    }
                }

            }

            //var consumer = new EventingBasicConsumer(_receivechannel);
            //consumer.Received += (c, result) =>
            //{
            //    string corrdId = string.Empty;
            //    try
            //    {
            //        if (null == c || result == null)
            //        {
            //            return;
            //        }
            //        var props = result.BasicProperties;
            //        corrdId = props.CorrelationId;

            //        var bodyString = Encoding.UTF8.GetString(result.Body);
            //        if (_callBackQueue.ContainsKey(props.CorrelationId))
            //            _callBackQueue[props.CorrelationId].Add(bodyString);
            //        _receivechannel.BasicAck(result.DeliveryTag, false);

            //    }
            //    catch (Exception ex)
            //    {
            //        this.logger.Error(ex.Message);
            //    }
            //    finally
            //    {
            //        BlockingCollection<string> bq = null;
            //        _callBackQueue.TryRemove(corrdId, out bq);
            //    }
            //};
            //_receivechannel.BasicConsume(replyTo, false, consumer);
        }



        public TMessage Call<TMessage>(string serviceId, string interfaceId, string method, params object[] p)
        {
            bool iserror = false;
            var result = default(TMessage);
            string queue = serviceId;
            if (null == _publishChannel)
            {
                _publishChannel = _connection.CreateModel();
            }
            if (null != _publishChannel)
            {
                string corrId = string.Empty;
                try
                {
                    var localmessage = "hello";
                    byte[] body = new byte[1024];// Encoding.UTF8.GetBytes(localmessage);

                    var properties = _publishChannel.CreateBasicProperties();
                    corrId = Guid.NewGuid().ToString();
                    _callBackQueue.TryAdd(corrId, new BlockingCollection<string>(1));

                    properties.CorrelationId = corrId;
                    properties.ContentEncoding = "UTF-8";
                    properties.ReplyTo = replyTo;

                    _publishChannel.BasicPublish(string.Empty, queue, properties, body);

                    string stringResult = string.Empty;
                    _callBackQueue[corrId].TryTake(out stringResult, 1000, this.tokenSource.Token);
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex.Message);
                    iserror = true;
                }
                finally
                {
                    BlockingCollection<string> bq = null;
                    if (iserror)
                    {
                        this.logger.Debug(corrId);
                    }
                    _callBackQueue.TryRemove(corrId, out bq);
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
