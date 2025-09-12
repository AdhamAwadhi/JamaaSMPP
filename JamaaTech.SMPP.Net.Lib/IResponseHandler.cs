/************************************************************************
 * (c) Jamaa Technologies - Interface introduced for pluggable response handling
 ************************************************************************/
using JamaaTech.Smpp.Net.Lib.Protocol;

namespace JamaaTech.Smpp.Net.Lib
{
    /// <summary>
    /// Abstraction over the handling of SMPP request/response correlation.
    /// </summary>
    public interface IResponseHandler
    {
        int DefaultResponseTimeout { get; }
        int Count { get; }

        void Handle(ResponsePDU pdu);
        ResponsePDU WaitResponse(RequestPDU pdu);
        ResponsePDU WaitResponse(RequestPDU pdu, int timeOut);
    }
}