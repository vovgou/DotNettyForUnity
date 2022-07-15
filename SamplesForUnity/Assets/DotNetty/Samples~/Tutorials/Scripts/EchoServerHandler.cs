using System;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace Echo.Server
{
    public class EchoServerHandler : ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = message as IByteBuffer;
            if (buffer != null)
            {
                UnityEngine.Debug.LogFormat("Received from client: {0}", buffer.ToString(Encoding.UTF8));
            }
            context.WriteAsync(message);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            UnityEngine.Debug.LogFormat("Exception:{}", exception);
            context.CloseAsync();
        }
    }
}