using Newtonsoft.Json;
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
    public int id;
    public int idItem0;
    public int category;
    public string typeItem0;
    public List<Item0_Attribute> item0_Attributes;
    public List<Attribute> nameAttributes;
    public int idSchool;
}

class InventoryController
{
    public async Task ReadCacheInventory(ClientConnection client)
    {
        int idAccount = RaceManager.Instance.GetIDAccount(client);

        try
        {
            var inventoryItem0Data = CacheManager.Instance.GetAccountData(idAccount).inventoryItem0s;

            List<InventoryResultPacket> inventoryResult = new List<InventoryResultPacket>();

            foreach (var inventoryItem0 in inventoryItem0Data)
            {
                inventoryResult.Add(new InventoryResultPacket
                {
                    cmd = "inventory_result",
                    id = inventoryItem0.id,
                    idItem0 = inventoryItem0.idItem0,
                    category = inventoryItem0.category,
                    typeItem0 = inventoryItem0.typeItem0,
                    idSchool = inventoryItem0.idSchool,
                });
            }

            string packet = JsonConvert.SerializeObject(inventoryResult);
            await RaceManager.Instance.SendPacketToClient(client, packet);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy inventory: " + ex.Message);
        }
    }
}