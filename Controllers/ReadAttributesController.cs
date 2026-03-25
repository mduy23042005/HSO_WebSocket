using System;
using System.Threading.Tasks;

public class ReadAttributesEquipmentRequestPacket
{
    public EnumCmdCode cmd;
    public int idAccount;
    public int id;
    public int idItem0_1;
}
public class ReadAttributesEquipmentResultPacket
{
    public EnumCmdCode cmd;
    public EquipmentData attributesData;
}

public class ReadAttributesInventoryRequestPacket
{
    public EnumCmdCode cmd;
    public int idAccount;
    public int id;
    public int idItem0;
}
public class ReadAttributesInventoryResultPacket
{
    public EnumCmdCode cmd;
    public InventoryItem0Data attributesItem0Data;
    public InventoryItem1Data attributesItem1Data;
    public InventoryItem2Data attributesItem2Data;
    public InventoryItem3Data attributesItem3Data;
    public InventoryItem4Data attributesItem4Data;
}

public class EquipItem0RequestPacket
{
    public EnumCmdCode cmd;
    public int idAccount;
    public int id;
    public int idItem0;
    public string slotName;
}

public class ReadAttributesController
{
    public async Task ReadAttributesEquipment(ClientConnection client, ReadAttributesEquipmentRequestPacket readAttributesPacket)
    {
        int idAccount = readAttributesPacket.idAccount;

        try
        {
            var equipmentData = CacheManager.Instance.GetAccountData(idAccount).equipments.Find(x => x.id == readAttributesPacket.id);

            ReadAttributesEquipmentResultPacket equipmentAttributesResult = new ReadAttributesEquipmentResultPacket
            {
                cmd = EnumCmdCode.equipmentAttributes,
                attributesData = new EquipmentData
                {
                    id = equipmentData.id,
                    idItem0_1 = equipmentData.idItem0_1,
                    nameItem0_1 = equipmentData.nameItem0_1,
                    category = equipmentData.category,
                    slotName = equipmentData.slotName,
                    item0_Attributes = equipmentData.item0_Attributes,
                    nameAttributes = equipmentData.nameAttributes,
                }
            };

            PacketWriterManager writer = new PacketWriterManager();
            writer.WriteInt((int)equipmentAttributesResult.cmd);
            writer.WriteInt(equipmentAttributesResult.attributesData.id);
            writer.WriteInt(equipmentAttributesResult.attributesData.idItem0_1);
            writer.WriteString(equipmentAttributesResult.attributesData.nameItem0_1);
            writer.WriteInt(equipmentAttributesResult.attributesData.category);
            writer.WriteString(equipmentAttributesResult.attributesData.slotName);
            writer.WriteListCount(equipmentAttributesResult.attributesData.item0_Attributes.Count);
            for (int i = 0; i < equipmentAttributesResult.attributesData.item0_Attributes.Count; i++)
            {
                writer.WriteInt(equipmentAttributesResult.attributesData.item0_Attributes[i].ID);
                writer.WriteInt(equipmentAttributesResult.attributesData.item0_Attributes[i].IDItem0);
                writer.WriteInt(equipmentAttributesResult.attributesData.item0_Attributes[i].IDAttribute);
                writer.WriteInt(equipmentAttributesResult.attributesData.item0_Attributes[i].Value);
                writer.WriteInt(equipmentAttributesResult.attributesData.item0_Attributes[i].Category);
                writer.WriteString(equipmentAttributesResult.attributesData.nameAttributes[i].NameAttribute);
            }

            await RaceManager.Instance.SendPacketToClient(client, writer.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy attribute equipment: " + ex.Message);
        }
    }

    //sau này sẽ có truyền thêm loại item (0, 1, 2, 3, 4) vào để phân biệt
    public async Task ReadAttributesInventory(ClientConnection client, ReadAttributesInventoryRequestPacket readAttributesPacket) 
    {
        int idAccount = readAttributesPacket.idAccount;

        try
        {
            var inventoryItem0Data = CacheManager.Instance.GetAccountData(idAccount).inventoryItem0s.Find(x => x.id == readAttributesPacket.id);

            ReadAttributesInventoryResultPacket inventoryItem0AttributesResult = new ReadAttributesInventoryResultPacket();

            inventoryItem0AttributesResult = new ReadAttributesInventoryResultPacket
            {
                cmd = EnumCmdCode.inventoryAttributes,
                attributesItem0Data = new InventoryItem0Data
                {
                    id = inventoryItem0Data.id,
                    idItem0 = inventoryItem0Data.idItem0,
                    nameItem0 = inventoryItem0Data.nameItem0,
                    typeItem0 = inventoryItem0Data.typeItem0,
                    category = inventoryItem0Data.category,
                    idSchool = inventoryItem0Data.idSchool,
                    level = inventoryItem0Data.level,
                    item0_Attributes = inventoryItem0Data.item0_Attributes,
                    nameAttributes = inventoryItem0Data.nameAttributes,
                },
                //attributesItem1Data = new InventoryItem1Data(),
                //attributesItem2Data = new InventoryItem2Data(),
                //attributesItem3Data = new InventoryItem3Data(),
                //attributesItem4Data = new InventoryItem4Data(),
            };

            PacketWriterManager writer = new PacketWriterManager();
            writer.WriteInt((int)inventoryItem0AttributesResult.cmd);
            writer.WriteInt(inventoryItem0AttributesResult.attributesItem0Data.id);
            writer.WriteInt(inventoryItem0AttributesResult.attributesItem0Data.idItem0);
            writer.WriteString(inventoryItem0AttributesResult.attributesItem0Data.nameItem0);
            writer.WriteString(inventoryItem0AttributesResult.attributesItem0Data.typeItem0);
            writer.WriteInt(inventoryItem0AttributesResult.attributesItem0Data.category);
            writer.WriteInt(inventoryItem0AttributesResult.attributesItem0Data.idSchool);
            writer.WriteInt(inventoryItem0AttributesResult.attributesItem0Data.level);
            writer.WriteListCount(inventoryItem0AttributesResult.attributesItem0Data.item0_Attributes.Count);
            for (int i = 0; i < inventoryItem0AttributesResult.attributesItem0Data.item0_Attributes.Count; i++)
            {
                writer.WriteInt(inventoryItem0AttributesResult.attributesItem0Data.item0_Attributes[i].ID);
                writer.WriteInt(inventoryItem0AttributesResult.attributesItem0Data.item0_Attributes[i].IDItem0);
                writer.WriteInt(inventoryItem0AttributesResult.attributesItem0Data.item0_Attributes[i].IDAttribute);
                writer.WriteInt(inventoryItem0AttributesResult.attributesItem0Data.item0_Attributes[i].Value);
                writer.WriteInt(inventoryItem0AttributesResult.attributesItem0Data.item0_Attributes[i].Category);
                writer.WriteString(inventoryItem0AttributesResult.attributesItem0Data.nameAttributes[i].NameAttribute);
            }

            await RaceManager.Instance.SendPacketToClient(client, writer.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy attribute inventory: " + ex.Message);
        }
    }

    public async Task EquipItem0(ClientConnection client, EquipItem0RequestPacket equipItem0Packet)
    {
        int idAccount = equipItem0Packet.idAccount;

        InventoryController inventoryController = new InventoryController();
        EquipmentController equipmentController = new EquipmentController();

        try
        {
            var accountData = CacheManager.Instance.GetAccountData(idAccount);
            if (accountData == null) return;

            var inventoryItem0Data = accountData.inventoryItem0s.Find(x => x.id == equipItem0Packet.id);
            var equipmentData = accountData.equipments.Find(x => x.slotName == equipItem0Packet.slotName);

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

            await WebAPIManager.Instance.PostAsync($"api/account/{idAccount}/equipItem0/{equipItem0Packet.id}?idAccount={idAccount}&id={equipItem0Packet.id}&slotName={equipItem0Packet.slotName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi trang bị item: " + ex.Message);
            return;
        }
    }
}