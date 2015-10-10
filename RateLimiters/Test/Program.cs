using Bert.RateLimiters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {

        private static Dictionary<string, string> _current;
        private static Dictionary<string, string> Current()
        {
            if (null == _current)
            {
                _current = new Dictionary<string, string>();
            }
            return _current;
        }
        static void Main(string[] args)
        {
            //*****************************************************************************************************//
            //*** Create an observable sequence from the enumerator which is yielding random emails within      ***//
            //*** 3 sec. continuously.  The enumeration of the enumerator will be scheduled to run on a thread  ***//
            //*** in the .NET thread pool so the main thread will not be blocked.                               ***//
            //*****************************************************************************************************//

            new Thread(new ThreadStart(DoSomethine)).Start();

            var obs = EndlessBarrageOfEmails().ToObservable(Scheduler.Default);


            //****************************************************************************************************//
            //*** Use the throttle operator to ONLY deliver an item that occurs with a 2 second interval       ***//
            //*** between it and the next item in the sequence. The throttle buffer will hold an item from the ***//
            //*** sequence waiting for the 2 second timespan to pass. If a new item is produced before the     ***//
            //*** time span expires, that new item will replace the old item in the buffer and the wait starts ***//
            //*** over. If the time span does expire before a new item is produced, then the item in the       ***//
            //*** buffer will be observed through any subscriptions on the sequence.                           ***//
            //***                                                                                              ***//
            //*** To be clear, an item is not guarnteed to be returned every 2 seconds. The use of throttle    ***//
            //*** here does guarntee that the subscriber will observe an item no faster than every 2 sec.      ***//
            //***                                                                                              ***//
            //*** Since a new email is generated at a random time within 3 seconds, the items which are        ***//
            //*** generated with 2 seconds of silence following them will also be random.                      ***//
            //***                                                                                              ***//
            //*** The timers associated with the 2 second time span are run on the .NET thread pool.           ***//
            //****************************************************************************************************//

            var obsThrottled = obs.Throttle(TimeSpan.FromSeconds(2), Scheduler.Default);


            //***********************************************************************************************//
            //*** Write each observed email to the console window. Also write a current timestamp to get  ***//
            //*** an idea of the time which has passed since the last item was observed. Notice, the time ***//
            //*** will not be less than 2 seconds but, will frequently exceed 2 sec.                      ***//
            //***********************************************************************************************//

            obsThrottled.Subscribe(i => Console.WriteLine("{0}\nTime Received {1}\n", i, DateTime.Now.ToString()));


            //*********************************************************************************************//
            //*** Main thread waiting on the user's ENTER key press.                                    ***//
            //*********************************************************************************************//

            Console.WriteLine("\nPress ENTER to exit...\n");
            Console.ReadLine();
        }

        static void Call()
        {
            var obs = _current.ToObservable(Scheduler.Default);
            var obsThrottled = obs.Throttle(TimeSpan.FromSeconds(2), Scheduler.Default);
            obsThrottled.Subscribe(i => Console.WriteLine("{0}\nTime Received {1}\n", i, DateTime.Now.ToString()));
        }

        static void DoSomethine()
        {
            while (true)
            {
                if (null != _current)
                {
                    _current.Add(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                    _current.Add(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                    _current.Add(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                }
                Thread.Sleep(1000);
            }

        }

        //*********************************************************************************************//
        //***                                                                                       ***//
        //*** This method will continually yield a random email at a random interval within 3 sec.  ***//
        //***                                                                                       ***//
        //*********************************************************************************************//
        static IEnumerable<string> EndlessBarrageOfEmails()
        {
            Random random = new Random();

            //***************************************************************//
            //*** For this example we are using this fixed list of emails ***//
            //***************************************************************//
            List<string> emails = new List<string> { "Email Msg from John ",
                                               "Email Msg from Bill ",
                                               "Email Msg from Marcy ",
                                               "Email Msg from Wes "};

            //***********************************************************************************//
            //*** Yield an email from the list continually at a random interval within 3 sec. ***//
            //***********************************************************************************//
            while (true)
            {
                emails.Add(Guid.NewGuid().ToString());
                yield return emails[random.Next(emails.Count)];
                Thread.Sleep(random.Next(3000));
            }
        }
    }
}
