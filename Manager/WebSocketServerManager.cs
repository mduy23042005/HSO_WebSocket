using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
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

class WebSocketServerManager
{
    static async Task Main(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://+:55556/");
        listener.Start();
        Console.WriteLine("Web Socket Server started at ws://+:55556/");
        await WebAPIManager.Instance.InitAPI();
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

                string IPClient = context.Request.RemoteEndPoint?.ToString() ?? "Unknown";
                var client = new ClientConnection(wsContext.WebSocket, IPClient);
                Console.WriteLine($"New socket connected! IP Client: {IPClient}");

                RaceManager.Instance.RegisterClient(client);
                _ = HandleClient(client);
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
                    result = await client.socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await client.socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        return;
                    }

                    messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                } while (!result.EndOfMessage);

                string fullMessage = messageBuffer.ToString();
                messageBuffer.Clear();

                await ReceivePacketFromClient(client, fullMessage);
            }
        }
        catch (WebSocketException wsex)
        {
            Console.WriteLine($"Client is disconnected by WebSocket Exception! {client.idConnection} | {client.ipRemote} | reason: {wsex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client is disconnected by Exception! {client.idConnection} | {client.ipRemote} | exception: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"Client disconnected! {client.idConnection} | {client.ipRemote} | state: {client.socket.State}");
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
                    await RaceManager.Instance.SendPacketToAllClients(json, client);
                    break;

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
                    await equipmentController.ReadDatabaseEquipment(client);
                    break;

                case "equipmentAttributes":
                    var equipmentAttributesRequestPacket = JsonConvert.DeserializeObject<ReadAttributesEquipmentRequestPacket>(json);
                    var readAttributesEquipmentController = new ReadAttributesController();
                    await readAttributesEquipmentController.ReadAttributesEquipment(client, equipmentAttributesRequestPacket.idAccount, equipmentAttributesRequestPacket.id);
                    break;

                case "inventory":
                    var inventoryPacket = JsonConvert.DeserializeObject<EquipmentRequestPacket>(json);
                    var inventoryController = new InventoryController();
                    await inventoryController.ReadDatabaseInventory(client);
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
                    await outfitSpritesController.ReadDatabaseOutfitSprites(client);
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
