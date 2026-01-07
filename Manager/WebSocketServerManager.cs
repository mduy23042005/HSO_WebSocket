using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class WebSocketServerManager
{
    static async Task Main(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://+:55556/");
        listener.Start();
        Console.WriteLine("Web Socket Server started at ws://+:55556/");
        await WebAPIManager.Instance.TestAPI();
        // 
        var cts = new CancellationTokenSource();
        _ = StartCleanupLoop(cts.Token);

        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();

            // Kiểm tra đây có phải WebSocket request không
            if (context.Request.IsWebSocketRequest)
            {
                HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                Console.WriteLine("New client connected!");
                RaceManager.Instance.RegisterClient(wsContext.WebSocket);
                _ = HandleClient(wsContext.WebSocket);
            }
            else
            {
                // Nếu không phải WebSocket thì trả về 400
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }
    static async Task StartCleanupLoop(CancellationToken token)
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

    static async Task HandleClient(WebSocket socket)
    {
        var buffer = new byte[2048];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                await ReceivePacketFromClient(socket, message);
            }
        }
        catch (WebSocketException wsex)
        {
            Console.WriteLine("Client disconnected unexpectedly: " + wsex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Other exception: " + ex.Message);
        }
        finally
        {
            if (socket.State != WebSocketState.Closed)
            {
                RaceManager.Instance.MarkLogOut(socket);
            }
        }
    }

    public static async Task ReceivePacketFromClient(WebSocket socket, string json)
    {
        JObject basePacket = JObject.Parse(json);
        string cmd = basePacket["cmd"]?.ToString();

        switch (cmd)
        {
            case "syncData":
                await RaceManager.Instance.SendPacketToAllClients(json, socket);
                break;

            case "login":
                var loginPacket = JsonConvert.DeserializeObject<LogInRequestPacket>(json);
                var loginController = new LogInController();
                loginController.ClickLogIn(socket, loginPacket.username, loginPacket.password);
                break;
            case "logout":
                await RaceManager.Instance.SendPacketToAllClients(json, socket);
                await RaceManager.Instance.SendPacketToClient(socket, json);
                RaceManager.Instance.MarkLogOut(socket);
                return;

            case "register":
                var registerPacket = JsonConvert.DeserializeObject<RegisterRequestPacket>(json);
                var registerController = new RegisterController();
                registerController.ClickRegister(socket, registerPacket.idSchool, registerPacket.username, registerPacket.password, registerPacket.nameChar, registerPacket.hair, registerPacket.blessingPoints);
                break;

            case "equipment":
                var equipmentPacket = JsonConvert.DeserializeObject<EquipmentRequestPacket>(json);
                var equipmentController = new EquipmentController();
                await equipmentController.ReadDatabaseEquipment(socket);
                break;

            case "equipmentAttributes":
                var equipmentAttributesRequestPacket = JsonConvert.DeserializeObject<ReadAttributesEquipmentRequestPacket>(json);
                var readAttributesEquipmentController = new ReadAttributesController();
                await readAttributesEquipmentController.ReadAttributesEquipment(socket, equipmentAttributesRequestPacket.idAccount, equipmentAttributesRequestPacket.id);
                break;

            case "inventory":
                var inventoryPacket = JsonConvert.DeserializeObject<EquipmentRequestPacket>(json);
                var inventoryController = new InventoryController();
                await inventoryController.ReadDatabaseInventory(socket);
                break;

            case "inventoryAttributes":
                var inventoryAttributesRequestPacket = JsonConvert.DeserializeObject<ReadAttributesInventoryRequestPacket>(json);
                var readAttributesInventoryController = new ReadAttributesController();
                await readAttributesInventoryController.ReadAttributesInventory(socket, inventoryAttributesRequestPacket.idAccount, inventoryAttributesRequestPacket.id);
                break;

            case "equipItem0":
                var equipItem0RequestPacket = JsonConvert.DeserializeObject<EquipItem0RequestPacket>(json);
                var equipItem0Controller = new ReadAttributesController();
                await equipItem0Controller.EquipItem0(socket, equipItem0RequestPacket.idAccount, equipItem0RequestPacket.id, equipItem0RequestPacket.idItem0, equipItem0RequestPacket.slotName);
                break;

            case "outfitSprites":
                var outfitSpritesPacket = JsonConvert.DeserializeObject<EquipmentRequestPacket>(json);
                var outfitSpritesController = new EquipmentController();
                await outfitSpritesController.ReadDatabaseOutfitSprites(socket);
                break;

            default:
                break;
        }
    }
}
