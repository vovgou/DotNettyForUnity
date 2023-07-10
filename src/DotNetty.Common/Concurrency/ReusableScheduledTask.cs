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
        const int CancellationProhibited = 1;
        const int CancellationRequested = 1 << 1;

        //#if NETSTANDARD2_1
        //https://blog.51cto.com/u_10006690/2726859
        //#else
        protected TaskCompletionSource Promise;
//#endif
        protected AbstractScheduledEventExecutor Executor;
        int volatileCancellationState;
        readonly Action action;
        public ReusableScheduledTask(IRunnable action) : this(action.Run)
        {
        }

        public ReusableScheduledTask(Action action)
        {
            this.action = action;
        }

        internal ReusableScheduledTask Reset(AbstractScheduledEventExecutor executor, PreciseTimeSpan deadline)
        {
            this.volatileCancellationState = 0;
            this.Executor = executor;
            this.Deadline = deadline;
            this.Promise = new TaskCompletionSource();
            return this;
        }

        public PreciseTimeSpan Deadline { get; protected set; }

        public bool Cancel()
        {
            if (this.Executor == null)
                return false;

            if (!this.AtomicCancellationStateUpdate(CancellationRequested, CancellationProhibited))
            {
                return false;
            }

            bool canceled = this.Promise.TrySetCanceled();
            if (canceled)
            {
                this.Executor.RemoveScheduled(this);
            }
            return canceled;
        }

        public Task Completion => this.Promise.Task;

        public TaskAwaiter GetAwaiter() => this.Completion.GetAwaiter();

        int IComparable<IScheduledRunnable>.CompareTo(IScheduledRunnable other)
        {
            Contract.Requires(other != null);

            return this.Deadline.CompareTo(other.Deadline);
        }

        public virtual void Run()
        {
            if (this.TrySetUncancelable())
            {
                var promise = this.Promise;
                try
                {
                    this.action();
                    promise.TryComplete();
                }
                catch (Exception ex)
                {
                    // todo: check for fatal
                    promise.TrySetException(ex);
                }
            }
        }

        bool TrySetUncancelable() => this.AtomicCancellationStateUpdate(CancellationProhibited, CancellationRequested);

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
