using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace Conejo
{
    /// <summary>
    /// <para>功能：</para>
    /// <para>作者：lome </para>
    /// <para>日期：2015/10/1 22:21:44 </para>
    /// <para>备注：本代码版权归慧择网所有，严禁外传 </para>
    /// </summary>
	public static class Extension
    {
        public static string Get(this Dictionary<string, string> dic, string key)
        {
            string o = string.Empty;
            //Stopwatch st = new Stopwatch();
            //st.Start();
            dic.TryGetValue(key, out o);
            while (string.IsNullOrEmpty(o))
            {
                dic.TryGetValue(key, out o);

            }
            //st.Stop();
            //Console.WriteLine("[" + key + "]Get耗时：" + st.ElapsedMilliseconds);
            // dic.Remove(key);
            return o;
        }
    }
}
