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
using JamaaTech.Smpp.Net.Lib.Protocol;
using System.Threading;
using System.Collections.Concurrent;

namespace JamaaTech.Smpp.Net.Lib
{
    public class ResponseHandler
    {
        #region Variables
        private int vDefaultResponseTimeout;
        private IDictionary<uint, ResponsePDU> vResponseQueue;
        private IDictionary<uint, PDUWaitContext> vWaitingQueue;
        private AutoResetEvent vResponseEvent;
        private AutoResetEvent vWaitingEvent;
        #endregion

        #region Constructors
        public ResponseHandler()
        {
            vDefaultResponseTimeout = 5000; //Five seconds
            vWaitingQueue = new Dictionary<uint, PDUWaitContext>(32);
            vResponseQueue = new Dictionary<uint, ResponsePDU>(32);
            vResponseEvent = new AutoResetEvent(true);
            vWaitingEvent = new AutoResetEvent(true);
        }
        #endregion

        #region Properties
        public int DefaultResponseTimeout
        {
            get { return vDefaultResponseTimeout; }
            set
            {
                int timeOut = 5000;
                if (value > timeOut) { timeOut = value; }
                Interlocked.Exchange(ref vDefaultResponseTimeout, timeOut);
            }
        }
        public int Count
        {
            get { lock (vResponseQueue) { return vResponseQueue.Count; } }
        }
        #endregion

        #region Methods
        #region Interface Methods
        public void Handle(ResponsePDU pdu)
        {
            AddResponse(pdu);
            vWaitingEvent.WaitOne();
            try
            {
                uint sequenceNumber = pdu.Header.SequenceNumber;
                PDUWaitContext waitContext;
                if (vWaitingQueue.TryGetValue(sequenceNumber, out waitContext))
                {
                    waitContext.AlertResponseReceived();
                    if (waitContext.TimedOut) { FetchResponse(sequenceNumber); }
                }
            }
            finally { vWaitingEvent.Set(); }
        }

        public ResponsePDU WaitResponse(RequestPDU pdu)
        {
            return WaitResponse(pdu, vDefaultResponseTimeout);
        }

        public ResponsePDU WaitResponse(RequestPDU pdu, int timeOut)
        {
            uint sequenceNumber = pdu.Header.SequenceNumber;
            ResponsePDU resp = FetchResponse(sequenceNumber);
            if (resp != null) { return resp; }
            if (timeOut < 5000) { timeOut = vDefaultResponseTimeout; }
            PDUWaitContext waitContext = new PDUWaitContext(sequenceNumber, timeOut);
            vWaitingEvent.WaitOne();
            try { vWaitingQueue[sequenceNumber] = waitContext; }
            finally { vWaitingEvent.Set(); }
            waitContext.WaitForAlert();
            resp = FetchResponse(sequenceNumber);
            if (resp == null) { throw new SmppResponseTimedOutException(); }
            return resp;
        }
        #endregion

        #region Helper Methods
        private void AddResponse(ResponsePDU pdu)
        {
            vResponseEvent.WaitOne();
            try { vResponseQueue[pdu.Header.SequenceNumber] = pdu; }
            finally { vResponseEvent.Set(); }
        }

        private ResponsePDU FetchResponse(uint sequenceNumber)
        {
            vResponseEvent.WaitOne();
            try
            {
                ResponsePDU pdu;
                vResponseQueue.TryGetValue(sequenceNumber, out pdu);
                return pdu;
            }
            finally { vResponseEvent.Set(); }
        }
        #endregion
        #endregion
    }
}
