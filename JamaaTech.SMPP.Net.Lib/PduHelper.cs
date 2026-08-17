using JamaaTech.Smpp.Net.Lib.Protocol;
using JamaaTech.Smpp.Net.Lib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamaaTech.Smpp.Net.Lib
{
    public static class PduHelper
    {
        public static PDU ParseFromBase64<T>(string data) where T : PDU
        {
            var pdu = ParseFromBase64(data);
            return pdu as T;
        }

        public static PDU ParseFromBase64(string data)
        {
            var packet = Convert.FromBase64String(data);
            return Parse(packet);
        }
        public static PDU ParseFromHex<T>(string data) where T : PDU
        {
            var pdu = ParseFromHex(data);
            return pdu as T;
        }

        public static PDU ParseFromHex(string data)
        {
            var packet = StringHelper.ConvertFromHexString(data);
            return Parse(packet);
        }

        public static PDU Parse(byte[] packet)
        {
            var bodyBytes = packet.Skip(16).ToArray();

            var pdu = PDU.CreatePDU(PDUHeader.Parse(new ByteBuffer(packet)));
            pdu.SetBodyData(new ByteBuffer(bodyBytes));

            return pdu;
        }
    }
}
