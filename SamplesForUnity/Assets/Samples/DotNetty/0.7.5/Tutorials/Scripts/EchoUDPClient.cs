using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Echo.Client
{
    public class EchoUDPClient
    {
        private bool isSsl = false;
        private IChannel channel;

        public bool Connected { get; private set; }
        public async Task<IChannel> ConnectAsync(string host, int port)
        {
            var group = new MultithreadEventLoopGroup(1);
            X509Certificate2 cert = null;
            string targetHost = null;
            if (isSsl)
            {
                //cert = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
                //targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
            }
            try
            {
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    //.Channel<SocketDatagramChannel>()
                    .ChannelFactory(()=>new SocketDatagramChannel(AddressFamily.InterNetwork))
                    .Handler(new ActionChannelInitializer<IChannel>(c =>
                    {
                        IChannelPipeline pipeline = c.Pipeline;

                        if (cert != null)
                        {
                            pipeline.AddLast("tls", new TlsHandler(stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true), new ClientTlsSettings(targetHost)));
                        }
                        pipeline.AddLast(new LoggingHandler("CLIENT-CONN"));
                        //pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        //pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                        pipeline.AddLast("echo", new EchoUDPClientHandler());
                    }));

                channel = await bootstrap.ConnectAsync(host, port).ConfigureAwait(false);
                this.Connected = true;
                _ = channel.CloseCompletion.ContinueWith(async (t) =>
                {
                    if (group != null)
                        await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                });
                return channel;
            }
            catch (Exception)
            {
                try
                {
                    if (channel != null)
                        await channel.CloseAsync().ConfigureAwait(false);
                }
                catch (Exception) { }

                if (group != null)
                    await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                this.Connected = false;
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            if (this.channel != null)
                await channel.DisconnectAsync().ConfigureAwait(false);
            this.Connected = false;
        }
    }
}
