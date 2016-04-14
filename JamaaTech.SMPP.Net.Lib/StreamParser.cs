/************************************************************************
 * Copyright (C) 2007 Jamaa Technologies
 *
 * This file is part of Jamaa SMPP Library.
 *
 * Jamaa SMPP Library is free software. You can redistribute it and/or modify
 * it under the terms of the Microsoft Reciprocal License (Ms-RL)
 *
 * You should have received a copy of the Microsoft Reciprocal License
 * along with Jamaa SMPP Library; See License.txt for more details.
 *
 * Author: Benedict J. Tesha
 * benedict.tesha@jamaatech.com, www.jamaatech.com
 *
 ************************************************************************/

using System;
using System.Net.Sockets;
using JamaaTech.Smpp.Net.Lib.Protocol;
using JamaaTech.Smpp.Net.Lib.Networking;
using JamaaTech.Smpp.Net.Lib.Util;
using System.Diagnostics;

namespace JamaaTech.Smpp.Net.Lib
{
    /// <summary>
    /// A class that reads underlying TCP/IP byte stream and interpret into pdu's
    /// </summary>
    public class StreamParser : RunningComponent
    {
        #region Variables
        private TcpIpSession vTcpIpSession;
        private PduProcessorCallback vProcessorCallback;
        private ResponseHandler vResponseHandler;
        //--
        private TraceSwitch vTraceSwitch;
        #endregion

        #region Events
        /// <summary>
        /// Raised when a malformed pdu is encountered by the parser.
        /// </summary>
        public event EventHandler<PDUErrorEventArgs> PDUError;
        /// <summary>
        /// Raised when an exception is thrown while a byte stream is being parsed or during pdu creation.
        /// </summary>
        public event EventHandler<ParserExceptionEventArgs> ParserException;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of the <see cref="StreamParser"/> class
        /// </summary>
        /// <param name="session">A <see cref="TcpIpSession"/></param>
        /// <param name="responseQueue">A <see cref="ResponseQueue"/> instance to which <see cref="ResponsePDU"/> pdu's are forwarded</param>
        /// <param name="requestProcessor">A callback delegate for processing <see cref="RequestPDU"/> pdu's</param>
        public StreamParser(TcpIpSession session,  ResponseHandler responseQueue,PduProcessorCallback requestProcessor)
        {
            if (session == null) { throw new ArgumentNullException("session"); }
            if (requestProcessor == null) { throw new ArgumentNullException("requestProcessor"); }
            if (responseQueue == null) { throw new ArgumentNullException("responseQueue"); }
            vTcpIpSession = session;
            vProcessorCallback = requestProcessor;
            vResponseHandler = responseQueue;
            //--Create and initialize a trace switch
            vTraceSwitch = new TraceSwitch("StreamParserSwitch", "Stream perser switch");
        }
        #endregion

        #region Properties

        #endregion

        #region Methods
        #region Interface Methods
        protected override void InitializeComponent()
        {
            base.InitializeComponent();
        }

        protected override void RunNow()
        {
            while (CanContinue())
            {
                try
                {
                    PDU pdu = WaitPDU();
                    if (pdu is RequestPDU) { vProcessorCallback.BeginInvoke(
                        (RequestPDU)pdu,AsyncCallBackProcessPduRequest,null); }
                    else if (pdu is ResponsePDU) { vResponseHandler.Handle(pdu as ResponsePDU); }
                }
                catch (PDUException) { /*Silent catch*/ }
                catch (TcpIpException) { /*Silent catch*/ }
                catch (Exception ex)
                {
                    if (vTraceSwitch.TraceError)
                    {
                        Trace.WriteLine(string.Format(
                            "200015:Unanticipated stream parser exception:{0};", ex));
                    }
                    throw;
                }
            }
        }

        #endregion

        #region Helper Methods
        private PDU WaitPDU()
        {
            PDUHeader header = null;
            PDU pdu = null;
            byte[] bodyBytes = null;
            byte[] headerBytes = null;
            //--
            try { headerBytes = ReadHeaderBytes(); }
            catch (TcpIpException tcpIp_ex_1) 
            {
                if (vTraceSwitch.TraceInfo)
                {
                    Trace.WriteLine(string.Format(
                        "200010:TCP/IP Exception encountered while reading pdu header bytes:{0};",
                        tcpIp_ex_1.Message));
                }
                HandleException(tcpIp_ex_1); throw; 
            }
            //--
            header = PDUHeader.Parse(new ByteBuffer(headerBytes));
            try { pdu = PDU.CreatePDU(header); }
            catch (InvalidPDUCommandException inv_ex)
            {
                ByteBuffer iBuffer = new ByteBuffer((int)header.CommandLength);
                iBuffer.Append(header.GetBytes());
                if (header.CommandLength > 16)
                {
                    try { iBuffer.Append(ReadBodyBytes((int)header.CommandLength - 16)); }
                    catch (TcpIpException tcpIp_ex_3) { HandleException(tcpIp_ex_3); }
                }
                if (vTraceSwitch.TraceWarning)
                {
                    Trace.WriteLine(string.Format(
                        "200011:Invalid PDU command type:{0};", iBuffer.DumpString()));
                }
                RaisePduErrorEvent(inv_ex, iBuffer.ToBytes(), header, null);
                throw;
            }
            //--
            try { bodyBytes = ReadBodyBytes((int)header.CommandLength - 16); }
            catch (TcpIpException tpcIp_ex_2) 
            {
                if (vTraceSwitch.TraceInfo)
                {
                    Trace.WriteLine(string.Format(
                        "200012:TCP/IP Exception encountered while reading pdu body bytes:{0};",
                        tpcIp_ex_2.Message));
                }
                HandleException(tpcIp_ex_2); throw; 
            }
            //--
            try { pdu.SetBodyData(new ByteBuffer(bodyBytes)); }
            catch (PDUException pdu_ex)
            {
                ByteBuffer pBuffer = new ByteBuffer((int)header.CommandLength);
                pBuffer.Append(headerBytes);
                pBuffer.Append(bodyBytes);
                RaisePduErrorEvent( pdu_ex, pBuffer.ToBytes(), header, pdu);
                if (vTraceSwitch.TraceWarning)
                {
                    Trace.WriteLine(string.Format(
                        "200013:Malformed PDU body received:{0} {1};", pBuffer.DumpString(), pdu_ex.Message));
                }
                throw;
            }
            return pdu;
        }

        private byte[] ReadHeaderBytes()
        {
            return WaitBytes(16);
        }

        private byte[] ReadBodyBytes(int length)
        {
            return WaitBytes(length);
        }

        private byte[] WaitBytes(int length)
        {
            byte[] bytes = new byte[length];
            int remaining = length;
            int received = 0;
            while (remaining > 0)
            {
                int receiveCount = vTcpIpSession.Receive(bytes, received, remaining);
                if (receiveCount == 0 && vTraceSwitch.TraceWarning)
                { Trace.WriteLine("200014:TCP/IP receive operation returned zero bytes;"); }
                received += receiveCount;
                remaining = length - received;
            }
            return bytes;
        }

        private void HandleException(Exception ex)
        {
            RaiseParserExceptionEvent(ex);
            StopOnNextCycle();
        }

        private void RaisePduErrorEvent(PDUException exception, byte[] byteDump, PDUHeader header, PDU pdu)
        {
            if (PDUError == null) { return; }
            PDUErrorEventArgs e = new PDUErrorEventArgs(exception, byteDump, header, pdu);
            foreach (EventHandler<PDUErrorEventArgs> del in PDUError.GetInvocationList())
            { del.BeginInvoke(this, e, AsyncCallBackRaisePduErrorEvent, del); }
        }

        private void RaiseParserExceptionEvent(Exception exception)
        {
            if (ParserException == null) { return; }
            ParserExceptionEventArgs e = new ParserExceptionEventArgs(exception);
            foreach (EventHandler<ParserExceptionEventArgs> del in ParserException.GetInvocationList())
            { del.BeginInvoke(this, e, AsyncCallBackRaiseParserExceptionEvent, del); }
        }

        private void AsyncCallBackRaisePduErrorEvent(IAsyncResult result)
        {
            EventHandler<PDUErrorEventArgs> del =
                (EventHandler<PDUErrorEventArgs>)result.AsyncState;
            del.EndInvoke(result);
        }

        private void AsyncCallBackRaiseParserExceptionEvent(IAsyncResult result)
        {
            EventHandler<ParserExceptionEventArgs> del =
                (EventHandler<ParserExceptionEventArgs>)result.AsyncState;
            del.EndInvoke(result);
        }

        private void AsyncCallBackProcessPduRequest(IAsyncResult result)
        {
            vProcessorCallback.EndInvoke(result);
        }
        #endregion
        #endregion
    }
}
