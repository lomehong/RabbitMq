using Conejo;
using RabbitMq.Common;
using RabbitMq.Common.Logger;
using RabbitMq.Common.Rpc;
using RabbitMq.Rpc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMq.Client
{
    class Program
    {
        /// <summary>
        /// 日志
        /// </summary>
        private static ILogger logger = null;

        private static Connection _connection = null;
        private static Channel _client = null;
        static IRpcClient _rpcClient = null;

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
            set
            {
                if (null == _channelPool)
                {
                    _channelPool = new Dictionary<int, Channel>();
                }
            }
        }

        private static long tps = 0;

        static void Main(string[] args)
        {
            logger = new EmptyLogger();

            _connection =
                Connection.Create(x => x
                .ConnectTo("localhost", "/")
                .WithCredentials("guest", "guest"));

            _rpcClient = new RpcClient();

            //_client =
            //    Channel.Create(_connection, x => x
            //        .ThroughDirectExchange("rpc")
            //            .WithRoutingKey("ping"));

            //int ii = 0;
            //int j = 0;

            //ThreadPool.GetMaxThreads(out ii, out j);//25,1000
            //ThreadPool.SetMaxThreads(50, 2000);

            int threadCount = 3;// Environment.ProcessorCount;

            if (args.Length > 0)
            {
                threadCount = int.Parse(args[0]);
            }

            try
            {
                // 起一个线程输出TPS
                Thread tpst = new Thread(new ThreadStart(OutputTps));
                tpst.Start();

                for (var i = 0; i < threadCount; i++)
                {
                    Task.Factory.StartNew(FearchDoWork);
                    //new Thread(new ThreadStart(FearchDoWork)).Start();
                    
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat(ex.Message);
            }

            Console.ReadKey();

        }

        static void OutputTps()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            while (true)
            {
                Thread.Sleep(1000);

                if (Interlocked.Read(ref tps) <= 0)
                {
                    continue;
                }
                Trace.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "]TPS:" + Interlocked.Read(ref tps));
                Interlocked.Exchange(ref tps, 0);
            }
        }

        static void FearchDoWork()
        {
            Console.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "]线程启动。");
            while (true)
            {
                //Execute();
                 Execute1();
                // LocalTest();
            }
        }


        static void Execute1()
        {
            IRpcClient client = new RpcClient();
            client.Call<Response>("ping", "abc", "ddd");
            Interlocked.Increment(ref tps);
        }
        static bool Execute()
        {
            Connection currentConnection;
            Channel currentChannel;
            ChannelPool.TryGetValue(Thread.CurrentThread.ManagedThreadId, out currentChannel);
            if (null == currentChannel)
            {      
                //currentConnection =
                //    Connection.Create(x => x
                //    .ConnectTo("localhost", "/")
                //    .WithCredentials("guest", "guest"));


                ChannelPool.Add(Thread.CurrentThread.ManagedThreadId, Channel.Create(_connection, x => x
                        .ThroughDirectExchange("rpc")
                            .WithRoutingKey("ping")));
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + "不存在，将创建");
                ChannelPool.TryGetValue(Thread.CurrentThread.ManagedThreadId, out currentChannel);
            }
            // Stopwatch st = new Stopwatch();
            //st.Start();
            //using (Channel client =
            //    Channel.Create(_connection, x => x
            //        .ThroughDirectExchange("rpc")
            //            .WithRoutingKey("ping")))
            //{

            var response = ChannelPool[Thread.CurrentThread.ManagedThreadId].Call<Request, Response>(new Request { Text = "hai" });
            //var response = client.Call<Request, Response>(new Request { Text = "hai" });
            Interlocked.Increment(ref tps);
            //}
           // st.Stop();
            //if (st.ElapsedMilliseconds > 50)
            //{
            //    logger.Debug("Call调用耗时：" + st.ElapsedMilliseconds);
            //}

            return true;
        }

        static void LocalTest()
        {
            Task<string> mainTask = new Task<string>(() =>
            {
                Thread.Sleep(5);
                Interlocked.Increment(ref tps);
                return "[" + Thread.CurrentThread.ManagedThreadId + "]test";                
            });
            mainTask.Start();
            var tmp = mainTask.Result;
           // Console.WriteLine(mainTask.Result);
        }
    }
}
