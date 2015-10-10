using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitMq.Common.Logger
{
    /// <summary>
    /// <para>功能：</para>
    /// <para>作者：hz0704027 </para>
    /// <para>日期：2015/9/17 14:03:56 </para>
    /// <para>备注：本代码版权归慧择网所有，严禁外传 </para>
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 写入一条 debug 等级的消息
        /// </summary>
        /// <param name="message">待写入的消息</param>
        void Debug(object message);

        /// <summary>
        /// 写入一条 debug 等级的消息
        /// </summary>
        /// <param name="format">格式</param>
        /// <param name="args">参数</param>
        void DebugFormat(string format, params object[] args);

        /// <summary>写入一条 debug 等级的消息
        /// </summary>
        /// <param name="message">待写入的消息</param>
        /// <param name="exception">异常</param>
        void Debug(object message, Exception exception);

        /// <summary>写入一条 info 等级的消息
        /// </summary>
        /// <param name="message">待写入的消息</param>
        void Info(object message);

        /// <summary>
        /// 写入一条 info 等级的消息
        /// </summary>
        /// <param name="format">格式</param>
        /// <param name="args">参数</param>
        void InfoFormat(string format, params object[] args);

        /// <summary>
        /// 写入一条 info 等级的消息
        /// </summary>
        /// <param name="message">待写入的消息</param>
        /// <param name="exception">异常</param>
        void Info(object message, Exception exception);

        /// <summary>
        /// 写入一条 error 等级的消息
        /// </summary>
        /// <param name="message">待写入的消息</param>
        void Error(object message);

        /// <summary>
        /// 写入一条 error 等级的消息
        /// </summary>
        /// <param name="format">格式</param>
        /// <param name="args">参数</param>
        void ErrorFormat(string format, params object[] args);

        /// <summary>
        /// 写入一条 error 等级的消息
        /// </summary>
        /// <param name="message">待写入的消息</param>
        /// <param name="exception">异常</param>
        void Error(object message, Exception exception);

        /// <summary>
        /// 写入一条 warnning 等级的消息
        /// </summary>
        /// <param name="message">待写入的消息</param>
        void Warn(object message);

        /// <summary>
        /// 写入一条 warnning 等级的消息
        /// </summary>
        /// <param name="format">格式</param>
        /// <param name="args">参数</param>
        void WarnFormat(string format, params object[] args);

        /// <summary>
        /// 写入一条 warnning 等级的消息
        /// </summary>
        /// <param name="message">待写入的消息</param>
        /// <param name="exception">异常</param>
        void Warn(object message, Exception exception);

        /// <summary>
        /// 写入一条 fatal 等级的消息
        /// </summary>
        /// <param name="message">待写入的消息</param>
        void Fatal(object message);

        /// <summary>
        /// 写入一条 fatal 等级的消息
        /// </summary>
        /// <param name="format">格式</param>
        /// <param name="args">参数</param>
        void FatalFormat(string format, params object[] args);

        /// <summary>
        /// 写入一条 fatal 等级的消息
        /// </summary>
        /// <param name="message">待写入的消息</param>
        /// <param name="exception">异常</param>
        void Fatal(object message, Exception exception);
    }
}
