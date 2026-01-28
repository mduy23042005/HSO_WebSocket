using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
    public int idItem0_1;
    public int category;
    public string nameItem;
    public int value;
    public int idAttribute;
    public string attributes;
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
    public int idItem0;
    public int category;
    public string nameItem;
    public string typeItem0;
    public int value;
    public int idAttribute;
    public string attributes;
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

            List<ReadAttributesEquipmentResultPacket> equipmentResult = new List<ReadAttributesEquipmentResultPacket>();

            foreach (var attribute in equipmentData.item0_Attributes)
            {
                var nameAttribute = equipmentData.nameAttributes.Find(x => x.IDAttribute == attribute.IDAttribute).NameAttribute;
                equipmentResult.Add(new ReadAttributesEquipmentResultPacket
                {
                    cmd = "equipmentAttributes_result",
                    idItem0_1 = equipmentData.idItem0_1,
                    category = equipmentData.category,
                    nameItem = equipmentData.nameItem0_1,

                    value = attribute.Value,
                    attributes = nameAttribute,
                });
            }

            string packet = JsonConvert.SerializeObject(equipmentResult);
            await RaceManager.Instance.SendPacketToClient(client, packet);
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

            List<ReadAttributesInventoryResultPacket> inventoryResult = new List<ReadAttributesInventoryResultPacket>();

            foreach (var attribute in inventoryItem0Data.item0_Attributes)
            {
                var nameAttribute = inventoryItem0Data.nameAttributes.Find(x => x.IDAttribute == attribute.IDAttribute).NameAttribute;

                inventoryResult.Add(new ReadAttributesInventoryResultPacket
                {
                    cmd = "inventoryAttributes_result",
                    idItem0 = inventoryItem0Data.idItem0,
                    category = inventoryItem0Data.category,
                    typeItem0 = inventoryItem0Data.typeItem0,
                    nameItem = inventoryItem0Data.nameItem0,
                    value = attribute.Value,
                    attributes = nameAttribute
                });
            }

            string packet = JsonConvert.SerializeObject(inventoryResult);
            await RaceManager.Instance.SendPacketToClient(client, packet);
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