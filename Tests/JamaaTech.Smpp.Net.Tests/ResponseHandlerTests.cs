using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JamaaTech.Smpp.Net.Lib;
using JamaaTech.Smpp.Net.Lib.Testing;
using JamaaTech.Smpp.Net.Lib.Protocol;
using Xunit;

namespace JamaaTech.Smpp.Net.Tests
{
    public class ResponseHandlerTests : IDisposable
    {
        private readonly int _originalMin;
        public ResponseHandlerTests()
        {
            _originalMin = ResponseHandlerV2.GetMinimumTimeoutForTesting();
            ResponseHandlerV2.SetMinimumTimeoutForTesting(30); // shrink min for fast tests
        }
        public void Dispose()
        {
            ResponseHandlerV2.SetMinimumTimeoutForTesting(_originalMin);
        }

        private static int GetWaitingQueueCount(ResponseHandlerV2 h)
        {
            var f = typeof(ResponseHandlerV2).GetField("vWaitingQueue", BindingFlags.Instance | BindingFlags.NonPublic);
            var dict = (System.Collections.IDictionary)f.GetValue(h);
            return dict.Count;
        }
        private static int GetResponseQueueCount(ResponseHandlerV2 h)
        {
            var f = typeof(ResponseHandlerV2).GetField("vResponseQueue", BindingFlags.Instance | BindingFlags.NonPublic);
            var dict = (System.Collections.IDictionary)f.GetValue(h);
            return dict.Count;
        }

        [Fact]
        public void Constructor_SetsDefaults()
        {
            var handler = new ResponseHandlerV2();
            Assert.Equal(ResponseHandlerV2.GetMinimumTimeoutForTesting(), handler.DefaultResponseTimeout);
            Assert.Equal(0, handler.Count);
        }

        [Fact]
        public void DefaultResponseTimeout_ClampToMinimum()
        {
            var handler = new ResponseHandlerV2();
            handler.DefaultResponseTimeout = 5; // below min 30
            Assert.Equal(30, handler.DefaultResponseTimeout);
        }

        [Fact]
        public void DefaultResponseTimeout_SetHigherValue()
        {
            var handler = new ResponseHandlerV2();
            handler.DefaultResponseTimeout = 120;
            Assert.Equal(120, handler.DefaultResponseTimeout);
        }

        [Fact]
        public void Handle_AddsResponse_WhenNoWaitingContext()
        {
            var handler = new ResponseHandlerV2();
            uint seq = 0xABCDEF;
            var resp = new TestResponsePDU(seq);

            handler.Handle(resp);

            Assert.Equal(1, handler.Count);

            var req = new TestRequestPDU(seq);
            var sw = Stopwatch.StartNew();
            var returned = handler.WaitResponse(req);
            sw.Stop();

            Assert.Same(resp, returned);
            Assert.True(sw.ElapsedMilliseconds < 50);
        }

        [Fact]
        public void WaitResponse_BlocksUntilResponseArrives()
        {
            var handler = new ResponseHandlerV2();
            uint seq = 0x1234;
            var req = new TestRequestPDU(seq);
            var resp = new TestResponsePDU(seq);

            var sw = Stopwatch.StartNew();
            var waitTask = Task.Run(() => handler.WaitResponse(req));

            Task.Delay(25).Wait();
            handler.Handle(resp);

            var result = waitTask.Result;
            sw.Stop();

            Assert.Same(resp, result);
            Assert.True(sw.ElapsedMilliseconds >= 20);
            Assert.True(sw.ElapsedMilliseconds < 500);
        }

        [Fact]
        public void WaitResponse_ThrowsOnTimeout_UsingDefault()
        {
            var handler = new ResponseHandlerV2();
            uint seq = 0x2222;
            var req = new TestRequestPDU(seq);

            var sw = Stopwatch.StartNew();
            Assert.Throws<SmppResponseTimedOutException>(() => handler.WaitResponse(req));
            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds >= 25, $"Elapsed {sw.ElapsedMilliseconds}");
            Assert.True(sw.ElapsedMilliseconds < 500);
            // waiting queue properly cleaned up after timeout (fixed issue)
            Assert.Equal(0, GetWaitingQueueCount(handler));
        }

        [Fact]
        public void WaitResponse_TimeoutParameterBelowMinimum_UsesDefaultTimeout()
        {
            var handler = new ResponseHandlerV2();
            handler.DefaultResponseTimeout = 90; // > min (30)
            uint seq = 0x3333;
            var req = new TestRequestPDU(seq);

            var sw = Stopwatch.StartNew();
            Assert.Throws<SmppResponseTimedOutException>(() => handler.WaitResponse(req, 5)); // 5 < min => use default (90)
            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds >= 80);
            Assert.True(sw.ElapsedMilliseconds < 800);
        }

        [Fact]
        public void Handle_ResponseArrivesAfterWaiterTimedOut_ExecutesTimedOutBranch()
        {
            var handler = new ResponseHandlerV2();
            uint seq = 0x4444;
            var req = new TestRequestPDU(seq);
            var resp = new TestResponsePDU(seq);

            Assert.Throws<SmppResponseTimedOutException>(() => handler.WaitResponse(req));
            int waitingAfterTimeout = GetWaitingQueueCount(handler); // timed-out entry present

            handler.Handle(resp); // triggers TimedOut branch path and cleans up the entry

            var fetched = handler.WaitResponse(new TestRequestPDU(seq));
            Assert.Same(resp, fetched);
            Assert.Equal(0, GetWaitingQueueCount(handler)); // entry properly cleaned up
        }

        [Fact]
        public void ResponseNotRemoved_AllowsSecondWaitResponse_ReturningSameObject()
        {
            var handler = new ResponseHandlerV2();
            uint seq = 0x5555;
            var resp = new TestResponsePDU(seq);
            handler.Handle(resp);
            var first = handler.WaitResponse(new TestRequestPDU(seq));
            var second = handler.WaitResponse(new TestRequestPDU(seq)); // same cached instance
            Assert.Same(resp, first);
            Assert.Same(first, second); // demonstrates stale retention (issue)
            Assert.Equal(1, GetResponseQueueCount(handler));
        }

        [Fact]
        public void Concurrency_WaitAndHandle_ManySequences()
        {
            int original = ResponseHandlerV2.GetMinimumTimeoutForTesting();
            ResponseHandlerV2.SetMinimumTimeoutForTesting(500);
            try
            {
                var handler = new ResponseHandlerV2();
                int n = 50;
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
            }
            finally
            {
                ResponseHandlerV2.SetMinimumTimeoutForTesting(original);
            }
        }

        [Fact]
        public void StaticMinTimeoutChange_AffectsNewInstancesButNotExistingValueUnlessPropertySet()
        {
            ResponseHandlerV2.SetMinimumTimeoutForTesting(10);
            var a = new ResponseHandlerV2();
            Assert.Equal(10, a.DefaultResponseTimeout);
            ResponseHandlerV2.SetMinimumTimeoutForTesting(100);
            // existing instance still has old default
            Assert.Equal(10, a.DefaultResponseTimeout);
            // setting below new min clamps to 100
            a.DefaultResponseTimeout = 20; // 20 < new min 100
            Assert.Equal(100, a.DefaultResponseTimeout); // shows global side effect (issue)
        }

        #region Threading Safety Tests

        [Fact]
        public void Handle_ConcurrentAccess_ThreadSafe()
        {
            var handler = new ResponseHandlerV2();
            var results = new ConcurrentBag<ResponsePDU>();
            var exceptions = new ConcurrentBag<Exception>();
            const int threadCount = 10;
            const int operationsPerThread = 100;

            // Test concurrent Handle operations
            var tasks = Enumerable.Range(0, threadCount).Select(threadId =>
                Task.Run(() =>
                {
                    try
                    {
                        for (int i = 0; i < operationsPerThread; i++)
                        {
                            uint seq = (uint)(threadId * operationsPerThread + i);
                            var resp = new TestResponsePDU(seq);
                            handler.Handle(resp);
                            results.Add(resp);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                })).ToArray();

            Task.WaitAll(tasks);

            // Verify no exceptions occurred
            Assert.Empty(exceptions);
            
            // Verify all responses were handled
            Assert.Equal(threadCount * operationsPerThread, results.Count);
            
            // Verify waiting queue is empty (no leaks)
            Assert.Equal(0, GetWaitingQueueCount(handler));
        }

        [Fact]
        public void WaitResponse_ConcurrentAccess_ThreadSafe()
        {
            var handler = new ResponseHandlerV2();
            var results = new ConcurrentBag<ResponsePDU>();
            var exceptions = new ConcurrentBag<Exception>();
            const int threadCount = 5;
            const int operationsPerThread = 20;

            // Test concurrent WaitResponse operations
            var tasks = Enumerable.Range(0, threadCount).Select(threadId =>
                Task.Run(() =>
                {
                    try
                    {
                        for (int i = 0; i < operationsPerThread; i++)
                        {
                            uint seq = (uint)(threadId * operationsPerThread + i);
                            var req = new TestRequestPDU(seq);
                            var resp = new TestResponsePDU(seq);
                            
                            // Start waiting for response
                            var waitTask = Task.Run(() => handler.WaitResponse(req));
                            
                            // Small delay then send response
                            Thread.Sleep(10);
                            handler.Handle(resp);
                            
                            var result = waitTask.Result;
                            results.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                })).ToArray();

            Task.WaitAll(tasks);

            // Verify no exceptions occurred
            Assert.Empty(exceptions);
            
            // Verify all responses were received
            Assert.Equal(threadCount * operationsPerThread, results.Count);
            
            // Verify waiting queue is empty (no leaks)
            Assert.Equal(0, GetWaitingQueueCount(handler));
        }

        [Fact]
        public async Task WaitResponseAsync_ConcurrentAccess_ThreadSafe()
        {
            var handler = new ResponseHandlerV2();
            var results = new ConcurrentBag<ResponsePDU>();
            var exceptions = new ConcurrentBag<Exception>();
            const int threadCount = 5;
            const int operationsPerThread = 20;

            // Test concurrent WaitResponseAsync operations
            var tasks = Enumerable.Range(0, threadCount).Select(threadId =>
                Task.Run(async () =>
                {
                    try
                    {
                        for (int i = 0; i < operationsPerThread; i++)
                        {
                            uint seq = (uint)(threadId * operationsPerThread + i);
                            var req = new TestRequestPDU(seq);
                            var resp = new TestResponsePDU(seq);
                            
                            // Start waiting for response
                            var waitTask = handler.WaitResponseAsync(req);
                            
                            // Small delay then send response
                            await Task.Delay(10);
                            handler.Handle(resp);
                            
                            var result = await waitTask;
                            results.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                })).ToArray();

            await Task.WhenAll(tasks);

            // Verify no exceptions occurred
            Assert.Empty(exceptions);
            
            // Verify all responses were received
            Assert.Equal(threadCount * operationsPerThread, results.Count);
            
            // Verify waiting queue is empty (no leaks)
            Assert.Equal(0, GetWaitingQueueCount(handler));
        }

        [Fact]
        public void Handle_ResponseArrivesBeforeWait_NoRaceCondition()
        {
            var handler = new ResponseHandlerV2();
            uint seq = 0x9999;
            var resp = new TestResponsePDU(seq);

            // Send response first
            handler.Handle(resp);

            // Then wait for it (should return immediately)
            var req = new TestRequestPDU(seq);
            var result = handler.WaitResponse(req);

            Assert.Same(resp, result);
            Assert.Equal(0, GetWaitingQueueCount(handler));
        }

        [Fact]
        public async Task Handle_ResponseArrivesBeforeWaitAsync_NoRaceCondition()
        {
            var handler = new ResponseHandlerV2();
            uint seq = 0x9999;
            var resp = new TestResponsePDU(seq);

            // Send response first
            handler.Handle(resp);

            // Then wait for it (should return immediately)
            var req = new TestRequestPDU(seq);
            var result = await handler.WaitResponseAsync(req);

            Assert.Same(resp, result);
            Assert.Equal(0, GetWaitingQueueCount(handler));
        }

        [Fact]
        public void WaitResponse_MultipleTimeouts_NoMemoryLeak()
        {
            var handler = new ResponseHandlerV2();
            const int timeoutCount = 50;

            // Create multiple timeouts
            for (int i = 0; i < timeoutCount; i++)
            {
                uint seq = (uint)(0x1000 + i);
                var req = new TestRequestPDU(seq);
                
                Assert.Throws<SmppResponseTimedOutException>(() => handler.WaitResponse(req));
            }

            // Verify waiting queue is empty (no memory leaks)
            Assert.Equal(0, GetWaitingQueueCount(handler));
        }

        [Fact]
        public async Task WaitResponseAsync_MultipleTimeouts_NoMemoryLeak()
        {
            var handler = new ResponseHandlerV2();
            const int timeoutCount = 50;

            // Create multiple timeouts
            var tasks = new Task[timeoutCount];
            for (int i = 0; i < timeoutCount; i++)
            {
                uint seq = (uint)(0x2000 + i);
                var req = new TestRequestPDU(seq);
                
                tasks[i] = Task.Run(async () =>
                {
                    try
                    {
                        await handler.WaitResponseAsync(req, 50); // Use short timeout
                        throw new Exception("Expected timeout exception");
                    }
                    catch (SmppResponseTimedOutException)
                    {
                        // Expected
                    }
                });
            }

            await Task.WhenAll(tasks);

            // Verify waiting queue is empty (no memory leaks)
            Assert.Equal(0, GetWaitingQueueCount(handler));
        }

        [Fact]
        public async Task WaitResponse_Cancellation_ProperCleanup()
        {
            var handler = new ResponseHandlerV2();
            var cts = new CancellationTokenSource();
            uint seq = 0x8888;
            var req = new TestRequestPDU(seq);

            // Start waiting with cancellation token
            var waitTask = handler.WaitResponseAsync(req, 1000, cts.Token);

            // Cancel after short delay
            Thread.Sleep(50);
            cts.Cancel();

            // Verify cancellation
            await Assert.ThrowsAsync<TaskCanceledException>(() => waitTask);

            // Verify waiting queue is empty (proper cleanup)
            Assert.Equal(0, GetWaitingQueueCount(handler));
        }

        [Fact]
        public void Handle_MixedSyncAsyncContexts_ThreadSafe()
        {
            var handler = new ResponseHandlerV2();
            var results = new ConcurrentBag<ResponsePDU>();
            var exceptions = new ConcurrentBag<Exception>();
            const int operationCount = 100;

            // Mix of sync and async operations
            var tasks = Enumerable.Range(0, operationCount).Select(i =>
            {
                uint seq = (uint)(0x3000 + i);
                var req = new TestRequestPDU(seq);
                var resp = new TestResponsePDU(seq);

                if (i % 2 == 0)
                {
                    // Sync operation
                    return Task.Run(() =>
                    {
                        try
                        {
                            var waitTask = Task.Run(() => handler.WaitResponse(req));
                            Thread.Sleep(10);
                            handler.Handle(resp);
                            var result = waitTask.Result;
                            results.Add(result);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    });
                }
                else
                {
                    // Async operation
                    return Task.Run(async () =>
                    {
                        try
                        {
                            var waitTask = handler.WaitResponseAsync(req);
                            await Task.Delay(10);
                            handler.Handle(resp);
                            var result = await waitTask;
                            results.Add(result);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    });
                }
            }).ToArray();

            Task.WaitAll(tasks);

            // Verify no exceptions occurred
            Assert.Empty(exceptions);
            
            // Verify all responses were received
            Assert.Equal(operationCount, results.Count);
            
            // Verify waiting queue is empty (no leaks)
            Assert.Equal(0, GetWaitingQueueCount(handler));
        }

        [Fact]
        public void Handle_ResponseAfterTimeout_ProperCleanup()
        {
            var handler = new ResponseHandlerV2();
            uint seq = 0x7777;
            var req = new TestRequestPDU(seq);
            var resp = new TestResponsePDU(seq);

            // Wait for timeout
            Assert.Throws<SmppResponseTimedOutException>(() => handler.WaitResponse(req));
            Assert.Equal(0, GetWaitingQueueCount(handler)); // Should be cleaned up

            // Send response after timeout
            handler.Handle(resp);

            // Response should be available for next request
            var req2 = new TestRequestPDU(seq);
            var result = handler.WaitResponse(req2);
            Assert.Same(resp, result);
        }

        [Fact]
        public void WaitResponse_StressTest_NoDeadlocks()
        {
            var handler = new ResponseHandlerV2();
            const int threadCount = 20;
            const int operationsPerThread = 50;
            var exceptions = new ConcurrentBag<Exception>();

            // Stress test with many concurrent operations
            var tasks = Enumerable.Range(0, threadCount).Select(threadId =>
                Task.Run(() =>
                {
                    try
                    {
                        for (int i = 0; i < operationsPerThread; i++)
                        {
                            uint seq = (uint)(threadId * operationsPerThread + i);
                            var req = new TestRequestPDU(seq);
                            var resp = new TestResponsePDU(seq);

                            // Randomly choose between immediate response or timeout
                            if (i % 3 == 0)
                            {
                                // Timeout case
                                Assert.Throws<SmppResponseTimedOutException>(() => handler.WaitResponse(req));
                            }
                            else
                            {
                                // Success case
                                var waitTask = Task.Run(() => handler.WaitResponse(req));
                                Thread.Sleep(5);
                                handler.Handle(resp);
                                var result = waitTask.Result;
                                Assert.Same(resp, result);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                })).ToArray();

            // Wait with timeout to detect deadlocks
            bool completed = Task.WaitAll(tasks, TimeSpan.FromSeconds(30));
            
            Assert.True(completed, "Stress test timed out - possible deadlock");
            Assert.Empty(exceptions);
            Assert.Equal(0, GetWaitingQueueCount(handler));
        }

        #endregion
    }
}
