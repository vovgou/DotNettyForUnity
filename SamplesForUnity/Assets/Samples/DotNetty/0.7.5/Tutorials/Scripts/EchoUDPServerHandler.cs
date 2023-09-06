using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Text;

namespace Echo.Server
{
    public class EchoUDPServerHandler : ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var packet = message as DatagramPacket;
            UnityEngine.Debug.LogFormat("Received from client: {0}", packet.Content.ToString(Encoding.UTF8));

            context.WriteAndFlushAsync(new DatagramPacket((IByteBuffer)packet.Content.Retain(), packet.Recipient, packet.Sender));
            if (packet != null)
                packet.Release();
        }

        //public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            UnityEngine.Debug.LogFormat("Exception:{}", exception);
            context.CloseAsync();
        }
    }
}
