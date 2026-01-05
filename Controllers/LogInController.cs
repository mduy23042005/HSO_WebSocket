using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Net.Http;

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
}

public class LogInController
{
    private static int idAcc;

    public async void ClickLogIn(WebSocket socket, string username, string password)
    {
        LogInResultPacket loginResult;

        string url = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/login?username={username}&password={password}";
        HttpResponseMessage res = await WebAPIManager.Instance.GetHttpClient().GetAsync(url);
        string json = await res.Content.ReadAsStringAsync();
        var acc = JsonConvert.DeserializeObject<Account>(json);

        if (acc != null)
        {
            loginResult = new LogInResultPacket
            {
                cmd = "login_result",
                success = acc != null,
                idAccount = acc.IDAccount,
                idSchool = acc.IDSchool,
                nameChar = acc.NameChar,
                hair = acc.Hair
            };
            idAcc = acc.IDAccount;
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
                hair = 0
            };
            Console.WriteLine($"Username hoặc Password không đúng.");
            return;
        }

        string packet = JsonConvert.SerializeObject(loginResult);
        RaceManager.Instance.SendPacketToClientAsync(socket, packet);
        RaceManager.Instance.BindAccountToClient(socket, idAcc);
    }

    public static int? GetIDAccount() //nếu bấm vào Đăng ký thì idAccount = 0
    {
        return idAcc;
    }
}
