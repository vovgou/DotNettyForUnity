// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Buffers
{
    using DotNetty.Common;
    sealed class PooledCompositeByteBuffer : CompositeByteBuffer
    {
        static readonly ThreadLocalPool<PooledCompositeByteBuffer> Recycler = new ThreadLocalPool<PooledCompositeByteBuffer>(handle => new PooledCompositeByteBuffer(handle));

        internal static PooledCompositeByteBuffer NewInstance(IByteBufferAllocator allocator, bool direct, int maxNumComponents)
        {
            PooledCompositeByteBuffer buffer = Recycler.Take();
            buffer.Init(allocator, direct, maxNumComponents);
            return buffer;
        }

        PooledCompositeByteBuffer(ThreadLocalPool.Handle handle)
        : base(handle)
        {
        }

        public override IByteBuffer RetainedSlice(int index, int length) => PooledSlicedByteBuffer.NewInstance(this, this, index, length);

        public override IByteBuffer RetainedDuplicate() => PooledDuplicatedByteBuffer.NewInstance(this, this, this.ReaderIndex, this.WriterIndex);
    }
}
