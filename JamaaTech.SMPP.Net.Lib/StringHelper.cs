using JamaaTech.Smpp.Net.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamaaTech.Smpp.Net.Lib
{
    public static class StringHelper
    {
        public static List<String> Split(string message, int maxPartLength)
        {
            var result = new List<String>();

            for (int i = 0; i < message.Length; i += maxPartLength)
            {
                var chunkSize = i + maxPartLength < message.Length ? maxPartLength : message.Length - i;
                var chunk = new char[chunkSize];
                message.CopyTo(i, chunk, 0, chunkSize);
                result.Add(new string(chunk));
            }

            return result;

        }

        public static int GetMaxMessageLength(DataCoding encoding, bool includeUdh, bool useUdh16Bit = false)
        {
            switch (encoding)
            {
                case DataCoding.SMSCDefault:
                    return includeUdh ? (useUdh16Bit ? 152 : 153) : 160;
                case DataCoding.Latin1:
                    return includeUdh ? (useUdh16Bit ? 133 : 134) : 140;
                case DataCoding.ASCII:
                    return includeUdh ? (useUdh16Bit ? 152 : 153) : 160;
                case DataCoding.UCS2:
                    return includeUdh ? (useUdh16Bit ? 66 : 67) : 70;
                default:
                    throw new InvalidOperationException("Invalid or unsuported encoding for text message ");
            }
        }

        public static byte[] ConvertFromHexString(string hex)
        {
#if NET8_0_OR_GREATER
            return Convert.FromHexString(hex);
#else
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
#endif
        }


    }
}
