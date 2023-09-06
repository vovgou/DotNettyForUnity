using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Text;

namespace Echo.Client
{
    public class EchoUDPClientHandler : ChannelHandlerAdapter
    {
        readonly IByteBuffer initialMessage;

        public EchoUDPClientHandler()
        {
            this.initialMessage = Unpooled.Buffer(256);
            byte[] messageBytes = Encoding.UTF8.GetBytes("Hello world");
            this.initialMessage.WriteBytes(messageBytes);
        }

        public override void ChannelActive(IChannelHandlerContext context) => context.WriteAndFlushAsync(this.initialMessage).ConfigureAwait(false);

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var packet = message as DatagramPacket;
            UnityEngine.Debug.LogFormat("Received from server: {0}", packet.Content.ToString(Encoding.UTF8));
            packet.Release();
            //context.WriteAsync(message);
        }

        //public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();



        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            UnityEngine.Debug.LogFormat("Exception: {0}", exception);
            context.CloseAsync();
        }
    }
}
