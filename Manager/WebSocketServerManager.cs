using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed class ClientConnection
{
    public Guid idConnection = Guid.NewGuid();
    public WebSocket socket;
    public string ipRemote;
    public DateTime connectedAt = DateTime.UtcNow;

    public ClientConnection(WebSocket socket, string ipRemote)
    {
        this.socket = socket;
        this.ipRemote = ipRemote;
    }
}

public class WebSocketServerManager
{
    static HttpListener listener;

    public static async Task Main(string[] args)
    {
        try
        {
            //Khởi động Web API
            await WebAPIManager.Instance.InitAPI();
            Console.WriteLine("Connected to Web API successfully! (http://localhost:55555)");

            //Kiểm tra port lắng nghe
            ListenToPort(55556);
            Console.WriteLine("Started Web Socket Server port: 55556 successfully!");

            //Khởi động Cleanup Loop
            var cts = new CancellationTokenSource();
            _ = InitCleanupLoop(cts.Token);
            Console.WriteLine("Initialized Cleanup Loop successfully!");

            //Khởi động Cache
            CacheManager.Instance.InitCache();
            Console.WriteLine("Initialized Cache successfully!");

            //Load data
            Console.WriteLine("Loading data...");
            await LoadData();

            //Chấp nhận kết nối từ client
            Console.WriteLine("\nWeb Socket Server is ready!");
            await AcceptClients();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex}");

            throw;
        }
    }

    private static void ListenToPort(int port)
    {
        listener = new HttpListener();
        listener.Prefixes.Add($"http://+:{port}/");
        listener.Start();
    }
    private static async Task LoadData()
    {
        string loadNPC = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/NPC/full";
        string loadMob = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Mob/full";
        string loadMap = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Map/full";
        string loadItem0 = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Item0/full";
        string loadItem1 = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Item1/full";
        string loadItem2 = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Item2/full";
        string loadItem3 = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Item3/full";
        string loadItem4 = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Item4/full";

        HttpResponseMessage res;
        string json;

        //Load NPC
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(loadNPC);
        json = await res.Content.ReadAsStringAsync();
        var npcList = JsonConvert.DeserializeObject<List<NPC>>(json);
        if (npcList != null)
        {
            foreach (var npc in npcList)
            {
                CacheManager.Instance.AddNPC(new NPCData
                {
                    npcs = npc
                });
            }
        }
        Console.WriteLine($"Loaded NPC data successfully! [{CacheManager.Instance.GetCountNPC()}]");

        //Load Mob
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(loadMob);
        json = await res.Content.ReadAsStringAsync();
        var mobList = JsonConvert.DeserializeObject<List<Mob>>(json);
        if (mobList != null)
        {
            foreach (var mob in mobList)
            {
                CacheManager.Instance.AddMob(new MobData
                {
                    mobs = mob
                });
            }
        }
        Console.WriteLine($"Loaded Mob data successfully! [{CacheManager.Instance.GetCountMob()}]");

        //Load Map
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(loadMap);
        json = await res.Content.ReadAsStringAsync();
        var mapList = JsonConvert.DeserializeObject<List<MapData>>(json);
        if (mapList != null)
        {
            foreach (var map in mapList)
            {
                CacheManager.Instance.AddMap(map);
            }
        }
        Console.WriteLine($"Loaded Map data successfully! [{CacheManager.Instance.GetCountMap()}]");

        //Load Item0
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(loadItem0);
        json = await res.Content.ReadAsStringAsync();
        var item0List = JsonConvert.DeserializeObject<List<Item0Data>>(json);
        if (item0List != null)
        {
            foreach (var item in item0List)
            {
                CacheManager.Instance.AddItem0(item);
            }
        }
        Console.WriteLine($"Loaded Item0 data successfully! [{CacheManager.Instance.GetCountItem0()}]");

        //Load Item1
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(loadItem1);
        json = await res.Content.ReadAsStringAsync();
        var item1List = JsonConvert.DeserializeObject<List<Item1Data>>(json);
        if (item1List != null)
        {
            foreach (var item in item1List)
            {
                CacheManager.Instance.AddItem1(item);
            }
        }
        Console.WriteLine($"Loaded Item1 data successfully! [{CacheManager.Instance.GetCountItem1()}]");

        //Load Item2
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(loadItem2);
        json = await res.Content.ReadAsStringAsync();
        var item2List = JsonConvert.DeserializeObject<List<Item2>>(json);
        if (item2List != null)
        {
            foreach (var item in item2List)
            {
                CacheManager.Instance.AddItem2(new Item2Data
                {
                    item2 = item
                });
            }
        }
        Console.WriteLine($"Loaded Item2 data successfully! [{CacheManager.Instance.GetCountItem2()}]");

        //Load Item3
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(loadItem3);
        json = await res.Content.ReadAsStringAsync();
        var item3List = JsonConvert.DeserializeObject<List<Item3>>(json);
        if (item3List != null)
        {
            foreach (var item in item3List)
            {
                CacheManager.Instance.AddItem3(new Item3Data
                {
                    item3 = item
                });
            }
        }
        Console.WriteLine($"Loaded Item3 data successfully! [{CacheManager.Instance.GetCountItem3()}]");

        //Load Item4
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(loadItem4);
        json = await res.Content.ReadAsStringAsync();
        var item4List = JsonConvert.DeserializeObject<List<Item4>>(json);
        if (item4List != null)
        {
            foreach (var item in item4List)
            {
                CacheManager.Instance.AddItem4(new Item4Data
                {
                    item4 = item
                });
            }
        }
        Console.WriteLine($"Loaded Item4 data successfully! [{CacheManager.Instance.GetCountItem4()}]");

        Console.WriteLine($"Loaded all data successfully!");
    }

    private static async Task InitCleanupLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            RaceManager.Instance.RemoveDisconnectedClients();

            try
            {
                await Task.Delay(1000, token);
            }
            catch (TaskCanceledException)
            {
                // server shutdown
                break;
            }
        }
    }
    private static async Task AcceptClients()
    {
        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();

            // Kiểm tra đây có phải Web Socket request không
            if (context.Request.IsWebSocketRequest)
            {
                HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);

                string IPClient = context.Request.RemoteEndPoint?.ToString() ?? "Unknown";
                var client = new ClientConnection(wsContext.WebSocket, IPClient);
                Console.WriteLine($"New socket connected! IP Client: {IPClient}");

                RaceManager.Instance.RegisterClient(client);
                _ = HandleClient(client);
            }
            else
            {
                // Nếu không phải Web Socket thì trả về 400
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    static async Task HandleClient(ClientConnection client)
    {
        var buffer = new byte[4096];
        var messageBuffer = new StringBuilder();

        try
        {
            while (client.socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;

                do
                {
                    if (client.socket.State != WebSocketState.Open)
                    {
                        return;
                    }

                    result = await client.socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }

                    messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                } while (!result.EndOfMessage);

                string fullMessage = messageBuffer.ToString();
                messageBuffer.Clear();

                await ReceivePacketFromClient(client, fullMessage);
            }
        }
        catch (WebSocketException)
        {
            // client disconnect bất thường
        }
        catch (OperationCanceledException)
        {
            // server shutdown
        }
        finally
        {
            Console.WriteLine($"Client disconnected! | {client.ipRemote} | state: {client.socket.State}");
            if (client.socket.State != WebSocketState.Closed)
            {
                RaceManager.Instance.MarkLogOut(client);
            }
        }
    }

    public static async Task ReceivePacketFromClient(ClientConnection client, string json)
    {
        try
        {
            var token = JToken.Parse(json);
            string cmd = null;
            switch (token.Type)
            {
                case JTokenType.Object:
                    cmd = token["cmd"]?.ToString();
                    break;

                case JTokenType.Array:
                    foreach (var item in token)
                    {
                        //Đệ quy để cho từng phần tử của JTokenType.Array quay ngược lại case JTokenType.Object
                        await ReceivePacketFromClient(client, item.ToString(Formatting.None)); 
                    }
                    break;
            }
            switch (cmd)
            {
                case "syncData":
                    {
                        var syncPacket = JsonConvert.DeserializeObject<SyncDataPacket>(json);
                        int idAccount = RaceManager.Instance.GetIDAccount(client);

                        var accountData = CacheManager.Instance.GetAccountData(idAccount);
                        if (accountData != null)
                        {
                            accountData.syncData = syncPacket;
                        }

                        var syncController = new SyncController();
                        await syncController.ReadCacheSyncData(client);
                        break;
                    }

                case "login":
                    var loginPacket = JsonConvert.DeserializeObject<LogInRequestPacket>(json);
                    var loginController = new LogInController();
                    await loginController.ClickLogIn(client, loginPacket.username, loginPacket.password);
                    break;
                case "logout":
                    RaceManager.Instance.MarkLogOut(client);
                    await RaceManager.Instance.SendPacketToClient(client, json);
                    return;

                case "register":
                    var registerPacket = JsonConvert.DeserializeObject<RegisterRequestPacket>(json);
                    var registerController = new RegisterController();
                    await registerController.ClickRegister(client, registerPacket.idSchool, registerPacket.username, registerPacket.password, registerPacket.nameChar, registerPacket.hair, registerPacket.blessingPoints);
                    break;

                case "equipment":
                    var equipmentPacket = JsonConvert.DeserializeObject<EquipmentRequestPacket>(json);
                    var equipmentController = new EquipmentController();
                    await equipmentController.ReadCacheEquipment(client);
                    break;

                case "equipmentAttributes":
                    var equipmentAttributesRequestPacket = JsonConvert.DeserializeObject<ReadAttributesEquipmentRequestPacket>(json);
                    var readAttributesEquipmentController = new ReadAttributesController();
                    await readAttributesEquipmentController.ReadAttributesEquipment(client, equipmentAttributesRequestPacket.idAccount, equipmentAttributesRequestPacket.id);
                    break;

                case "inventory":
                    var inventoryPacket = JsonConvert.DeserializeObject<EquipmentRequestPacket>(json);
                    var inventoryController = new InventoryController();
                    await inventoryController.ReadCacheInventory(client);
                    break;

                case "inventoryAttributes":
                    var inventoryAttributesRequestPacket = JsonConvert.DeserializeObject<ReadAttributesInventoryRequestPacket>(json);
                    var readAttributesInventoryController = new ReadAttributesController();
                    await readAttributesInventoryController.ReadAttributesInventory(client, inventoryAttributesRequestPacket.idAccount, inventoryAttributesRequestPacket.id);
                    break;

                case "equipItem0":
                    var equipItem0RequestPacket = JsonConvert.DeserializeObject<EquipItem0RequestPacket>(json);
                    var equipItem0Controller = new ReadAttributesController();
                    await equipItem0Controller.EquipItem0(client, equipItem0RequestPacket.idAccount, equipItem0RequestPacket.id, equipItem0RequestPacket.idItem0, equipItem0RequestPacket.slotName);
                    break;

                case "outfitSprites":
                    var outfitSpritesPacket = JsonConvert.DeserializeObject<EquipmentRequestPacket>(json);
                    var outfitSpritesController = new EquipmentController();
                    await outfitSpritesController.ReadCacheOutfitSprites(client);
                    break;

                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error processing packet: " + ex.Message);
        }
    }
}
