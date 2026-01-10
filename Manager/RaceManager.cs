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
    private readonly HashSet<WebSocket> logoutSockets = new HashSet<WebSocket>();

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
    public int GetIDAccount(WebSocket socket)
    {
        if (socket == null)
            return 0;

        lock (clientCollectionLock)
        {
            return clientAccountMapping.TryGetValue(socket, out var idAccount) ? idAccount : 0;
        }
    }

    private List<WebSocket> CreateClientSnapshot()
    {
        lock (clientCollectionLock)
        {
            return new List<WebSocket>(connectedClients);
        }
    }

    public async Task SendPacketToAllClients(string packet, WebSocket excludedClient = null)
    {
        if (string.IsNullOrEmpty(packet))
            return;

        List<WebSocket> snapshot = CreateClientSnapshot();
        List<Task> tasks = new List<Task>();

        foreach (WebSocket client in snapshot)
        {
            if (client == excludedClient)
                continue;

            if (client.State != WebSocketState.Open)
                continue;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await SendPacketToClient(client, packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[BROADCAST ERROR] " + ex.Message);
                    MarkLogOut(client);
                }
            }));
        }

        await Task.WhenAll(tasks);
    }
    public async Task SendPacketToClient(WebSocket targetClient, string packet)
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

        try
        {
            await sendLock.WaitAsync();
            if (targetClient.State != WebSocketState.Open)
                return;

            byte[] packetBytes = Encoding.UTF8.GetBytes(packet);

            await targetClient.SendAsync(new ArraySegment<byte>(packetBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (ObjectDisposedException)
        {
            MarkLogOut(targetClient);
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine("[SEND WS ERROR] " + ex.Message);
            MarkLogOut(targetClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[SEND ERROR] " + ex);
            MarkLogOut(targetClient);
        }
        finally
        {
            try
            {
                sendLock.Release();
            }
            catch { }
        }
    }

    public void MarkLogOut(WebSocket socket)
    {
        if (socket == null)
            return;

        lock (clientCollectionLock)
        {
            logoutSockets.Add(socket);
        }
    }

    public void RemoveDisconnectedClients()
    {
        List<WebSocket> needCleanup = new List<WebSocket>();

        lock (clientCollectionLock)
        {
            foreach (var socket in connectedClients)
            {
                if (socket == null)
                {
                    needCleanup.Add(socket);
                    continue;
                }

                // logout chủ động
                if (logoutSockets.Contains(socket))
                {
                    needCleanup.Add(socket);
                    continue;
                }

                // disconnect bất ngờ
                if (socket.State != WebSocketState.Open)
                {
                    needCleanup.Add(socket);
                }
            }
        }

        foreach (var socket in needCleanup)
        {
            if (socket == null)
                continue;

            UnregisterClient(socket);

            if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
            {
                try
                {
                    socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Logout", CancellationToken.None).Wait();
                }
                catch { }
            }
            socket.Dispose();
        }
    }
}
