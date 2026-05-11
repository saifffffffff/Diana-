using System;
using System.Collections.Generic;
using System.Text;

namespace Diana.Core.Dtos;

public class ClientRequest
{
    public string Content { get; set; }

    public string Username { get; set; }

    public string RoomKey { get; set; }

    public string RoomName { get; set; }

    public string ImageBase64 { get; set; }

    public MessageType Type { get; set; }

    public static ClientRequest LeaveRoom(string roomKey, string username) =>
        new ClientRequest { Type = MessageType.LeaveRoom, RoomKey = roomKey, Username = username };

    public static ClientRequest JoinRoom(string roomKey, string username) =>
        new ClientRequest { Type = MessageType.JoinRoom, RoomKey = roomKey, Username = username };

    public static ClientRequest SendMessage(string roomKey, string username, string content, string imageBase64 = null) =>
        new ClientRequest { Type = MessageType.SendMessage, Username = username, Content = content, RoomKey = roomKey, ImageBase64 = imageBase64 };

    public static ClientRequest CreateRoom(string roomKey, string roomName, string username, string content) =>
        new ClientRequest { Type = MessageType.CreateRoom, Content = content, RoomKey = roomKey, RoomName = roomName, Username = username };

    public static ClientRequest GetRoomList() => new ClientRequest { Type = MessageType.GetRoomList };
}

