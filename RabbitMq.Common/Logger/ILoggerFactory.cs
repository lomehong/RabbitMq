using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitMq.Common.Logger
{
    /// <summary>
    /// <para>功能：</para>
    /// <para>作者：hz0704027 </para>
    /// <para>日期：2015/9/17 14:04:49 </para>
    /// <para>备注：本代码版权归慧择网所有，严禁外传 </para>
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// 创建具有给定的日志记录器名称的日志记录器。
        /// </summary>
        /// <param name="name">
        /// 日志名称
        /// </param>
        /// <returns>
        /// 返回一个 <see cref="ILogger"/> 类型实例
        /// </returns>
        ILogger Create(string name);

        /// <summary>
        /// 创建一个给定类型的日志记录器
        /// </summary>
        /// <param name="type">
        /// 类型
        /// </param>
        /// <returns>
        /// 返回一个 <see cref="ILogger"/> 类型实例。
        /// </returns>
        ILogger Create(Type type);
    }
}
