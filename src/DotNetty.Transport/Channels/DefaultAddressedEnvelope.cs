// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Transport.Channels
{
    using System.Diagnostics.Contracts;
    using System.Net;
    using System.Threading;
    using DotNetty.Common;
    using DotNetty.Common.Utilities;

    public class DefaultAddressedEnvelope<T> : IAddressedEnvelope<T>
    {
        static readonly ThreadLocalPool<DefaultAddressedEnvelope<T>> Recycler = new ThreadLocalPool<DefaultAddressedEnvelope<T>>(h => new DefaultAddressedEnvelope<T>(h));

        public static DefaultAddressedEnvelope<T> NewInstance(T content, EndPoint recipient)
        {
            return NewInstance(content, null, recipient);
        }

        public static DefaultAddressedEnvelope<T> NewInstance(T content, EndPoint sender, EndPoint recipient)
        {
            DefaultAddressedEnvelope<T> envelope = Recycler.Take();
            envelope.Init(content, sender, recipient);
            return envelope;
        }

        readonly ThreadLocalPool.Handle recyclerHandle;
        volatile int referenceCount = 1;
        protected DefaultAddressedEnvelope(ThreadLocalPool.Handle handle)
        {
            this.recyclerHandle = handle;
        }

        protected void Init(T content, EndPoint sender, EndPoint recipient)
        {
            Contract.Requires(content != null);
            Contract.Requires(sender != null || recipient != null);

            this.Content = content;
            this.Sender = sender;
            this.Recipient = recipient;
            this.referenceCount = 1;
        }

        public DefaultAddressedEnvelope(T content, EndPoint recipient)
            : this(content, null, recipient)
        {
        }

        public DefaultAddressedEnvelope(T content, EndPoint sender, EndPoint recipient)
        {
            Contract.Requires(content != null);
            Contract.Requires(sender != null || recipient != null);

            this.Content = content;
            this.Sender = sender;
            this.Recipient = recipient;
        }

        public T Content { get; private set; }

        public EndPoint Sender { get; private set; }

        public EndPoint Recipient { get; private set; }

        public virtual int ReferenceCount => this.referenceCount;
        //public int ReferenceCount
        //{
        //    get
        //    {
        //        var counted = this.Content as IReferenceCounted;
        //        return counted?.ReferenceCount ?? 1;
        //    }
        //}

        protected internal void SetReferenceCount(int value) => this.referenceCount = value;

        public virtual IReferenceCounted Retain() => this.Retain0(1);

        public virtual IReferenceCounted Retain(int increment)
        {
            Contract.Requires(increment > 0);

            return this.Retain0(increment);
        }

        IReferenceCounted Retain0(int increment)
        {
            while (true)
            {
                int refCnt = this.referenceCount;
                int nextCnt = refCnt + increment;

                // Ensure we not resurrect (which means the refCnt was 0) and also that we encountered an overflow.
                if (nextCnt <= increment)
                {
                    throw new IllegalReferenceCountException(refCnt, increment);
                }
                if (Interlocked.CompareExchange(ref this.referenceCount, refCnt + increment, refCnt) == refCnt)
                {
                    break;
                }
            }
            return this;
        }

        //public virtual IReferenceCounted Retain()
        //{
        //    ReferenceCountUtil.Retain(this.Content);
        //    return this;
        //}

        //public virtual IReferenceCounted Retain(int increment)
        //{
        //    ReferenceCountUtil.Retain(this.Content, increment);
        //    return this;
        //}

        public virtual IReferenceCounted Touch() => this;

        //public virtual IReferenceCounted Touch()
        //{
        //    ReferenceCountUtil.Touch(this.Content);
        //    return this;
        //}

        public virtual IReferenceCounted Touch(object hint) => this;

        //public virtual IReferenceCounted Touch(object hint)
        //{
        //    ReferenceCountUtil.Touch(this.Content, hint);
        //    return this;
        //}

        public virtual bool Release() => this.Release0(1);

        public virtual bool Release(int decrement)
        {
            Contract.Requires(decrement > 0);

            return this.Release0(decrement);
        }

        bool Release0(int decrement)
        {
            while (true)
            {
                int refCnt = this.ReferenceCount;
                if (refCnt < decrement)
                {
                    throw new IllegalReferenceCountException(refCnt, -decrement);
                }

                if (Interlocked.CompareExchange(ref this.referenceCount, refCnt - decrement, refCnt) == refCnt)
                {
                    if (refCnt == decrement)
                    {
                        this.Deallocate();
                        return true;
                    }

                    return false;
                }
            }
        }

        protected internal virtual void Deallocate()
        {
            if (this.Content != null)
                ReferenceCountUtil.SafeRelease(this.Content);

            this.Content = default(T);
            this.Sender = null;
            this.Recipient = null;
            if (this.recyclerHandle != null)
                this.recyclerHandle.Release(this);
        }

        //public bool Release() => ReferenceCountUtil.Release(this.Content);

        //public bool Release(int decrement) => ReferenceCountUtil.Release(this.Content, decrement);

        public override string ToString() => $"DefaultAddressedEnvelope<{typeof(T)}>"
            + (this.Sender != null
                ? $"({this.Sender} => {this.Recipient}, {this.Content})"
                : $"(=> {this.Recipient}, {this.Content})");
    }
}