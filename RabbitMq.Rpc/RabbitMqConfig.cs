using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Rpc
{
    /// <summary>
    /// <para>功能：RabbitMQ配置</para>
    /// <para>作者：hz0704027 </para>
    /// <para>日期：2014/12/15 9:41:01 </para>
    /// <para>备注：本代码版权归慧择网所有，严禁外传 </para>
    /// </summary>
    public class RabbitMqConfig
    {
        /// <summary>
        /// 获取或设置主机名
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// 获取或设置用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 获取或设置用户密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 获取或设置虚拟主机
        /// </summary>
        public string VirtualHost { get; set; }

        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// 获取或设置端口
        /// </summary>
        public int Port { get; set; }
    }
}
