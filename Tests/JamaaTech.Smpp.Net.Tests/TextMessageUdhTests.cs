using System.Collections.Generic;
using JamaaTech.Smpp.Net.Client;
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Protocol;
using Xunit;

namespace JamaaTech.Smpp.Net.Tests
{
    public class TextMessageUdhTests
    {
        [Fact]
        public void GetPdus_DefaultsTo8BitUdh()
        {
            var message = new TestTextMessage
            {
                Text = new string('a', 134)
            };

            List<byte[]> messageParts = message.GetMessageParts(DataCoding.UCS2);

            Assert.Equal(2, messageParts.Count);
            Assert.Equal(67, message.MaxMessageLength);
            Assert.Equal(new byte[] { 0x05, 0x00, 0x03 }, messageParts[0][0..3]);
            Assert.Equal(2, messageParts[0][4]);
            Assert.Equal(1, messageParts[0][5]);
            Assert.Equal(140, messageParts[0].Length);
        }

        [Fact]
        public void GetPdus_16BitReference_UsesUdh16AndSmallerParts()
        {
            var message = new TestTextMessage
            {
                Text = new string('a', 134),
                UseUdh16Bit = true
            };

            List<byte[]> messageParts = message.GetMessageParts(DataCoding.UCS2);

            Assert.Equal(3, messageParts.Count);
            Assert.Equal(66, message.MaxMessageLength);
            Assert.Equal(new byte[] { 0x06, 0x08, 0x04 }, messageParts[0][0..3]);
            Assert.Equal(3, messageParts[0][5]);
            Assert.Equal(1, messageParts[0][6]);
            Assert.Equal(139, messageParts[0].Length);
        }

        [Fact]
        public void DefaultSegmentIdGenerator_Uses16BitRange()
        {
            var generator = new DefaultSegmentIdGenerator();

            int segmentId = 0;
            for (int i = 0; i <= 256; i++)
            {
                segmentId = generator.NextSegmentId("source", "destination");
            }

            Assert.Equal(256, segmentId);
            Assert.Equal(new byte[] { 0x01, 0x00 }, new Udh16(segmentId, 2, 1).GetBytes()[3..5]);
            Assert.Equal(0, new Udh(segmentId, 2, 1).GetBytes()[3]);
        }

        [Fact]
        public void GetPdus_MultiPart_ReturnsIndependentPdusWithUniqueSequences()
        {
            var message = new TestTextMessageMultiPart
            {
                Text = new string('a', 134),
                UseUdh16Bit = true
            };

            List<SendSmPDU> pdus = message.GetPdus(DataCoding.UCS2);

            Assert.Equal(3, pdus.Count);
            Assert.All(pdus, pdu => Assert.IsType<SubmitSmMultiPart>(pdu));
            Assert.NotSame(pdus[0], pdus[1]);
            Assert.NotSame(pdus[1], pdus[2]);
            Assert.NotEqual(pdus[0].Header.SequenceNumber, pdus[1].Header.SequenceNumber);
            Assert.NotEqual(pdus[1].Header.SequenceNumber, pdus[2].Header.SequenceNumber);
            Assert.False(((SubmitSmMultiPart)pdus[0]).HasResponse);
            Assert.False(((SubmitSmMultiPart)pdus[1]).HasResponse);
            Assert.True(((SubmitSmMultiPart)pdus[2]).HasResponse);
            Assert.Equal(1, pdus[0].GetMessageBytes()[6]);
            Assert.Equal(2, pdus[1].GetMessageBytes()[6]);
            Assert.Equal(3, pdus[2].GetMessageBytes()[6]);
        }

        private sealed class TestTextMessage : TextMessage
        {
            public List<byte[]> GetMessageParts(DataCoding dataCoding)
            {
                var parts = new List<byte[]>();
                var encodingService = new SmppEncodingService();

                foreach (SendSmPDU pdu in base.GetPDUs(dataCoding, encodingService))
                {
                    parts.Add(pdu.GetMessageBytes());
                }

                return parts;
            }
        }

        private sealed class TestTextMessageMultiPart : TextMessageMultiPart
        {
            public List<SendSmPDU> GetPdus(DataCoding dataCoding)
            {
                var pdus = new List<SendSmPDU>();
                var encodingService = new SmppEncodingService();

                foreach (SendSmPDU pdu in base.GetPDUs(dataCoding, encodingService))
                {
                    pdus.Add(pdu);
                }

                return pdus;
            }
        }
    }
}
