using System;
using System.Diagnostics.CodeAnalysis;
using JamaaTech.Smpp.Net.Lib.Protocol;
using JamaaTech.Smpp.Net.Lib.Util;

namespace JamaaTech.Smpp.Net.Lib.Testing
{
    /// <summary>
    /// Public test stub for RequestPDU to allow unit testing of components (e.g. ResponseHandler)
    /// without exposing internal constructors to external assemblies.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class TestRequestPDU : RequestPDU
    {
        public TestRequestPDU(uint sequence)
            : base(new PDUHeader(CommandType.BindTransmitter, sequence)) { }

        public override ResponsePDU CreateDefaultResponce() => new TestResponsePDU(Header.SequenceNumber);
        public override SmppEntityType AllowedSource => (SmppEntityType)0;
        public override SmppSessionState AllowedSession => (SmppSessionState)0;
        protected override byte[] GetBodyData() => new byte[0];
        protected override void Parse(ByteBuffer buffer) { }
    }

    /// <summary>
    /// Public test stub for ResponsePDU used by unit tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class TestResponsePDU : ResponsePDU
    {
        public TestResponsePDU(uint sequence)
            : base(new PDUHeader(CommandType.BindTransmitterResp, sequence)) { }

        public override SmppEntityType AllowedSource => (SmppEntityType)0;
        public override SmppSessionState AllowedSession => (SmppSessionState)0;
        protected override byte[] GetBodyData() => new byte[0];
        protected override void Parse(ByteBuffer buffer) { }
    }
}
