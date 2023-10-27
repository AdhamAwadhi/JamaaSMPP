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
using System.Diagnostics;
using System.Threading;
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Logging;
using JamaaTech.Smpp.Net.Lib.Protocol;
using JamaaTech.Smpp.Net.Lib.Protocol.Tlv;
using JamaaTech.Smpp.Net.Lib.Util;

namespace JamaaTech.Smpp.Net.Client;

public class SmppClient : IDisposable
{
    private static readonly Common.Logging.ILog Log = Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    #region Variables

    private SmppConnectionProperties _vProperties;
    private SmppClientSession _vTrans;
    private SmppClientSession _vRecv;
    private Exception _vLastException;
    private SmppConnectionState _vState;
    private object _vConnSyncRoot;
    private Timer _vTimer;
    private int _vTimeOut;
    private int _vAutoReconnectDelay;
    private string _vName;
    private int _vKeepAliveInterval;
    private SendMessageCallBack _vSendMessageCallBack;
    private bool _vStarted;

    private SmppEncodingService _vSmppEncodingService;

    //--
    private static TraceSwitch _vTraceSwitch = new TraceSwitch("SmppClientSwitch", "SmppClient trace switch");

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
        _vProperties = new SmppConnectionProperties();
        _vSmppEncodingService = new SmppEncodingService();
        _vConnSyncRoot = new object();
        _vAutoReconnectDelay = 10000;
        _vTimeOut = 5000;
        //--
        _vTimer = new Timer(AutoReconnectTimerEventHandler, null, Timeout.Infinite,
            _vAutoReconnectDelay);
        //--
        _vName = "";
        _vState = SmppConnectionState.Closed;
        _vKeepAliveInterval = 30000;
        //--
        _vSendMessageCallBack += SendMessage;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating the time in miliseconds to wait before attemping to reconnect after a connection is lost
    /// </summary>
    public int AutoReconnectDelay
    {
        get => _vAutoReconnectDelay;
        set => _vAutoReconnectDelay = value;
    }

    /// <summary>
    /// Indicates the current state of <see cref="SmppClient"/>
    /// </summary>
    public SmppConnectionState ConnectionState => _vState;

    /// <summary>
    /// Gets or sets a value that indicates the time in miliseconds in which Enquire Link PDUs are periodically sent
    /// </summary>
    public int KeepAliveInterval
    {
        get => _vKeepAliveInterval;
        set => _vKeepAliveInterval = value;
    }

    /// <summary>
    /// Gets or sets a value that specifies the name for this <see cref="SmppClient"/>
    /// </summary>
    public string Name
    {
        get => _vName;
        set => _vName = value;
    }

    /// <summary>
    /// Gets an instance of <see cref="SmppConnectionProperties"/> that represents connection properties for this <see cref="SmppClient"/>
    /// </summary>
    public SmppConnectionProperties Properties => _vProperties;

    /// <summary>
    /// Gets or sets a value that speficies the amount of time after which a synchronous <see cref="SmppClient.SendMessage"/> call will timeout
    /// </summary>
    public int ConnectionTimeout
    {
        get => _vTimeOut;
        set => _vTimeOut = value;
    }

    /// <summary>
    /// Gets a <see cref="System.Boolean"/> value indicating if an instance of <see cref="SmppClient"/> is started
    /// </summary>
    public bool Started => _vStarted;

    public SmppEncodingService SmppEncodingService
    {
        get => _vSmppEncodingService;
        set => _vSmppEncodingService = value;
    }

    /// <summary>
    /// Gets a <see cref="System.Exception"/> indicating if an instance of <see cref="SmppClient"/> has an occurred exception while connecting.
    /// </summary>
    /// <value>
    /// The last exception.
    /// </value>
    public Exception LastException => _vLastException;

    #endregion

    #region Methods

    #region Interface Methods

    /// <summary>
    /// Sends message to a remote SMPP server
    /// </summary>
    /// <param name="message">A message to send</param>
    /// <param name="timeOut">A value in miliseconds after which the send operation times out</param>
    public virtual void SendMessage(ShortMessage message, int timeOut)
    {
        if (message == null)
        {
            throw new ArgumentNullException("message");
        }

        //Check if connection is open
        if (_vState != SmppConnectionState.Connected)
        {
            throw new SmppClientException(
                "Sending message operation failed because the SmppClient is not connected");
        }

        var srcAddress = new SmppAddress(_vProperties.AddressTon, _vProperties.AddressNpi,
            string.IsNullOrWhiteSpace(message.SourceAddress) ? Properties.SourceAddress : message.SourceAddress);
        var destAddress = new SmppAddress() { Address = message.DestinationAddress };
        var messagePdUs = message.GetMessagePDUs(_vProperties.DefaultEncoding, _vSmppEncodingService, destAddress,
            srcAddress);
        foreach (SendSmPDU pdu in messagePdUs)
        {
            string messageId = null;
            if (Log.IsDebugEnabled)
                Log.DebugFormat("SendMessage SendSmPDU: {0}",
                    LoggingExtensions.DumpString(pdu, _vSmppEncodingService));
            var resp = SendPdu(pdu, timeOut);
            if (resp is SubmitSmResp submitSmResp)
            {
                if (Log.IsDebugEnabled)
                    Log.DebugFormat("SendMessage Response: {0}",
                        LoggingExtensions.DumpString(resp, _vSmppEncodingService));
                messageId = submitSmResp.MessageID;
            }

            if (message.ReceiptedMessageId is { Length: > 0 })
                message.ReceiptedMessageId += $",{messageId}";
            else
                message.ReceiptedMessageId += messageId;
            RaiseMessageSentEvent(message);
        }
    }

    /// <summary>
    /// Send PDU to a remote SMPP server 
    /// </summary>
    /// <param name="pdu"><see cref="RequestPDU"/></param>
    /// <param name="timeout">A value in miliseconds after which the send operation times out</param>
    /// <returns><see cref="ResponsePDU"/></returns>
    protected virtual ResponsePDU SendPdu(RequestPDU pdu, int timeout)
    {
        var resp = _vTrans.SendPdu(pdu, timeout);
        if (resp.Header.ErrorCode != SmppErrorCode.ESME_ROK)
        {
            throw new SmppException(resp.Header.ErrorCode);
        }

        return resp;
    }

    /// <summary>
    /// Sends message to a remote SMPP server
    /// </summary>
    /// <param name="message">A message to send</param>
    public virtual void SendMessage(ShortMessage message)
    {
        SendMessage(message, _vTrans.DefaultResponseTimeout);
    }

    /// <summary>
    /// Sends message asynchronously to a remote SMPP server
    /// </summary>
    /// <param name="message">A message to send</param>
    /// <param name="timeout">A value in miliseconds after which the send operation times out</param>
    /// <param name="callback">An <see cref="AsyncCallback"/> delegate</param>
    /// <param name="state">An object that contains state information for this request</param>
    /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous send message operation</returns>
    public virtual IAsyncResult BeginSendMessage(ShortMessage message, int timeout, AsyncCallback callback,
        object state)
    {
#if NET40
            return vSendMessageCallBack.BeginInvoke(message, timeout, callback, state);

#else
        return System.Threading.Tasks.Task.Run(() => _vSendMessageCallBack(message, timeout));
#endif
    }

    /// <summary>
    /// Sends message asynchronously to a remote SMPP server
    /// </summary>
    /// <param name="message">A message to send</param>
    /// <param name="callback">An <see cref="AsyncCallback"/> delegate</param>
    /// <param name="state">An object that contains state information for this request</param>
    /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous send message operation</returns>
    public virtual IAsyncResult BeginSendMessage(ShortMessage message, AsyncCallback callback, object state)
    {
        int timeout = 0;
        timeout = _vTrans.DefaultResponseTimeout;
        return BeginSendMessage(message, timeout, callback, state);
    }

    /// <summary>
    /// Ends a pending asynchronous send message operation
    /// </summary>
    /// <param name="result">An <see cref="IAsyncResult"/> that stores state information for this asynchronous operation</param>
    public virtual void EndSendMessage(IAsyncResult result)
    {
        _vSendMessageCallBack.EndInvoke(result);
    }

    /// <summary>
    /// Starts <see cref="SmppClient"/> and immediately connects to a remote SMPP server
    /// </summary>
    public virtual void Start()
    {
        _vStarted = true;
        _vTimer.Change(0, _vAutoReconnectDelay);
        RaiseStateChangedEvent(true);
    }

    /// <summary>
    /// Starts <see cref="SmppClient"/> and waits for a specified amount of time before establishing connection
    /// </summary>
    /// <param name="connectDelay">A value in miliseconds to wait before establishing connection</param>
    public virtual void Start(int connectDelay)
    {
        if (connectDelay < 0)
        {
            connectDelay = 0;
        }

        _vStarted = true;
        _vTimer.Change(connectDelay, _vAutoReconnectDelay);
        RaiseStateChangedEvent(true);
    }

    /// <summary>
    /// Immediately attempts to reestablish a lost connection without waiting for <see cref="SmppClient"/> to automatically reconnect
    /// </summary>
    public virtual void ForceConnect()
    {
        Open(_vTimeOut);
    }

    /// <summary>
    /// Immediately attempts to reestablish a lost connection without waiting for <see cref="SmppClient"/> to automatically reconnect
    /// </summary>
    /// <param name="timeout">A time in miliseconds after which a connection operation times out</param>
    public virtual void ForceConnect(int timeout)
    {
        Open(timeout);
    }

    /// <summary>
    /// Shuts down <see cref="SmppClient"/>
    /// </summary>
    public virtual void Shutdown()
    {
        if (!_vStarted)
        {
            return;
        }

        _vStarted = false;
        StopTimer();
        CloseSession();
        RaiseStateChangedEvent(false);
    }

    /// <summary>
    /// Restarts <see cref="SmppClient"/>
    /// </summary>
    public virtual void Restart()
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
            if (Monitor.TryEnter(_vConnSyncRoot))
            {
                //No thread is in a connecting or reconnecting state
                if (_vState != SmppConnectionState.Closed)
                {
                    _vLastException =
                        new InvalidOperationException("You cannot open while the instance is already connected");
                    throw _vLastException;
                }

                //
                SessionBindInfo bindInfo = null;
                bool useSepConn = false;
                lock (_vProperties.SyncRoot)
                {
                    bindInfo = _vProperties.GetBindInfo();
                    useSepConn = _vProperties.CanSeparateConnections;
                }

                try
                {
                    OpenSession(bindInfo, useSepConn, timeOut);
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("OpenSession: {0}", ex, ex.Message);
                    if (_vTraceSwitch.TraceError)
                    {
                        Trace.TraceError(ex.ToString());
                    }

                    _vLastException = ex;
                    throw;
                }

                _vLastException = null;
            }
            else
            {
                //Another thread is already in either a connecting or reconnecting state
                //Wait until the thread finishes
                Monitor.Enter(_vConnSyncRoot);
                //Now, the thread has finished connecting,
                //Check on the result if the thread encountered any problem during connection
                if (_vLastException != null)
                {
                    throw _vLastException;
                }
            }
        }
        finally
        {
            Monitor.Exit(_vConnSyncRoot);
        }
    }

    private void OpenSession(SessionBindInfo bindInfo, bool useSeparateConnections, int timeOut)
    {
        ChangeState(SmppConnectionState.Connecting);
        var connStateConfig = (useSeparateConnections, bindInfo.AllowReceive, bindInfo.AllowTransmit);
        switch (connStateConfig)
        {
            case (true, true, true):
                //Create two separate sessions for sending and receiving
                //ReceiveConnection
                try
                {
                    bindInfo.AllowReceive = true;
                    bindInfo.AllowTransmit = false;
                    _vRecv = SmppClientSession.Bind(bindInfo, timeOut, _vSmppEncodingService);
                    InitializeSession(_vRecv);
                }
                catch
                {
                    ChangeState(SmppConnectionState.Closed);
                    //Start reconnect timer
                    StartTimer();
                    throw;
                }

                //Transmit Connection
                try
                {
                    bindInfo.AllowReceive = false;
                    bindInfo.AllowTransmit = true;
                    _vTrans = SmppClientSession.Bind(bindInfo, timeOut, _vSmppEncodingService);
                    InitializeSession(_vTrans);
                }
                catch
                {
                    try
                    {
                        _vRecv.EndSession();
                    }
                    catch
                    {
                        /*Silent catch*/
                    }

                    _vRecv = null;
                    ChangeState(SmppConnectionState.Closed);
                    //Start reconnect timer
                    StartTimer();
                    throw;
                }

                ChangeState(SmppConnectionState.Connected);
                break;
            case (_, true, false):
                //ReceiveConnection
                try
                {
                    bindInfo.AllowReceive = true;
                    bindInfo.AllowTransmit = false;
                    _vRecv = SmppClientSession.Bind(bindInfo, timeOut, _vSmppEncodingService);
                    InitializeSession(_vRecv);
                }
                catch
                {
                    ChangeState(SmppConnectionState.Closed);
                    //Start reconnect timer
                    StartTimer();
                    throw;
                }

                ChangeState(SmppConnectionState.Connected);
                break;
            case (_, false, true):
                //Transmit Connection
                try
                {
                    bindInfo.AllowReceive = false;
                    bindInfo.AllowTransmit = true;
                    _vTrans = SmppClientSession.Bind(bindInfo, timeOut, _vSmppEncodingService);
                    InitializeSession(_vTrans);
                }
                catch
                {
                    try
                    {
                        _vRecv.EndSession();
                    }
                    catch
                    {
                        /*Silent catch*/
                    }

                    _vRecv = null;
                    ChangeState(SmppConnectionState.Closed);
                    //Start reconnect timer
                    StartTimer();
                    throw;
                }

                ChangeState(SmppConnectionState.Connected);
                break;
            case (false, _, _):
                try
                {
                    SmppClientSession session = SmppClientSession.Bind(bindInfo, timeOut, _vSmppEncodingService);
                    if (bindInfo.AllowTransmit)
                        _vTrans = session;
                    if (bindInfo.AllowReceive)
                        _vRecv = session;
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

                break;
            case (_, false, false):
            default:
                _vTrans = null;
                _vRecv = null;
                ChangeState(SmppConnectionState.Closed);
                break;
        }
    }

    private void CloseSession()
    {
        SmppConnectionState oldState = SmppConnectionState.Closed;

        oldState = _vState;
        if (_vState == SmppConnectionState.Closed)
        {
            return;
        }

        _vState = SmppConnectionState.Closed;

        RaiseConnectionStateChangeEvent(SmppConnectionState.Closed, oldState);
        if (_vTrans != null)
        {
            _vTrans.EndSession();
        }

        if (_vRecv != null)
        {
            _vRecv.EndSession();
        }

        _vTrans = null;
        _vRecv = null;
    }

    private void InitializeSession(SmppClientSession session)
    {
        session.EnquireLinkInterval = _vKeepAliveInterval;
        session.PduReceived += PduReceivedEventHander;
        session.SessionClosed += SessionClosedEventHandler;
    }

    private void ChangeState(SmppConnectionState newState)
    {
        SmppConnectionState oldState = SmppConnectionState.Closed;
        oldState = _vState;
        _vState = newState;
        _vProperties.SmscId = newState == SmppConnectionState.Connected ? _vTrans.SmscID : "";
        RaiseConnectionStateChangeEvent(newState, oldState);
    }

    private void RaiseMessageReceivedEvent(ShortMessage message)
    {
        if (MessageReceived != null)
        {
            MessageReceived(this, new MessageEventArgs(message));
        }
    }

    private void RaiseMessageDeliveredEvent(ShortMessage message)
    {
        if (MessageDelivered != null)
        {
            MessageDelivered(this, new MessageEventArgs(message));
        }
    }

    private void RaiseMessageSentEvent(ShortMessage message)
    {
        if (MessageSent != null)
        {
            MessageSent(this, new MessageEventArgs(message));
        }
    }

    private void RaiseConnectionStateChangeEvent(SmppConnectionState newState, SmppConnectionState oldState)
    {
        if (ConnectionStateChanged == null)
        {
            return;
        }

        ConnectionStateChangedEventArgs e =
            new ConnectionStateChangedEventArgs(newState, oldState, _vAutoReconnectDelay);
        ConnectionStateChanged(this, e);
        if (e.ReconnectInteval < 5000)
        {
            e.ReconnectInteval = 5000;
        }

        Interlocked.Exchange(ref _vAutoReconnectDelay, e.ReconnectInteval);
    }

    private void RaiseStateChangedEvent(bool started)
    {
        if (StateChanged == null)
        {
            return;
        }

        StateChangedEventArgs e = new StateChangedEventArgs(started);
        StateChanged(this, e);
    }

    private void PduReceivedEventHander(object sender, PduReceivedEventArgs e)
    {
        //This handler is interested in SingleDestinationPDU only
        SingleDestinationPDU pdu = e.Request as SingleDestinationPDU;
        if (pdu == null)
        {
            return;
        }

        if (Log.IsDebugEnabled)
            Log.DebugFormat("Received PDU: {0}", LoggingExtensions.DumpString(pdu, _vSmppEncodingService));

        if (_vTraceSwitch.TraceVerbose)
        {
            Trace.WriteLine(string.Format("PduReceived: RequestType: {0}", e.Request?.GetType()?.Name));
        }

        ShortMessage message = null;
        try
        {
            message = MessageFactory.CreateMessage(pdu);
        }
        catch (SmppException smppEx)
        {
            Log.ErrorFormat("200019:SMPP message decoding failure - {0} - {1} {2}", smppEx, smppEx.ErrorCode,
                new ByteBuffer(pdu.GetBytes()).DumpString(), smppEx.Message);
            if (_vTraceSwitch.TraceError)
            {
                Trace.WriteLine(string.Format(
                    "200019:SMPP message decoding failure - {0} - {1} {2};",
                    smppEx.ErrorCode, new ByteBuffer(pdu.GetBytes()).DumpString(), smppEx.Message));
            }

            //Notify the SMSC that we encountered an error while processing the message
            e.Response = pdu.CreateDefaultResponse();
            e.Response.Header.ErrorCode = smppEx.ErrorCode;
            return;
        }
        catch (Exception ex)
        {
            Log.ErrorFormat("200019:SMPP message decoding failure - {0}", ex,
                new ByteBuffer(pdu.GetBytes()).DumpString());
            if (_vTraceSwitch.TraceError)
            {
                Trace.WriteLine(string.Format(
                    "200019:SMPP message decoding failure - {0} {1};",
                    new ByteBuffer(pdu.GetBytes()).DumpString(), ex.Message));
            }

            //Let the receiver know that this message was rejected
            e.Response = pdu.CreateDefaultResponse();
            e.Response.Header.ErrorCode = SmppErrorCode.ESME_RX_P_APPN; //ESME Receiver Reject Message
            return;
        }

        if (message != null && Log.IsDebugEnabled)
            Log.DebugFormat("PduReceived: message: {0}",
                LoggingExtensions.DumpString(message, _vSmppEncodingService));

        if (_vTraceSwitch.TraceVerbose)
        {
#if DEBUG
            Console.WriteLine(string.Format(
                "PduReceived: pdu: Header:{0}, EsmClass:{1}, ServiceType:{2}, DataCoding:{3}", pdu.Header,
                pdu.EsmClass, pdu.ServiceType, pdu.DataCoding));
#endif
            Trace.WriteLine(string.Format(
                "PduReceived: pdu: Header:{0}, EsmClass:{1}, ServiceType:{2}, DataCoding:{3}", pdu.Header,
                pdu.EsmClass, pdu.ServiceType, pdu.DataCoding));
            if (message != null)
                Trace.WriteLine(string.Format(
                    "PduReceived: message: DestinationAddress:{0}, MessageCount:{1}, ReceiptedMessageId:{2}, RegisterDeliveryNotification:{3}, SegmentID:{4}, SequenceNumber:{5}, SourceAddress:{6}, UserMessageReference:{7}",
                    message.DestinationAddress, message.MessageCount, message.ReceiptedMessageId,
                    message.RegisterDeliveryNotification, message.SegmentID, message.SequenceNumber,
                    message.SourceAddress, message.UserMessageReference));
        }

        //If we have just a normal message
        if ((((byte)pdu.EsmClass) | 0xc3) == 0xc3)
        {
            RaiseMessageReceivedEvent(message);
        }
        //Or if we have received a delivery receipt
        else if ((pdu.EsmClass & EsmClass.DeliveryReceipt) == EsmClass.DeliveryReceipt)
        {
            // Extract receipted message id
            message.ReceiptedMessageId = pdu.GetOptionalParamString(Tag.receipted_message_id);
            // Extract receipted message state
            message.MessageState = pdu.GetOptionalParamByte<MessageState>(Tag.message_state);
            // Extract receipted network error code
            message.NetworkErrorCode = pdu.GetOptionalParamBytes(Tag.network_error_code);
            // Extract user message reference
            message.UserMessageReference = pdu.GetOptionalParamString(Tag.user_message_reference);
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
        _vTimer.Change(_vAutoReconnectDelay, _vAutoReconnectDelay);
    }

    private void StopTimer()
    {
        _vTimer.Change(Timeout.Infinite, _vAutoReconnectDelay);
    }

    void AutoReconnectTimerEventHandler(object state)
    {
        //Do not reconnect if AutoReconnectDalay < 0 or if SmppClient is shutdown
        if (AutoReconnectDelay <= 0 || !Started)
        {
            return;
        }

        //Stop the timer from raising subsequent events before
        //the current thread exists
        StopTimer();

        int timeOut = 0;
        timeOut = _vTimeOut;
        try
        {
            Open(timeOut);
        }
        catch (Exception)
        {
            /*Do nothing*/
        }

        if (_vState == SmppConnectionState.Closed)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }

    #endregion

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposeManagedResorces)
    {
        try
        {
            Shutdown();
            if (_vTimer != null)
            {
                _vTimer.Dispose();
            }
        }
        catch
        {
            /*Sielent catch*/
        }
    }

    #endregion
}