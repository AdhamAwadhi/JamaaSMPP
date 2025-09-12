using System.Threading;
using System;
using JamaaTech.Smpp.Net.Lib.Protocol;
using System.Threading.Tasks;

namespace JamaaTech.Smpp.Net.Lib
{
    public class PDUWaitContextAsync : IDisposable
    {
        #region Variables
        private readonly uint vSequenceNumber;
        private readonly TaskCompletionSource<ResponsePDU> vTaskCompletionSource;
        private readonly CancellationToken vCancellationToken;
        private CancellationTokenRegistration vCancellationTokenRegistration;
        private Timer vTimeoutTimer;
        private bool vTimedOut;
        private volatile bool vCompleted;
        private readonly object vSyncRoot = new object();
        #endregion

        #region Constructors
        public PDUWaitContextAsync(uint sequenceNumber, int timeOut, TaskCompletionSource<ResponsePDU> tcs, CancellationToken cancellationToken)
        {
            vSequenceNumber = sequenceNumber;
            vTaskCompletionSource = tcs;
            vCancellationToken = cancellationToken;
            vTimedOut = false;
            vCompleted = false;

            // Set up cancellation token registration
            vCancellationTokenRegistration = cancellationToken.Register(() =>
            {
                lock (vSyncRoot)
                {
                    if (!vCompleted)
                    {
                        vCompleted = true;
#if NET40
                        // In .NET 4.0, we don't have TrySetCanceled with CancellationToken
                        vTaskCompletionSource.TrySetCanceled();
#else
                        vTaskCompletionSource.TrySetCanceled(cancellationToken);
#endif
                        Cleanup();
                    }
                }
            });

            // Set up timeout timer
            vTimeoutTimer = new Timer(OnTimeout, null, timeOut, Timeout.Infinite);
        }
        #endregion

        #region Properties
        public uint SequenceNumber
        {
            get { return vSequenceNumber; }
        }

        public bool TimedOut
        {
            get { return vTimedOut; }
        }

        public bool Completed
        {
            get { return vCompleted; }
        }
        #endregion

        #region Methods
        public void AlertResponseReceived(ResponsePDU response)
        {
            lock (vSyncRoot)
            {
                if (!vCompleted)
                {
                    vCompleted = true;
                    vTaskCompletionSource.TrySetResult(response);
                    Cleanup();
                }
            }
        }

        private void OnTimeout(object state)
        {
            lock (vSyncRoot)
            {
                if (!vCompleted)
                {
                    vTimedOut = true;
                    vCompleted = true;
                    vTaskCompletionSource.TrySetException(new SmppResponseTimedOutException());
                    Cleanup();
                }
            }
        }

        private void Cleanup()
        {
            vTimeoutTimer?.Dispose();
            vCancellationTokenRegistration.Dispose();
        }

        public void Dispose()
        {
            lock (vSyncRoot)
            {
                if (!vCompleted)
                {
                    vCompleted = true;
#if NET40
                    // In .NET 4.0, we don't have TrySetCanceled with CancellationToken
                    vTaskCompletionSource.TrySetCanceled();
#else
                        vTaskCompletionSource.TrySetCanceled(vCancellationToken);
#endif
                    Cleanup();
                }
            }
        }
        #endregion
    }
}
