using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitMq.Common.Rpc
{
    /// <summary>
    /// <para>功能：Rpc客户端接口</para>
    /// <para>作者：hz0704027 </para>
    /// <para>日期：2015/3/20 9:30:07 </para>
    /// <para>备注：本代码版权归慧择网所有，严禁外传 </para>
    /// </summary>
    public interface IRpcClient
    {
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TMessage"> 返回值的类型 </typeparam>
        /// <param name="serviceId"> 服务标识 </param>
        /// <param name="interfaceId"> 接口标识 </param>
        /// <param name="method"> 方法标识 </param>
        /// <param name="p"> 传递的参数 </param>
        /// <returns> 返回执行结果 </returns>
        TMessage Call<TMessage>(string serviceId, string interfaceId, string method, params object[] p);

        /// <summary>
        /// 调用远程无返回值的方法
        /// </summary>
        /// <param name="serviceId"> 服务标识 </param>
        /// <param name="interfaceId"> 接口标识 </param>
        /// <param name="method"> 方法标识 </param>
        /// <param name="p">传递的参数</param>
        void Excute(string serviceId, string interfaceId, string method, params object[] p);
    }
}
