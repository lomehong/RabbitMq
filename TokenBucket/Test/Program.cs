using Esendex.TokenBucket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a token bucket with a capacity of 1 token that refills at a fixed interval of 1 token/sec.
            ITokenBucket bucket = TokenBuckets.Construct()
              .WithCapacity(100)
              .WithFixedIntervalRefillStrategy(100, TimeSpan.FromSeconds(1))
              .Build();

            // ...

            while (true)
            {
                // Consume a token from the token bucket.  If a token is not available this method will block until
                // the refill strategy adds one to the bucket.
                bucket.Consume(1);

                Poll();
            }
        }

       static void Poll()
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"));
        }
    }
}
