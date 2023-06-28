using System;

namespace DotNetty.Common.Concurrency
{
    sealed class RepeatedScheduledTask : ScheduledTask
    {
        readonly object action;
        readonly bool fixedRate;
        public RepeatedScheduledTask(AbstractScheduledEventExecutor executor, IRunnable action, PreciseTimeSpan deadline, PreciseTimeSpan period, bool fixedRate)
            : base(executor, deadline, new TaskCompletionSource())
        {
            if (period.Ticks == 0)
                throw new ArgumentException("period: 0 (expected: != 0)");

            this.Period = period;
            this.fixedRate = fixedRate;
            this.action = action;
        }

        public RepeatedScheduledTask(AbstractScheduledEventExecutor executor, Action action, PreciseTimeSpan deadline, PreciseTimeSpan period, bool fixedRate)
            : base(executor, deadline, new TaskCompletionSource())
        {
            if (period.Ticks == 0)
                throw new ArgumentException("period: 0 (expected: != 0)");

            this.Period = period;
            this.fixedRate = fixedRate;
            this.action = action;
        }

        public PreciseTimeSpan Period { get; }

        protected override void Execute()
        {
            if (action is Action a)
                a();
            else if (action is IRunnable runnable)
                runnable.Run();

        }

        public override void Run()
        {
            try
            {
                this.Execute();
                //this.Promise.TryComplete();
                if (!Executor.IsShutdown)
                {
                    if (fixedRate)
                        this.Deadline = PreciseTimeSpan.FromTicks(Deadline.Ticks + Period.Ticks);
                    else
                        this.Deadline = PreciseTimeSpan.Deadline(this.Period);
                    this.Executor.Schedule(this);
                }
                else
                {
                    this.Promise.TryComplete();
                }
            }
            catch (Exception ex)
            {
                // todo: check for fatal
                this.Promise.TrySetException(ex);
            }
        }

    }
}
