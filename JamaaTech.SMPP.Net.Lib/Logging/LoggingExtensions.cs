using System;
using System.Collections.Generic;
using System.Text;

namespace JamaaTech.Smpp.Net.Lib.Logging
{
    public static class LoggingExtensions
    {
#if NET2
        public static string DumpStrig(this object obj)
#else
        public static string DumpStrig(object obj)
#endif
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} -- ", obj.GetType().Name);

            foreach (var property in obj.GetType().GetProperties())
            {
                object value = "--";

                try
                {
                    value = property.GetValue(obj, null);
                }
                catch { }

                if (value is byte[])
                {
                    var ba = value as byte[];
                    var hex = new StringBuilder(ba.Length * 2);
                    foreach (byte b in ba) hex.AppendFormat("{0:x2}", b);
                    value = hex.ToString();
                }

                sb.AppendFormat("{0}:{1} ", property.Name, value);
            }

            return sb.ToString();
        }
    }
}
