using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    private static TcpClient client;
    private static NetworkStream stream;

    static void Main(string[] args)
    {
        string serverIp = "127.0.0.1"; // Change to your server IP if necessary
        int port = 13000;

        client = new TcpClient(serverIp, port);
        stream = client.GetStream();

        Console.WriteLine("Connected to the chat server!");

        // Start a thread to read messages from the server
        Thread readThread = new Thread(ReadMessages);
        readThread.Start();

        string message;
        while (true)
        {
            message = Console.ReadLine();
            SendMessage(message);
        }
    }

    private static void ReadMessages()
    {
        byte[] buffer = new byte[1024];
        int bytesRead;

        while (true)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(receivedMessage);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading from server: {e.Message}");
                break;
            }
        }
    }

    private static void SendMessage(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            byte[] msgBuffer = Encoding.UTF32.GetBytes(message);
            stream.Write(msgBuffer, 0, msgBuffer.Length);
        }
    }
}
