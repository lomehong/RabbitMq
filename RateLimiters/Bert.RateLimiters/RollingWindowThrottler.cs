using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Bert.RateLimiters
{
    /// <summary>
    /// 用于控制每单位时间的动作的发生率。
    /// </summary>
    /// <remarks>
    /// The algorithm uses a rolling time window so that precise control over the rate is achieved (without any bursts)
    /// </remarks>
    public class RollingWindowThrottler
    {
        /// <summary>
        /// 单位时间内允许出现的最大数量
        /// </summary>
        private readonly int occurrences;

        private readonly long timeUnitTicks;
        private readonly object syncRoot = new object();
        private int remainingTokens;

        /// <summary>
        /// provides a queue of timestamps expressed in ticks when reservations expire
        /// </summary>
        private readonly Queue<long> expirationTimestampsQueue;
        private long nextCheckTime;

        /// <summary>
        /// 构建了节流器的实例。
        /// </summary>
        /// <param name="occurrences">单位时间内允许出现的最大数量。</param>
        /// <param name="timeUnit">The time unit in which the occurences are constrained.</param>
        public RollingWindowThrottler(int occurrences, TimeSpan timeUnit)
        {
            if(occurrences<=0)
                throw new ArgumentOutOfRangeException("occurrences", "Number of occurences must be a positive integer");


            this.occurrences = occurrences;
            timeUnitTicks = timeUnit.Ticks;
            remainingTokens = occurrences;
            expirationTimestampsQueue = new Queue<long>(occurrences);
        }


        /// <summary>
        /// Total number of occurrences of an action which are allowed for a time unit
        /// </summary>
        public int Occurrences
        {
            get { return occurrences; }
        }

        /// <summary>
        /// The time unit in which the maximal number of <see cref="Occurrences"/> are allowed..
        /// </summary>
        public TimeSpan TimeUnit
        {
            get { return TimeSpan.FromTicks(timeUnitTicks); }
        }

        /// <summary>
        /// Returns the number of available tokens which can be reserved until the end of current time unit
        /// </summary>
        public int AvailableTokens
        {
            get
            {
                lock (syncRoot)
                {
                    return remainingTokens;
                }
            }
        }

        /// <summary>
        /// Tries to reserve one token in the configured time unit.
        /// </summary>
        /// <param name="waitTimeMillis">total suggested wait time in milliseconds till tokens will become available for reservation</param>
        /// <returns>true if the caller should throttle/wait, or false if reservation was made successfully.</returns>
        public bool ShouldThrottle(out long waitTimeMillis)
        {
            return ShouldThrottle(1, out waitTimeMillis);
        }


        /// <summary>
        /// Tries to reserve <see cref="tokens"/> in the configured time unit.
        /// </summary>
        /// <param name="tokens">total number of reservations</param>
        /// <param name="waitTimeMillis">total suggested wait time in milliseconds till tokens will become available for reservation</param>
        /// <returns>true if the caller should throttle/wait, or false if reservation was made successfully.</returns>
        public bool ShouldThrottle(int tokens, out long waitTimeMillis)
        {
            if (tokens <= 0) throw new ArgumentOutOfRangeException("tokens", "Should be positive integer greater than 0");
            var currentTime = SystemTime.UtcNow.Ticks;

            lock (syncRoot)
            {
                CheckExitTimeQueue();
                if (remainingTokens - tokens >= 0)
                {
                    remainingTokens -= tokens;
                    waitTimeMillis = 0;
                    long timeToExit = unchecked (currentTime + timeUnitTicks);
                    for (int i = 0; i < tokens; i++)
                        expirationTimestampsQueue.Enqueue(timeToExit);
                    return false;
                }

                waitTimeMillis = (nextCheckTime - currentTime) / TimeSpan.TicksPerMillisecond;
                return true;
            }
        }

        private void CheckExitTimeQueue()
        {
            if(nextCheckTime > SystemTime.UtcNow.Ticks)
                return;

            while (expirationTimestampsQueue.Count > 0 && expirationTimestampsQueue.Peek() <= SystemTime.UtcNow.Ticks)
            {
                expirationTimestampsQueue.Dequeue();
                remainingTokens++;
            }

            //try to determine next check time
            if (expirationTimestampsQueue.Count > 0)
            {
                var item = expirationTimestampsQueue.Peek();
                nextCheckTime = item;
            }
            else
                nextCheckTime = timeUnitTicks;
        }
        
    }
}