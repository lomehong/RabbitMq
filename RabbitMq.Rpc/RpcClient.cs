using Conejo;
using RabbitMq.Common;
using RabbitMq.Common.Logger;
using RabbitMq.Common.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Rpc
{
    public class RpcClient : IRpcClient
    {
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger logger = null;

        private static Connection _connection = null;
        private static Channel _client = null;

        private static object lok = new object();
        private static bool isInitChannel = false;

        private static RpcClient _current;

        private static RpcClient Current
        {
            get
            {
                if (null == _current)
                {
                    _current = new RpcClient();
                }

                return _current;
            }
        }

        /// <summary>
        /// 最大并发线程数，一般情况下是逻辑处理器的数量
        /// </summary>
        private static int _maxConcurrentThreads;

        /// <summary>
        /// Channel池
        /// </summary>
        private static Dictionary<int, Channel> _channelPool;

        /// <summary>
        /// 获取Channel池
        /// </summary>
        private static Dictionary<int, Channel> ChannelPool
        {
            get
            {
                if (null == _channelPool)
                {
                    _channelPool = new Dictionary<int, Channel>();
                }
                return _channelPool;
            }
        }

        private static Channel _currentChannel;
        private static Channel CurrentChannel()
        {
            if (null == _currentChannel)
            {
                _currentChannel = Channel.Create(_connection, x => x
                                         .ThroughDirectExchange("rpc")
                                             .WithRoutingKey("ping"));
            }
            return _currentChannel;
        }

        public RpcClient()
        {
            logger = new EmptyLogger();
            _connection =
                Connection.Create(x => x
                .ConnectTo("localhost", "/")
                .WithCredentials("guest", "guest"));
        }

        /// <summary>
        /// 初始化Channel池
        /// </summary>
        /// <param name="queue">队列名称</param>
        private void InitChannel(string queue)
        {
            if (!isInitChannel)
            {
                lock (lok)
                {
                    if (isInitChannel)
                    {
                        return;
                    }

                    int coreCount = 0;
                    foreach (var item in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
                    {
                        coreCount += int.Parse(item["NumberOfCores"].ToString());
                    }

                    _maxConcurrentThreads = coreCount;
                    for (var i = 0; i < _maxConcurrentThreads; i++)
                    {
                        ChannelPool.Add(i, Channel.Create(_connection, x => x
                                        .ThroughDirectExchange("rpc")
                                            .WithRoutingKey(queue)));
                    }
                    isInitChannel = true;
                }
            }

        }

        /// <summary>
        /// 从Channel池中取出一个Channel使用
        /// </summary>
        /// <param name="queue"></param>
        /// <returns>返回一个Channel</returns>
        private Channel GetChannel(string queue)
        {
            if (!isInitChannel)
            {
                InitChannel(queue);
            }
            Channel channel = null;
            if (null != ChannelPool && ChannelPool.Count > 0)
            {
                ChannelPool.TryGetValue(new Random().Next(_maxConcurrentThreads), out channel);
            }

            return channel;
        }

        public TMessage Call<TMessage>(string serviceId, string interfaceId, string method, params object[] p)
        {
            //  var result = default(TMessage);
            // string queue = "ping";

            //Channel currentChannel;
            //ChannelPool.TryGetValue(Thread.CurrentThread.ManagedThreadId, out currentChannel);
            //if (null == currentChannel)
            //{
            //    lock (lok)
            //    {
            //        ChannelPool.Add(Thread.CurrentThread.ManagedThreadId, Channel.Create(_connection, x => x
            //                             .ThroughDirectExchange("rpc")
            //                                 .WithRoutingKey("ping")));
            //    }
            //    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + "不存在，将创建");
            //    ChannelPool.TryGetValue(Thread.CurrentThread.ManagedThreadId, out currentChannel);
            //}

            //DictionaryBasedKeyLockEngine manager = new DictionaryBasedKeyLockEngine();
            //manager.Invoke("", channel => channel.Call<Request, Response>(new Request { Text = "hai" }));

            //var response = ChannelPool[Thread.CurrentThread.ManagedThreadId].Call<Request, Response>(new Request { Text = "hai" });

            //var response = CurrentChannel().Call<Request, Response>(new Request { Text = "hai" });
            return Current.InternalCall<TMessage>(serviceId, interfaceId, method, p);
        }

        private TMessage InternalCall<TMessage>(string serviceId, string interfaceId, string method, params object[] p)
        {
            var result = default(TMessage);
            string queue = "ping";
            var response = CurrentChannel().Call<Request, Response>(new Request { Text = "hai" });
            return result;
        }

        private Result<Response> Execute(Channel channel)
        {
            return channel.Call<Request, Response>(new Request { Text = "hai" });
        }

        public void Excute(string serviceId, string interfaceId, string method, params object[] p)
        {
            throw new NotImplementedException();
        }
    }
}
