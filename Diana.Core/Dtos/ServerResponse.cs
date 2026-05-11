using System;
using System.Collections.Generic;
using System.Text;

namespace Diana.Core.Dtos;

public class ServerResponse
{
    public string Content { get; set; }

    public string Username { get; set; }

    public string RoomKey { get; set; }

    public string RoomName { get; set; }

    public string ImageBase64 { get; set; }

    public string RoomOwner { get; private set; }


    public DateTime TimeStamp { get; set; } = DateTime.Now;

    public MessageType Type { get; set; }

    public static ServerResponse UserLeft(string roomKey, string username, string content = null) =>
        new ServerResponse
        {
            Type = MessageType.UserLeft,
            RoomKey = roomKey,
            Username = username,
            Content = content ?? $"{username} has left the room"
        };

    public static ServerResponse UserJoined(string roomKey, string username, string content = null) =>
        new ServerResponse
        {
            Type = MessageType.UserJoined,
            RoomKey = roomKey,
            Username = username,
            Content = content ?? $"{username} has joined the room"
        };

    public static ServerResponse JoinFailed(string content, string roomKey = null) =>
        new ServerResponse
        {
            Type = MessageType.JoinFailed,
            Content = content,
            RoomKey = roomKey
        };

    public static ServerResponse JoinSuccess(string roomKey, string username, string roomName, string OwnerName) =>
        new ServerResponse
        {
            Type = MessageType.JoinSuccess,
            RoomKey = roomKey,
            Username = username,
            RoomName = roomName,
            RoomOwner = OwnerName
        };

    public static ServerResponse SendingMessageFailed(string roomKey, string username, string content) =>
        new ServerResponse
        {
            Type = MessageType.SendingMessageFailed,
            RoomKey = roomKey,
            Username = username,
            Content = content
        };

    public static ServerResponse CreateRoomFailed(string roomKey, string roomName, string username, string content) =>
        new ServerResponse { Type = MessageType.RoomCreateFailed, RoomName = roomName, RoomKey = roomKey, Content = content };

    public static ServerResponse CreateRoomSuccess(string content) => new ServerResponse { Type = MessageType.RoomCreateSuccess, Content = content };
}

