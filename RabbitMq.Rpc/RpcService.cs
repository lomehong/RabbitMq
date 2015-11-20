using RabbitMq.Common.Logger;
using RabbitMq.Common.Rpc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMq.Rpc
{
    /// <summary>
    /// <para>功能：</para>
    /// <para>作者：hz0704027 </para>
    /// <para>日期：2015/11/19 17:00:49 </para>
    /// <para>备注：本代码版权归慧择网所有，严禁外传 </para>
    /// </summary>
	public class RpcService : IRpcService
    {
        /// <summary>
        /// 日志
        /// </summary>
        private ILogger _logger = null;

        /// <summary>
        /// 连接工厂
        /// </summary>
        static IConnectionFactory _factory;

        /// <summary>
        /// 连接
        /// </summary>
        static IConnection _connection = null;

        /// <summary>
        /// 线程控制
        /// </summary>
        readonly CancellationTokenSource _tokenSource;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RpcService()
        {
            this._logger = new EmptyLogger();
            this._tokenSource = new CancellationTokenSource();

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
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public void StartService()
        {

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
                try
                {
                    if (DateTime.Now.Second > 30)
                    {
                        throw new Exception(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            var body = request.Body;
                            var props = request.BasicProperties;
                            var replyProps = c.Model.CreateBasicProperties();
                            replyProps.CorrelationId = props.CorrelationId;

                            byte[] responseBytes = new byte[1024];
                            //Thread.Sleep(100);
                            //if (DateTime.Now.Second > 30)
                            //{
                            //    throw new Exception("test");
                            //}
                            c.Model.BasicPublish(exchange: "",
                                                     routingKey: props.ReplyTo,
                                                     basicProperties: replyProps,
                                                     body: responseBytes);
                        }
                        catch (Exception ex)
                        {
                            this._logger.Error(ex.Message);
                        }
                        finally
                        {

                        }
                    });
                }
                catch (Exception e)
                {
                    this._logger.Error(e.Message);
                }
                finally
                {
                    c.Model.BasicAck(deliveryTag: request.DeliveryTag,
                                                        multiple: false);
                }
            };


            for (var i = 0; i < 20; i++)
            {
                _channel.BasicConsume(queue: "rpc_queue",
                       noAck: false,
                       consumer: eventConsumer);
            }
            this._logger.Info(" [x] Awaiting RPC requests[" + Thread.CurrentThread.ManagedThreadId + "]");
        }

        private void Do(object state)
        {
            DoEntity entity1 = state as DoEntity;
            BasicDeliverEventArgs ea = entity1.ea;
            IModel channel = entity1.channel;
            var body = ea.Body;
            var props = ea.BasicProperties;
            var replyProps = channel.CreateBasicProperties();
            if (null != props && !string.IsNullOrEmpty(props.CorrelationId))
            {
                replyProps.CorrelationId = props.CorrelationId;
            }
            try
            {
                byte[] responseBytes = new byte[1024];
                channel.BasicPublish(exchange: "",
                                     routingKey: props.ReplyTo,
                                     basicProperties: replyProps,
                                     body: responseBytes);
                channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                 multiple: false);
            }
            catch (Exception ex)
            {
                this._logger.Error(ex.Message);
            }
            finally
            {

            }
        }
    }

    public class DoEntity
    {
        public BasicDeliverEventArgs ea { set; get; }
        public IModel channel { set; get; }
    }
}
