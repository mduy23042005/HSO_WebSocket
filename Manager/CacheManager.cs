using System;
using System.Collections.Generic;

public class NPCData
{
    public NPC npc;
    public int posX;
    public int posY;
}
public class MobData
{
    public Mob mob;
    public int id;
    public int posX;
    public int posY;

    public MobsController mobsAI;
}
public class MapData
{
    public Map map;
    public List<MobData> mobsData;
    public List<NPCData> npcsData;
}

//Trang bị riêng từng school
public class Item0Data
{
    public Item0 item0;
    public List<Item0_Attribute> item0_Attributes;
    public List<Attribute> nameAttributes;
}
//Trang bị chung
public class Item1Data
{
    public Item1 item1;
    public List<Item1_Attribute> item1_Attributes;
    public List<Attribute> nameAttributes;
}
//Vật phẩm nhiệm vụ
public class Item2Data
{
    public Item2 item2;
}
//Vật phẩm tiêu hao
public class Item3Data
{
    public Item3 item3;
}
//Vật phẩm cường hóa
public class Item4Data
{
    public Item4 item4;
}

//Tài nguyên của player
public class EquipmentData
{
    public int id;
    public int idItem0_1;
    public string nameItem0_1;
    public int category;
    public string slotName;
    public List<Item0_Attribute> item0_Attributes;
    public List<Attribute> nameAttributes;
}
public class InventoryItem0Data
{
    public int id;
    public int idItem0;
    public string nameItem0;
    public string typeItem0;
    public int category;
    public int idSchool;
    public int level;
    public List<Item0_Attribute> item0_Attributes;
    public List<Attribute> nameAttributes;
}
public class InventoryItem1Data
{
    public int id;
    public int idItem1;
    public string nameItem1;
    public string typeItem1;
    public int level;
    public List<Item1_Attribute> item1_Attributes;
    public List<Attribute> nameAttributes;
}
public class InventoryItem2Data
{
    public int id;
    public int idItem2;
    public string nameItem2;
    public int level;
    public int quality;
}
public class InventoryItem3Data
{
    public int id;
    public int idItem3;
    public string nameItem3;
    public int level;
    public string details;
    public int quality;
}
public class InventoryItem4Data
{
    public int id;
    public int idItem4;
    public string nameItem4;
    public int level;
    public string details;
    public int quality;
}
public class ChestData
{
    public List<Chest_ItemX> chestItems;
}

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
    public int idAccount;
    public PlayerState stateData;
    public Direction directionData;
    public List<PartBodyData> partBodyTransforms;
}
public class PlayerTransformData
{
    public int idAccount;
    public PositionData positionData;
    public ScaleData scaleData;
}
public class PlayerData
{
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
}

public class AccountData
{
    public Account account;
    public List<EquipmentData> equipments;
    public List<InventoryItem0Data> inventoryItem0s;
    public List<InventoryItem1Data> inventoryItem1s;
    public List<InventoryItem2Data> inventoryItem2s;
    public List<InventoryItem3Data> inventoryItem3s;
    public List<InventoryItem4Data> inventoryItem4s;
    public Chest chest;

    public PlayerData playerData;
    public PlayerStateData playerStateData;
    public PlayerTransformData playerTransformData;
}

public class CacheManager
{
    private static readonly Lazy<CacheManager> lazyInstance = new Lazy<CacheManager>(() => new CacheManager());
    public static CacheManager Instance => lazyInstance.Value;

    public Dictionary<int, MapData> maps;
    public Dictionary<int, MobData> mobs;
    public Dictionary<int, NPCData> npcs;

    public Dictionary<int, Item0Data> item0s;
    public Dictionary<int, Item1Data> item1s;
    public Dictionary<int, Item2Data> item2s;
    public Dictionary<int, Item3Data> item3s;
    public Dictionary<int, Item4Data> item4s;

    public Dictionary<int, AccountData> accounts;

    public void InitCache()
    {
        maps = new Dictionary<int, MapData>();
        mobs = new Dictionary<int, MobData>();
        npcs = new Dictionary<int, NPCData>();

        item0s = new Dictionary<int, Item0Data>();
        item1s = new Dictionary<int, Item1Data>();
        item2s = new Dictionary<int, Item2Data>();
        item3s = new Dictionary<int, Item3Data>();
        item4s = new Dictionary<int, Item4Data>();

        accounts = new Dictionary<int, AccountData>();
    }

    //Map
    public void AddMap(MapData data)
    {
        maps[data.map.IDMap] = data;
    }
    public MapData GetMap(int mapId)
    {
        maps.TryGetValue(mapId, out var data);
        return data;
    }
    public int GetCountMap()
    {
        return maps.Count;
    }

    //Mob
    public void AddMob(MobData data)
    {
        mobs[data.mob.IDMob] = data;
    }
    public MobData GetMob(int mobId)
    {
        mobs.TryGetValue(mobId, out var data);
        return data;
    }
    public int GetCountMob()
    {
        return mobs.Count;
    }

    //NPC
    public void AddNPC(NPCData data)
    {
        npcs[data.npc.IDNPC] = data;
    }
    public NPCData GetNPC(int npcId)
    {
        npcs.TryGetValue(npcId, out var data);
        return data;
    }
    public int GetCountNPC()
    {
        return npcs.Count;
    }

    //Item0
    public void AddItem0(Item0Data data)
    {
        item0s[data.item0.IDItem0] = data;
    }
    public Item0Data GetItem0(int id)
    {
        item0s.TryGetValue(id, out var data);
        return data;
    }
    public int GetCountItem0()
    {
        return item0s.Count;
    }

    //Item1
    public void AddItem1(Item1Data data)
    {
        item1s[data.item1.IDItem1] = data;
    }
    public Item1Data GetItem1(int id)
    {
        item1s.TryGetValue(id, out var data);
        return data;
    }
    public int GetCountItem1()
    {
        return item1s.Count;
    }

    //Item2
    public void AddItem2(Item2Data data)
    {
        item2s[data.item2.IDItem2] = data;
    }
    public Item2Data GetItem2(int id)
    {
        item2s.TryGetValue(id, out var data);
        return data;
    }
    public int GetCountItem2()
    {
        return item2s.Count;
    }

    //Item3
    public void AddItem3(Item3Data data)
    {
        item3s[data.item3.IDItem3] = data;
    }
    public Item3Data GetItem3(int id)
    {
        item3s.TryGetValue(id, out var data);
        return data;
    }
    public int GetCountItem3()
    {
        return item3s.Count;
    }

    //Item4
    public void AddItem4(Item4Data data)
    {
        item4s[data.item4.IDItem4] = data;
    }
    public Item4Data GetItem4(int id)
    {
        item4s.TryGetValue(id, out var data);
        return data;
    }
    public int GetCountItem4()
    {
        return item4s.Count;
    }

    //Account
    public void AddAccountData(AccountData data)
    {
        accounts[data.account.IDAccount] = data;
    }
    public AccountData GetAccountData(int accountId)
    {
        accounts.TryGetValue(accountId, out var data);
        return data;
    }
    public void RemoveAccountData(int accountId)
    {
        accounts.Remove(accountId);
    }
    public bool IsAccountOnline(int accountId)
    {
        return accounts.ContainsKey(accountId);
    }
    public void ClearAccounts()
    {
        accounts.Clear();
    }
}