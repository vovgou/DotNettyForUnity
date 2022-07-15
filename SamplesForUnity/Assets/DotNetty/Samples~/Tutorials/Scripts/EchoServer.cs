using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace Echo.Server
{
    public class EchoServer
    {
        private bool isSsl = false;
        private int port = 8007;
        private IChannel boundChannel;

        public EchoServer() : this(8007)
        {
        }

        public EchoServer(int port)
        {
            this.port = port;
        }

        public bool Started { get; private set; }

        public async Task Start()
        {
            IEventLoopGroup bossGroup = new MultithreadEventLoopGroup(1);
            IEventLoopGroup workerGroup = new MultithreadEventLoopGroup();
            try
            {
                X509Certificate2 tlsCertificate = null;
                if (isSsl)
                {
                    //tlsCertificate = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
                }

                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup).Channel<TcpServerSocketChannel>();

                bootstrap
                    .Option(ChannelOption.SoBacklog, 100)
                    .Handler(new LoggingHandler("SRV-LSTN"))
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        if (tlsCertificate != null)
                        {
                            pipeline.AddLast("tls", TlsHandler.Server(tlsCertificate));
                        }
                        pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                        pipeline.AddLast("echo", new EchoServerHandler());
                    }));

                this.boundChannel = await bootstrap.BindAsync(port);
                this.Started = true;
                _ = boundChannel.CloseCompletion.ContinueWith(async (t) =>
                      {
                          await Task.WhenAll(bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)), workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
                      });
            }
            catch (Exception)
            {
                if (boundChannel != null)
                    await boundChannel.CloseAsync();

                await Task.WhenAll(bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)), workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
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
