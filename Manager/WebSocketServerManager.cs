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
    private HttpListener listener;
    private CancellationTokenSource shutdownCts = new CancellationTokenSource();
    private volatile bool isShuttingDown = false;
    TimeZoneInfo vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    private DateTime time;

    public static void Main(string[] args)
    {
        WebSocketServerManager server = new WebSocketServerManager();
        Console.WriteLine("Starting Web Socket Server...");
        server.RunWebSocketServer().GetAwaiter().GetResult();
    }

    public async Task RunWebSocketServer()
    {
        try
        {
            //Khởi động Web API
            await WebAPIManager.Instance.InitAPI();
            time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Connected to Web API successfully! (http://localhost:55555)");

            //Kiểm tra port lắng nghe
            ListenToPort(55556);
            time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Started Web Socket Server port: 55556 successfully!");

            //Khởi động Cleanup Loop
            _ = InitCleanupLoop(shutdownCts.Token);
            time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Initialized Cleanup Loop successfully!");

            //Khởi động Cache
            CacheManager.Instance.InitCache();
            time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Initialized Cache successfully!");

            //Load data
            time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loading data...");
            await LoadData();

            _ = SyncMobsLoop();
            Console.WriteLine("[Server] SyncMob loop started.");

            Task.Run(ListenForQuit);

            //Chấp nhận kết nối từ client
            time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            Console.WriteLine($"\n[Server] {time.ToString("hh:mm:ss tt")} Web Socket Server is ready!");
            await AcceptClients();
        }
        catch (Exception ex)
        {
            time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Fatal error: {ex}");

            throw;
        }
    }

    private void ListenToPort(int port)
    {
        listener = new HttpListener();
        listener.Prefixes.Add($"http://+:{port}/");
        listener.Start();
    }
    private async Task LoadData()
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
        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded NPC data successfully! [{CacheManager.Instance.GetCountNPC()}]");

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
        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded Mob data successfully! [{CacheManager.Instance.GetCountMob()}]");

        //Load Map
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(loadMap);
        json = await res.Content.ReadAsStringAsync();
        var mapList = JsonConvert.DeserializeObject<List<MapData>>(json);
        if (mapList != null)
        {
            foreach (var map in mapList)
            {
                if (map.mobsData != null)
                {
                    foreach (var mob in map.mobsData)
                    {
                        mob.mobsAI = new MobsController(mob.posX, mob.posY, 6, 6);
                    }
                }

                CacheManager.Instance.AddMap(map);
            }

        }
        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded Map data successfully! [{CacheManager.Instance.GetCountMap()}]");

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
        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded Item0 data successfully! [{CacheManager.Instance.GetCountItem0()}]");

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
        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded Item1 data successfully! [{CacheManager.Instance.GetCountItem1()}]");

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
                    item2s = item
                });
            }
        }
        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded Item2 data successfully! [{CacheManager.Instance.GetCountItem2()}]");

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
                    item3s = item
                });
            }
        }
        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded Item3 data successfully! [{CacheManager.Instance.GetCountItem3()}]");

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
                    item4s = item
                });
            }
        }
        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded Item4 data successfully! [{CacheManager.Instance.GetCountItem4()}]");

        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded all data successfully!");
    }
    private async Task InitCleanupLoop(CancellationToken token)
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
    private async Task AcceptClients()
    {
        while (!shutdownCts.IsCancellationRequested)
        {
            try
            {
                HttpListenerContext context = await listener.GetContextAsync();

                // Kiểm tra đây có phải Web Socket request không
                if (context.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);

                    string IPClient = context.Request.RemoteEndPoint?.ToString() ?? "Unknown";
                    var client = new ClientConnection(wsContext.WebSocket, IPClient);

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
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
    }
    private async Task HandleClient(ClientConnection client)
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

                    result = await client.socket.ReceiveAsync(new ArraySegment<byte>(buffer), shutdownCts.Token);

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

        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            if (client.socket.State != WebSocketState.Closed)
            {
                RaceManager.Instance.MarkLogOut(client);
            }
        }
    }
    private void ListenForQuit()
    {
        while (!shutdownCts.IsCancellationRequested)
        {
            Console.Write("> ");
            string input = Console.ReadLine();

            if (input == null)
                continue;

            if (input == "q")
            {
                Console.WriteLine("Do you want to quit? (y/n): ");
                string confirm = Console.ReadLine();

                if (confirm == null)
                    continue;

                if (confirm == "y")
                {
                    ShutdownServer();
                    return;
                }
                else
                {
                    Console.WriteLine("Cancel shutdown.");
                }
            }
        }
    }
    private void ShutdownServer()
    {
        if (isShuttingDown) return;
        isShuttingDown = true;

        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Shutting down...");

        // Báo toàn bộ task dừng
        shutdownCts.Cancel();

        // Logout toàn bộ client
        RaceManager.Instance.MarkLogOutAll();

        // Stop listener
        listener.Stop();

        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Shutdown completed.");
        Environment.Exit(0);
    }
    private async Task SyncMobsLoop()
    {
        const int tick_ms = 100; // 10 tick / giây
        DateTime lastTick = DateTime.UtcNow;

        while (!shutdownCts.IsCancellationRequested)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                float deltaTime = (float)(now - lastTick).TotalSeconds;
                lastTick = now;

                // Update mob logic
                var mobs = CacheManager.Instance.GetMap(1).mobsData; //tạm thời là mà map có id 1, sau này khi main player ở map nào thì gửi map đó
                foreach (var mob in mobs)
                {
                    mob.mobsAI.Attack(deltaTime);
                    mob.mobsAI.Move(deltaTime);
                }

                // Build sync packet
                var mobSnapshots = new List<object>();
                foreach (var mob in mobs)
                {
                    var pos = mob.mobsAI.GetPosition();
                    mobSnapshots.Add(new
                    {
                        id = mob.id,
                        idMob = mob.mobs.IDMob,
                        posX = pos.X,
                        posY = pos.Y,
                        state = mob.mobsAI.GetState(),
                        idState = mob.mobsAI.GetIDState(),
                        direction = mob.mobsAI.GetDirection(),
                    });
                }

                if (mobSnapshots.Count > 0)
                {
                    var syncMobData = new
                    {
                        cmd = "syncMobs",
                        mobsData = mobSnapshots
                    };

                    string packet = JsonConvert.SerializeObject(syncMobData);
                    await RaceManager.Instance.SendPacketToAllClients(packet);
                }

                await Task.Delay(tick_ms, shutdownCts.Token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SyncMob] Error: " + ex.Message);
            }
        }
    }

    public async Task ReceivePacketFromClient(ClientConnection client, string json)
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
                        var syncPacket = JsonConvert.DeserializeObject<PlayerDataPacket>(json);
                        int idAccount = RaceManager.Instance.GetIDAccount(client);

                        var accountData = CacheManager.Instance.GetAccountData(idAccount);
                        if (accountData != null)
                        {
                            accountData.syncData = syncPacket;
                        }

                        var syncOtherPlayers = new PlayerController();
                        await syncOtherPlayers.ReadCacheSyncData(client);
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
