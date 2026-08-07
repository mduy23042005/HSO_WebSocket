using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
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
    private TimeZoneInfo vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    private DateTime time;
    private readonly Dictionary<int, List<ClientConnection>> mapPlayersForSyncPlayers = new Dictionary<int, List<ClientConnection>>();
    private readonly Dictionary<int, List<ClientConnection>> mapPlayersForSyncMobs = new Dictionary<int, List<ClientConnection>>();
    private AStarManager astar = new AStarManager();

    public static void Main(string[] args)
    {
        WebSocketServerManager server = new WebSocketServerManager();
        Console.WriteLine("Starting Web Socket Server...");
        server.RunWebSocketServer().GetAwaiter().GetResult();
    }

    private async Task RunWebSocketServer()
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

            //Khởi động Cleanup Loop để dọn dẹp client ngắt kết nối thụ động
            _ = Task.Run(() => InitCleanupLoop(shutdownCts.Token));
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

            _ = Task.Run(() => UpdateMobsLoop());
            Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} UpdateMobs loop started.");

            _ = Task.Run(() => SyncMobsLoop());
            Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} SyncMobs loop started.");

            _ = Task.Run(() => SyncOtherPlayersLoop());
            Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} SyncOtherPlayers loop started.");
            
            _ = Task.Run(ListenForQuit);

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
        string loadMap = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Map/full";
        string loadItem0 = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Item0/full";
        string loadItem1 = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Item1/full";
        string loadItem2 = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Item2/full";
        string loadItem3 = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Item3/full";
        string loadItem4 = $"{WebAPIManager.Instance.GetApiUrl()}/api/load/Item4/full";

        HttpResponseMessage res;
        string json;

        #region Load Map
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(loadMap);
        json = await res.Content.ReadAsStringAsync();
        var mapList = JsonConvert.DeserializeObject<List<MapData>>(json);
        if (mapList != null)
        {
            foreach (var mapData in mapList)
            {
                CacheManager.Instance.AddMap(mapData);
                CacheManager.Instance.AddClientMap(mapData);

                MapController mapController = new MapController();
                mapController.InitMap(mapData);
            }
        }
        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Inited Map data successfully! [{CacheManager.Instance.GetCountInitedMap()}]");

        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded Map data successfully! [{CacheManager.Instance.GetCountMap()}]");
        #endregion

        #region Load NPC
        if (mapList != null)
        {
            foreach (var map in mapList)
            {
                foreach (var npcData in map.npcsData)
                {
                    CacheManager.Instance.AddNPC(new NPCData
                    {
                        npc = npcData.npc,
                        posX = npcData.posX,
                        posY = npcData.posY,
                    });
                }
            }
        }
        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded NPC data successfully! [{CacheManager.Instance.GetCountNPC()}]");
        #endregion

        #region Load Mob
        if (mapList != null)
        {
            foreach (var map in mapList)
            {
                foreach (var mobData in map.mobsData)
                {
                    //những data khác đã có sẵn khi load API rồi
                    mobData.mobsAI = new MobsController(mobData.posX, mobData.posY, 6, 6);
                    mobData.damage = mobData.mob.Level * 10;
                    mobData.hp = mobData.mob.HP;

                    CacheManager.Instance.AddMob(mobData);
                }
            }
        }
        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded Mob data successfully! [{CacheManager.Instance.GetCountMob()}]");
        #endregion

        #region Load Item0 - Item4
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
                    item2 = item
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
                    item3 = item
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
                    item4 = item
                });
            }
        }
        time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Loaded Item4 data successfully! [{CacheManager.Instance.GetCountItem4()}]");
        #endregion

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

                    time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
                    Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Socket connected: {client.ipRemote}.");

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
        var messageBuffer = new List<byte>();

        try
        {
            while (client.socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;

                do
                {
                    if (client.socket.State != WebSocketState.Open)
                        return;

                    result = await client.socket.ReceiveAsync(new ArraySegment<byte>(buffer), shutdownCts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                        return;

                    messageBuffer.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));

                } while (!result.EndOfMessage);

                byte[] fullMessage = messageBuffer.ToArray();
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
    private async Task UpdateMobsLoop()
    {
        const int targetTickRate = 20;
        const int tickMS = 1000 / targetTickRate;

        var stopwatch = new Stopwatch();

        while (!shutdownCts.IsCancellationRequested)
        {
            stopwatch.Restart();
            try
            {
                float deltaTime = 1f / targetTickRate;

                for (int i = 1; i <= CacheManager.Instance.GetCountInitedMap(); i = i + 1)
                {
                    var map = CacheManager.Instance.GetMap(i);

                    if (map == null || map.mobsData == null)
                        continue;
                    var mobs = map.mobsData;

                    foreach (var mob in mobs)
                    {
                        var hpMob = CacheManager.Instance.GetMob(mob.id).hp;

                        if (hpMob > 0)
                        {
                            mob.mobsAI.Attack(deltaTime, map, mob.damage);
                            mob.mobsAI.Move(deltaTime, map);
                        }
                        else
                        {
                            mob.mobsAI.Die();

                            if (!mob.isRespawning)
                            {
                                mob.isRespawning = true;

                                mob.mobsAI.Die();

                                _ = Task.Run(async () =>
                                {
                                    await Task.Delay(3000);

                                    mob.hp = mob.mob.HP;
                                    mob.mobsAI.Respawn();
                                    mob.isRespawning = false;
                                });
                            }
                        }
                    }                
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Server] Update Mobs error: " + ex.Message);
            }

            stopwatch.Stop();
            int sleep = tickMS - (int)stopwatch.ElapsedMilliseconds;
            if (sleep > 0)
                await Task.Delay(sleep, shutdownCts.Token);
        }
    }
    private async Task SyncMobsLoop()
    {
        const int targetTickRate = 20;
        const int tickMS = 1000 / targetTickRate;

        var stopwatch = new Stopwatch();

        while (!shutdownCts.IsCancellationRequested)
        {
            stopwatch.Restart();
            try
            {
                foreach (var kv in mapPlayersForSyncMobs)
                    kv.Value.Clear();

                var clients = RaceManager.Instance.GetAllClients();

                foreach (var client in clients)
                {
                    //kiểm tra player có hợp lệ không
                    int idAccount = RaceManager.Instance.GetIDAccount(client);
                    if (idAccount <= 0) 
                        continue;

                    var accountData = CacheManager.Instance.GetAccountData(idAccount);
                    if (accountData?.playerTransformData == null) 
                        continue;

                    //thêm player vào map
                    int idMap = CacheManager.Instance.GetClientMapID(accountData.playerData.nameMap);

                    if (!mapPlayersForSyncMobs.ContainsKey(idMap))
                        mapPlayersForSyncMobs[idMap] = new List<ClientConnection>();

                    mapPlayersForSyncMobs[idMap].Add(client);
                }

                foreach (var kv in mapPlayersForSyncMobs)
                {
                    int mapId = kv.Key;
                    var clientsInMap = kv.Value;

                    var map = CacheManager.Instance.GetMap(mapId);
                    if (map == null || map.mobsData == null)
                        continue;
                    var mobs = map.mobsData;

                    var mobSnapshots = new List<SyncMobsResultData>();
                    var mobDeadSnapshots = new List<SyncMobsResultData>();

                    foreach (var mob in mobs)
                    {
                        var pos = mob.mobsAI.GetCurrentPosition();
                        var hpMob = CacheManager.Instance.GetMob(mob.id).hp;

                        var currentPosition = CacheManager.Instance.GetMob(mob.id).mobsAI.GetCurrentPosition();
                        var currentTile = astar.GetTileType(map, currentPosition.X, currentPosition.Y);

                        if (hpMob > 0)
                        {
                            mobSnapshots.Add(new SyncMobsResultData
                            {
                                id = mob.id,
                                idMob = mob.mob.IDMob,
                                maxHP = mob.mob.HP,
                                hp = hpMob,
                                level = mob.mob.Level,
                                posX = pos.X,
                                posY = pos.Y,
                                state = mob.mobsAI.GetState(),
                                idState = mob.mobsAI.GetIDState(),
                                direction = mob.mobsAI.GetDirection(),
                                currentTile = currentTile,
                            });
                        }
                        else
                        {
                            mobDeadSnapshots.Add(new SyncMobsResultData
                            {
                                id = mob.id,
                                idMob = mob.mob.IDMob,
                                maxHP = mob.mob.HP,
                                hp = hpMob,
                                level = mob.mob.Level,
                                posX = pos.X,
                                posY = pos.Y,
                                state = mob.mobsAI.GetState(),
                                idState = mob.mobsAI.GetIDState(),
                                direction = mob.mobsAI.GetDirection(),
                                currentTile = currentTile,
                            });
                        }
                    }

                    if (mobSnapshots.Count > 0)
                    {
                        PacketWriterManager writer = new PacketWriterManager();
                        writer.WriteInt((int)EnumCmdCode.syncMobsData);
                        writer.WriteListCount(mobSnapshots.Count);

                        foreach (var mobData in mobSnapshots)
                        {
                            writer.WriteInt(mobData.id);
                            writer.WriteInt(mobData.idMob);
                            writer.WriteInt(mobData.maxHP);
                            writer.WriteInt(mobData.hp);
                            writer.WriteInt(mobData.level);
                            writer.WriteFloat(mobData.posX);
                            writer.WriteFloat(mobData.posY);
                            writer.WriteInt((int)mobData.state);
                            writer.WriteInt(mobData.idState);
                            writer.WriteInt((int)mobData.direction);
                            writer.WriteInt((int)mobData.currentTile);
                        }
                        byte[] packet = writer.ToArray();
                        // gửi chỉ cho client trong map này
                        foreach (var client in clientsInMap)
                        {
                            await RaceManager.Instance.SendPacketToClient(client, packet);
                        }
                    }
                    if (mobDeadSnapshots.Count > 0)
                    {
                        PacketWriterManager writer = new PacketWriterManager();
                        writer.WriteInt((int)EnumCmdCode.syncMobsDeadData);
                        writer.WriteListCount(mobDeadSnapshots.Count);

                        foreach (var mobDead in mobDeadSnapshots)
                        {
                            writer.WriteInt(mobDead.id);
                            writer.WriteInt(mobDead.idMob);
                            writer.WriteInt(mobDead.maxHP);
                            writer.WriteInt(mobDead.hp);
                            writer.WriteInt(mobDead.level);
                            writer.WriteFloat(mobDead.posX);
                            writer.WriteFloat(mobDead.posY);
                            writer.WriteInt((int)mobDead.state);
                            writer.WriteInt(mobDead.idState);
                            writer.WriteInt((int)mobDead.direction);
                            writer.WriteInt((int)mobDead.currentTile);
                        }
                        byte[] packet = writer.ToArray();
                        // gửi chỉ cho client trong map này
                        foreach (var client in clientsInMap)
                        {
                            await RaceManager.Instance.SendPacketToClient(client, packet);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Server] Sync Mobs error: " + ex.Message);
            }

            stopwatch.Stop();
            int sleep = tickMS - (int)stopwatch.ElapsedMilliseconds;
            if (sleep > 0)
                await Task.Delay(sleep, shutdownCts.Token);
        }
    }
    private async Task SyncOtherPlayersLoop()
    {
        const int targetTickRate = 30;
        const int tickMS = 1000 / targetTickRate;

        var stopwatch = new Stopwatch();

        while (!shutdownCts.IsCancellationRequested)
        {
            stopwatch.Restart();

            try
            {
                foreach (var kv in mapPlayersForSyncPlayers)
                    kv.Value.Clear();

                var clients = RaceManager.Instance.GetAllClients();

                foreach (var client in clients)
                {
                    int idAccount = RaceManager.Instance.GetIDAccount(client);
                    if (idAccount <= 0)
                        continue;

                    var accountData = CacheManager.Instance.GetAccountData(idAccount);

                    if (accountData == null || accountData.playerData == null || accountData.playerTransformData == null || accountData.playerStateData == null)
                        continue;

                    int idMap = CacheManager.Instance.GetClientMapID(accountData.playerData.nameMap);

                    if (!mapPlayersForSyncPlayers.ContainsKey(idMap))
                        mapPlayersForSyncPlayers[idMap] = new List<ClientConnection>();

                    mapPlayersForSyncPlayers[idMap].Add(client);
                }

                foreach (var kv in mapPlayersForSyncPlayers)
                {
                    var clientsInMap = kv.Value;
                    if (clientsInMap.Count <= 0) continue;

                    foreach (var receiveClient in clientsInMap)
                    {
                        int receiveClientIDAccount = RaceManager.Instance.GetIDAccount(receiveClient);
                        if (receiveClientIDAccount <= 0)
                            continue;

                        if (clientsInMap.Count - 1 <= 0)
                            continue;

                        PacketWriterManager writer = new PacketWriterManager();
                        writer.WriteInt((int)EnumCmdCode.syncPlayerData);
                        writer.WriteListCount(clientsInMap.Count - 1);

                        foreach (var client in clientsInMap)
                        {
                            int idAccount = RaceManager.Instance.GetIDAccount(client);

                            if (idAccount <= 0 || idAccount == receiveClientIDAccount)
                                continue;

                            var accountData = CacheManager.Instance.GetAccountData(idAccount);
                            if (accountData == null || accountData.playerData == null || accountData.playerTransformData == null || accountData.playerStateData == null)
                                continue;

                            writer.WriteInt(accountData.playerData.idAccount);
                            writer.WriteInt(accountData.playerData.level);
                            writer.WriteInt(accountData.playerData.idSchool);
                            writer.WriteInt(accountData.playerData.hair);
                            writer.WriteInt(accountData.playerData.weapon);
                            writer.WriteInt(accountData.playerData.helmet);
                            writer.WriteInt(accountData.playerData.armor);
                            writer.WriteInt(accountData.playerData.legArmor);
                            writer.WriteInt(accountData.playerData.maxHP);
                            writer.WriteInt(accountData.playerData.hp);
                            writer.WriteInt((int)accountData.playerData.currentTile);

                            writer.WriteFloat(accountData.playerTransformData.positionData.x);
                            writer.WriteFloat(accountData.playerTransformData.positionData.y);

                            writer.WriteFloat(accountData.playerTransformData.scaleData.x);

                            writer.WriteInt((int)accountData.playerStateData.stateData);
                            writer.WriteInt((int)accountData.playerStateData.directionData);

                            writer.WriteListCount(accountData.playerStateData.partBodyTransforms.Count);
                            foreach (var partBodyData in accountData.playerStateData.partBodyTransforms)
                            {
                                writer.WriteInt((int)partBodyData.category);
                                writer.WriteInt((int)partBodyData.label);
                            }
                        }

                        byte[] packet = writer.ToArray();
                        await RaceManager.Instance.SendPacketToClient(receiveClient, packet);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Server] Sync other player error: " + ex.Message);
            }

            stopwatch.Stop();
            int sleep = tickMS - (int)stopwatch.ElapsedMilliseconds;

            if (sleep > 0)
                await Task.Delay(sleep, shutdownCts.Token);
        }
    }

    private void ListenForQuit()
    {
        while (!shutdownCts.IsCancellationRequested)
        {
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

    private async Task ReceivePacketFromClient(ClientConnection client, byte[] data)
    {
        try
        {
            PacketReaderManager reader = new PacketReaderManager(data);
            EnumCmdCode cmd = (EnumCmdCode)reader.ReadInt();

            switch (cmd)
            {
                case EnumCmdCode.syncPlayerData:
                    var syncPlayerController = new PlayerController();
                    await syncPlayerController.UpdatePlayerInfo(client, data);
                    break;

                case EnumCmdCode.login:
                    var loginPacket = new LogInRequestPacket();
                    loginPacket.username = reader.ReadString();
                    loginPacket.password = reader.ReadString();

                    var loginController = new LogInController();
                    await loginController.ClickLogIn(client, loginPacket);
                    break;

                case EnumCmdCode.logout:
                    await RaceManager.Instance.SendPacketToClient(client, data);
                    RaceManager.Instance.ForceLogout(client);
                    return;

                case EnumCmdCode.register:
                    var registerPacket = new RegisterRequestPacket();
                    registerPacket.idSchool = reader.ReadInt();
                    registerPacket.nameChar = reader.ReadString();
                    registerPacket.username = reader.ReadString();
                    registerPacket.password = reader.ReadString();
                    registerPacket.hair = reader.ReadInt();
                    registerPacket.blessingPoints = reader.ReadInt();

                    var registerController = new RegisterController();
                    await registerController.ClickRegister(client, registerPacket);
                    break;

                case EnumCmdCode.equipment:
                    var equipmentController = new EquipmentController();
                    await equipmentController.ReadCacheEquipment(client);
                    break;

                case EnumCmdCode.equipmentAttributes:
                    var equipmentAttributesRequestPacket = new ReadAttributesEquipmentRequestPacket();
                    equipmentAttributesRequestPacket.idAccount = reader.ReadInt();
                    equipmentAttributesRequestPacket.id = reader.ReadInt();
                    equipmentAttributesRequestPacket.idItem0_1 = reader.ReadInt();

                    var readAttributesEquipmentController = new ReadAttributesController();
                    await readAttributesEquipmentController.ReadAttributesEquipment(client, equipmentAttributesRequestPacket);
                    break;

                case EnumCmdCode.inventory:
                    var inventoryController = new InventoryController();
                    await inventoryController.ReadCacheInventory(client);
                    break;

                case EnumCmdCode.inventoryAttributes:
                    var inventoryAttributesRequestPacket = new ReadAttributesInventoryRequestPacket();
                    inventoryAttributesRequestPacket.idAccount = reader.ReadInt();
                    inventoryAttributesRequestPacket.id = reader.ReadInt();
                    inventoryAttributesRequestPacket.idItem0 = reader.ReadInt();

                    var readAttributesInventoryController = new ReadAttributesController();
                    await readAttributesInventoryController.ReadAttributesInventory(client, inventoryAttributesRequestPacket);
                    break;

                case EnumCmdCode.equipItem0:
                    var equipItem0RequestPacket = new EquipItem0RequestPacket();
                    equipItem0RequestPacket.idAccount = reader.ReadInt();
                    equipItem0RequestPacket.id = reader.ReadInt();
                    equipItem0RequestPacket.idItem0 = reader.ReadInt();
                    equipItem0RequestPacket.slotName = reader.ReadString();

                    var equipItem0Controller = new ReadAttributesController();
                    await equipItem0Controller.EquipItem0(client, equipItem0RequestPacket);
                    break;

                case EnumCmdCode.outfitSprites:
                    var outfitSpritesController = new EquipmentController();
                    await outfitSpritesController.ReadCacheOutfitSprites(client);
                    break;

                case EnumCmdCode.playerAttackMob:
                    var playerAttackDataPacket = new PlayerAttackDataPacket();
                    playerAttackDataPacket.idAccount = reader.ReadInt();
                    playerAttackDataPacket.aimedMobID = reader.ReadInt();

                    var playerController = new PlayerController(playerAttackDataPacket.idAccount);
                    await playerController.PlayerAttack(client, playerAttackDataPacket);
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
