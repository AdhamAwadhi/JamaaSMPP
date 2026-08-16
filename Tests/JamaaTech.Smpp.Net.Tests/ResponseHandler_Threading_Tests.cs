using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Testing;
using JamaaTech.Smpp.Net.Lib.Protocol;
using Xunit;
using Xunit.Abstractions;

namespace JamaaTech.Smpp.Net.Tests
{
    public class ResponseHandler_Threading_Tests : IDisposable
    {
        private readonly ITestOutputHelper output;
        private readonly int _originalMin;
        public ResponseHandler_Threading_Tests(ITestOutputHelper output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));

            _originalMin = ResponseHandler.GetMinimumTimeoutForTesting();
            ResponseHandler.SetMinimumTimeoutForTesting(30); // shrink min for fast tests
        }
        public void Dispose()
        {
            ResponseHandler.SetMinimumTimeoutForTesting(_originalMin);
        }

        private static int GetWaitingQueueCount(ResponseHandler h)
        {
            var f = typeof(ResponseHandler).GetField("vWaitingQueue", BindingFlags.Instance | BindingFlags.NonPublic);
            var dict = (System.Collections.IDictionary)f.GetValue(h);
            return dict.Count;
        }
        private static int GetResponseQueueCount(ResponseHandler h)
        {
            var f = typeof(ResponseHandler).GetField("vResponseQueue", BindingFlags.Instance | BindingFlags.NonPublic);
            var dict = (System.Collections.IDictionary)f.GetValue(h);
            return dict.Count;
        }

        [Fact]
        public void Concurrency_WaitAndHandle_ManySequences()
        {
            int original = ResponseHandler.GetMinimumTimeoutForTesting();
            ResponseHandler.SetMinimumTimeoutForTesting(500);
            try
            {
                var handler = new ResponseHandler();
                int n = 50000;
                var seqs = Enumerable.Range(1, n).Select(i => (uint)i).ToArray();
                var tasks = seqs.Select(s => Task.Run(() => handler.WaitResponse(new TestRequestPDU(s)))).ToArray();
                // deliver in reverse order
                seqs.Reverse();
                foreach (var s in seqs)
                {
                    handler.Handle(new TestResponsePDU(s));
                }
                Task.WaitAll(tasks);
                Assert.Equal(n, handler.Count); // all responses cached (also indicates not removed)
                var waitingCount = GetWaitingQueueCount(handler);
                var responseCount = GetResponseQueueCount(handler);


                output.WriteLine($"WaitingQueueCount: {waitingCount}");
                output.WriteLine($"ResponseQueueCount: {responseCount}");
            }
            finally
            {
                ResponseHandler.SetMinimumTimeoutForTesting(original);
            }
        }
    }
}
