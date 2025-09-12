using System.Collections.Generic;
using System.Threading;
using System;
using JamaaTech.Smpp.Net.Lib.Protocol;
using System.Threading.Tasks;

namespace JamaaTech.Smpp.Net.Lib
{

    public class ResponseHandlerV2 : IResponseHandler
    {
        #region Variables
        private int vDefaultResponseTimeout;
        private IDictionary<uint, ResponsePDU> vResponseQueue;
        private IDictionary<uint, object> vWaitingQueue;
        private readonly object vResponseLock = new object();
        private readonly object vWaitingLock = new object();
        // Minimum enforced timeout (was hard-coded 5000). Made adjustable for testing.
        private static int sMinTimeout = 5000;
        #endregion

        #region Testing Helpers
        /// <summary>
        /// Adjust minimum timeout (intended for unit testing). Use cautiously in production.
        /// </summary>
        public static void SetMinimumTimeoutForTesting(int milliseconds)
        {
            if (milliseconds < 1) { milliseconds = 1; }
            Interlocked.Exchange(ref sMinTimeout, milliseconds);
        }
        /// <summary>
        /// Returns current minimum enforced timeout.
        /// </summary>
        public static int GetMinimumTimeoutForTesting() { return sMinTimeout; }
        #endregion

        #region Constructors
        public ResponseHandlerV2()
        {
            vDefaultResponseTimeout = sMinTimeout; //Default min
            vWaitingQueue = new Dictionary<uint, object>(32);
            vResponseQueue = new Dictionary<uint, ResponsePDU>(32);
        }
        #endregion

        #region Properties
        public int DefaultResponseTimeout
        {
            get { return vDefaultResponseTimeout; }
            set
            {
                int timeOut = sMinTimeout;
                if (value > timeOut) { timeOut = value; }
                Interlocked.Exchange(ref vDefaultResponseTimeout, timeOut);
            }
        }
        public int Count
        {
            get { lock (vResponseLock) { return vResponseQueue.Count; } }
        }
        #endregion

        #region Methods
        #region Interface Methods
        public void Handle(ResponsePDU pdu)
        {
            uint sequenceNumber = pdu.Header.SequenceNumber;

            // First, add the response to the queue
            lock (vResponseLock)
            {
                vResponseQueue[sequenceNumber] = pdu;
            }

            // Then check for waiting contexts and notify them
            lock (vWaitingLock)
            {
                object waitContext;
                if (vWaitingQueue.TryGetValue(sequenceNumber, out waitContext))
                {
                    if (waitContext is PDUWaitContext syncContext)
                    {
                        syncContext.AlertResponseReceived();
                        if (syncContext.TimedOut)
                        {
                            // Remove from waiting queue since it's been processed
                            RemoveWaitingQueue(sequenceNumber);
                        }
                    }
                    else if (waitContext is PDUWaitContextAsync asyncContext)
                    {
                        asyncContext.AlertResponseReceived(pdu);
                        // Remove from waiting queue since it's been processed
                        RemoveWaitingQueue(sequenceNumber);
                    }
                }
            }
        }

        public ResponsePDU WaitResponse(RequestPDU pdu)
        {
            return WaitResponse(pdu, vDefaultResponseTimeout);
        }

        public ResponsePDU WaitResponse(RequestPDU pdu, int timeOut)
        {
            uint sequenceNumber = pdu.Header.SequenceNumber;

            // Check if response already exists
            ResponsePDU resp = FetchResponse(sequenceNumber);
            if (resp != null) { return resp; }

            if (timeOut < sMinTimeout) { timeOut = vDefaultResponseTimeout; }
            PDUWaitContext waitContext = new PDUWaitContext(sequenceNumber, timeOut);

            // Add to waiting queue
            lock (vWaitingLock)
            {
                // Double-check if response arrived while we were waiting for the lock
                resp = FetchResponse(sequenceNumber);
                if (resp != null) { return resp; }

                vWaitingQueue[sequenceNumber] = waitContext;
            }

            // Wait for the response
            bool responseReceived = waitContext.WaitForAlert();

            // Fetch the response after waiting
            resp = FetchResponse(sequenceNumber);

            // Clean up the waiting queue entry only if we didn't get a response (timeout case)
            if (resp == null)
            {
                lock (vWaitingLock)
                {
                    RemoveWaitingQueue(sequenceNumber);
                }
                throw new SmppResponseTimedOutException();
            }

            return resp;
        }

#if !NET40
        public async Task<ResponsePDU> WaitResponseAsync(RequestPDU pdu, int timeOut, CancellationToken cancellationToken = default)
        {
            uint sequenceNumber = pdu.Header.SequenceNumber;

            // Check if response already exists
            ResponsePDU resp = FetchResponse(sequenceNumber);
            if (resp != null) { return resp; }

            if (timeOut < sMinTimeout) { timeOut = vDefaultResponseTimeout; }

            var tcs = new TaskCompletionSource<ResponsePDU>();
            var waitContext = new PDUWaitContextAsync(sequenceNumber, timeOut, tcs, cancellationToken);

            // Add to waiting queue
            lock (vWaitingLock)
            {
                // Double-check if response arrived while we were waiting for the lock
                resp = FetchResponse(sequenceNumber);
                if (resp != null)
                {
                    waitContext.Dispose(); // Clean up the context
                    return resp;
                }

                vWaitingQueue[sequenceNumber] = waitContext;
            }

            try
            {
                return await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                // Clean up the waiting queue entry if the task completed (timeout or cancellation)
                if (tcs.Task.IsCompleted)
                {
                    lock (vWaitingLock)
                    {
                        vWaitingQueue.Remove(sequenceNumber);
                    }
                }
            }
        }

        public async Task<ResponsePDU> WaitResponseAsync(RequestPDU pdu, CancellationToken cancellationToken = default)
        {
            return await WaitResponseAsync(pdu, vDefaultResponseTimeout, cancellationToken).ConfigureAwait(false);
        }
#endif
        #endregion

        #region Helper Methods
        private void AddResponse(ResponsePDU pdu)
        {
            lock (vResponseLock)
            {
                vResponseQueue[pdu.Header.SequenceNumber] = pdu;
            }
        }

        private ResponsePDU FetchResponse(uint sequenceNumber)
        {
            lock (vResponseLock)
            {
                ResponsePDU pdu;
                vResponseQueue.TryGetValue(sequenceNumber, out pdu);
                return pdu;
            }
        }

        private void RemoveWaitingQueue(uint sequenceNumber)
        {
            vWaitingQueue.Remove(sequenceNumber);
        }

        #endregion
        #endregion
    }
}
