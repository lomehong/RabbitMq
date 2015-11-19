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

        static IRpcClient _rpcClient;

        private static long tps = 0;

        static void Main(string[] args)
        {
            logger = new EmptyLogger();

            _rpcClient = new RpcClient();
            int threadCount = 20;// Environment.ProcessorCount;

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
                    Thread.Sleep(1);
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
                Execute();
                // Execute1();
                // LocalTest();
            }
        }


        static void Execute1()
        {
            IRpcClient client = new RpcClient();
            client.Call<Response>("remoting-examples-server", "com.hzins.remoting.example.api.facade.UserFacade", "getUserName", long.Parse("1"));
            Interlocked.Increment(ref tps);
        }
        static bool Execute()
        {
            var response = _rpcClient.Call<string>("rpc_queue", string.Empty, string.Empty);
            Interlocked.Increment(ref tps);

            return true;
        }
    }
}
