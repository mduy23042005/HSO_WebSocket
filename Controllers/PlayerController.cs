using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

public class PlayerSyncData
{
    public PlayerData playerData;
    public PlayerTransformData playerTransformData;
    public PlayerStateData playerStateData;
}
public class PlayerSyncDataRequestPacket
{
    public string cmd;
    public PlayerSyncData playerSyncData;
}
public class OtherPlayerSyncData
{
    public PlayerData otherPlayerData;
    public PlayerTransformData otherPlayerTransformData;
    public PlayerStateData otherPlayerStateData;
}

public class PlayerController
{

}