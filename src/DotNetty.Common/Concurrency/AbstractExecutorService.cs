// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Concurrency
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class AbstractExecutorService : IExecutorService
    {
        private const int POOL_CAPACITY = 128;
        private ActionTaskQueueNodePool actionTaskQueueNodeFactory = new ActionTaskQueueNodePool(POOL_CAPACITY);//Use object pool to optimize GC. by Clark

        private StateActionTaskQueueNodePool stateActionTaskQueueNodeFactory = new StateActionTaskQueueNodePool(POOL_CAPACITY);//Use object pool to optimize GC. by Clark

        private StateActionWithContextTaskQueueNodePool stateActionWithContextTaskQueueNodeFactory = new StateActionWithContextTaskQueueNodePool(POOL_CAPACITY);//Use object pool to optimize GC. by Clark

        /// <inheritdoc cref="IExecutorService"/>
        public abstract bool IsShutdown { get; }

        /// <inheritdoc cref="IExecutorService"/>
        public abstract bool IsTerminated { get; }

        /// <inheritdoc cref="IExecutorService"/>
        public Task<T> SubmitAsync<T>(Func<T> func) => this.SubmitAsync(func, CancellationToken.None);

        /// <inheritdoc cref="IExecutorService"/>
        public Task<T> SubmitAsync<T>(Func<T> func, CancellationToken cancellationToken)
        {
            var node = new FuncSubmitQueueNode<T>(func, cancellationToken);
            this.Execute(node);
            return node.Completion;
        }

        /// <inheritdoc cref="IExecutorService"/>
        public Task<T> SubmitAsync<T>(Func<object, T> func, object state) => this.SubmitAsync(func, state, CancellationToken.None);

        /// <inheritdoc cref="IExecutorService"/>
        public Task<T> SubmitAsync<T>(Func<object, T> func, object state, CancellationToken cancellationToken)
        {
            var node = new StateFuncSubmitQueueNode<T>(func, state, cancellationToken);
            this.Execute(node);
            return node.Completion;
        }

        /// <inheritdoc cref="IExecutorService"/>
        public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state) =>
            this.SubmitAsync(func, context, state, CancellationToken.None);

        /// <inheritdoc cref="IExecutorService"/>
        public Task<T> SubmitAsync<T>(
            Func<object, object, T> func,
            object context,
            object state,
            CancellationToken cancellationToken)
        {
            var node = new StateFuncWithContextSubmitQueueNode<T>(func, context, state, cancellationToken);
            this.Execute(node);
            return node.Completion;
        }

        /// <inheritdoc cref="IExecutor"/>
        public abstract void Execute(IRunnable task);

        /// <inheritdoc cref="IExecutor"/>
        public void Execute(Action<object> action, object state) => this.Execute(stateActionTaskQueueNodeFactory.Allocate(action, state));

        /// <inheritdoc cref="IExecutor"/>
        public void Execute(Action<object, object> action, object context, object state) => this.Execute(stateActionWithContextTaskQueueNodeFactory.Allocate(action, context, state));

        /// <inheritdoc cref="IExecutor"/>
        public void Execute(Action action) => this.Execute(actionTaskQueueNodeFactory.Allocate(action));

        #region Queuing data structures

        //Use object pool to optimize GC. by Clark
        sealed class ActionTaskQueueNodePool
        {
            private int capacity;
            private ConcurrentQueue<ActionTaskQueueNode> queue;
            public ActionTaskQueueNodePool(int capacity)
            {
                this.capacity = capacity;
                this.queue = new ConcurrentQueue<ActionTaskQueueNode>();
            }

            public ActionTaskQueueNode Allocate(Action action)
            {
                ActionTaskQueueNode task;
                if (!queue.TryDequeue(out task))
                    task = new ActionTaskQueueNode(this);
                task.action = action;
                return task;
            }

            public void Free(ActionTaskQueueNode task)
            {
                if (queue.Count >= capacity)
                    return;

                task.action = null;
                queue.Enqueue(task);
            }
        }

        sealed class ActionTaskQueueNode : IRunnable
        {
            private readonly ActionTaskQueueNodePool pool;
            public Action action;

            public ActionTaskQueueNode(ActionTaskQueueNodePool pool)
            {
                this.pool = pool;
            }

            //public ActionTaskQueueNode(Action action)
            //{
            //    this.action = action;
            //}

            public void Run()
            {
                try
                {
                    this.action();
                }
                finally
                {
                    this.pool.Free(this);
                }
            }
        }

        //Use object pool to optimize GC. by Clark
        sealed class StateActionTaskQueueNodePool
        {
            private int capacity;
            private ConcurrentQueue<StateActionTaskQueueNode> queue;
            public StateActionTaskQueueNodePool(int capacity)
            {
                this.capacity = capacity;
                this.queue = new ConcurrentQueue<StateActionTaskQueueNode>();
            }

            public StateActionTaskQueueNode Allocate(Action<object> action, object state)
            {
                StateActionTaskQueueNode task;
                if (!queue.TryDequeue(out task))
                    task = new StateActionTaskQueueNode(this);
                task.action = action;
                task.state = state;
                return task;
            }

            public void Free(StateActionTaskQueueNode task)
            {
                if (queue.Count >= capacity)
                    return;

                task.action = null;
                task.state = null;
                queue.Enqueue(task);
            }
        }

        sealed class StateActionTaskQueueNode : IRunnable
        {
            private readonly StateActionTaskQueueNodePool pool;
            public Action<object> action;
            public object state;

            public StateActionTaskQueueNode(StateActionTaskQueueNodePool pool)
            {
                this.pool = pool;
            }

            //public StateActionTaskQueueNode(Action<object> action, object state)
            //{
            //    this.action = action;
            //    this.state = state;
            //}

            public void Run()
            {
                try
                {
                    this.action(this.state);
                }
                finally
                {
                    this.pool.Free(this);
                }
            }
        }

        //Use object pool to optimize GC. by Clark
        sealed class StateActionWithContextTaskQueueNodePool
        {
            private int capacity;
            private ConcurrentQueue<StateActionWithContextTaskQueueNode> queue;
            public StateActionWithContextTaskQueueNodePool(int capacity)
            {
                this.capacity = capacity;
                this.queue = new ConcurrentQueue<StateActionWithContextTaskQueueNode>();
            }

            public StateActionWithContextTaskQueueNode Allocate(Action<object, object> action, object context, object state)
            {
                StateActionWithContextTaskQueueNode task;
                if (!queue.TryDequeue(out task))
                    task = new StateActionWithContextTaskQueueNode(this);
                task.action = action;
                task.context = context;
                task.state = state;
                return task;
            }

            public void Free(StateActionWithContextTaskQueueNode task)
            {
                if (queue.Count >= capacity)
                    return;

                task.action = null;
                task.context = null;
                task.state = null;
                queue.Enqueue(task);
            }
        }

        sealed class StateActionWithContextTaskQueueNode : IRunnable
        {
            private readonly StateActionWithContextTaskQueueNodePool pool;
            public Action<object, object> action;
            public object context;
            public object state;

            public StateActionWithContextTaskQueueNode(StateActionWithContextTaskQueueNodePool pool)
            {
                this.pool = pool;
            }
            //public StateActionWithContextTaskQueueNode(Action<object, object> action, object context, object state)
            //{
            //    this.action = action;
            //    this.context = context;
            //    this.state = state;
            //}

            public void Run()
            {
                try
                {
                    this.action(this.context, this.state);
                }
                finally
                {
                    this.pool.Free(this);
                }
            }
        }

        abstract class FuncQueueNodeBase<T> : IRunnable
        {
            readonly TaskCompletionSource<T> promise;
            readonly CancellationToken cancellationToken;

            protected FuncQueueNodeBase(TaskCompletionSource<T> promise, CancellationToken cancellationToken)
            {
                this.promise = promise;
                this.cancellationToken = cancellationToken;
            }

            public Task<T> Completion => this.promise.Task;

            public void Run()
            {
                if (this.cancellationToken.IsCancellationRequested)
                {
                    this.promise.TrySetCanceled();
                    return;
                }

                try
                {
                    T result = this.Call();
                    this.promise.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    // todo: handle fatal
                    this.promise.TrySetException(ex);
                }
            }

            protected abstract T Call();
        }

        sealed class FuncSubmitQueueNode<T> : FuncQueueNodeBase<T>
        {
            readonly Func<T> func;

            public FuncSubmitQueueNode(Func<T> func, CancellationToken cancellationToken)
                : base(new TaskCompletionSource<T>(), cancellationToken)
            {
                this.func = func;
            }

            protected override T Call() => this.func();
        }

        sealed class StateFuncSubmitQueueNode<T> : FuncQueueNodeBase<T>
        {
            readonly Func<object, T> func;

            public StateFuncSubmitQueueNode(Func<object, T> func, object state, CancellationToken cancellationToken)
                : base(new TaskCompletionSource<T>(state), cancellationToken)
            {
                this.func = func;
            }

            protected override T Call() => this.func(this.Completion.AsyncState);
        }

        sealed class StateFuncWithContextSubmitQueueNode<T> : FuncQueueNodeBase<T>
        {
            readonly Func<object, object, T> func;
            readonly object context;

            public StateFuncWithContextSubmitQueueNode(
                Func<object, object, T> func,
                object context,
                object state,
                CancellationToken cancellationToken)
                : base(new TaskCompletionSource<T>(state), cancellationToken)
            {
                this.func = func;
                this.context = context;
            }

            protected override T Call() => this.func(this.context, this.Completion.AsyncState);
        }

        #endregion
    }
}