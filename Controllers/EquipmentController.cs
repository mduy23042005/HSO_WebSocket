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
    public List<EquipmentData> equipmentData;
}

class EquipmentController
{
    public async Task ReadCacheEquipment(ClientConnection client)
    {
        int idAccount = RaceManager.Instance.GetIDAccount(client);

        try
        {
            var equipmentData = CacheManager.Instance.GetAccountData(idAccount).equipments;

            EquipmentResultPacket equipmentResult = new EquipmentResultPacket
            {
                cmd = "equipment_result",
                equipmentData = new List<EquipmentData>()
            };
            foreach (var equippedItem in equipmentData)
            {
                equipmentResult.equipmentData.Add(new EquipmentData
                {
                    id = equippedItem.id,
                    idItem0_1 = equippedItem.idItem0_1,
                    nameItem0_1 = equippedItem.nameItem0_1,
                    category = equippedItem.category,
                    slotName = equippedItem.slotName,
                    item0_Attributes = equippedItem.item0_Attributes,
                    nameAttributes = equippedItem.nameAttributes
                });
            }

            await RaceManager.Instance.SendPacketToClient(client, equipmentResult);
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

            EquipmentResultPacket outfitSpritesResult = new EquipmentResultPacket
            {
                cmd = "outfitSprites_result",
                equipmentData = new List<EquipmentData>()
            };
            foreach (var equippedItem in equipmentData)
            {
                outfitSpritesResult.equipmentData.Add(new EquipmentData
                {
                    id = equippedItem.id,
                    idItem0_1 = equippedItem.idItem0_1,
                    nameItem0_1 = equippedItem.nameItem0_1,
                    category = equippedItem.category,
                    slotName = equippedItem.slotName,
                    item0_Attributes = equippedItem.item0_Attributes,
                    nameAttributes = equippedItem.nameAttributes
                });
            }

            await RaceManager.Instance.SendPacketToClient(client, outfitSpritesResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy equipment: " + ex.Message);
        }
    }
}