// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Concurrency
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Reuse ReusableScheduledTask to optimize GC.
    /// </summary>
    public class ReusableScheduledTask : IScheduledRunnable
    {
        protected TaskCompletionSource currSource;
        protected TaskCompletionSource backSource;
        protected AbstractScheduledEventExecutor Executor;
        readonly Action action;
        public ReusableScheduledTask(IRunnable action) : this(action.Run)
        {
        }

        public ReusableScheduledTask(Action action)
        {
            this.action = action;
            this.currSource = new TaskCompletionSource();
            this.currSource.TryComplete();
            this.backSource = new TaskCompletionSource();
        }

        internal ReusableScheduledTask Reset(AbstractScheduledEventExecutor executor, PreciseTimeSpan deadline)
        {
            if (!currSource.IsExpired)
                throw new InvalidOperationException();

            this.Executor = executor;
            this.Deadline = deadline;
            var source = this.backSource;
            this.backSource = currSource;
            this.currSource = source;
            this.currSource.Reset();
            return this;
        }

        public PreciseTimeSpan Deadline { get; protected set; }

        public bool IsCompleted => currSource.IsCompleted;

        public bool Cancel()
        {
            if (this.Executor == null)
                return false;

            if (!currSource.TrySetCanceled())
                return false;

            this.Executor.RemoveScheduled(this);
            return true;
        }

        public Task Completion => this.currSource.AsTask();
        public TaskAwaiter GetAwaiter() => this.Completion.GetAwaiter();

        int IComparable<IScheduledRunnable>.CompareTo(IScheduledRunnable other)
        {
            Contract.Requires(other != null);

            return this.Deadline.CompareTo(other.Deadline);
        }

        public virtual void Run()
        {
            PreciseTimeSpan time = this.Deadline - PreciseTimeSpan.FromStart;
            if (time.Ticks > 0)
                return;

            var source = this.currSource;
            if (source.TrySetUncancelable())
            {
                try
                {
                    this.action();
                    source.TryComplete();
                }
                catch (Exception ex)
                {
                    source.TrySetException(ex);
                }
            }
        }

        protected class TaskCompletionSource
        {
            const int CancellationProhibited = 1;
            const int CancellationRequested = 1 << 1;

            private static Task CANCELED_TASK = Task.FromException(new OperationCanceledException());
            private static Task COMPLETED_TASK = Task.CompletedTask;
            private int volatileCancellationState;
            private Exception exception;
            private bool canceled = false;
            private int done = 0;
            private bool running = false;
            private TaskCompletionSource<int> taskSource;
            private Task exceptionTask;
            private object _lock = new object();
            public TaskCompletionSource()
            {
            }

            public void Reset()
            {
                lock (_lock)
                {
                    this.volatileCancellationState = 0;
                    this.exception = null;
                    this.canceled = false;
                    this.done = 0;
                    this.exceptionTask = null;
                    this.taskSource = null;
                    this.running = false;
                }
            }

            public bool IsExpired => done == 1 || running;

            public bool IsCompleted => done == 1;

            public bool TryComplete()
            {
                if (Interlocked.CompareExchange(ref done, 1, 0) == 0)
                {
                    if (taskSource != null)
                        return taskSource.TrySetResult(0);
                    return true;
                }
                return false;
            }

            public bool TrySetCanceled()
            {
                if (Interlocked.CompareExchange(ref done, 1, 0) == 0)
                {
                    if (!AtomicCancellationStateUpdate(CancellationRequested, CancellationProhibited))
                        return false;

                    if (taskSource != null)
                        taskSource.TrySetCanceled();

                    this.canceled = true;
                    return true;
                }
                return false;
            }

            public bool TrySetException(Exception exception)
            {
                if (Interlocked.CompareExchange(ref done, 1, 0) == 0)
                {
                    this.exception = exception;
                    if (taskSource != null)
                        return taskSource.TrySetException(exception);
                    return true;
                }
                return false;
            }

            public bool TrySetUncancelable()
            {
                if (this.AtomicCancellationStateUpdate(CancellationProhibited, CancellationRequested))
                {
                    this.running = true;
                    return true;
                }
                return false;
            }

            public Task AsTask()
            {
                lock (_lock)
                {
                    if (done == 1)
                    {
                        if (canceled)
                            return CANCELED_TASK;

                        if (exception != null)
                        {
                            if (exceptionTask == null)
                                exceptionTask = Task.FromException(exception);
                            return exceptionTask;
                        }

                        return COMPLETED_TASK;
                    }

                    if (taskSource == null)
                        taskSource = new TaskCompletionSource<int>();

                    return taskSource.Task;
                }
            }

            bool AtomicCancellationStateUpdate(int newBits, int illegalBits)
            {
                int cancellationState = Volatile.Read(ref this.volatileCancellationState);
                int oldCancellationState;
                do
                {
                    oldCancellationState = cancellationState;
                    if ((cancellationState & illegalBits) != 0)
                    {
                        return false;
                    }
                    cancellationState = Interlocked.CompareExchange(ref this.volatileCancellationState, cancellationState | newBits, cancellationState);
                }
                while (cancellationState != oldCancellationState);

                return true;
            }
        }
    }
}
