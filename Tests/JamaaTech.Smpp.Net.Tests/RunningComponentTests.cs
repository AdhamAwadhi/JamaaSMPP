using System;
using System.Threading;
using System.Threading.Tasks;
using JamaaTech.Smpp.Net.Lib.Util;
using Xunit;

namespace JamaaTech.Smpp.Net.Tests
{
    public class TestRunningComponent : RunningComponent
    {
        private int _runCount = 0;
        private bool _shouldThrow = false;
        private readonly ManualResetEventSlim _runStarted = new ManualResetEventSlim(false);

        public int RunCount => _runCount;
        public ManualResetEventSlim RunStarted => _runStarted;

        public void SetShouldThrow(bool shouldThrow)
        {
            _shouldThrow = shouldThrow;
        }

        protected override void RunNow()
        {
            _runStarted.Set();
            _runCount++;

            if (_shouldThrow)
            {
                throw new InvalidOperationException("Test exception");
            }

            // Simulate some work
            while (CanContinue())
            {
                Thread.Sleep(10);
            }
        }

        public new bool CanContinue()
        {
            return base.CanContinue();
        }

        public new void StopOnNextCycle()
        {
            base.StopOnNextCycle();
        }
    }

    public class RunningComponentTests
    {
        [Fact]
        public void Start_StartsComponent()
        {
            var component = new TestRunningComponent();

            component.Start();

            // Wait for component to start
            Assert.True(component.RunStarted.Wait(TimeSpan.FromSeconds(5)));
            Assert.True(component.Running);
            Assert.True(component.RunCount > 0);

            component.Stop();
        }

        [Fact]
        public void Stop_StopsComponent()
        {
            var component = new TestRunningComponent();

            component.Start();
            Assert.True(component.RunStarted.Wait(TimeSpan.FromSeconds(5)));

            component.Stop();

            // Wait a bit for the component to stop
            Thread.Sleep(100);
            Assert.False(component.Running);
        }

        [Fact]
        public void Stop_AllowCompleteCycle_StopsGracefully()
        {
            var component = new TestRunningComponent();

            component.Start();
            Assert.True(component.RunStarted.Wait(TimeSpan.FromSeconds(5)));

            component.Stop(true); // Allow complete cycle

            // Wait for graceful shutdown
            Thread.Sleep(200);
            Assert.False(component.Running);
        }

        [Fact]
        public void Stop_DisallowCompleteCycle_StopsWithTimeout()
        {
            var component = new TestRunningComponent();

            component.Start();
            Assert.True(component.RunStarted.Wait(TimeSpan.FromSeconds(5)));

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            component.Stop(false); // Don't allow complete cycle
            stopwatch.Stop();

            // Should stop within reasonable time (not immediately due to graceful shutdown)
            Assert.True(stopwatch.ElapsedMilliseconds < 10000); // 10 seconds max
            Assert.False(component.Running);
        }

        [Fact]
        public void Start_MultipleCalls_OnlyStartsOnce()
        {
            var component = new TestRunningComponent();

            component.Start();
            component.Start();
            component.Start();

            Assert.True(component.RunStarted.Wait(TimeSpan.FromSeconds(5)));
            Assert.True(component.Running);

            component.Stop();
        }

        [Fact]
        public void Stop_MultipleCalls_OnlyStopsOnce()
        {
            var component = new TestRunningComponent();

            component.Start();
            Assert.True(component.RunStarted.Wait(TimeSpan.FromSeconds(5)));

            component.Stop();
            component.Stop();
            component.Stop();

            Thread.Sleep(100);
            Assert.False(component.Running);
        }

        [Fact]
        public void RunNow_Exception_StopsComponent()
        {
            var component = new TestRunningComponent();
            component.SetShouldThrow(true);

            component.Start();

            // Component should stop due to exception
            Thread.Sleep(200);
            Assert.False(component.Running);
        }

        [Fact]
        public void Running_Property_ThreadSafe()
        {
            var component = new TestRunningComponent();
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            // Start the component
            component.Start();
            Assert.True(component.RunStarted.Wait(TimeSpan.FromSeconds(5)));

            // Multiple threads reading the Running property
            var tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        for (int j = 0; j < 100; j++)
                        {
                            var running = component.Running;
                            // Should not throw
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
            }

            Task.WaitAll(tasks);

            // Should not have any exceptions
            Assert.Empty(exceptions);

            component.Stop();
        }

        [Fact]
        public void StartStop_ConcurrentAccess_ThreadSafe()
        {
            var component = new TestRunningComponent();
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            // Multiple threads trying to start/stop
            var tasks = new Task[20];
            for (int i = 0; i < 20; i++)
            {
                int threadId = i;
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        if (threadId % 2 == 0)
                        {
                            component.Start();
                        }
                        else
                        {
                            component.Stop();
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
            }

            Task.WaitAll(tasks);

            // Should not have any exceptions
            Assert.Empty(exceptions);

            // Final state should be consistent
            var finalRunning = component.Running;
            // Either running or stopped, but not in inconsistent state
        }

        [Fact]
        public void CanContinue_ReturnsCorrectValue()
        {
            var component = new TestRunningComponent();

            // Before starting, should return true (component is not stopped)
            Assert.True(component.CanContinue());

            component.Start();
            Assert.True(component.RunStarted.Wait(TimeSpan.FromSeconds(5)));

            // While running, should return true
            Assert.True(component.CanContinue());

            component.Stop();
            Thread.Sleep(100);

            // After stopping, should return false
            Assert.False(component.CanContinue());
        }

        [Fact]
        public void StopOnNextCycle_SetsFlag()
        {
            var component = new TestRunningComponent();

            component.Start();
            Assert.True(component.RunStarted.Wait(TimeSpan.FromSeconds(5)));

            component.StopOnNextCycle();

            // Component should stop on next cycle
            Thread.Sleep(200);
            Assert.False(component.Running);
        }
    }
}
