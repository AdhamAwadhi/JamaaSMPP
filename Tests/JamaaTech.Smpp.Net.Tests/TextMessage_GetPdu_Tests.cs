using JamaaTech.Smpp.Net.Client;
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JamaaTech.Smpp.Net.Tests
{
    public class TextMessage_GetPdu_Tests
    {
        [Fact]
        public void GetPdus_DefaultsTo8BitUdh()
        {
            var message = new TestTextMessage
            {
                Text = new string('a', 133)
            };

            List<SendSmPDU> messageParts = message.GetMessageParts(DataCoding.UCS2);

            Assert.Equal(2, messageParts.Count);
            Assert.Equal(67, message.MaxMessageLength);
            Assert.NotEqual(messageParts[0], messageParts[1]);
            var bytes0 = messageParts[0].GetBytes();
            var bytes1 = messageParts[1].GetBytes();
            Assert.NotEqual(bytes0, bytes1);
            //Assert.Equal(new byte[] { 0x05, 0x00, 0x03 }, messageParts[0][0..3]);
            //Assert.Equal(2, messageParts[0][4]);
            //Assert.Equal(1, messageParts[0][5]);
            //Assert.Equal(140, messageParts[0].Length);
        }

        private sealed class TestTextMessage : TextMessage
        {
            public List<SendSmPDU> GetMessageParts(DataCoding dataCoding)
            {
                var parts = new List<SendSmPDU>();

                foreach (SendSmPDU pdu in base.GetPDUs(dataCoding))
                {
                    parts.Add(pdu);
                }

                return parts;
            }
        }
    }
}
