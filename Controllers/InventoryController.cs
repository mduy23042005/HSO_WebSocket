using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class InventoryRequestPacket
{
    public string cmd;
    public int idAccount;
}
public class InventoryResultPacket
{
    public string cmd;
    public List<InventoryItem0Data> inventoryItem0Data;
    public List<InventoryItem1Data> inventoryItem1Data;
    public List<InventoryItem2Data> inventoryItem2Data;
    public List<InventoryItem3Data> inventoryItem3Data;
    public List<InventoryItem4Data> inventoryItem4Data;
}

class InventoryController
{
    public async Task ReadCacheInventory(ClientConnection client)
    {
        int idAccount = RaceManager.Instance.GetIDAccount(client);

        try
        {
            var inventoryItem0Data = CacheManager.Instance.GetAccountData(idAccount).inventoryItem0s;

            InventoryResultPacket inventoryResult = new InventoryResultPacket
            { 
                cmd = "inventory_result",
                inventoryItem0Data = new List<InventoryItem0Data>(),
                inventoryItem1Data = new List<InventoryItem1Data>(),
                inventoryItem2Data = new List<InventoryItem2Data>(),
                inventoryItem3Data = new List<InventoryItem3Data>(),
            };

            // tạm thời chỉ có item0
            foreach (var inventoryItem0 in inventoryItem0Data) 
            {
                inventoryResult.inventoryItem0Data.Add(new InventoryItem0Data
                {
                    id = inventoryItem0.id,
                    idItem0 = inventoryItem0.idItem0,
                    nameItem0 = inventoryItem0.nameItem0,
                    typeItem0 = inventoryItem0.typeItem0,
                    category = inventoryItem0.category,
                    idSchool = inventoryItem0.idSchool,
                    item0_Attributes = inventoryItem0.item0_Attributes,
                    nameAttributes = inventoryItem0.nameAttributes
                });
            }

            await RaceManager.Instance.SendPacketToClient(client, inventoryResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy inventory: " + ex.Message);
        }
    }
}