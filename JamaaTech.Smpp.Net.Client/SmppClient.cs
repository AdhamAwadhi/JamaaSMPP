/************************************************************************
 * Copyright (C) 2008 Jamaa Technologies
 *
 * This file is part of Jamaa SMPP Client Library.
 *
 * Jamaa SMPP Client Library is free software. You can redistribute it and/or modify
 * it under the terms of the Microsoft Reciprocal License (Ms-RL)
 *
 * You should have received a copy of the Microsoft Reciprocal License
 * along with Jamaa SMPP Client Library; See License.txt for more details.
 *
 * Author: Benedict J. Tesha
 * benedict.tesha@jamaatech.com, www.jamaatech.com
 *
 ************************************************************************/

using System;
using System.Threading;
using System.Diagnostics;
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Protocol;
using JamaaTech.Smpp.Net.Lib.Util;
using JamaaTech.Smpp.Net.Lib.Protocol.Tlv;

namespace JamaaTech.Smpp.Net.Client
{
    public class SmppClient : IDisposable
    {
        #region Variables
        private SmppConnectionProperties vProperties;
        private SmppClientSession vTrans;
        private SmppClientSession vRecv;
        private Exception vLastException;
        private SmppConnectionState vState;
        private object vConnSyncRoot;
        private System.Threading.Timer vTimer;
        private int vTimeOut;
        private int vAutoReconnectDelay;
        private string vName;
        private int vKeepAliveInterval;
        private SendMessageCallBack vSendMessageCallBack;
        private bool vStarted;
        //--
        private static TraceSwitch vTraceSwitch = new TraceSwitch("SmppClientSwitch", "SmppClient trace switch");
        #endregion

        #region Events
        /// <summary>
        /// Occurs when a message is received
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// Occurs when a message delivery notification is received
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageDelivered;

        /// <summary>
        /// Occurs when connection state changes
        /// </summary>
        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        /// <summary>
        /// Occurs when a message is successfully sent
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageSent;

        /// <summary>
        /// Occurs when <see cref="SmppClient"/> is started or shut down
        /// </summary>
        public event EventHandler<StateChangedEventArgs> StateChanged;
        #endregion

        #region Constructors
        public SmppClient()
        {
            vProperties = new SmppConnectionProperties();
            vConnSyncRoot = new object();
            vAutoReconnectDelay = 10000;
            vTimeOut = 5000;
            //--
            vTimer = new System.Threading.Timer(AutoReconnectTimerEventHandler, null, Timeout.Infinite, vAutoReconnectDelay);
            //--
            vName = "";
            vState = SmppConnectionState.Closed;
            vKeepAliveInterval = 30000;
            //--
            vSendMessageCallBack += SendMessage;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating the time in miliseconds to wait before attemping to reconnect after a connection is lost
        /// </summary>
        public int AutoReconnectDelay
        {
            get { return vAutoReconnectDelay; }
            set { vAutoReconnectDelay = value; }
        }

        /// <summary>
        /// Indicates the current state of <see cref="SmppClient"/>
        /// </summary>
        public SmppConnectionState ConnectionState
        {
            get { return vState; }
        }

        /// <summary>
        /// Gets or sets a value that indicates the time in miliseconds in which Enquire Link PDUs are periodically sent
        /// </summary>
        public int KeepAliveInterval
        {
            get { return vKeepAliveInterval; }
            set { vKeepAliveInterval = value; }
        }

        /// <summary>
        /// Gets or sets a value that specifies the name for this <see cref="SmppClient"/>
        /// </summary>
        public string Name
        {
            get { return vName; }
            set { vName = value; }
        }

        /// <summary>
        /// Gets an instance of <see cref="SmppConnectionProperties"/> that represents connection properties for this <see cref="SmppClient"/>
        /// </summary>
        public SmppConnectionProperties Properties
        {
            get { return vProperties; }
        }

        /// <summary>
        /// Gets or sets a value that speficies the amount of time after which a synchronous <see cref="SmppClient.SendMessage"/> call will timeout
        /// </summary>
        public int ConnectionTimeout
        {
            get { return vTimeOut; }
            set { vTimeOut = value; }
        }

        /// <summary>
        /// Gets a <see cref="System.Boolean"/> value indicating if an instance of <see cref="SmppClient"/> is started
        /// </summary>
        public bool Started
        {
            get { return vStarted; }
        }
        #endregion

        #region Methods
        #region Interface Methods
        /// <summary>
        /// Sends message to a remote SMMP server
        /// </summary>
        /// <param name="message">A message to send</param>
        /// <param name="timeOut">A value in miliseconds after which the send operation times out</param>
        public void SendMessage(ShortMessage message, int timeOut)
        {
            if (message == null) { throw new ArgumentNullException("message"); }

            //Check if connection is open
            if (vState != SmppConnectionState.Connected)
            { throw new SmppClientException("Sending message operation failed because the SmppClient is not connected"); }

            string messageId = null;
            foreach (SendSmPDU pdu in message.GetMessagePDUs(vProperties.DefaultEncoding))
            {
                ResponsePDU resp = vTrans.SendPdu(pdu, timeOut);
                if (resp.Header.ErrorCode != SmppErrorCode.ESME_ROK)
                { throw new SmppException(resp.Header.ErrorCode); }
                var submitSmResp = resp as SubmitSmResp;
                if (submitSmResp != null)
                {
                    messageId = ((SubmitSmResp)resp).MessageID;
                }
                message.ReceiptedMessageId = messageId;
                RaiseMessageSentEvent(message);
            }
        }

        /// <summary>
        /// Sends message to a remote SMPP server
        /// </summary>
        /// <param name="message">A message to send</param>
        public void SendMessage(ShortMessage message)
        {
            SendMessage(message, vTrans.DefaultResponseTimeout);
        }

        /// <summary>
        /// Sends message asynchronously to a remote SMPP server
        /// </summary>
        /// <param name="message">A message to send</param>
        /// <param name="timeout">A value in miliseconds after which the send operation times out</param>
        /// <param name="callback">An <see cref="AsyncCallback"/> delegate</param>
        /// <param name="state">An object that contains state information for this request</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous send message operation</returns>
        public IAsyncResult BeginSendMessage(ShortMessage message, int timeout, AsyncCallback callback, object state)
        {
            return vSendMessageCallBack.BeginInvoke(message, timeout, callback, state);
        }

        /// <summary>
        /// Sends message asynchronously to a remote SMPP server
        /// </summary>
        /// <param name="message">A message to send</param>
        /// <param name="callback">An <see cref="AsyncCallback"/> delegate</param>
        /// <param name="state">An object that contains state information for this request</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous send message operation</returns>
        public IAsyncResult BeginSendMessage(ShortMessage message, AsyncCallback callback, object state)
        {
            int timeout = 0;
            timeout = vTrans.DefaultResponseTimeout;
            return BeginSendMessage(message, timeout, callback, state);
        }

        /// <summary>
        /// Ends a pending asynchronous send message operation
        /// </summary>
        /// <param name="result">An <see cref="IAsyncResult"/> that stores state information for this asynchronous operation</param>
        public void EndSendMessage(IAsyncResult result)
        {
            vSendMessageCallBack.EndInvoke(result);
        }

        /// <summary>
        /// Starts <see cref="SmppClient"/> and immediately connects to a remote SMPP server
        /// </summary>
        public void Start()
        {

            vStarted = true;
            vTimer.Change(0, vAutoReconnectDelay);
            RaiseStateChangedEvent(true);
        }

        /// <summary>
        /// Starts <see cref="SmppClient"/> and waits for a specified amount of time before establishing connection
        /// </summary>
        /// <param name="connectDelay">A value in miliseconds to wait before establishing connection</param>
        public void Start(int connectDelay)
        {
            if (connectDelay < 0) { connectDelay = 0; }
            vStarted = true;
            vTimer.Change(connectDelay, vAutoReconnectDelay);
            RaiseStateChangedEvent(true);
        }

        /// <summary>
        /// Immediately attempts to reestablish a lost connection without waiting for <see cref="SmppClient"/> to automatically reconnect
        /// </summary>
        public void ForceConnect()
        {
            Open(vTimeOut);
        }

        /// <summary>
        /// Immediately attempts to reestablish a lost connection without waiting for <see cref="SmppClient"/> to automatically reconnect
        /// </summary>
        /// <param name="timeout">A time in miliseconds after which a connection operation times out</param>
        public void ForceConnect(int timeout)
        {
            Open(timeout);
        }

        /// <summary>
        /// Shuts down <see cref="SmppClient"/>
        /// </summary>
        public void Shutdown()
        {
            if (!vStarted) { return; }
            vStarted = false;
            StopTimer();
            CloseSession();
            RaiseStateChangedEvent(false);
        }

        /// <summary>
        /// Restarts <see cref="SmppClient"/>
        /// </summary>
        public void Restart()
        {
            Shutdown();
            Start();
        }
        #endregion

        #region Helper Methods
        private void Open(int timeOut)
        {
            try
            {
                if (Monitor.TryEnter(vConnSyncRoot))
                {
                    //No thread is in a connecting or reconnecting state
                    if (vState != SmppConnectionState.Closed)
                    {
                        vLastException = new InvalidOperationException("You cannot open while the instance is already connected");
                        throw vLastException;
                    }
                    //
                    SessionBindInfo bindInfo = null;
                    bool useSepConn = false;
                    lock (vProperties.SyncRoot)
                    {
                        bindInfo = vProperties.GetBindInfo();
                        useSepConn = vProperties.InterfaceVersion == InterfaceVersion.v33;
                    }
                    try { OpenSession(bindInfo, useSepConn, timeOut); }
                    catch (Exception ex) { vLastException = ex; throw; }
                    vLastException = null;
                }
                else
                {
                    //Another thread is already in either a connecting or reconnecting state
                    //Wait until the thread finishes
                    Monitor.Enter(vConnSyncRoot);
                    //Now, the thread has finished connecting,
                    //Check on the result if the thread encountered any problem during connection
                    if (vLastException != null) { throw vLastException; }
                }
            }
            finally
            {
                Monitor.Exit(vConnSyncRoot);
            }
        }

        private void OpenSession(SessionBindInfo bindInfo, bool useSeparateConnections, int timeOut)
        {
            ChangeState(SmppConnectionState.Connecting);
            if (useSeparateConnections)
            {
                //Create two separate sessions for sending and receiving
                try
                {
                    bindInfo.AllowReceive = true;
                    bindInfo.AllowTransmit = false;
                    vRecv = SmppClientSession.Bind(bindInfo, timeOut);
                    InitializeSession(vRecv);
                }
                catch
                {
                    ChangeState(SmppConnectionState.Closed);
                    //Start reconnect timer
                    StartTimer();
                    throw;
                }
                //--
                try
                {
                    bindInfo.AllowReceive = false;
                    bindInfo.AllowTransmit = true;
                    vTrans = SmppClientSession.Bind(bindInfo, timeOut);
                    InitializeSession(vTrans);
                }
                catch
                {
                    try { vRecv.EndSession(); }
                    catch {/*Silent catch*/}
                    vRecv = null;
                    ChangeState(SmppConnectionState.Closed);
                    //Start reconnect timer
                    StartTimer();
                    throw;
                }
                ChangeState(SmppConnectionState.Connected);
            }
            else
            {
                //Use a single session for both sending and receiving
                bindInfo.AllowTransmit = true;
                bindInfo.AllowReceive = true;
                try
                {
                    SmppClientSession session = SmppClientSession.Bind(bindInfo, timeOut);
                    vTrans = session;
                    vRecv = session;
                    InitializeSession(session);
                    ChangeState(SmppConnectionState.Connected);
                }
                catch (SmppException ex)
                {
                    if (ex.ErrorCode == SmppErrorCode.ESME_RINVCMDID)
                    {
                        //If SMSC returns ESME_RINVCMDID (Invalid command id)
                        //the SMSC might not be supporting the BindTransceiver PDU
                        //Therefore, we can try to use bind with separate connections
                        OpenSession(bindInfo, true, timeOut);
                    }
                    else
                    {
                        ChangeState(SmppConnectionState.Closed);
                        //Start background timer
                        StartTimer();
                        throw;
                    }
                }
                catch
                {
                    ChangeState(SmppConnectionState.Closed);
                    StartTimer();
                    throw;
                }
            }
        }

        private void CloseSession()
        {
            SmppConnectionState oldState = SmppConnectionState.Closed;

            oldState = vState;
            if (vState == SmppConnectionState.Closed) { return; }
            vState = SmppConnectionState.Closed;

            RaiseConnectionStateChangeEvent(SmppConnectionState.Closed, oldState);
            if (vTrans != null) { vTrans.EndSession(); }
            if (vRecv != null) { vRecv.EndSession(); }
            vTrans = null;
            vRecv = null;
        }

        private void InitializeSession(SmppClientSession session)
        {
            session.EnquireLinkInterval = vKeepAliveInterval;
            session.PduReceived += PduReceivedEventHander;
            session.SessionClosed += SessionClosedEventHandler;
        }

        private void ChangeState(SmppConnectionState newState)
        {
            SmppConnectionState oldState = SmppConnectionState.Closed;
            oldState = vState;
            vState = newState;
            vProperties.SmscID = newState == SmppConnectionState.Connected ? vTrans.SmscID : "";
            RaiseConnectionStateChangeEvent(newState, oldState);
        }

        private void RaiseMessageReceivedEvent(ShortMessage message)
        {
            if (MessageReceived != null) { MessageReceived(this, new MessageEventArgs(message)); }
        }

        private void RaiseMessageDeliveredEvent(ShortMessage message)
        {
            if (MessageDelivered != null) { MessageDelivered(this, new MessageEventArgs(message)); }
        }

        private void RaiseMessageSentEvent(ShortMessage message)
        {
            if (MessageSent != null) { MessageSent(this, new MessageEventArgs(message)); }
        }

        private void RaiseConnectionStateChangeEvent(SmppConnectionState newState, SmppConnectionState oldState)
        {
            if (ConnectionStateChanged == null) { return; }
            ConnectionStateChangedEventArgs e = new ConnectionStateChangedEventArgs(newState, oldState, vAutoReconnectDelay);
            ConnectionStateChanged(this, e);
            if (e.ReconnectInteval < 5000) { e.ReconnectInteval = 5000; }
            Interlocked.Exchange(ref vAutoReconnectDelay, e.ReconnectInteval);
        }

        private void RaiseStateChangedEvent(bool started)
        {
            if (StateChanged == null) { return; }
            StateChangedEventArgs e = new StateChangedEventArgs(started);
            StateChanged(this, e);
        }

        private void PduReceivedEventHander(object sender, PduReceivedEventArgs e)
        {
            //This handler is interested in SingleDestinationPDU only
            SingleDestinationPDU pdu = e.Request as SingleDestinationPDU;
            if (pdu == null) { return; }
            ShortMessage message = null;
            try { message = MessageFactory.CreateMessage(pdu); }
            catch (SmppException smppEx)
            {
                if (vTraceSwitch.TraceError)
                {
                    Trace.WriteLine(string.Format(
                        "200019:SMPP message decoding failure - {0} - {1} {2};",
                        smppEx.ErrorCode, new ByteBuffer(pdu.GetBytes()).DumpString(), smppEx.Message));
                }
                //Notify the SMSC that we encountered an error while processing the message
                e.Response = pdu.CreateDefaultResponce();
                e.Response.Header.ErrorCode = smppEx.ErrorCode;
                return;
            }
            catch (Exception ex)
            {
                if (vTraceSwitch.TraceError)
                {
                    Trace.WriteLine(string.Format(
                        "200019:SMPP message decoding failure - {0} {1};",
                        new ByteBuffer(pdu.GetBytes()).DumpString(), ex.Message));
                }
                //Let the receiver know that this message was rejected
                e.Response = pdu.CreateDefaultResponce();
                e.Response.Header.ErrorCode = SmppErrorCode.ESME_RX_P_APPN; //ESME Receiver Reject Message
                return;
            }
            //If we have just a normal message
            if ((((byte)pdu.EsmClass) | 0xc3) == 0xc3)
            { RaiseMessageReceivedEvent(message); }
            //Or if we have received a delivery receipt
            else if ((pdu.EsmClass & EsmClass.DeliveryReceipt) == EsmClass.DeliveryReceipt)
            {
                // Extract receipted message id
                var receiptedMessageIdTlv = pdu.Tlv.GetTlvByTag(Tag.receipted_message_id);
                string receiptedMessageId = null;
                if (receiptedMessageIdTlv != null)
                {
                    receiptedMessageId = SMPPEncodingUtil.GetCStringFromBytes(receiptedMessageIdTlv.RawValue);
                }
                message.ReceiptedMessageId = receiptedMessageId;

                // Extract user message reference
                var userMessageReferenceTlv = pdu.Tlv.GetTlvByTag(Tag.user_message_reference);
                string userMessageReference = null;
                if (userMessageReferenceTlv != null)
                {
                    userMessageReference = SMPPEncodingUtil.GetCStringFromBytes(userMessageReferenceTlv.RawValue);
                }
                message.UserMessageReference = userMessageReference;
                RaiseMessageDeliveredEvent(message);
            }
        }

        private void SessionClosedEventHandler(object sender, SmppSessionClosedEventArgs e)
        {
            if (e.Reason != SmppSessionCloseReason.EndSessionCalled)
            {
                //Start timer 
                StartTimer();
            }
            CloseSession();
        }

        private void StartTimer()
        {
            vTimer.Change(vAutoReconnectDelay, vAutoReconnectDelay);
        }

        private void StopTimer()
        {
            vTimer.Change(Timeout.Infinite, vAutoReconnectDelay);
        }

        void AutoReconnectTimerEventHandler(object state)
        {
            //Do not reconnect if AutoReconnectDalay < 0 or if SmppClient is shutdown
            if (AutoReconnectDelay <= 0 || !Started) { return; }
            //Stop the timer from raising subsequent events before
            //the current thread exists
            StopTimer();

            int timeOut = 0;
            timeOut = vTimeOut;
            try { Open(timeOut); }
            catch (Exception) {/*Do nothing*/}

            if (vState == SmppConnectionState.Closed)
            { StartTimer(); }
            else
            { StopTimer(); }
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManagedResorces)
        {
            try
            {
                Shutdown();
                if (vTimer != null) { vTimer.Dispose(); }
            }
            catch { /*Sielent catch*/ }
        }
        #endregion
    }
}
