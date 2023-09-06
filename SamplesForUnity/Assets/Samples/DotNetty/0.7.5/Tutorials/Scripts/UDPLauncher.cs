using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Unity;
using Echo.Client;
using Echo.Server;
using System;
using System.Text;
using UnityEngine;

public class UDPLauncher : MonoBehaviour
{
    EchoUDPServer server;
    //EchoClient client;
    EchoUDPClient client;
    IChannel channel;
    //private string host = "127.0.0.1";
    //private int port = 8007;
    //private string host = "127.0.0.1";
    private string host = "192.168.1.253";
    private int port = 8800;
    void Start()
    {
        UnityLoggerFactory.Default.Level = Level.DEBUG;

        server = new EchoUDPServer(port);
        client = new EchoUDPClient();

        Environment.SetEnvironmentVariable("io.netty.noPreferDirect", "false");
    }


    void OnGUI()
    {
        int x = 50;
        int y = 50;
        int width = 200;
        int height = 100;
        int i = 0;
        int padding = 10;

        GUI.skin.button.fontSize = 25;

        if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), server.Started ? "Stop Server" : "Start Server"))
        {
            if (server.Started)
                _ = server.Stop();
            else
                _ = server.Start();
        }

        if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), client.Connected ? "Disconnect" : "Connect"))
        {
            if (client.Connected)
                _ = client.DisconnectAsync();
            else
                Connect();
        }

        if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), "Send Message"))
        {
            IByteBuffer message = Unpooled.Buffer(256);
            message.WriteBytes(Encoding.UTF8.GetBytes("this is a request."));
            this.channel.WriteAndFlushAsync(message);
        }
    }

    private async void Connect()
    {
        channel = await client.ConnectAsync(host, port);
    }

    private void OnDestroy()
    {
        if (server != null && server.Started)
            _ = server.Stop();

        if (client != null && client.Connected)
            _ = client.DisconnectAsync();
    }
}