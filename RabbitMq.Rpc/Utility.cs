using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace RabbitMq.Rpc
{
    /// <summary>
    /// <para>功能：</para>
    /// <para>作者：hz0704027 </para>
    /// <para>日期：2015/10/19 18:11:05 </para>
    /// <para>备注：本代码版权归慧择网所有，严禁外传 </para>
    /// </summary>
	public static class Utility
    {
        public static RabbitMqConfig GetRabbitMqConfig()
        {
            var rabbitmqJson = File.ReadAllText(GetBinFolder() + "Config\\rabbitmq.config");
            var rabbitMqConfig = JsonConvert.DeserializeObject<RabbitMqConfig>(rabbitmqJson);
            return rabbitMqConfig;
        }
        /// <summary>
        /// 根据应用程序类型获取友好的应用程序名称（主要是文件夹的名称）
        /// </summary>
        /// <returns></returns>
        public static string GetFriendlyApplicationName()
        {
            string location = string.Empty;
            string applicationName = string.Empty;

            location = GetBinFolder();
            if (!string.IsNullOrEmpty(location))
            {
                string[] tmp = location.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                if (HttpContext.Current != null)
                {
                    applicationName = tmp[tmp.Length - 2];
                }
                else if (OperationContext.Current != null)
                {
                    applicationName = tmp[tmp.Length - 1];
                }
                else
                {
                    if (location.ToLower().IndexOf("\\bin\\") != -1)
                    {
                        applicationName = tmp[tmp.Length - 3];
                    }
                    else
                    {
                        applicationName = tmp[tmp.Length - 1];
                    }
                }
            }
            else
            {
                string[] tmp = AppDomain.CurrentDomain.BaseDirectory.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                applicationName = tmp[tmp.Length - 1];
            }
            return applicationName;
        }

        /// <summary>
        /// 根据当前应用程序类型返回Bin目录路径
        /// </summary>
        /// <returns></returns>
        public static string GetBinFolder()
        {
            string location;

            if (HttpContext.Current != null)
            {
                // web (IIS/WCF ASP compatibility mode)context
                location = HttpRuntime.BinDirectory;
            }
            else if (OperationContext.Current != null)
            {
                // pure wcf context
                location = HostingEnvironment.ApplicationPhysicalPath;
            }
            else
            {
                // no special hosting context (console/winform etc)
                location = AppDomain.CurrentDomain.BaseDirectory;
            }

            return location;
        }
    }
}
