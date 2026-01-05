using Newtonsoft.Json;
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
        listener.Prefixes.Add("https://localhost:55556/");
        listener.Start();
        Console.WriteLine("Web Socket Server started at wss://localhost:55556/");
        await WebAPIManager.Instance.TestAPI();

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
            RaceManager.Instance.UnregisterClient(socket);
            socket.Dispose();
        }
    }

    public static async Task ReceivePacketFromClient(WebSocket socket, string json)
    {
        dynamic basePacket = JsonConvert.DeserializeObject(json);
        string cmd = basePacket.cmd;

        switch (cmd)
        {
            case "syncData":
                await RaceManager.Instance.SendPacketToAllClientsAsync(json, socket);
                break;

            case "login":
                var loginPacket = JsonConvert.DeserializeObject<LogInRequestPacket>(json);
                var loginController = new LogInController();
                loginController.ClickLogIn(socket, loginPacket.username, loginPacket.password);
                break;
            // từ từ làm tiếp
            case "logout":
                await RaceManager.Instance.SendPacketToAllClientsAsync(json, socket);
                Console.WriteLine($"Received log out packet: {json}");
                break;

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
                //Tạm thời không làm gì với các packet khác
                break;
        }
    }
}
