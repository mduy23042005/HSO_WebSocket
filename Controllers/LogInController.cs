using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

//class hứng dữ liệu từ client gửi lên
class LogInRequestPacket
{
    public string cmd;
    public string username;
    public string password;
}
//class trả dữ liệu về client
class LogInResultPacket
{
    public string cmd;
    public bool success;
    public int idAccount;
    public int idSchool;
    public string nameChar;
    public int hair;
    public string message;
}

class LogOutRequestPacket
{
    public string cmd;
    public int idAccount;
}

public class LogInController
{
    public async Task ClickLogIn(ClientConnection client, string username, string password)
    {
        LogInResultPacket loginResult;

        string urlAccount = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/login?username={username}&password={password}";
        HttpResponseMessage res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlAccount);
        string json = await res.Content.ReadAsStringAsync();
        var acc = JsonConvert.DeserializeObject<Account>(json);

        if (acc != null)
        {
            if (CacheManager.Instance.IsAccountOnline(acc.IDAccount))
            {
                loginResult = new LogInResultPacket
                {
                    cmd = "login_result",
                    success = false,
                    idAccount = 0,
                    idSchool = 0,
                    nameChar = null,
                    hair = 0,
                    message = "Account is already logged in another client."
                };

                string packetDeny = JsonConvert.SerializeObject(loginResult);
                await RaceManager.Instance.SendPacketToClient(client, packetDeny);
                return;
            }

            loginResult = new LogInResultPacket
            {
                cmd = "login_result",
                success = acc != null,
                idAccount = acc.IDAccount,
                idSchool = acc.IDSchool,
                nameChar = acc.NameChar,
                hair = acc.Hair,
                message = $"Đăng nhập {acc.NameChar} thành công."
            };

            await LoadAccountData(acc);
            RaceManager.Instance.BindAccountToClient(client, acc.IDAccount);
        }
        else
        {
            loginResult = new LogInResultPacket
            {
                cmd = "login_result",
                success = false,
                idAccount = 0,
                idSchool = 0,
                nameChar = null,
                hair = 0,
                message = "Username hoặc Password không đúng."
            };
            return;
        }

        string packet = JsonConvert.SerializeObject(loginResult);
        await RaceManager.Instance.SendPacketToClient(client, packet);
    }
    private async Task LoadAccountData(Account acc)
    {
        HttpResponseMessage res;
        string json;

        string urlEquipment = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{acc.IDAccount}/equipment?idAccount={acc.IDAccount}";
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlEquipment);
        json = await res.Content.ReadAsStringAsync();
        var equipmentData = JsonConvert.DeserializeObject<List<EquipmentData>>(json);

        string urlInventoryItem0 = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{acc.IDAccount}/inventoryItem0?idAccount={acc.IDAccount}";
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlInventoryItem0);
        json = await res.Content.ReadAsStringAsync();
        var inventoryItem0Data = JsonConvert.DeserializeObject<List<InventoryItem0Data>>(json);

        string urlInventoryItem1 = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{acc.IDAccount}/inventoryItem1?idAccount={acc.IDAccount}";
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlInventoryItem1);
        json = await res.Content.ReadAsStringAsync();
        var inventoryItem1Data = JsonConvert.DeserializeObject<List<InventoryItem1Data>>(json);

        string urlInventoryItem2 = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{acc.IDAccount}/inventoryItem2?idAccount={acc.IDAccount}";
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlInventoryItem2);
        json = await res.Content.ReadAsStringAsync();
        var inventoryItem2Data = JsonConvert.DeserializeObject<List<InventoryItem2Data>>(json);

        string urlInventoryItem3 = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{acc.IDAccount}/inventoryItem3?idAccount={acc.IDAccount}";
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlInventoryItem3);
        json = await res.Content.ReadAsStringAsync();
        var inventoryItem3Data = JsonConvert.DeserializeObject<List<InventoryItem3Data>>(json);

        string urlInventoryItem4 = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{acc.IDAccount}/inventoryItem4?idAccount={acc.IDAccount}";
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlInventoryItem4);
        json = await res.Content.ReadAsStringAsync();
        var inventoryItem4Data = JsonConvert.DeserializeObject<List<InventoryItem4Data>>(json);

        AccountData accountData = new AccountData();
        accountData.account = acc;
        accountData.equipments = equipmentData;
        if (inventoryItem0Data != null)
        {
            accountData.inventoryItem0s = inventoryItem0Data;
        }
        if (inventoryItem2Data != null)
        {
            accountData.inventoryItem2s = inventoryItem2Data;
        }
        if (inventoryItem3Data != null)
        {
            accountData.inventoryItem3s = inventoryItem3Data;
        }
        if (inventoryItem4Data != null)
        {
            accountData.inventoryItem4s = inventoryItem4Data;
        }

        CacheManager.Instance.AddAccountData(accountData);
    }
}
