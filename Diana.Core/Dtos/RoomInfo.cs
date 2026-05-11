using System;
using System.Collections.Generic;
using System.Text;

namespace Diana.Core.Dtos;

public class RoomInfo
{
    public string RoomKey { get; set; }
    public string RoomName { get; set; }
    public int MemberCount { get; set; }
    public string OwnerName { get; set; }
}
