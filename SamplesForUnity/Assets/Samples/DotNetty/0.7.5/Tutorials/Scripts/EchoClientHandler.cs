using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Text;

namespace Echo.Client
{
    public class EchoClientHandler : ChannelHandlerAdapter
    {
        readonly IByteBuffer initialMessage;

        public EchoClientHandler()
        {
            this.initialMessage = Unpooled.Buffer(256);
            byte[] messageBytes = Encoding.UTF8.GetBytes("Hello world");
            this.initialMessage.WriteBytes(messageBytes);
        }

        public override void ChannelActive(IChannelHandlerContext context) => context.WriteAndFlushAsync(this.initialMessage);

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            UnityEngine.Debug.LogFormat("Received from server: {0}", byteBuffer.ToString(Encoding.UTF8));

            byteBuffer.Release();
            //context.WriteAsync(message);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            UnityEngine.Debug.LogFormat("Exception: {0}", exception);
            context.CloseAsync();
        }
    }
}