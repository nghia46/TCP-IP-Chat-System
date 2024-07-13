using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    private static List<TcpClient> clients = new List<TcpClient>();
    private static TcpListener server;

    static void Main(string[] args)
    {
        IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
        int port = 13000;
        server = new TcpListener(iPAddress, port);
        server.Start();
        Console.WriteLine($"Chat server started on ip:{iPAddress} port:{port})");

        // Start a thread to read server messages
        Thread serverThread = new Thread(ReadServerInput);
        serverThread.Start();

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            clients.Add(client);
            Console.WriteLine("Client connected!");

            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.Unicode.GetString(buffer, 0, bytesRead);
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
            clients.Remove(client);
            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }

    private static void BroadcastMessage(string message, TcpClient sender)
    {
        byte[] msgBuffer = Encoding.UTF32.GetBytes(message);

        foreach (var client in clients)
        {
            if (client != sender)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(msgBuffer, 0, msgBuffer.Length);
            }
        }
    }

    private static void ReadServerInput()
    {
        string serverMessage;
        while (true)
        {
            serverMessage = Console.ReadLine();
            BroadcastMessage("Server: " + serverMessage, null); // null for sender means broadcast
        }
    }
}
