using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class EquipmentRequestPacket
{
    public string cmd;
    public int idAccount;
}
public class EquipmentResultPacket
{
    public string cmd;
    public int id;
    public int idItem0_1;
    public int category;
    public List<Item0_Attribute> item0_Attributes;
    public List<Attribute> nameAttributes;
}

class EquipmentController
{
    public async Task ReadCacheEquipment(ClientConnection client)
    {
        int idAccount = RaceManager.Instance.GetIDAccount(client);

        try
        {
            var equipmentData = CacheManager.Instance.GetAccountData(idAccount).equipments;

            List<EquipmentResultPacket> equipmentResult = new List<EquipmentResultPacket>();

            foreach (var equippedItem in equipmentData)
            {
                equipmentResult.Add(new EquipmentResultPacket
                {
                    cmd = "equipment_result",
                    id = equippedItem.id,
                    idItem0_1 = equippedItem.idItem0_1,
                    category = equippedItem.category,
                    item0_Attributes = equippedItem.item0_Attributes,
                    nameAttributes = equippedItem.nameAttributes
                });
            }

            string packet = JsonConvert.SerializeObject(equipmentResult);
            await RaceManager.Instance.SendPacketToClient(client, packet);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy equipment: " + ex.Message);
        }
    }
    public async Task ReadCacheOutfitSprites(ClientConnection client)
    {
        int idAccount = RaceManager.Instance.GetIDAccount(client);

        try
        {
            var equipmentData = CacheManager.Instance.GetAccountData(idAccount).equipments;

            List<EquipmentResultPacket> equipmentResult = new List<EquipmentResultPacket>();

            foreach (var equippedItem in equipmentData)
            {
                equipmentResult.Add(new EquipmentResultPacket
                {
                    cmd = "outfitSprites_result",
                    id = equippedItem.id,
                    idItem0_1 = equippedItem.idItem0_1,
                    category = equippedItem.category,
                    item0_Attributes = equippedItem.item0_Attributes,
                    nameAttributes = equippedItem.nameAttributes
                });
            }

            string packet = JsonConvert.SerializeObject(equipmentResult);
            await RaceManager.Instance.SendPacketToClient(client, packet);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy equipment: " + ex.Message);
        }
    }
}