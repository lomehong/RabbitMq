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
        /// ��ͼ��Ͱ������һ��ָ�����������ơ���������Ʊ����ĵ��ˣ��򷵻� <code>true</code>
        ///  ���򷵻� <code>false</code> ��
        /// </summary>
        /// <param name="numTokens">��Ͱ�����ĵ����Ƶ�������������һ����������</param>
        /// <returns><code>true</code> if the tokens were consumed, <code>false</code> otherwise.</returns>
        bool TryConsume(long numTokens);

        /// <summary>
        /// ��Ͱ������һ����һ�����ơ����û�����ƣ���˷���������ֱ��һ�����ƿ��á�
        /// </summary>
        void Consume();

        /// <summary>
        /// ��Ͱ�����Ķ�����ơ����û���㹻�����ƣ���ô�����������ֹ
        /// </summary>
        /// <param name="numTokens">��Ͱ�����ĵ����Ƶ�������������һ����������</param>
        void Consume(long numTokens);
    }
}