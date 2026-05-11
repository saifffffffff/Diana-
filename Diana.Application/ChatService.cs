using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Diana.Core.Interfaces;
using Diana.Core.Dtos;

namespace Diana.Application;

public class ChatService : IChatService
{
    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private StreamWriter? _writer;

    public bool IsConnected => _tcp?.Connected ?? false;

    public event Action<ServerResponse>? OnMessageReceived;

    public void Connect(string ipAddress, int port)
    {
        try
        {
            _tcp = new TcpClient(ipAddress, port);
            _stream = _tcp.GetStream();
            _writer = new StreamWriter(_stream) { AutoFlush = true };

            var RecieveThread = new Thread(Recieve) { IsBackground = true };
            RecieveThread.Start();

        }
        catch
        {
            Disconnect();
            throw new Exception("Failed to connect to the server.");
        }
        
    }

    public void Disconnect()
    {
        if (!IsConnected) return;
        _tcp.Dispose();
    }

    public void Recieve()
    {

        StreamReader reader = new StreamReader(_stream!);
        string line;
        try
        {
            while (  ( line =  reader.ReadLine() ) != null )
            {

                if (string.IsNullOrWhiteSpace(line)) continue;

                var msg = JsonSerializer.Deserialize<ServerResponse>(line);

                OnMessageReceived?.Invoke(msg!);
            
            }
        }
        catch  (Exception ex) 
        {
            throw ex;
        }
        finally
        {
            reader.Close();
            Disconnect();
        }
    }

    public async Task SendAsync(ClientRequest msg)
    {
        if (!IsConnected) return;

        var json = JsonSerializer.Serialize(msg);
        
        await _writer!.WriteLineAsync(json);
    }
}
