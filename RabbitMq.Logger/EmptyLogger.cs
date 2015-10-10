using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Common.Logger
{
    public class EmptyLogger : ILogger
    {
        #region ILogger Members

        /// <summary>Returns false.
        /// </summary>
        public bool IsDebugEnabled
        {
            get
            {
                return false;
            }
        }

        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message">消息</param>
        public void Debug(object message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
        }

        /// <summary>Do nothing.
        /// </summary>
        /// <param name="format">格式</param>
        /// <param name="args">参数</param>
        public void DebugFormat(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(format, args);
        }

        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="exception">异常</param>
        public void Debug(object message, Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
        }

        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message">消息</param>
        public void Info(object message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
        }

        /// <summary>Do nothing.
        /// </summary>
        /// <param name="format">格式</param>
        /// <param name="args">参数</param>
        public void InfoFormat(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(format, args);
        }

        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="exception">异常</param>
        public void Info(object message, Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
        }

        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message">消息</param>
        public void Error(object message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        /// <param name="format">格式</param>
        /// <param name="args">参数</param>
        public void ErrorFormat(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(format, args);
        }

        /// <summary>Do nothing.
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="exception">异常</param>
        public void Error(object message, Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        /// <param name="message">消息</param>
        public void Warn(object message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        /// <param name="format">格式</param>
        /// <param name="args">参数</param>
        public void WarnFormat(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(format, args);
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="exception">异常</param>
        public void Warn(object message, Exception exception)
        {

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        /// <param name="message">消息</param>
        public void Fatal(object message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(message);
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        /// <param name="format">格式</param>
        /// <param name="args">参数</param>
        public void FatalFormat(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(format, args);
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="exception">异常</param>
        public void Fatal(object message, Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(message);
        }

        #endregion
    }
}
