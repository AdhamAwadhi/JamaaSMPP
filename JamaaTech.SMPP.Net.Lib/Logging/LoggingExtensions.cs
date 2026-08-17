using JamaaTech.Smpp.Net.Lib.Protocol.Tlv;
using System;
using System.Text;

namespace JamaaTech.Smpp.Net.Lib.Logging
{
    public static class LoggingExtensions
    {
        private static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Func<object, string> DumpString { get; set; } = DumpStringDefault;

        public static string DumpStringWithTry(object obj)
        {
            try
            {
                return DumpStringDefault(obj);
            }
            catch (Exception ex)
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error(ex);
                return null;
            }
        }

        public static string DumpStringDefault(object obj)
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
                    value = BytesToString(value as byte[]);
                }
                else if (value is TlvCollection)
                {
                    value = TlvCollectionToString(value as TlvCollection);
                }

                sb.AppendFormat("{0}:{1} ", property.Name, value);
            }

            return sb.ToString();
        }

        private static string TlvCollectionToString(TlvCollection tlvCollection)
        {
            var tags = new StringBuilder();
            tags.Append("[");
            foreach (var tlv in tlvCollection)
            {
                tags.AppendFormat("{0}:{1} ", tlv.Tag, BytesToString(tlv.RawValue));
            }
            tags.Append("]");

            return tags.ToString();
        }

        private static string BytesToString(byte[] value)
        {
            try
            {
                if (SmppEncodingService.Instance != null)
                    return SmppEncodingService.Instance.GetCStringFromBytes(value);

                return BytesToStringHex(value);
            }
            catch (Exception)
            {
                return BytesToStringHex(value);
            }
        }

        private static string BytesToStringHex(byte[] value)
        {
            var ba = value;
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba) hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
