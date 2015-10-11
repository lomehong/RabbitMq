using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Common.RateLimiters
{
    public class TokenBucket
    {
        private static Queue<string> _callFuns;
        public TMessage Invoke<TMessage>(Func<string, TMessage> fun)
        {
            var result = default(TMessage);


            while (_callFuns.Count < 100)
            {
                _callFuns.Dequeue();
                fun(string.Empty);
            }
            return result;
        }
    }
}
