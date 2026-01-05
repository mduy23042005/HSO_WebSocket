using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed class RaceManager
{
    private static readonly Lazy<RaceManager> lazyInstance = new Lazy<RaceManager>(() => new RaceManager());

    //Tìm bug
    private readonly Dictionary<WebSocket, int> socketSendCounter = new Dictionary<WebSocket, int>();

    public static RaceManager Instance => lazyInstance.Value;

    private readonly List<WebSocket> connectedClients;
    private readonly Dictionary<WebSocket, int> clientAccountMapping;

    private readonly object clientCollectionLock;

    private RaceManager()
    {
        connectedClients = new List<WebSocket>();
        clientAccountMapping = new Dictionary<WebSocket, int>();
        clientCollectionLock = new object();
    }

    public void RegisterClient(WebSocket webSocket)
    {
        if (webSocket == null)
            throw new ArgumentNullException(nameof(webSocket));

        lock (clientCollectionLock)
        {
            if (!connectedClients.Contains(webSocket))
            {
                connectedClients.Add(webSocket);
            }
        }
    }
    public void UnregisterClient(WebSocket webSocket)
    {
        if (webSocket == null)
            return;

        lock (clientCollectionLock)
        {
            connectedClients.Remove(webSocket);
            clientAccountMapping.Remove(webSocket);
            socketSendCounter.Remove(webSocket);
        }
    }

    public void BindAccountToClient(WebSocket webSocket, int idAccount)
    {
        if (webSocket == null)
            throw new ArgumentNullException(nameof(webSocket));

        lock (clientCollectionLock)
        {
            clientAccountMapping[webSocket] = idAccount;
        }
    }
    public bool TryGetIDAccount(WebSocket webSocket, out int idAccount)
    {
        lock (clientCollectionLock)
        {
            return clientAccountMapping.TryGetValue(webSocket, out idAccount);
        }
    }
    public int GetConnectedClientCount()
    {
        lock (clientCollectionLock)
        {
            return connectedClients.Count;
        }
    }

    private List<WebSocket> CreateClientSnapshot()
    {
        lock (clientCollectionLock)
        {
            return new List<WebSocket>(connectedClients);
        }
    }

    public async Task SendPacketToAllClientsAsync(string packet, WebSocket excludedClient = null)
    {
        if (string.IsNullOrEmpty(packet))
            return;

        byte[] packetBytes = Encoding.UTF8.GetBytes(packet);
        List<WebSocket> snapshot = CreateClientSnapshot();

        List<Task> sendTasks = new List<Task>();

        foreach (WebSocket client in snapshot)
        {
            if (client == excludedClient)
                continue;

            if (client.State == WebSocketState.Open)
            {
                sendTasks.Add(client.SendAsync(new ArraySegment<byte>(packetBytes), WebSocketMessageType.Text, true, CancellationToken.None));
            }
        }

        await Task.WhenAll(sendTasks);
    }
    public async Task SendPacketToClientAsync(WebSocket targetClient, string packet)
    {
        if (targetClient == null)
            return;

        if (targetClient.State != WebSocketState.Open)
            return;

        if (string.IsNullOrEmpty(packet))
            return;

        lock (clientCollectionLock)
        {
            if (!socketSendCounter.ContainsKey(targetClient))
                socketSendCounter[targetClient] = 0;

            socketSendCounter[targetClient]++;
        }

        try
        {
            byte[] packetBytes = Encoding.UTF8.GetBytes(packet);

            await targetClient.SendAsync(new ArraySegment<byte>(packetBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[SEND ERROR] " + ex.ToString());
            throw;
        }
        finally
        {
            lock (clientCollectionLock)
            {
                socketSendCounter[targetClient]--;
            }
        }
    }

    public void RemoveDisconnectedClients()
    {
        lock (clientCollectionLock)
        {
            connectedClients.RemoveAll(socket => socket == null || socket.State != WebSocketState.Open);
        }
    }
}
