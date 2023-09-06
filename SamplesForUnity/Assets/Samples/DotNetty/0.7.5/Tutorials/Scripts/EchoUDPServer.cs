using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Echo.Server
{
    public class EchoUDPServer
    {
        private bool isSsl = false;
        private int port = 8007;
        private IChannel boundChannel;

        public EchoUDPServer() : this(8007)
        {
        }

        public EchoUDPServer(int port)
        {
            this.port = port;
        }

        public bool Started { get; private set; }

        public async Task Start()
        {
            IEventLoopGroup bossGroup = new MultithreadEventLoopGroup(1);
            try
            {
   

                var bootstrap = new Bootstrap();
                bootstrap.Group(bossGroup);//.Channel<SocketDatagramChannel>();
                bootstrap.ChannelFactory(() => new SocketDatagramChannel(AddressFamily.InterNetwork));
                bootstrap
                    .Option(ChannelOption.SoBacklog, 100)
                    .Handler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                        //pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        //pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                        pipeline.AddLast("echo", new EchoUDPServerHandler());
                    }));

                this.boundChannel = await bootstrap.BindAsync(port);
                this.Started = true;
                _ = boundChannel.CloseCompletion.ContinueWith(async (t) =>
                {
                    await bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
                });
            }
            catch (Exception)
            {
                if (boundChannel != null)
                    await boundChannel.CloseAsync();

                await bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
                throw;
            }
        }

        public async Task Stop()
        {
            if (boundChannel != null)
                await boundChannel.CloseAsync();

            this.Started = false;
        }
    }
}
