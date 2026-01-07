using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Net.WebSockets;

public class RegisterRequestPacket
{
    public string cmd;
    public int idSchool;
    public string nameChar;
    public string username;
    public string password;
    public int hair;
    public int blessingPoints;
}
public class RegisterResultPacket
{
    public string cmd;
    public bool success;
}
class RegisterController
{
    public async void ClickRegister(WebSocket socket, int idSchool, string username, string password, string nameChar, int hair, int blessingPoints)
    {
        int weapon = 0, helmet = 0, armor = 0, legArmor = 0;
        switch (idSchool)
        {
            case 1: weapon = 1; helmet = 9; armor = 17; legArmor = 25; break;
            case 2: weapon = 2; helmet = 10; armor = 18; legArmor = 26; break;
            case 3: weapon = 3; helmet = 11; armor = 19; legArmor = 27; break;
            case 4: weapon = 4; helmet = 12; armor = 20; legArmor = 28; break;
        }
        // Tạo object để gửi lên API
        var registerData = new
        {
            Account = new Account
            {
                Username = username,
                Password = password,
                NameChar = nameChar,
                IDSchool = idSchool,
                Level = 1,
                SkillPoints = 0,
                StatPoints = 0,
                Exp = 0,
                Hair = hair,
                Gold = 20000,
                Gem = 2000,
                Point0 = 0,
                Point1 = 0,
                Point2 = 0,
                Point3 = 0,
                PointArena = 0,
                PointActive = 100000,
                Skill0 = 1,
                Skill1 = 0,
                Skill2 = 0,
                Skill3 = 0,
                Skill4 = 0,
                Skill5 = 0,
                Skill6 = 0,
                Skill7 = 0,
                Skill8 = 0,
                Skill9 = 0,
                Skill10 = 0,
                Skill11 = 0,
                Skill12 = 0,
                Skill13 = 0,
                Skill14 = 0,
                Skill15 = 0,
                Skill16 = 0,
                Skill17 = 0,
                Skill18 = 0,
                Skill19 = 0,
                Skill20 = 0,
                Clan = null,
                BlessingPoints = blessingPoints
            },
            Equipment = new List<Account_Equipment>
            {
                new Account_Equipment { IDItem0_1 = weapon, SlotName = "Weapon", Category = 1 },
                new Account_Equipment { IDItem0_1 = helmet, SlotName = "Helmet", Category = 1 },
                new Account_Equipment { IDItem0_1 = armor, SlotName = "Armor", Category = 1 },
                new Account_Equipment { IDItem0_1 = legArmor, SlotName = "LegArmor", Category = 1 },
                new Account_Equipment { IDItem0_1 = 0, SlotName = "Gloves", Category = 1 },
                new Account_Equipment { IDItem0_1 = 0, SlotName = "Shoes", Category = 1 },
                new Account_Equipment { IDItem0_1 = 0, SlotName = "Ring1", Category = 1 },
                new Account_Equipment { IDItem0_1 = 0, SlotName = "Ring2", Category = 1 },
                new Account_Equipment { IDItem0_1 = 0, SlotName = "Necklace", Category = 1 },
                new Account_Equipment { IDItem0_1 = 0, SlotName = "Medal", Category = 1 },
                new Account_Equipment { IDItem0_1 = 0, SlotName = "Cloak", Category = 1 },
                new Account_Equipment { IDItem0_1 = 0, SlotName = "Wing", Category = 1 },
                new Account_Equipment { IDItem0_1 = 0, SlotName = "SkinWing", Category = 1 },
                new Account_Equipment { IDItem0_1 = 0, SlotName = "Mounts", Category = 1 },
                new Account_Equipment { IDItem0_1 = 0, SlotName = "Pet", Category = 1 },
                new Account_Equipment { IDItem0_1 = 0, SlotName = "Skin", Category = 1 },
            }
        };

        string json = JsonConvert.SerializeObject(registerData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage result = await WebAPIManager.Instance.GetHttpClient().PostAsync($"{WebAPIManager.Instance.GetApiUrl()}/api/account/register", content);

        RegisterResultPacket registerResult;

        if (result.IsSuccessStatusCode)
        {
            Console.WriteLine("Đăng ký thành công!");

            registerResult = new RegisterResultPacket
            {
                cmd = "register_result",
                success = result.IsSuccessStatusCode
            };
        }
        else
        {
            string errorMsg = await result.Content.ReadAsStringAsync();
            Console.WriteLine($"Đăng ký thất bại: {errorMsg}");

            registerResult = new RegisterResultPacket
            {
                cmd = "register_result",
                success = result.IsSuccessStatusCode
            };
        }

        string packet = JsonConvert.SerializeObject(registerResult);
        await RaceManager.Instance.SendPacketToClient(socket, packet);
    }
}