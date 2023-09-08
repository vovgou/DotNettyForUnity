// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Transport.Channels.Sockets
{
    using System.Net;
    using DotNetty.Buffers;
    using DotNetty.Common;

    public class DatagramPacket : DefaultAddressedEnvelope<IByteBuffer>, IByteBufferHolder
    {
        static readonly ThreadLocalPool<DatagramPacket> Recycler = new ThreadLocalPool<DatagramPacket>(h => new DatagramPacket(h));

        public static new DatagramPacket NewInstance(IByteBuffer content, EndPoint recipient)
        {
            return NewInstance(content, null, recipient);
        }

        public static new DatagramPacket NewInstance(IByteBuffer content, EndPoint sender, EndPoint recipient)
        {
            DatagramPacket packet = Recycler.Take();
            packet.Init(content, sender, recipient);
            return packet;
        }

        DatagramPacket(ThreadLocalPool.Handle handle) : base(handle)
        {
        }

        public DatagramPacket(IByteBuffer message, EndPoint recipient)
            : base(message, recipient)
        {
        }

        public DatagramPacket(IByteBuffer message, EndPoint sender, EndPoint recipient)
            : base(message, sender, recipient)
        {
        }

        public virtual IByteBufferHolder Copy() => NewInstance(this.Content.Copy(), this.Sender, this.Recipient);

        public virtual IByteBufferHolder Duplicate() => new DuplicateDatagramPacket(this, this.Content.Duplicate(), this.Sender, this.Recipient);

        public virtual IByteBufferHolder RetainedDuplicate() => this.Replace(this.Content.RetainedDuplicate());

        public virtual IByteBufferHolder Replace(IByteBuffer content) => NewInstance(content, this.Recipient, this.Sender);

        sealed class DuplicateDatagramPacket : DatagramPacket
        {
            private readonly IReferenceCounted referenceCountDelegate;
            public DuplicateDatagramPacket(IReferenceCounted referenceCountDelegate, IByteBuffer message, EndPoint sender, EndPoint recipient) : base(message, sender, recipient)
            {
                this.referenceCountDelegate = referenceCountDelegate;
            }

            public override IByteBufferHolder Duplicate() => new DuplicateDatagramPacket(referenceCountDelegate, this.Content.Duplicate(), this.Sender, this.Recipient);

            public override int ReferenceCount => this.referenceCountDelegate.ReferenceCount;

            public override IReferenceCounted Retain()
            {
                referenceCountDelegate.Retain();
                return this;
            }

            public override IReferenceCounted Retain(int increment)
            {
                referenceCountDelegate.Retain(increment);
                return this;
            }

            public override IReferenceCounted Touch()
            {
                referenceCountDelegate.Touch();
                return this;
            }

            public override IReferenceCounted Touch(object hint)
            {
                referenceCountDelegate.Touch(hint);
                return this;
            }

            public override bool Release() => referenceCountDelegate.Release();

            public override bool Release(int decrement) => referenceCountDelegate.Release(decrement);
        }
    }
}