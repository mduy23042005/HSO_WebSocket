using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
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
    public int idSchool;
}

class InventoryController
{
    public async Task ReadDatabaseInventory(WebSocket socket)
    {
        int idAccount = LogInController.GetIDAccount() ?? 0;
        string urlItems = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{idAccount}/inventory";

        List<InventoryResultPacket> inventoryResult;

        try
        {
            HttpResponseMessage res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlItems);
            string json = await res.Content.ReadAsStringAsync();
            List<InventoryResultPacket> data = JsonConvert.DeserializeObject<List<InventoryResultPacket>>(json);

            inventoryResult = new List<InventoryResultPacket>();

            foreach (var inventoryItem in data)
            {
                inventoryResult.Add(new InventoryResultPacket
                {
                    cmd = "inventory_result",
                    id = inventoryItem.id,
                    idItem0 = inventoryItem.idItem0,
                    category = inventoryItem.category,
                    typeItem0 = inventoryItem.typeItem0,
                    idSchool = inventoryItem.idSchool 
                });
            }

            string packet = JsonConvert.SerializeObject(inventoryResult);
            RaceManager.Instance.SendPacketToClientAsync(socket, packet);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy inventory: " + ex.Message);
        }
    }
}