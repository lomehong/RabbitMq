using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

//Yes not folowing pattern of nesting and naming, but just consolodating items for the demo
namespace TestHelpers.Domain
{

    [Serializable]
    public class WResponse
    {
        public string Message;

        public static byte[] Serialize(WResponse workload)
        {
            var s = new BinaryFormatter();
            var str = new MemoryStream();
            s.Serialize(str, workload);
            str.Position = 0;
            var b = new byte[str.Length];
            str.Read(b, 0, (int)str.Length);

            return b;
        }

        public static WResponse Deserialize(byte[] bytes)
        {
            var s = new BinaryFormatter();
            var str = new MemoryStream(bytes);

            return (WResponse)s.Deserialize(str);
        }
    }

    [Serializable]
    public class WMessage
    {
        public string Name;
        public DateTime CreateDate;
        public string Body;

        public static byte[] Serialize(WMessage wMessage)
        {
            var s = new BinaryFormatter();
            var str = new MemoryStream();
            s.Serialize(str, wMessage);
            str.Position = 0;
            var b = new byte[str.Length];
            str.Read(b, 0, (int)str.Length);
            return b;
        }

        public static WMessage Deserialize(byte[] bytes)
        {
            var s = new BinaryFormatter();
            var str = new MemoryStream(bytes);
            var workload = (WMessage)s.Deserialize(str);
            return workload;
        }

        public override String ToString()
        {
            return String.Format("Name: {1}{0}CreatedDate: {2}{0}Body: {3}{0}",
                Environment.NewLine,
                Name,
                CreateDate.ToShortTimeString(),
                Body);
        }
    }

    //possible refactor, make these generic, complexity probably not necessary tho...
    //otherwise .Deserialize() doesn't work for both, ambiguous  definition issue

    public static class WMessageExtensions
    {
        public static byte[] Serialize(this WMessage src)
        {
            return WMessage.Serialize(src);
        }
    }

    public static class WResponseExtensions
    {
        public static byte[] Serialize(this WResponse src)
        {
            return WResponse.Serialize(src);
        }
    }
}
