using System;
using System.Collections.Generic;

public class NPCData
{
    public NPC npcs;
}
public class MobData
{
    public Mob mobs;
}
public class MapData
{
    public Map maps;
    public List<Map_Mob> map_Mobs;
    public List<Map_NPC> map_NPCs;
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
    public List<Item0_Attribute> item0_Attributes;
    public List<Attribute> nameAttributes;
}
public class InventoryItem1Data
{
    public Account_Item1 item1Data;
    public List<Item1_Attribute> item1_Attributes;
    public List<Attribute> nameAttributes;
}
public class InventoryItem2Data
{
    public Account_Item2 item2Data;
}
public class InventoryItem3Data
{
    public Account_Item3 item3Data;
}
public class InventoryItem4Data
{
    public Account_Item4 item4Data;
}
public class ChestData
{
    public List<Chest_ItemX> chestItems;
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

    public SyncDataPacket syncData;
}

public class CacheManager
{
    private static readonly Lazy<CacheManager> lazyInstance = new Lazy<CacheManager>(() => new CacheManager());
    public static CacheManager Instance => lazyInstance.Value;

    public Dictionary<int, MapData> maps;
    public Dictionary<int, MobData> mobs;
    public Dictionary<int, NPCData> npcs;

    public Dictionary<int, Item0Data> items0;
    public Dictionary<int, Item1Data> items1;
    public Dictionary<int, Item2Data> items2;
    public Dictionary<int, Item3Data> items3;
    public Dictionary<int, Item4Data> items4;

    public Dictionary<int, AccountData> accounts;

    public void InitCache()
    {
        maps = new Dictionary<int, MapData>();
        mobs = new Dictionary<int, MobData>();
        npcs = new Dictionary<int, NPCData>();

        items0 = new Dictionary<int, Item0Data>();
        items1 = new Dictionary<int, Item1Data>();
        items2 = new Dictionary<int, Item2Data>();
        items3 = new Dictionary<int, Item3Data>();
        items4 = new Dictionary<int, Item4Data>();

        accounts = new Dictionary<int, AccountData>();
    }

    //Map
    public void AddMap(MapData data)
    {
        maps[data.maps.IDMap] = data;
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
        mobs[data.mobs.IDMob] = data;
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
        npcs[data.npcs.IDNPC] = data;
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
        items0[data.item0.IDItem0] = data;
    }
    public Item0Data GetItem0(int id)
    {
        items0.TryGetValue(id, out var data);
        return data;
    }
    public int GetCountItem0()
    {
        return items0.Count;
    }

    //Item1
    public void AddItem1(Item1Data data)
    {
        items1[data.item1.IDItem1] = data;
    }
    public Item1Data GetItem1(int id)
    {
        items1.TryGetValue(id, out var data);
        return data;
    }
    public int GetCountItem1()
    {
        return items1.Count;
    }

    //Item2
    public void AddItem2(Item2Data data)
    {
        items2[data.item2.IDItem2] = data;
    }
    public Item2Data GetItem2(int id)
    {
        items2.TryGetValue(id, out var data);
        return data;
    }
    public int GetCountItem2()
    {
        return items2.Count;
    }

    //Item3
    public void AddItem3(Item3Data data)
    {
        items3[data.item3.IDItem3] = data;
    }
    public Item3Data GetItem3(int id)
    {
        items3.TryGetValue(id, out var data);
        return data;
    }
    public int GetCountItem3()
    {
        return items3.Count;
    }

    //Item4
    public void AddItem4(Item4Data data)
    {
        items4[data.item4.IDItem4] = data;
    }
    public Item4Data GetItem4(int id)
    {
        items4.TryGetValue(id, out var data);
        return data;
    }
    public int GetCountItem4()
    {
        return items4.Count;
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
