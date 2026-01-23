using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public struct PacketHeader
{
    public int packetSize; // Includes header + payload size
    public int senderId;
    public short packetId;

    public void SetFromRawData(byte[] data, int offset = 0)
    {
        int index = offset;

        packetSize = System.BitConverter.ToInt32(data, index);
        index += sizeof(int);

        senderId = System.BitConverter.ToInt32(data, index);
        index += sizeof(int);

        packetId = System.BitConverter.ToInt16(data, index);
        index += sizeof(short);
    }

    static public int SizeOf()
    {
        return sizeof(int) + sizeof(int) + sizeof(short);
    }
}

public class Packet
{
    public byte[] SerializedData;

    public void SetHeader(PacketHeader header)
    {
        Array.Resize(ref SerializedData, PacketHeader.SizeOf());

        int index = 0;
        Array.Copy(System.BitConverter.GetBytes(header.packetSize), 0, SerializedData, index, sizeof(int));
        index += sizeof(int);
        
        Array.Copy(System.BitConverter.GetBytes(header.senderId), 0, SerializedData, index, sizeof(int));
        index += sizeof(int);
        
        Array.Copy(System.BitConverter.GetBytes(header.packetId), 0, SerializedData, index, sizeof(short));
        index += sizeof(short);
    }

    public void GetHeader(ref PacketHeader header)
    {
        if (SerializedData.Length < PacketHeader.SizeOf())
        {
            return;
        }

        int index = 0;
        header.packetSize = System.BitConverter.ToInt32(SerializedData, index);
        index += sizeof(int);

        header.senderId = System.BitConverter.ToInt32(SerializedData, index);
        index += sizeof(int);

        header.packetId = System.BitConverter.ToInt16(SerializedData, index);
        index += sizeof(short);
    }

    public void SetPayload(byte[] payload)
    {
        Array.Resize(ref SerializedData, PacketHeader.SizeOf() + payload.Length);
        Array.Copy(payload, 0, SerializedData, PacketHeader.SizeOf(), payload.Length);
    }

    public void GetPayload(ref byte[] payload)
    {
        if (SerializedData.Length <= PacketHeader.SizeOf())
        {
            payload = new byte[0];
            return;
        }

        int payloadSize = SerializedData.Length - PacketHeader.SizeOf();
        payload = new byte[payloadSize];
        Array.Copy(SerializedData, PacketHeader.SizeOf(), payload, 0, payloadSize);
    }

    public ReadOnlyMemory<byte> GetPayload()
    {
        return new ReadOnlyMemory<byte>(SerializedData, PacketHeader.SizeOf(), SerializedData.Length - PacketHeader.SizeOf());
    }

    public void SetHeaderAndPayload(PacketHeader header, byte[] payload)
    {
        Array.Resize(ref SerializedData, PacketHeader.SizeOf() + payload.Length);

        int index = 0;

        Array.Copy(System.BitConverter.GetBytes(header.packetSize), 0, SerializedData, index, sizeof(int));
        index += sizeof(int);

        Array.Copy(System.BitConverter.GetBytes(header.senderId), 0, SerializedData, index, sizeof(int));
        index += sizeof(int);

        Array.Copy(System.BitConverter.GetBytes(header.packetId), 0, SerializedData, index, sizeof(short));
        index += sizeof(short);

        Array.Copy(payload, 0, SerializedData, index, payload.Length);
    }
}

public class AsyncTcpClient
{
    private TcpClient client;
    private NetworkStream stream;

    private byte[] receiveBuffer;
    private int reciveBufferLen;

    private CancellationTokenSource recieveCTS;

    private SemaphoreSlim sendLock;

    private Dictionary<int, TaskCompletionSource<Packet>> pendingRequests = new();

    public event Action<Packet> OnPacketReceived;

    public AsyncTcpClient(int recvBufferSize = 8192)
    {
        client = new TcpClient();
        receiveBuffer = new byte[recvBufferSize];
        reciveBufferLen = 0;
    }

    public async Task Connect(string ipAddress, int port)
    {
        try
        {
            if (client.Connected)
            {
                Logger.Warn("AsyncTcpClient Connect: Already connected.");
                return;
            }

            await client.ConnectAsync(ipAddress, port);

            stream = client.GetStream();
            recieveCTS = new CancellationTokenSource();
            reciveBufferLen = 0;
            sendLock = new SemaphoreSlim(1, 1);
            _ = HandleRecieve();
        }
        catch (Exception e)
        {
            Logger.Error($"AsyncTcpClient Connect Exception: {e}");
        }
    }

    private async Task HandleRecieve()
    {
        try
        {
            while (client.Connected && !recieveCTS.Token.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(receiveBuffer, reciveBufferLen, receiveBuffer.Length - reciveBufferLen, recieveCTS.Token);
                if (bytesRead == 0)
                {
                    // Connection closed
                    break;
                }

                reciveBufferLen += bytesRead;

                int offset = 0;
                while (reciveBufferLen - offset >= PacketHeader.SizeOf())
                {
                    PacketHeader header = new PacketHeader();
                    header.SetFromRawData(receiveBuffer, offset);

                    if (reciveBufferLen - offset < header.packetSize)
                    {
                        // Not enough data for a full packet
                        break;
                    }

                    Packet packet = new Packet();
                    Array.Resize(ref packet.SerializedData, header.packetSize);
                    Array.Copy(receiveBuffer, offset, packet.SerializedData, 0, header.packetSize);

                    if (pendingRequests.TryGetValue(header.senderId, out var tcs))
                    {
                        tcs.SetResult(packet);
                        pendingRequests.Remove(header.senderId);
                    }
                    else
                    {
                        OnPacketReceived?.Invoke(packet);
                    }
                    
                    offset += header.packetSize;
                }

                if (offset > 0)
                {
                    Buffer.BlockCopy(receiveBuffer, offset, receiveBuffer, 0, reciveBufferLen - offset);
                    reciveBufferLen -= offset;
                }
            }
        }
        catch (OperationCanceledException e)
        {
            Logger.Info($"AsyncTcpClient HandleReceive Canceled: {e}");
        }
        catch (IOException)
        {
            // ignore
        }
        catch (Exception e)
        {
            Logger.Error($"AsyncTcpClient HandleReceive Exception: {e}");
        }
    }

    public async Task SendPacket(Packet packet)
    {
        await sendLock.WaitAsync();

        try
        {
            if (client.Connected)
            {
                await stream.WriteAsync(packet.SerializedData, 0, packet.SerializedData.Length);
            }
        }
        finally
        {
            sendLock.Release();
        }
    }

    public async Task SendPackets(params Packet[] packets)
    {
        if (!client.Connected)
        {
            return;
        }

        int totalSize = 0;
        foreach (var p in packets)
        {
            totalSize += p.SerializedData.Length;
        }

        // ArrayPool에서 버퍼 가져오기
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(totalSize);

        int offset = 0;
        foreach (var p in packets)
        {
            Buffer.BlockCopy(p.SerializedData, 0, buffer, offset, p.SerializedData.Length);
            offset += p.SerializedData.Length;
        }

        await sendLock.WaitAsync();

        try
        {
            await stream.WriteAsync(buffer, 0, totalSize);
        }
        finally
        {
            sendLock.Release();
        }

        // 사용한 버퍼 반환
        System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
    }

    public async Task<Packet> SendRequestAndWaitResponse(Packet requestPacket, int timeoutMillis = 5000)
    {
        PacketHeader requestHeader = new PacketHeader();
        requestPacket.GetHeader(ref requestHeader);

        var tcs = new TaskCompletionSource<Packet>();
        pendingRequests[requestHeader.senderId] = tcs;

        await SendPacket(requestPacket);

        if (timeoutMillis <= 0)
        {
            return await tcs.Task;
        }

        using (var cts = new CancellationTokenSource(timeoutMillis))
        {
            using (cts.Token.Register(() => tcs.TrySetCanceled()))
            {
                try
                {
                    return await tcs.Task;
                }
                catch (TaskCanceledException)
                {
                    pendingRequests.Remove(requestHeader.senderId);
                    throw new TimeoutException("Request timed out.");
                }
            }
        }
    }

    public void Disconnect()
    {
        if (!client.Connected)
        {
            return;
        }

        recieveCTS?.Cancel();

        try
        {
            stream?.Close();
            client.Client?.Shutdown(SocketShutdown.Both);
        }
        catch (ObjectDisposedException)
        {
            // ignore
        }
        catch (Exception e)
        {
            Logger.Error($"AsyncTcpClient Disconnect Exception: {e}");
        }
        finally
        {
            client.Close();
            client = new TcpClient();
            stream = null;
            recieveCTS = null;
        }
    }

    public bool IsConnected()
    {
        return client.Connected;
    }
}