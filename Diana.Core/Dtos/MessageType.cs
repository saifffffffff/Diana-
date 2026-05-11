using System;
using System.Collections.Generic;
using System.Text;

namespace Diana.Core.Dtos;

public enum MessageType
{
    // client -> server
    JoinRoom,
    LeaveRoom,
    SendMessage,
    CreateRoom,
    GetRoomList,

    // server -> client
    JoinFailed,
    JoinSuccess,
    SendingMessageFailed,
    RoomCreateFailed,
    RoomCreateSuccess,
    UserLeft,
    UserJoined
}
