using RabbitMq.Common.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitMq.Common.Logger
{
    /// <summary>
    /// <para>功能：</para>
    /// <para>作者：lome </para>
    /// <para>日期：2015/10/1 21:06:15 </para>
    /// <para>备注：本代码版权归慧择网所有，严禁外传 </para>
    /// </summary>
    public class EmptyLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// 空日志实现
        /// </summary>
        private static readonly EmptyLogger Logger = new EmptyLogger();

        /// <summary>
        /// 按名称创建一个空的记录器实例。
        /// </summary>
        /// <param name="name">实例名称</param>
        /// <returns>返回日志记录器</returns>
        public ILogger Create(string name)
        {
            return Logger;
        }

        /// <summary>
        /// 根据给定的类型创建一个空的记录器实例。
        /// </summary>
        /// <param name="type">实例类型</param>
        /// <returns>返回日志记录器</returns>
        public ILogger Create(Type type)
        {
            return Logger;
        }
    }
}
