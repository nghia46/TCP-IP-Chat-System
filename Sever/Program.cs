using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sever;

abstract class Program
{
    private static List<TcpClient?>? _clients = new();
    private static TcpListener? _server;

    static void Main(string[] args)
    {
        var iPAddress = IPAddress.Parse("127.0.0.1");
        const int port = 13000;
        _server = new TcpListener(iPAddress, port);
        _server.Start();
        Console.WriteLine($"Chat server started on ip:{iPAddress} port:{port})");

        // Start a thread to read server messages
        var serverThread = new Thread(ReadServerInput);
        serverThread.Start();

        while (true)
        {
            var client = _server.AcceptTcpClient();
            _clients?.Add(client);
            Console.WriteLine("Client connected!");

            var clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private static void HandleClient(TcpClient? client)
    {
        var stream = client?.GetStream();
        var buffer = new byte[1024];

        try
        {
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                var message = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received from client: {message}");
                BroadcastMessage("Client: " + message, client);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception: {e}");
        }
        finally
        {
            _clients?.Remove(client);
            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }

    private static void BroadcastMessage(string message, TcpClient? sender)
    {
        var msgBuffer = Encoding.UTF32.GetBytes(message);
            
        foreach (var client in _clients)
        {
            if (client == sender) continue;
            var stream = client?.GetStream();
            stream?.Write(msgBuffer, 0, msgBuffer.Length);
        }
    }

    private static void ReadServerInput()
    {
        while (true)
        {
            var serverMessage = Console.ReadLine();
            BroadcastMessage("Server: " + serverMessage, null); // null for sender means broadcast
        }
    }
}