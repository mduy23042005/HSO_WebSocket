using System.Collections.Generic;
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
public class PositionData
{
    public float x;
    public float y;
    public float z;
}
public class RotationData
{
    public float x;
    public float y;
    public float z;
}
public class ScaleData
{
    public float x;
    public float y;
    public float z;
}
public class ColorData
{
    public float r;
    public float g;
    public float b;
    public float a;
}
public class PartBodyData
{
    public string category;
    public string label;
    public PositionData positionData;
    public RotationData rotationData;
    public ScaleData scaleData;
    public ColorData colorData;
}
public class PlayerStateData
{
    public PlayerState stateData;
    public Direction directionData;
    public List<PartBodyData> partBodyTransforms;
}
public class PlayerTransformData
{
    public PositionData positionData;
    public ScaleData scaleData;
}
public class PlayerData
{
    public string nameMap;
    public int idAccount;
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
    public int maxHP;
    public int maxMP;
    public int hp;
    public int mp;
}

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

public class PlayerAttackDataPacket
{
    public EnumCmdCode cmd;
    public int idAccount;
    public int aimedMobID;
}
public class PlayerAttackMobDataResult
{
    public EnumCmdCode cmd;
    public int aimedMobID;
    public int damage;
    public int hpMobAfterAttack;
}
public class OtherPlayerAttackMobDataResult
{
    public EnumCmdCode cmd;
    public int aimedMobID;
    public int damage;
    public int hpMobAfterAttack;
}

public class PlayerController
{
    private int maxHP;
    private int maxMP;

    private int hp;
    private int mp;

    private int damage;

    // Contructer này để lấy dữ liệu từ cache ra khi cần tính toán hoặc xử lý logic
    public PlayerController(int idAccount)
    {
        maxHP = CacheManager.Instance.GetAccountData(idAccount).playerData.maxHP;
        maxMP = CacheManager.Instance.GetAccountData(idAccount).playerData.maxMP;
        hp = CacheManager.Instance.GetAccountData(idAccount).playerData.hp;
        mp = CacheManager.Instance.GetAccountData(idAccount).playerData.mp;

        damage = 125;
    }
    // Contructer này để vừa login vào nó khởi tạo và đưa vào cache luôn
    public PlayerController(int idAccount, int point0, int point1, int point2, int point3)
    {
        maxHP = point0 * 100;
        maxMP = point3 * 100;
        hp = maxHP;
        mp = maxMP;

        damage = 125;
    }

    public int GetMaxHP()
    {
        return maxHP;
    }
    public int GetMaxMP()
    {
        return maxMP;
    }
    public int GetHP()
    {
        return hp;
    }
    public int GetMP()
    {
        return mp;
    }

    public async Task PlayerAttack(ClientConnection client, PlayerAttackDataPacket data)
    {
        var mob = CacheManager.Instance.GetMob(data.aimedMobID);

        if (mob == null)
            return;

        mob.hp = mob.hp - damage;

        PlayerAttackMobDataResult playerAttackDataResult = new PlayerAttackMobDataResult();
        playerAttackDataResult.cmd = EnumCmdCode.playerAttackMob;
        playerAttackDataResult.aimedMobID = data.aimedMobID;
        playerAttackDataResult.damage = damage;
        playerAttackDataResult.hpMobAfterAttack = mob.hp;

        OtherPlayerAttackMobDataResult otherPlayerAttackDataResult = new OtherPlayerAttackMobDataResult();
        otherPlayerAttackDataResult.cmd = EnumCmdCode.otherPlayerAttackMob;
        otherPlayerAttackDataResult.aimedMobID = data.aimedMobID;
        otherPlayerAttackDataResult.damage = damage;
        otherPlayerAttackDataResult.hpMobAfterAttack = mob.hp;

        PacketWriterManager writer = new PacketWriterManager();
        writer.WriteInt((int)playerAttackDataResult.cmd);
        writer.WriteInt(playerAttackDataResult.aimedMobID);
        writer.WriteInt(playerAttackDataResult.damage);
        writer.WriteInt(playerAttackDataResult.hpMobAfterAttack);
        await RaceManager.Instance.SendPacketToClient(client, writer.ToArray());

        PacketWriterManager writer1 = new PacketWriterManager();
        writer1.WriteInt((int)otherPlayerAttackDataResult.cmd);
        writer1.WriteInt(otherPlayerAttackDataResult.aimedMobID);
        writer1.WriteInt(otherPlayerAttackDataResult.damage);
        writer1.WriteInt(otherPlayerAttackDataResult.hpMobAfterAttack);
        await RaceManager.Instance.SendPacketToAllClients(writer1.ToArray(), client);
    }
}