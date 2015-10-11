namespace Esendex.TokenBucket
{
    /// <summary>
    /// A token bucket is used for rate limiting access to a portion of code.
    /// 
    /// See <a href="http://en.wikipedia.org/wiki/Token_bucket">Token Bucket on Wikipedia</a>
    /// See <a href="http://en.wikipedia.org/wiki/Leaky_bucket">Leaky Bucket on Wikipedia</a>
    /// </summary>
    public interface ITokenBucket
    {
        /// <summary>
        /// Attempt to consume a single token from the bucket.  If it was consumed then <code>true</code>
        /// is returned, otherwise <code>false</code> is returned.
        /// </summary>
        /// <returns><code>true</code> if the tokens were consumed, <code>false</code> otherwise.</returns>
        bool TryConsume();

        /// <summary>
        /// 试图从桶中消耗一个指定数量的令牌。如果该令牌被消耗掉了，则返回 <code>true</code>
        ///  否则返回 <code>false</code> 。
        /// </summary>
        /// <param name="numTokens">从桶中消耗的令牌的数量，必须是一个正整数。</param>
        /// <returns><code>true</code> if the tokens were consumed, <code>false</code> otherwise.</returns>
        bool TryConsume(long numTokens);

        /// <summary>
        /// 从桶中消耗一个单一的令牌。如果没有令牌，则此方法将阻塞直到一个令牌可用。
        /// </summary>
        void Consume();

        /// <summary>
        /// 从桶中消耗多个令牌。如果没有足够的令牌，那么这个方法将阻止
        /// </summary>
        /// <param name="numTokens">从桶中消耗的令牌的数量，必须是一个正整数。</param>
        void Consume(long numTokens);
    }
}