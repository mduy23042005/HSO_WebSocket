using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
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
}

class EquipmentController
{
    public async Task ReadDatabaseEquipment(ClientConnection client)
    {
        int idAccount = RaceManager.Instance.GetIDAccount(client);

        string urlItems = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{idAccount}/equipment?idAccount={idAccount}";

        List<EquipmentResultPacket> equipmentResult;

        try
        {
            HttpResponseMessage res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlItems);
            string json = await res.Content.ReadAsStringAsync();
            List<EquipmentResultPacket> data = JsonConvert.DeserializeObject<List<EquipmentResultPacket>>(json);

            equipmentResult = new List<EquipmentResultPacket>();

            foreach (var equippedItem in data)
            {
                equipmentResult.Add(new EquipmentResultPacket
                {
                    cmd = "equipment_result",
                    id = equippedItem.id,
                    idItem0_1 = equippedItem.idItem0_1,
                    category = equippedItem.category
                });
            }

            string packet = JsonConvert.SerializeObject(equipmentResult);
            await RaceManager.Instance.SendPacketToClient(client, packet);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy equipment: " + ex.Message);
        }
    }
    public async Task ReadDatabaseOutfitSprites(ClientConnection client)
    {
        int idAccount = RaceManager.Instance.GetIDAccount(client);

        string urlItems = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{idAccount}/equipment?idAccount={idAccount}";

        List<EquipmentResultPacket> equipmentResult;

        try
        {
            HttpResponseMessage res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlItems);
            string json = await res.Content.ReadAsStringAsync();
            List<EquipmentResultPacket> data = JsonConvert.DeserializeObject<List<EquipmentResultPacket>>(json);

            equipmentResult = new List<EquipmentResultPacket>();

            foreach (var equippedItem in data)
            {
                equipmentResult.Add(new EquipmentResultPacket
                {
                    cmd = "outfitSprites_result",
                    id = equippedItem.id,
                    idItem0_1 = equippedItem.idItem0_1,
                    category = equippedItem.category
                });
            }

            string packet = JsonConvert.SerializeObject(equipmentResult);
            await RaceManager.Instance.SendPacketToClient(client, packet);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy equipment: " + ex.Message);
        }
    }
}