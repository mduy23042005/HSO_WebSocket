using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed class RaceManager
{
    private static readonly Lazy<RaceManager> lazyInstance = new Lazy<RaceManager>(() => new RaceManager());
    private readonly Dictionary<ClientConnection, SemaphoreSlim> socketSendLocks = new Dictionary<ClientConnection, SemaphoreSlim>();
    private readonly HashSet<ClientConnection> logoutClients = new HashSet<ClientConnection>();

    public static RaceManager Instance => lazyInstance.Value;

    private readonly List<ClientConnection> connectedClients;
    private readonly Dictionary<ClientConnection, int> clientAccountMapping;

    private readonly object clientCollectionLock;

    private RaceManager()
    {
        connectedClients = new List<ClientConnection>();
        socketSendLocks = new Dictionary<ClientConnection, SemaphoreSlim>();
        clientAccountMapping = new Dictionary<ClientConnection, int>();
        logoutClients = new HashSet<ClientConnection>();
        clientCollectionLock = new object();
    }

    public void RegisterClient(ClientConnection client)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        lock (clientCollectionLock)
        {
            if (!connectedClients.Contains(client))
            {
                connectedClients.Add(client);
                socketSendLocks[client] = new SemaphoreSlim(1, 1);
            }
        }
    }
    public void UnregisterClient(ClientConnection client)
    {
        if (client == null)
            return;

        lock (clientCollectionLock)
        {
            connectedClients.Remove(client);
            clientAccountMapping.Remove(client);

            if (socketSendLocks.TryGetValue(client, out var semaphore))
            {
                semaphore.Dispose();
                socketSendLocks.Remove(client);
            }
        }
    }

    public void BindAccountToClient(ClientConnection client, int idAccount)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        lock (clientCollectionLock)
        {
            clientAccountMapping[client] = idAccount;
        }
    }
    public int GetIDAccount(ClientConnection client)
    {
        if (client == null)
            return 0;

        lock (clientCollectionLock)
        {
            return clientAccountMapping.TryGetValue(client, out var idAccount) ? idAccount : 0;
        }
    }

    private List<ClientConnection> CreateClientSnapshot()
    {
        lock (clientCollectionLock)
        {
            return new List<ClientConnection>(connectedClients);
        }
    }

    public async Task SendPacketToAllClients(string packet, ClientConnection excludedClient = null)
    {
        if (string.IsNullOrEmpty(packet))
            return;

        List<ClientConnection> snapshot = CreateClientSnapshot();
        List<Task> tasks = new List<Task>();

        foreach (ClientConnection client in snapshot)
        {
            if (client == excludedClient)
                continue;

            if (client.socket.State != WebSocketState.Open)
                continue;

            tasks.Add(SendPacketToClient(client, packet));
        }

        await Task.WhenAll(tasks);
    }
    public async Task SendPacketToClient(ClientConnection targetClient, string packet)
    {
        if (targetClient == null)
            return;

        if (targetClient.socket.State != WebSocketState.Open)
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
            if (targetClient.socket.State != WebSocketState.Open)
                return;

            byte[] packetBytes = Encoding.UTF8.GetBytes(packet);

            if (targetClient.socket.State == WebSocketState.Open)
            {
                await targetClient.socket.SendAsync(new ArraySegment<byte>(packetBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
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

    public void MarkLogOut(ClientConnection socket)
    {
        if (socket == null)
            return;

        lock (clientCollectionLock)
        {
            logoutClients.Add(socket);
        }
    }

    public void RemoveDisconnectedClients()
    {
        List<ClientConnection> needCleanup = new List<ClientConnection>();

        lock (clientCollectionLock)
        {
            foreach (var client in connectedClients)
            {
                if (client == null)
                {
                    needCleanup.Add(client);
                    continue;
                }

                // logout chủ động
                if (logoutClients.Contains(client))
                {
                    needCleanup.Add(client);
                    continue;
                }

                // disconnect bất ngờ
                if (client.socket.State != WebSocketState.Open)
                {
                    needCleanup.Add(client);
                }
            }
        }

        foreach (var client in needCleanup)
        {
            if (client == null)
                continue;

            if (client.socket.State == WebSocketState.Open || client.socket.State == WebSocketState.CloseReceived)
            {
                try
                {
                    client.socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Logout", CancellationToken.None).Wait();
                }
                catch { }
            }

            UnregisterClient(client);
            client.socket.Dispose();

            lock (clientCollectionLock)
            {
                logoutClients.Remove(client);
            }
        }
    }
}
