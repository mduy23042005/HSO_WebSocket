using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
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
    public async Task ReadAttributesEquipment(WebSocket socket, int idAcc, int id)
    {
        int idAccount = idAcc;
        List<ReadAttributesEquipmentResultPacket> equipmentResult;

        try
        {
            string urlListAttributes = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{idAccount}/equipItem/{id}/listAttributes?id={id}";
            HttpResponseMessage res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlListAttributes);
            string json = await res.Content.ReadAsStringAsync();
            List<ReadAttributesEquipmentResultPacket> listIDAttributeEquip = JsonConvert.DeserializeObject<List<ReadAttributesEquipmentResultPacket>>(json);

            equipmentResult = new List<ReadAttributesEquipmentResultPacket>();

            foreach (var r in listIDAttributeEquip)
            {
                string urlNameAttribute = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{idAccount}/equipItem/{id}/listAttributes/{r.idAttribute}?idAttribute={r.idAttribute}";
                res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlNameAttribute);
                json = await res.Content.ReadAsStringAsync();
                string nameAttribute = JsonConvert.DeserializeObject<string>(json);

                equipmentResult.Add(new ReadAttributesEquipmentResultPacket
                {
                    cmd = "equipmentAttributes_result",
                    idItem0_1 = r.idItem0_1,
                    category = r.category,
                    nameItem = r.nameItem,
                    value = r.value,
                    attributes = nameAttribute
                });
            }

            string packet = JsonConvert.SerializeObject(equipmentResult);
            await RaceManager.Instance.SendPacketToClientAsync(socket, packet);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy attribute equipment: " + ex.Message);
        }
    }

    public async Task ReadAttributesInventory(WebSocket socket, int idAcc, int id)
    {
        int idAccount = idAcc;

        List<ReadAttributesInventoryResultPacket> inventoryResult;

        try
        {
            string urlListAttributes = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{idAccount}/inventoryItem/{id}/listAttributes?id={id}";
            HttpResponseMessage res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlListAttributes);
            string json = await res.Content.ReadAsStringAsync();
            List<ReadAttributesInventoryResultPacket> listIDAttributeEquip = JsonConvert.DeserializeObject<List<ReadAttributesInventoryResultPacket>>(json);

            inventoryResult = new List<ReadAttributesInventoryResultPacket>();

            foreach (var r in listIDAttributeEquip)
            {
                string urlNameAttribute = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{idAccount}/inventoryItem/{id}/listAttributes/{r.idAttribute}?idAttribute={r.idAttribute}";
                res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlNameAttribute);
                json = await res.Content.ReadAsStringAsync();
                string nameAttribute = JsonConvert.DeserializeObject<string>(json);

                inventoryResult.Add(new ReadAttributesInventoryResultPacket
                {
                    cmd = "inventoryAttributes_result",
                    idItem0 = r.idItem0,
                    category = r.category,
                    typeItem0 = r.typeItem0,
                    nameItem = r.nameItem,
                    value = r.value,
                    attributes = nameAttribute
                });
            }

            string packet = JsonConvert.SerializeObject(inventoryResult);
            await RaceManager.Instance.SendPacketToClientAsync(socket, packet);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Lỗi khi lấy attribute inventory: " + ex.Message);
        }
    }

    public async Task EquipItem0(WebSocket socket, int idAcc, int id, int idItem0, string slotName)
    {
        int idAccount = idAcc;

        InventoryController inventoryController = new InventoryController();
        EquipmentController equipmentController = new EquipmentController();

        try
        {
            await WebAPIManager.Instance.PostAsync($"api/account/{idAccount}/equipItem0/{id}?idAccount={idAccount}&id={id}&slotName={slotName}");

            await inventoryController.ReadDatabaseInventory(socket);
            await equipmentController.ReadDatabaseEquipment(socket);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Lỗi khi trang bị item: " + ex.Message);
            return;
        }
    }
}