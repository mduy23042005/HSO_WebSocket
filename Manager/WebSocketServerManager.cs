using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} SyncMob loop started.");

            _ = SyncOtherPlayersLoop();
            Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} SyncOtherPlayers loop started.");
            
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
                    mobData.mobsAI = new MobsController(mobData.posX, mobData.posY, 6, 6);
                    CacheManager.Instance.AddMob(new MobData
                    {
                        mob = mobData.mob,
                        id = mobData.id,
                        posX = mobData.posX,
                        posY = mobData.posY,
                    });
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
    private async Task SyncMobsLoop()
    {
        const int targetTickRate = 20;
        const int tickMS = 1000 / targetTickRate;

        var stopwatch = new System.Diagnostics.Stopwatch();

        while (!shutdownCts.IsCancellationRequested)
        {
            stopwatch.Restart();
            try
            {
                float deltaTime = 1f / targetTickRate;

                //tạo danh sách các map có player
                var mapPlayers = new Dictionary<int, List<ClientConnection>>();

                var clients = RaceManager.Instance.GetAllClients();

                foreach (var client in clients)
                {
                    //kiểm tra player có hợp lệ không
                    int idAccount = RaceManager.Instance.GetIDAccount(client);
                    if (idAccount <= 0) continue;

                    var acc = CacheManager.Instance.GetAccountData(idAccount);
                    if (acc?.playerTransformData == null) continue;

                    //thêm player vào map
                    int idMap = CacheManager.Instance.GetClientMapID(acc.playerData.nameMap);

                    if (!mapPlayers.ContainsKey(idMap))
                        mapPlayers[idMap] = new List<ClientConnection>();

                    mapPlayers[idMap].Add(client);
                }

                foreach (var kv in mapPlayers)
                {
                    int mapId = kv.Key;
                    var clientsInMap = kv.Value;

                    var map = CacheManager.Instance.GetMap(mapId);
                    if (map == null || map.mobsData == null)
                        continue;
                    var mobs = map.mobsData;

                    var mobSnapshots = new List<SyncMobsResultData>();
                    foreach (var mob in mobs)
                    {
                        mob.mobsAI.Attack(deltaTime, map);
                        mob.mobsAI.Move(deltaTime, map);

                        var pos = mob.mobsAI.GetCurrentPosition();
                        mobSnapshots.Add(new SyncMobsResultData
                        {
                            id = mob.id,
                            idMob = mob.mob.IDMob,
                            nameMob = mob.mob.NameMob,
                            posX = pos.X,
                            posY = pos.Y,
                            state = mob.mobsAI.GetState(),
                            idState = mob.mobsAI.GetIDState(),
                            direction = mob.mobsAI.GetDirection(),
                        });
                    }

                    if (mobSnapshots.Count > 0)
                    {
                        PacketWriterManager writer = new PacketWriterManager();
                        writer.WriteInt((int)EnumCmdCode.syncMobData);
                        writer.WriteListCount(mobSnapshots.Count);

                        foreach (var mobData in mobSnapshots)
                        {
                            writer.WriteInt(mobData.id);
                            writer.WriteInt(mobData.idMob);
                            writer.WriteString(mobData.nameMob);
                            writer.WriteFloat(mobData.posX);
                            writer.WriteFloat(mobData.posY);
                            writer.WriteString(mobData.state);
                            writer.WriteInt(mobData.idState);
                            writer.WriteInt(mobData.direction);
                        }

                        byte[] packet = writer.ToArray();

                        // gửi CHỈ cho client trong map này
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
                Console.WriteLine("[SyncMob] Error: " + ex.Message);
            }

            stopwatch.Stop();
            int sleep = tickMS - (int)stopwatch.ElapsedMilliseconds;
            if (sleep > 0)
                await Task.Delay(sleep, shutdownCts.Token);
        }
    }
    private async Task SyncOtherPlayersLoop()
    {
        const int targetTickRate = 70;
        const int tickMS = 1000 / targetTickRate;

        var stopwatch = new System.Diagnostics.Stopwatch();

        while (!shutdownCts.IsCancellationRequested)
        {
            stopwatch.Restart();

            try
            {
                var playerSnapshots = new List<OtherPlayerSyncData>();

                var clients = RaceManager.Instance.GetAllClients();

                foreach (var client in clients)
                {
                    int idAccount = RaceManager.Instance.GetIDAccount(client);
                    if (idAccount <= 0)
                        continue;

                    var accountData = CacheManager.Instance.GetAccountData(idAccount);

                    if (accountData == null || accountData.playerData == null || accountData.playerTransformData == null || accountData.playerStateData == null)
                        continue;

                    playerSnapshots.Add(new OtherPlayerSyncData
                    {
                        otherPlayerData = accountData.playerData,
                        otherPlayerTransformData = accountData.playerTransformData,
                        otherPlayerStateData = accountData.playerStateData
                    });
                }

                if (playerSnapshots.Count > 0)
                {
                    var syncPacket = new
                    {
                        cmd = EnumCmdCode.syncPlayerData,
                        otherPlayersData = playerSnapshots
                    };

                    PacketWriterManager writer = new PacketWriterManager();
                    writer.WriteInt((int)syncPacket.cmd);
                    writer.WriteListCount(syncPacket.otherPlayersData.Count);
                    foreach (var otherPlayer in syncPacket.otherPlayersData)
                    {
                        writer.WriteInt(otherPlayer.otherPlayerData.idAccount);
                        writer.WriteString(otherPlayer.otherPlayerData.nameChar);
                        writer.WriteInt(otherPlayer.otherPlayerData.level);
                        writer.WriteInt(otherPlayer.otherPlayerData.idSchool);
                        writer.WriteInt(otherPlayer.otherPlayerData.hair);
                        writer.WriteInt(otherPlayer.otherPlayerData.weapon);
                        writer.WriteInt(otherPlayer.otherPlayerData.helmet);
                        writer.WriteInt(otherPlayer.otherPlayerData.armor);
                        writer.WriteInt(otherPlayer.otherPlayerData.legArmor);
                        writer.WriteInt(otherPlayer.otherPlayerData.gloves);
                        writer.WriteInt(otherPlayer.otherPlayerData.shoes);
                        writer.WriteInt(otherPlayer.otherPlayerData.ring1);
                        writer.WriteInt(otherPlayer.otherPlayerData.ring2);
                        writer.WriteInt(otherPlayer.otherPlayerData.necklace);
                        writer.WriteInt(otherPlayer.otherPlayerData.medal);
                        writer.WriteInt(otherPlayer.otherPlayerData.cloak);
                        writer.WriteInt(otherPlayer.otherPlayerData.wing);
                        writer.WriteInt(otherPlayer.otherPlayerData.skinWing);
                        writer.WriteInt(otherPlayer.otherPlayerData.mounts);
                        writer.WriteInt(otherPlayer.otherPlayerData.pet);
                        writer.WriteInt(otherPlayer.otherPlayerData.skin);

                        writer.WriteFloat(otherPlayer.otherPlayerTransformData.positionData.x);
                        writer.WriteFloat(otherPlayer.otherPlayerTransformData.positionData.y);
                        writer.WriteFloat(otherPlayer.otherPlayerTransformData.positionData.z);
                        writer.WriteFloat(otherPlayer.otherPlayerTransformData.scaleData.x);
                        writer.WriteFloat(otherPlayer.otherPlayerTransformData.scaleData.y);
                        writer.WriteFloat(otherPlayer.otherPlayerTransformData.scaleData.z);

                        writer.WriteInt((int)otherPlayer.otherPlayerStateData.stateData);
                        writer.WriteInt((int)otherPlayer.otherPlayerStateData.directionData);
                        writer.WriteListCount(otherPlayer.otherPlayerStateData.partBodyTransforms.Count);
                        foreach (var partBodyData in otherPlayer.otherPlayerStateData.partBodyTransforms)
                        {
                            writer.WriteString(partBodyData.category);
                            writer.WriteString(partBodyData.label);
                            writer.WriteFloat(partBodyData.positionData.x);
                            writer.WriteFloat(partBodyData.positionData.y);
                            writer.WriteFloat(partBodyData.positionData.z);
                            writer.WriteFloat(partBodyData.rotationData.x);
                            writer.WriteFloat(partBodyData.rotationData.y);
                            writer.WriteFloat(partBodyData.rotationData.z);
                            writer.WriteFloat(partBodyData.scaleData.x);
                            writer.WriteFloat(partBodyData.scaleData.y);
                            writer.WriteFloat(partBodyData.scaleData.z);
                            writer.WriteFloat(partBodyData.colorData.r);
                            writer.WriteFloat(partBodyData.colorData.g);
                            writer.WriteFloat(partBodyData.colorData.b);
                            writer.WriteFloat(partBodyData.colorData.a);
                        }
                    }

                    await RaceManager.Instance.SendPacketToAllClients(writer.ToArray());
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SyncPlayers] Error: " + ex.Message);
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
                    var playerData = new PlayerData
                    {
                        nameMap = reader.ReadString(),
                        idAccount = reader.ReadInt(),
                        nameChar = reader.ReadString(),
                        level = reader.ReadInt(),
                        idSchool = reader.ReadInt(),
                        hair = reader.ReadInt(),
                        weapon = reader.ReadInt(),
                        helmet = reader.ReadInt(),
                        armor = reader.ReadInt(),
                        legArmor = reader.ReadInt(),
                        gloves = reader.ReadInt(),
                        shoes = reader.ReadInt(),
                        ring1 = reader.ReadInt(),
                        ring2 = reader.ReadInt(),
                        necklace = reader.ReadInt(),
                        medal = reader.ReadInt(),
                        cloak = reader.ReadInt(),
                        wing = reader.ReadInt(),
                        skinWing = reader.ReadInt(),
                        mounts = reader.ReadInt(),
                        pet = reader.ReadInt(),
                        skin = reader.ReadInt(),
                    };

                    var playerTransformData = new PlayerTransformData 
                    { 
                        positionData = new PositionData
                        {
                            x = reader.ReadFloat(),
                            y = reader.ReadFloat(),
                            z = reader.ReadFloat()
                        },
                        scaleData = new ScaleData
                        {
                            x = reader.ReadFloat(),
                            y = reader.ReadFloat(),
                            z = reader.ReadFloat()
                        }
                    };
                        
                    var playerStateData = new PlayerStateData();
                    playerStateData.stateData = (PlayerState)reader.ReadInt();
                    playerStateData.directionData = (Direction)reader.ReadInt();
                    playerStateData.partBodyTransforms = new List<PartBodyData>();
                    int countPartBodyTransform = reader.ReadInt();
                    for (int i = 0; i < countPartBodyTransform; i++)
                    {
                        playerStateData.partBodyTransforms.Add(new PartBodyData
                        {
                            category = reader.ReadString(),
                            label = reader.ReadString(),
                            positionData = new PositionData
                            {
                                x = reader.ReadFloat(),
                                y = reader.ReadFloat(),
                                z = reader.ReadFloat()
                            },
                            rotationData = new RotationData
                            {
                                x = reader.ReadFloat(),
                                y = reader.ReadFloat(),
                                z = reader.ReadFloat()
                            },
                            scaleData = new ScaleData
                            {
                                x = reader.ReadFloat(),
                                y = reader.ReadFloat(),
                                z = reader.ReadFloat()
                            },
                            colorData = new ColorData
                            {
                                r = reader.ReadFloat(),
                                g = reader.ReadFloat(),
                                b = reader.ReadFloat(),
                                a = reader.ReadFloat()
                            },
                        });
                    }

                    var idMap = CacheManager.Instance.GetClientMapID(playerData.nameMap);
                    var map = CacheManager.Instance.GetMap(idMap);
                    var accountData = CacheManager.Instance.GetAccountData(playerData.idAccount);
                    MapController mapController = new MapController();
                    if (!mapController.IsWalkable(map, playerTransformData.positionData.x, playerTransformData.positionData.y))
                    {
                        if (accountData != null && accountData.playerTransformData != null)
                        {
                            var callBackPacket = new
                            {
                                cmd = EnumCmdCode.syncCallBack,
                                positionData = new
                                {
                                    x = accountData.playerTransformData.positionData.x,
                                    y = accountData.playerTransformData.positionData.y,
                                    z = accountData.playerTransformData.positionData.z
                                },
                                scaleData = new
                                {
                                    x = accountData.playerTransformData.scaleData.x,
                                    y = accountData.playerTransformData.scaleData.y,
                                    z = accountData.playerTransformData.scaleData.z
                                }
                            };

                            PacketWriterManager writer = new PacketWriterManager();
                            writer.WriteInt((int)callBackPacket.cmd);

                            writer.WriteFloat(callBackPacket.positionData.x);
                            writer.WriteFloat(callBackPacket.positionData.y);
                            writer.WriteFloat(callBackPacket.positionData.z);

                            writer.WriteFloat(callBackPacket.scaleData.x);
                            writer.WriteFloat(callBackPacket.scaleData.y);
                            writer.WriteFloat(callBackPacket.scaleData.z);

                            await RaceManager.Instance.SendPacketToClient(client, writer.ToArray());
                        }
                        return;
                    }

                    if (accountData != null)
                    {
                        accountData.playerData = playerData;
                        accountData.playerTransformData = playerTransformData;
                        accountData.playerStateData = playerStateData;
                    }
                    break;

                case EnumCmdCode.syncAtkData:

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
