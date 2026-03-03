using System;
using System.Threading.Tasks;

public class ReadAttributesEquipmentRequestPacket
{
    public string cmd;
    public int idAccount;
    public int id;
    public int idItem0_1;
}
public class ReadAttributesEquipmentResultPacket
{
    public string cmd;
    public EquipmentData attributesData;
}

public class ReadAttributesInventoryRequestPacket
{
    public string cmd;
    public int idAccount;
    public int id;
    public int idItem0;
}
public class ReadAttributesInventoryResultPacket
{
    public string cmd;
    public InventoryItem0Data attributesItem0Data;
    public InventoryItem1Data attributesItem1Data;
    public InventoryItem2Data attributesItem2Data;
    public InventoryItem3Data attributesItem3Data;
    public InventoryItem4Data attributesItem4Data;
}

public class EquipItem0RequestPacket
{
    public string cmd;
    public int idAccount;
    public int id;
    public int idItem0;
    public string slotName;
}

class ReadAttributesController
{
    public async Task ReadAttributesEquipment(ClientConnection client, int idAcc, int id)
    {
        int idAccount = idAcc;

        try
        {
            var equipmentData = CacheManager.Instance.GetAccountData(idAccount).equipments.Find(x => x.id == id);

            ReadAttributesEquipmentResultPacket equipmentAttributesResult = new ReadAttributesEquipmentResultPacket
            {
                cmd = "equipmentAttributes_result",
                attributesData = new EquipmentData()
            };

            equipmentAttributesResult.attributesData = new EquipmentData
            {
                id = equipmentData.id,
                idItem0_1 = equipmentData.idItem0_1,
                nameItem0_1 = equipmentData.nameItem0_1,
                category = equipmentData.category,
                slotName = equipmentData.slotName,
                item0_Attributes = equipmentData.item0_Attributes,
                nameAttributes = equipmentData.nameAttributes
            };

            await RaceManager.Instance.SendPacketToClient(client, equipmentAttributesResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy attribute equipment: " + ex.Message);
        }
    }

    public async Task ReadAttributesInventory(ClientConnection client, int idAcc, int id) //sau này sẽ có truyền thêm loại item (0, 1, 2, 3, 4) vào để phân biệt
    {
        int idAccount = idAcc;

        try
        {
            var inventoryItem0Data = CacheManager.Instance.GetAccountData(idAccount).inventoryItem0s.Find(x => x.id == id);

            ReadAttributesInventoryResultPacket inventoryAttributesResult = new ReadAttributesInventoryResultPacket();

            inventoryAttributesResult = new ReadAttributesInventoryResultPacket
            {
                cmd = "inventoryAttributes_result",
                attributesItem0Data = new InventoryItem0Data(),
                attributesItem1Data = new InventoryItem1Data(),
                attributesItem2Data = new InventoryItem2Data(),
                attributesItem3Data = new InventoryItem3Data(),
                attributesItem4Data = new InventoryItem4Data(),
            };

            inventoryAttributesResult.attributesItem0Data = new InventoryItem0Data
            {
                id = inventoryItem0Data.id,
                idItem0 = inventoryItem0Data.idItem0,
                nameItem0 = inventoryItem0Data.nameItem0,
                typeItem0 = inventoryItem0Data.typeItem0,
                category = inventoryItem0Data.category,
                idSchool = inventoryItem0Data.idSchool,
                level = inventoryItem0Data.level,
                item0_Attributes = inventoryItem0Data.item0_Attributes,
                nameAttributes = inventoryItem0Data.nameAttributes
            };

            await RaceManager.Instance.SendPacketToClient(client, inventoryAttributesResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy attribute inventory: " + ex.Message);
        }
    }

    public async Task EquipItem0(ClientConnection client, int idAcc, int id, int idItem0, string slotName)
    {
        int idAccount = idAcc;

        InventoryController inventoryController = new InventoryController();
        EquipmentController equipmentController = new EquipmentController();

        try
        {
            var accountData = CacheManager.Instance.GetAccountData(idAccount);
            if (accountData == null) return;

            var inventoryItem0Data = accountData.inventoryItem0s.Find(x => x.id == id);
            var equipmentData = accountData.equipments.Find(x => x.slotName == slotName);

            var tempEquipmentItem = new EquipmentData
            {
                idItem0_1 = equipmentData.idItem0_1,
                nameItem0_1 = equipmentData.nameItem0_1,
                category = equipmentData.category,
                item0_Attributes = equipmentData.item0_Attributes,
                nameAttributes = equipmentData.nameAttributes
            };

            equipmentData.idItem0_1 = inventoryItem0Data.idItem0;
            equipmentData.nameItem0_1 = inventoryItem0Data.nameItem0;
            equipmentData.category = inventoryItem0Data.category;
            equipmentData.item0_Attributes = inventoryItem0Data.item0_Attributes;
            equipmentData.nameAttributes = inventoryItem0Data.nameAttributes;

            inventoryItem0Data.idItem0 = tempEquipmentItem.idItem0_1;
            inventoryItem0Data.nameItem0 = tempEquipmentItem.nameItem0_1;
            inventoryItem0Data.category = tempEquipmentItem.category;
            inventoryItem0Data.item0_Attributes = tempEquipmentItem.item0_Attributes;
            inventoryItem0Data.nameAttributes = tempEquipmentItem.nameAttributes;

            await equipmentController.ReadCacheEquipment(client);
            await inventoryController.ReadCacheInventory(client);

            await WebAPIManager.Instance.PostAsync($"api/account/{idAccount}/equipItem0/{id}?idAccount={idAccount}&id={id}&slotName={slotName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi trang bị item: " + ex.Message);
            return;
        }
    }
}