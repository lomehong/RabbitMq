using System;
using System.Text;
using Conejo.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.ServiceModel;
using System.Web.Hosting;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Conejo
{
    public class Channel : IDisposable
    {
        private readonly ISerializer _serializer;
        private readonly Lazy<IModel> _channel;
        private readonly Lazy<IModel> _callBackChannel;

        private string replyTo;
        private static Dictionary<string, string> _callBackQueue;

        private static Dictionary<string, string> CallBackQueue
        {
            get
            {
                if (null == _callBackQueue)
                {
                    _callBackQueue = new Dictionary<string, string>();
                }

                return _callBackQueue;
            }
            set
            {
                if (null == _callBackQueue)
                {
                    _callBackQueue = new Dictionary<string, string>();
                }
            }
        }

        private static object lok2 = new object();

        private static long _isRunCallBack;

        public Channel(
            Connection connection,
            ChannelConfiguration configuration)
        {
            _serializer = connection.Configuration.Serializer;
            Configuration = configuration;
            _channel = new Lazy<IModel>(connection.CreateChannel);
            _callBackChannel = new Lazy<IModel>(connection.CreateChannel);
            replyTo = "RpcCallBack_" + GetFriendlyApplicationName();
        }

        public static Channel Create(
            Connection connection,
            Action<ChannelConfigurationDsl> config)
        {
            return new Channel(connection, ChannelConfiguration.Create(config));
        }

        public static Channel Create(
            Connection connection,
            ChannelConfiguration channelConfiguration,
            Action<ChannelConfigurationDsl> config)
        {
            config(new ChannelConfigurationDsl(channelConfiguration));
            return new Channel(connection, channelConfiguration);
        }

        public ChannelConfiguration Configuration { get; private set; }

        public void Close()
        {
            _channel.Value.Close();
            _channel.Value.Dispose();
        }

        public void Dispose()
        {
            Close();
        }

        public Result Publish<TMessage>(TMessage message)
            where TMessage : class, new()
        {
            try
            {
                EnsureExchangeAndQueue();

                _channel.Value.BasicPublish(
                    Configuration.ExchangeName,
                    BuildRoutingKey(message),
                    CreateBasicProperties(),
                    Encoding.UTF8.GetBytes(_serializer.Serialize(message)));
            }
            catch (Exception exception)
            {
                if (Configuration.ExceptionHandler == null) throw;
                Configuration.ExceptionHandler(exception);
                return new Result(exception);
            }
            return new Result();
        }

        public Result Subscribe<TMessage>(Action<TMessage> handler)
            where TMessage : class, new()
        {
            try
            {
                EnsureExchangeAndQueue();

                var consumer = new EventingBasicConsumer(_channel.Value);

                consumer.Received += (c, result) =>
                {
                    if (Configuration.QueueAcknowledgeReciept)
                        _channel.Value.BasicAck(result.DeliveryTag, false);
                    handler(result.Body == null ? null : _serializer.Deserialize<TMessage>(
                        Encoding.UTF8.GetString(result.Body)));
                };

                _channel.Value.BasicConsume(Configuration.QueueName,
                    !Configuration.QueueAcknowledgeReciept, consumer);
            }
            catch (Exception exception)
            {
                if (Configuration.ExceptionHandler == null) throw;
                Configuration.ExceptionHandler(exception);
                return new Result(exception);
            }
            return new Result();
        }

        public Result<TMessage> Dequeue<TMessage>()
            where TMessage : class, new()
        {
            return Dequeue<TMessage>(x => x.Queue.Dequeue());
        }

        public Result<TMessage> Dequeue<TMessage>(bool wait)
            where TMessage : class, new()
        {
            return wait ? Dequeue<TMessage>() :
                Dequeue<TMessage>(x => x.Queue.DequeueNoWait(
                    new BasicDeliverEventArgs()));
        }

        public Result<TMessage> Dequeue<TMessage>(int timeout)
            where TMessage : class, new()
        {
            return Dequeue<TMessage>(x =>
            {
                BasicDeliverEventArgs result;
                return x.Queue.Dequeue(timeout, out result) ?
                    result : new BasicDeliverEventArgs();
            });
        }

        private Result<TMessage> Dequeue<TMessage>(Func<QueueingBasicConsumer, BasicDeliverEventArgs> dequeue)
            where TMessage : class, new()
        {
            try
            {
                EnsureExchangeAndQueue();

                var consumer = new QueueingBasicConsumer(_channel.Value);
                _channel.Value.BasicConsume(Configuration.QueueName,
                    !Configuration.QueueAcknowledgeReciept, consumer);
                var result = dequeue(consumer);
                return new Result<TMessage>(result.Body == null ? null :
                    _serializer.Deserialize<TMessage>(Encoding.UTF8.GetString(result.Body)));
            }
            catch (Exception exception)
            {
                if (Configuration.ExceptionHandler == null) throw;
                Configuration.ExceptionHandler(exception);
                return new Result<TMessage>(exception);
            }
        }

        public Result Serve<TRequest, TResponse>(Func<TRequest, TResponse> handler)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            try
            {
                EnsureExchangeAndQueue();

                var consumer = new EventingBasicConsumer(_channel.Value);

                consumer.Received += (c, request) =>
                {
                    Task.Factory.StartNew(() =>
                    {
                        //Console.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "]do:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"));
                        if (Configuration.QueueAcknowledgeReciept)
                            _channel.Value.BasicAck(request.DeliveryTag, false);

                        var response = handler(request.Body == null ? null : _serializer.Deserialize<TRequest>(
                            Encoding.UTF8.GetString(request.Body)));

                        var responseProperties = CreateBasicProperties();
                        responseProperties.CorrelationId = request.BasicProperties.CorrelationId;

                        _channel.Value.BasicPublish(
                            "",
                            request.BasicProperties.ReplyTo,
                            responseProperties,
                            Encoding.UTF8.GetBytes(_serializer.Serialize(response)));
                    });
                };

                _channel.Value.BasicConsume(Configuration.QueueName,
                    !Configuration.QueueAcknowledgeReciept, consumer);
            }
            catch (Exception exception)
            {
                if (Configuration.ExceptionHandler == null) throw;
                Configuration.ExceptionHandler(exception);
                return new Result(exception);
            }
            return new Result();
        }

        public Result<TResponse> Call<TRequest, TResponse>(TRequest message)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            return Call<TRequest, TResponse>(message, x => x.Queue.Dequeue());
        }

        public Result<TResponse> Call<TRequest, TResponse>(TRequest message, bool wait)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            return wait ? Dequeue<TResponse>() :
                Call<TRequest, TResponse>(message, x => x.Queue.DequeueNoWait(
                    new BasicDeliverEventArgs()));
        }

        public Result<TResponse> Call<TRequest, TResponse>(TRequest message, int timeout)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            return Call<TRequest, TResponse>(message, x =>
            {
                BasicDeliverEventArgs result;
                return x.Queue.Dequeue(timeout, out result) ?
                    result : new BasicDeliverEventArgs();
            });
        }

        private Result<TResponse> Call<TRequest, TResponse>(TRequest message,
            Func<QueueingBasicConsumer, BasicDeliverEventArgs> dequeue)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            try
            {
                //Console.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "]线程启动调用。");
                //Stopwatch st = new Stopwatch();
                //st.Start();
                Task<string> mainTask = new Task<string>(() =>
                {
                    IBasicProperties requestProperties;
                    lock (lok2)
                    {
                        EnsureExchangeAndQueue();
                        requestProperties = CreateBasicProperties();
                        _channel.Value.QueueDeclare(replyTo, false, false, true, null);
                    }
                    requestProperties.CorrelationId = Guid.NewGuid().ToString();
                    requestProperties.ReplyTo = replyTo;

                    //Task task1 = Task.Factory.StartNew(() =>
                    //{
                    if (Interlocked.Read(ref _isRunCallBack) < 1)
                    {

                        Interlocked.Increment(ref _isRunCallBack);
                        var consumer = new EventingBasicConsumer(_callBackChannel.Value);
                        consumer.Received += (c, result) =>
                        {
                            Task.Factory.StartNew(() =>
                            {
                                // Console.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "]do:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"));
                                if (null != result && null != result.BasicProperties && !string.IsNullOrEmpty(result.BasicProperties.CorrelationId))
                                {
                                    CallBackQueue.Add(result.BasicProperties.CorrelationId, Encoding.UTF8.GetString(result.Body));
                                }

                                if (Configuration.QueueAcknowledgeReciept)
                                    _callBackChannel.Value.BasicAck(result.DeliveryTag, false);
                            });

                        };
                        _callBackChannel.Value.BasicConsume(replyTo,
                            !Configuration.QueueAcknowledgeReciept, consumer);
                    }
                    //});

                    //Task.WaitAny(task1);
                    _channel.Value.BasicPublish(
                      Configuration.ExchangeName,
                      BuildRoutingKey(message),
                      requestProperties,
                      Encoding.UTF8.GetBytes(_serializer.Serialize(message)));

                    return CallBackQueue.Get(requestProperties.CorrelationId);
                    //st.Stop();
                    //Console.WriteLine("["+requestProperties.CorrelationId + "]wanc,haoshi:" + st.ElapsedMilliseconds);
                    // return new Result<TResponse>(_serializer.Deserialize<TResponse>(tmp));
                });

                mainTask.Start();
                return new Result<TResponse>(_serializer.Deserialize<TResponse>(mainTask.Result));
            }
            catch (Exception exception)
            {
                if (Configuration.ExceptionHandler == null) throw;
                Configuration.ExceptionHandler(exception);
                return new Result<TResponse>(exception);
            }
        }

        public Result Call<TRequest, TResponse>(TRequest message, Action<TResponse> handler)
            where TRequest : class, new()
            where TResponse : class, new()
        {
            try
            {
                EnsureExchangeAndQueue();

                var requestProperties = CreateBasicProperties();
                requestProperties.CorrelationId = Guid.NewGuid().ToString();
                requestProperties.ReplyTo = _channel.Value.QueueDeclare(Guid.NewGuid().ToString(), false, true, true, null);

                _channel.Value.BasicPublish(
                    Configuration.ExchangeName,
                    BuildRoutingKey(message),
                    requestProperties,
                    Encoding.UTF8.GetBytes(_serializer.Serialize(message)));

                var consumer = new EventingBasicConsumer(_channel.Value);

                consumer.Received += (c, result) =>
                {
                    //while (true)
                    //{
                    //if (result.BasicProperties != null &&
                    //    result.BasicProperties.CorrelationId !=
                    //    requestProperties.CorrelationId) continue;
                    handler(result.Body == null ? null : _serializer.Deserialize<TResponse>(
                        Encoding.UTF8.GetString(result.Body)));
                    if (Configuration.QueueAcknowledgeReciept)
                        _channel.Value.BasicAck(result.DeliveryTag, false);
                    //    break;
                    //}
                };

                _channel.Value.BasicConsume(requestProperties.ReplyTo,
                    !Configuration.QueueAcknowledgeReciept, consumer);

                return new Result();
            }
            catch (Exception exception)
            {
                if (Configuration.ExceptionHandler == null) throw;
                Configuration.ExceptionHandler(exception);
                return new Result<TResponse>(exception);
            }
        }

        public void EnsureExchangeAndQueue()
        {
            EnsureExchange();
            EnsureQueue();
        }

        public void EnsureExchange()
        {
            if (string.IsNullOrEmpty(Configuration.ExchangeName)) return;

            _channel.Value.ExchangeDeclare(
                Configuration.ExchangeName.ToLower(),
                Configuration.ExchangeType.ToRabbitExchangeType(),
                Configuration.ExchangeDurable,
                Configuration.ExchangeAutoDelete, null);
        }

        public void EnsureQueue()
        {
            if (Configuration.QueueName.IsNullOrEmpty()) return;

            _channel.Value.QueueDeclare(
                Configuration.QueueName.ToLower(),
                Configuration.QueueDurable,
                Configuration.QueueExclusive,
                Configuration.QueueAutoDelete, null);

            if (string.IsNullOrEmpty(Configuration.ExchangeName)) return;

            _channel.Value.QueueBind(
                Configuration.QueueName.ToLower(),
                Configuration.ExchangeName.ToLower(),
                Configuration.QueueRoutingKey ?? "");
        }

        public void DeleteExchange()
        {
            if (string.IsNullOrEmpty(Configuration.ExchangeName)) return;
            _channel.Value.ExchangeDelete(Configuration.ExchangeName);
        }

        public void DeleteQueue()
        {
            _channel.Value.QueueDelete(Configuration.QueueName);
        }

        private string BuildRoutingKey<TMessage>(TMessage message)
        {
            return (Configuration.ExchangeType == ExchangeType.Topic ||
                    Configuration.ExchangeType == ExchangeType.Direct) &&
                    Configuration.ExchangeRoutingKey != null ?
                        Configuration.ExchangeRoutingKey(message) ?? "" : "";
        }

        private IBasicProperties CreateBasicProperties()
        {
            var basicProperties = _channel.Value.CreateBasicProperties();
            basicProperties.ContentType = "application/json";
            basicProperties.ContentEncoding = "utf-8";
            return basicProperties;
        }

        /// <summary>
        /// 根据当前应用程序类型返回Bin目录路径
        /// </summary>
        /// <returns></returns>
        private string GetBinFolder()
        {
            string location;

            if (HttpContext.Current != null)
            {
                // web (IIS/WCF ASP compatibility mode)context
                location = HttpRuntime.BinDirectory;
            }
            else if (OperationContext.Current != null)
            {
                // pure wcf context
                location = HostingEnvironment.ApplicationPhysicalPath;
            }
            else
            {
                // no special hosting context (console/winform etc)
                location = AppDomain.CurrentDomain.BaseDirectory;
            }

            return location;
        }

        /// <summary>
        /// 根据应用程序类型获取友好的应用程序名称（主要是文件夹的名称）
        /// </summary>
        /// <returns></returns>
        private string GetFriendlyApplicationName()
        {
            string location = string.Empty;
            string applicationName = string.Empty;

            location = GetBinFolder();
            if (!string.IsNullOrEmpty(location))
            {
                string[] tmp = location.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                if (HttpContext.Current != null)
                {
                    applicationName = tmp[tmp.Length - 2];
                }
                else if (OperationContext.Current != null)
                {
                    applicationName = tmp[tmp.Length - 1];
                }
                else
                {
                    if (location.ToLower().IndexOf("\\bin\\") != -1)
                    {
                        applicationName = tmp[tmp.Length - 3];
                    }
                    else
                    {
                        applicationName = tmp[tmp.Length - 1];
                    }
                }
            }
            else
            {
                string[] tmp = AppDomain.CurrentDomain.BaseDirectory.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                applicationName = tmp[tmp.Length - 1];
            }
            return applicationName;
        }
    }

    public class TimeoutException : Exception
    {
        public TimeoutException() : base(string.Format("RPC call timed out.")) { }
    }
}
