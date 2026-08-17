using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Util;
using Xunit;

namespace JamaaTech.Smpp.Net.Tests
{
    public class UdhTests
    {
        [Fact]
        public void Udh16_GetBytes_Uses16BitReferenceNumber()
        {
            Udh udh = new Udh16(0xABCD, 3, 2);

            byte[] bytes = udh.GetBytes();

            Assert.Equal(new byte[] { 0x06, 0x08, 0x04, 0xAB, 0xCD, 0x03, 0x02 }, bytes);
        }

        [Fact]
        public void Parse_16BitReferenceNumber_ReturnsUdh16()
        {
            var buffer = new ByteBuffer(new byte[] { 0x06, 0x08, 0x04, 0xAB, 0xCD, 0x03, 0x02 });

            Udh udh = Udh.Parse(buffer);

            Assert.IsType<Udh16>(udh);
            Assert.Equal(0xABCD, udh.SegmentID);
            Assert.Equal(3, udh.MessageCount);
            Assert.Equal(2, udh.MessageSequence);
        }

        [Fact]
        public void Udh_GetBytes_ContinuesToUse8BitReferenceNumber()
        {
            var udh = new Udh(0xAB, 3, 2);

            byte[] bytes = udh.GetBytes();

            Assert.Equal(new byte[] { 0x05, 0x00, 0x03, 0xAB, 0x03, 0x02 }, bytes);
        }
    }
}
