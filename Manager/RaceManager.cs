using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed class RaceManager
{
    private static readonly Lazy<RaceManager> lazyInstance = new Lazy<RaceManager>(() => new RaceManager());
    private readonly Dictionary<WebSocket, SemaphoreSlim> socketSendLocks = new Dictionary<WebSocket, SemaphoreSlim>();


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
                socketSendLocks[webSocket] = new SemaphoreSlim(1, 1);
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

            if (socketSendLocks.TryGetValue(webSocket, out var semaphore))
            {
                semaphore.Dispose();
                socketSendLocks.Remove(webSocket);
            }
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

        List<WebSocket> snapshot = CreateClientSnapshot();
        List<Task> tasks = new List<Task>();

        foreach (WebSocket client in snapshot)
        {
            if (client == excludedClient)
                continue;

            if (client.State == WebSocketState.Open)
            {
                tasks.Add(SendPacketToClientAsync(client, packet)
                    .ContinueWith(t =>
                        {
                            if (t.Exception != null)
                                Console.WriteLine("[BROADCAST ERROR] " + t.Exception.InnerException);
                        }));
            }
        }
        await Task.WhenAll(tasks);
    }
    public async Task SendPacketToClientAsync(WebSocket targetClient, string packet)
    {
        if (targetClient == null)
            return;

        if (targetClient.State != WebSocketState.Open)
            return;

        if (string.IsNullOrEmpty(packet))
            return;

        SemaphoreSlim sendLock;
        lock (clientCollectionLock)
        {
            if (!socketSendLocks.TryGetValue(targetClient, out sendLock))
                return;
        }
        await sendLock.WaitAsync();

        try
        {
            byte[] packetBytes = Encoding.UTF8.GetBytes(packet);

            await targetClient.SendAsync(new ArraySegment<byte>(packetBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[SEND ERROR] " + ex.ToString());
            UnregisterClient(targetClient);
        }
        finally 
        { 
            sendLock.Release();  
        }
    }

    public void RemoveDisconnectedClients()
    {
        lock (clientCollectionLock)
        {
            foreach (var socket in new List<WebSocket>(connectedClients))
            {
                if (socket == null || socket.State != WebSocketState.Open)
                {
                    connectedClients.Remove(socket);
                    clientAccountMapping.Remove(socket);

                    if (socketSendLocks.TryGetValue(socket, out var semaphore))
                    {
                        semaphore.Dispose();
                        socketSendLocks.Remove(socket);
                    }
                }
            }
        }
    }
}
