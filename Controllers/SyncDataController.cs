using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

public enum PlayerState
{
    Stand = 0,
    Move = 1,
    Attack = 2,
    Injured = 3,
    Die = 4
}
public enum Direction
{
    Front = 0,
    Back = 1,
    Left = 2,
    Right = 3,
}
public class SyncDataPacket
{
    public string cmd;
    public int idAccount;
    public float posX;
    public float posY;
    public float lastPosX;
    public float lastPosY;
    public PlayerState state;
    public Direction direction;
    public int frame;
    public string nameChar;
    public int level;
    public int idSchool;
    public int hair;
    public int weapon;
    public int helmet;
    public int armor;
    public int legArmor;
    public int gloves;
    public int shoes;
    public int ring1;
    public int ring2;
    public int necklace;
    public int medal;
    public int cloak;
    public int wing;
    public int skinWing;
    public int mounts;
    public int pet;
    public int skin;
}

class SyncController
{
    public async Task ReadCacheSyncData(ClientConnection client)
    {
        int idAccount = RaceManager.Instance.GetIDAccount(client);

        try
        {
            var accountData = CacheManager.Instance.GetAccountData(idAccount);
            if (accountData == null || accountData.syncData == null)
                return;

            var data = accountData.syncData;
            data.cmd = "syncData";

            var packet = JsonConvert.SerializeObject(data);
            await RaceManager.Instance.SendPacketToAllClients(packet, client);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi sync data: " + ex.Message);
        }
    }
}