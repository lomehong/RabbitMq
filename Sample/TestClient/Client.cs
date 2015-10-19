using System;
using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;
using TestHelpers;
using TestHelpers.Domain;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TestClient
{

    class Client
    {
        static long tps = 0;

        static IConnection _connection;
        static SimpleRpcClient _client;

        //README: set TestClient and TestServer both as startup projects, then hit F5
        static void Main(string[] args)
        {
            // 起一个线程输出TPS
            Thread tpst = new Thread(new ThreadStart(OutputTps));
            tpst.Start();

            for (var i = 0; i < 1; i++)
            {
                Task.Factory.StartNew(FearchDoWork);
            }

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
            // Console.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "]线程启动。");
            while (true)
            {
                Execute();
            }
        }

        static void Execute()
        {
            //using (IModel ch = _connection.CreateModel())
            //{
            //    var textToSend = "intial message on startup";
            //    ch.ExchangeDeclare(Helper.ExchangeName, "direct");

            //    var queueName = ch.EnsureQueue();

            //    var client = new SimpleRpcClient(ch, queueName)
            //    {
            //        TimeoutMilliseconds = 4000
            //    };

            //    client.TimedOut += TimedOutHandler;
            //    client.Disconnected += DisconnectedHandler;

            //    var msgToSend = new WMessage
            //    {
            //        CreateDate = DateTime.Now,
            //        Name = "TestRequest",
            //        Body = textToSend
            //    }
            //            .Serialize();


            //    IBasicProperties replyProp;
            //    IBasicProperties prop = ch.CreateBasicProperties();

            //    var replyMessageBytes = client.Call(prop, msgToSend, out replyProp);

            //    var replyId = replyProp.MessageId;

            //    var response = WResponse.Deserialize(replyMessageBytes);

            //}

            MyClient client = new MyClient();
            client.Call("1");
            Interlocked.Increment(ref tps);
            client.Call("2");
            Interlocked.Increment(ref tps);
        }

        private static void DisconnectedHandler(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected");
        }

        private static void TimedOutHandler(object sender, EventArgs e)
        {
            Console.WriteLine("Timeout");
        }
    }
}
