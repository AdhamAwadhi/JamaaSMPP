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
using System.Collections.Generic;
using System.Text;
using JamaaTech.Smpp.Net.Lib.Networking;
using JamaaTech.Smpp.Net.Lib.Protocol;
using System.Timers;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using JamaaTech.Smpp.Net.Lib.Util;

namespace JamaaTech.Smpp.Net.Lib
{
    public class SmppClientSession
    {
        private static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Variables
        private Timer vTimer;
        private PDUTransmitter vTrans;
        private ResponseHandler vRespHandler;
        private StreamParser vStreamParser;
        private TcpIpSession vTcpIpSession;
        private object vSyncRoot;
        private bool vIsAlive;
        private SmppSessionState vState;
        private int vDefaultResponseTimeout;

        private string vSmscId;
        private string vSystemId;
        private string vPassword;
        private TypeOfNumber vAddressTon;
        private NumberingPlanIndicator vAddressNpi;
        private SmppEncodingService vSmppEncodingService;

        private SendPduCallback vCallback;
        //--
        private static TraceSwitch vTraceSwitch =
            new TraceSwitch("SmppClientSessionSwitch", "SmppClientSession class switch");
        #endregion

        #region Constants
        /// <summary>
        /// Default delay between consecutive EnquireLink commands
        /// </summary>
        private const int DEFAULT_DELAY = 60000;
        #endregion

        #region Events
        public event EventHandler<PduReceivedEventArgs> PduReceived;
        public event EventHandler<SmppSessionClosedEventArgs> SessionClosed;
        #endregion

        #region Constructors
        private SmppClientSession(SmppEncodingService smppEncodingService)
        {
            vSmppEncodingService = smppEncodingService;
            InitializeTimer();
            vSyncRoot = new object();
            vDefaultResponseTimeout = 5000;
            vSmscId = "";
            vSystemId = "";
            vPassword = "";
            vCallback = new SendPduCallback(SendPdu);
            //-- Create and initialize trace switch
        }
        #endregion

        #region Properties
        public bool IsAlive
        {
            get { lock (vSyncRoot) { return vIsAlive; } }
        }

        public SmppSessionState State
        {
            get { lock (vSyncRoot) { return vState; } }
        }

        public string SmscID
        {
            get { return vSmscId; }
        }

        public string SystemID
        {
            get { return vSystemId; }
        }

        public string Password
        {
            get { return vPassword; }
        }

        public NumberingPlanIndicator AddressNpi
        {
            get { return vAddressNpi; }
        }

        public TypeOfNumber AddressTon
        {
            get { return vAddressTon; }
        }

        public int EnquireLinkInterval
        {
            get { return (int)vTimer.Interval; }
            set
            {
                if (value < 1000)//If the value is less than one second
                {
                    throw new ArgumentException("EnqureLink interval cannot be less than 1000 millseconds (1 second)");
                }
                vTimer.Interval = (double)value;
            }
        }

        public TcpIpSessionProperties TcpIpProperties
        {
            get { return vTcpIpSession.Properties; }
        }

        public int DefaultResponseTimeout
        {
            get { return vDefaultResponseTimeout; }
            set { vDefaultResponseTimeout = value; }
        }

        public object SyncRoot
        {
            get { return vSyncRoot; }
            set { vSyncRoot = value; }
        }

        public SmppEncodingService SmppEncodingService
        {
            get { return vSmppEncodingService; }
            set { vSmppEncodingService = value; }
        }
        #endregion

        #region Methods
        #region Interface Methods
        public ResponsePDU SendPdu(RequestPDU pdu)
        {
            int timeout = 0;
            lock (vSyncRoot) { timeout = vDefaultResponseTimeout; }
            return SendPdu(pdu, timeout);
        }

        public ResponsePDU SendPdu(RequestPDU pdu, int timeout)
        {
            SendPduBase(pdu);
            if (pdu.HasResponse)
            {
                try { return vRespHandler.WaitResponse(pdu, timeout); }
                catch (SmppResponseTimedOutException)
                {
                    _Log.Error("200016:PDU send operation timed out;");
                    if (vTraceSwitch.TraceWarning)
                    { Trace.WriteLine("200016:PDU send operation timed out;"); }
                    throw;
                }
            }
            else { return null; }
        }

        private void SendPduBase(PDU pdu)
        {
            if (pdu == null) { throw new ArgumentNullException("pdu"); }
            if (!(CheckState(pdu) && (pdu.AllowedSource & SmppEntityType.ESME) == SmppEntityType.ESME))
            { throw new SmppException(SmppErrorCode.ESME_RINVBNDSTS, "Incorrect bind status for given command"); }
            try { vTrans.Send(pdu); }
            catch (Exception ex)
            {
                ByteBuffer buffer = new ByteBuffer(pdu.GetBytes());
                _Log.ErrorFormat("200022:PDU send operation failed - {0}", ex, buffer.DumpString());
                if (vTraceSwitch.TraceInfo)
                {                    
                    Trace.WriteLine(string.Format(
                        "200022:PDU send operation failed - {0} {1};",
                        buffer.DumpString(), ex.Message));
                }
            }
        }

        public IAsyncResult BeginSendPdu(RequestPDU pdu, int timeout, AsyncCallback callback, object @object)
        {
            return vCallback.BeginInvoke(pdu, timeout, callback, @object);
        }

        public IAsyncResult BeginSendPdu(RequestPDU pdu, AsyncCallback callback, object @object)
        {
            int timeout = 0;
            lock (vSyncRoot) { timeout = vDefaultResponseTimeout; }
            return BeginSendPdu(pdu, timeout, callback, @object);
        }

        public ResponsePDU EndSendPdu(IAsyncResult result)
        {
            return vCallback.EndInvoke(result);
        }

        public void EndSession()
        {
            EndSession(SmppSessionCloseReason.EndSessionCalled, null);
        }

        public static SmppClientSession Bind(SessionBindInfo bindInfo, int timeOut, SmppEncodingService smppEncodingService)
        {
            try
            {
                TcpIpSession tcpIpSession = null;
                if (bindInfo == null) { throw new ArgumentNullException("bindInfo"); }
                //--
                tcpIpSession = CreateTcpIpSession(bindInfo);
                //--
                SmppClientSession smppSession = new SmppClientSession(smppEncodingService);
                smppSession.vTcpIpSession = tcpIpSession;
                smppSession.ChangeState(SmppSessionState.Open);
                smppSession.AssembleComponents();
                try { smppSession.BindSession(bindInfo, timeOut); }
                catch (Exception)
                {
                    smppSession.DestroyTcpIpSession();
                    smppSession.DisassembleComponents();
                    throw;
                }
                return smppSession;
            }
            catch (Exception ex)
            {
                _Log.ErrorFormat("200017:SMPP bind operation failed: {0}", ex, ex.Message);
                if (vTraceSwitch.TraceInfo)
                {
                    string traceMessage = "200017:SMPP bind operation failed:";
                    if (ex is SmppException) { traceMessage += (ex as SmppException).ErrorCode.ToString() + " - "; }
                    traceMessage += ex.Message;
                    Trace.WriteLine(traceMessage);
                }
                throw;
            }
        }
        #endregion

        #region Helper Methods
        private void EndSession(SmppSessionCloseReason reason, Exception exception)
        {
            lock (vSyncRoot)
            {
                if (!vIsAlive) { return; }
                vIsAlive = false;
                ChangeState(SmppSessionState.Closed);
            }
            if (reason != SmppSessionCloseReason.UnbindRequested)
            {
                //If unbind request was received, do not try to unbind again
                Unbind unbind = new Unbind(SmppEncodingService);
                try
                {
                    vTrans.Send(unbind);
                    vRespHandler.WaitResponse(unbind, 1000);
                }
                catch {/*Silent catch*/}
            }
            DestroyTcpIpSession();
            DisassembleComponents();
            DestroyTimer();
            RaiseSessionClosedEvent(reason, exception);
        }

        private static TcpIpSession CreateTcpIpSession(SessionBindInfo bindInfo)
        {
            //Check that Host is not an empty string or null
            if (string.IsNullOrEmpty(bindInfo.ServerName))
            { throw new InvalidOperationException("Host cannot be an empty string or null"); }
            //Check the port number is not set to an invalid value
            if (bindInfo.Port < IPEndPoint.MinPort || bindInfo.Port > IPEndPoint.MaxPort)
            {
                throw new InvalidOperationException(
                   string.Format("Port must be set to a valid value between '{0}' and '{1}'",
                   IPEndPoint.MinPort, IPEndPoint.MaxPort));
            }
            IPAddress address = null;
            TcpIpSession session = null;
            if (!IPAddress.TryParse(bindInfo.ServerName, out address))
            { session = TcpIpSession.OpenClientSession(bindInfo.ServerName, bindInfo.Port); }
            else { session = TcpIpSession.OpenClientSession(address, bindInfo.Port); }
            return session;
        }

        private void DestroyTcpIpSession()
        {
            if (vTcpIpSession == null) { return; }
            vTcpIpSession.SessionClosed -= TcpIpSessionClosedEventHandler;
            vTcpIpSession.EndSession();
            vTcpIpSession = null;
        }

        private void AssembleComponents()
        {
            vTrans = new PDUTransmitter(vTcpIpSession);
            vRespHandler = new ResponseHandler();
            vStreamParser = new StreamParser(
                vTcpIpSession, vRespHandler, new PduProcessorCallback(PduRequestProcessorCallback), vSmppEncodingService);
            vStreamParser.ParserException += ParserExceptionEventHandler;
            vStreamParser.PDUError += PduErrorEventHandler;
            //Start stream parser
            vStreamParser.Start();
        }

        private void DisassembleComponents()
        {
            if (vStreamParser == null) { return; }
            vStreamParser.ParserException -= ParserExceptionEventHandler;
            vStreamParser.PDUError -= PduErrorEventHandler;
            //Stop parser first
            vStreamParser.Stop(true);
            vStreamParser = null;
            vTrans = null;
            vRespHandler = null;
        }

        private void BindSession(SessionBindInfo bindInfo, int timeOut)
        {
            vTcpIpSession.SessionClosed += TcpIpSessionClosedEventHandler;

            BindRequest bindReq = bindInfo.CreatePdu(SmppEncodingService);
            vTrans.Send(bindReq);
            BindResponse bindResp = null;
            try { bindResp = (BindResponse)vRespHandler.WaitResponse(bindReq, timeOut); }
            catch (SmppResponseTimedOutException ex)
            { throw new SmppBindException(ex); }
            if (bindResp.Header.ErrorCode != 0)
            { throw new SmppBindException(bindResp.Header.ErrorCode); }
            //Copy settings
            vSmscId = bindResp.SystemID;
            vSystemId = bindInfo.SystemID;
            vPassword = bindInfo.Password;
            vAddressTon = bindInfo.AddressTon;
            vAddressNpi = bindInfo.AddressNpi;
            //Start timer
            vTimer.Start();
            vIsAlive = true;
            switch (bindReq.Header.CommandType)
            {
                case CommandType.BindTransceiver:
                    ChangeState(SmppSessionState.Transceiver);
                    break;
                case CommandType.BindReceiver:
                    ChangeState(SmppSessionState.Receiver);
                    break;
                case CommandType.BindTransmitter:
                    ChangeState(SmppSessionState.Transmitter);
                    break;
            }
        }

        private void InitializeTimer()
        {
            vTimer = new Timer(DEFAULT_DELAY);
            vTimer.Elapsed += new ElapsedEventHandler(TimerCallback);
        }

        private void DestroyTimer()
        {
            try { vTimer.Stop(); vTimer.Close(); }
            catch {/*Silent catch*/}
        }

        private void ChangeState(SmppSessionState newState)
        {
            lock (vSyncRoot)
            {
                vState = newState;
            }
        }

        private bool CheckState(PDU pdu)
        {
            return (int)(pdu.AllowedSession & vState) != 0;
        }

        private void TimerCallback(object sender, ElapsedEventArgs e)
        {
            EnquireLink enqLink = new EnquireLink(SmppEncodingService);
            //Send EnquireLink with 5 seconds response timeout
            try
            {
                EnquireLinkResp resp = (EnquireLinkResp)SendPdu(enqLink, 5000);
            }
            catch (SmppResponseTimedOutException)
            {
                //If there was no response, we conclude that this session is no longer active
                //Shutdown this session
                EndSession(SmppSessionCloseReason.EnquireLinkTimeout, null);
            }
            catch (SmppException) { /*Silent catch*/} //Incorrect bind status for a given command
            catch (TcpIpException) {/*Silent catch*/ }
        }

        private void PduRequestProcessorCallback(RequestPDU pdu)
        {
            ResponsePDU resp = null;
            if (pdu is Unbind)
            {
                resp = pdu.CreateDefaultResponce();
                try { SendPduBase(resp); }
                catch {/*silent catch*/}
                EndSession(SmppSessionCloseReason.UnbindRequested, null);
                return;
            }
            resp = RaisePduReceivedEvent(pdu);
            if (resp == null)
            {
                if (pdu.HasResponse)
                {
                    resp = pdu.CreateDefaultResponce();
                }
            }
            if (resp != null)
            { try { SendPduBase(resp); } catch {/*Silent catch*/} }
        }

        private void TcpIpSessionClosedEventHandler(object sender, TcpIpSessionClosedEventArgs e)
        {
            switch (e.CloseReason)
            {
                case SessionCloseReason.EndSessionCalled:
                    EndSession(SmppSessionCloseReason.TcpIpSessionClosed, e.Exception);
                    break;
                case SessionCloseReason.ExceptionThrown:
                    EndSession(SmppSessionCloseReason.TcpIpSessionError, e.Exception);
                    break;
            }
        }

        private void ParserExceptionEventHandler(object sender, ParserExceptionEventArgs e)
        {
            EndSession(SmppSessionCloseReason.TcpIpSessionError, e.Exception);
        }

        private void PduErrorEventHandler(object sender, PDUErrorEventArgs e)
        {
            ResponsePDU resp = null;
            if (e.Pdu is RequestPDU)
            {
                RequestPDU req = (RequestPDU)e.Pdu;
                resp = req.CreateDefaultResponce();
                resp.Header.ErrorCode = e.Exception.ErrorCode;
            }
            else
            {
                resp = new GenericNack(e.Header, vSmppEncodingService);
                resp.Header.ErrorCode = e.Exception.ErrorCode;
            }
            try { SendPduBase(resp); }
            catch {/*silent catch*/}
        }

        private ResponsePDU RaisePduReceivedEvent(RequestPDU pdu)
        {
            /*
             * PduReceived event is not raised asynchronously as this method is 
             * being called asynchronously by a worker thread from the thread pool.
             */
            if (PduReceived == null) { return null; }
            PduReceivedEventArgs e = new PduReceivedEventArgs(pdu);
            PduReceived(this, e);
            return e.Response;
        }

        private void RaiseSessionClosedEvent(SmppSessionCloseReason reason, Exception exception)
        {
            if (SessionClosed == null) { return; }
            SmppSessionClosedEventArgs e = new SmppSessionClosedEventArgs(reason, exception);
            foreach (EventHandler<SmppSessionClosedEventArgs> del in SessionClosed.GetInvocationList())
            { del.BeginInvoke(this, e, AsyncCallBackRaiseSessionClosedEvent, del); }
        }

        private void AsyncCallBackRaiseSessionClosedEvent(IAsyncResult result)
        {
            EventHandler<SmppSessionClosedEventArgs> del =
                (EventHandler<SmppSessionClosedEventArgs>)result.AsyncState;
            del.EndInvoke(result);
        }
        #endregion
        #endregion
    }
}
