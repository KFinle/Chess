using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    //Singleton Implement.
    public static Server Instance { set; get; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        DontDestroyOnLoad(transform.gameObject);
        Instance = this;
    }
    public NetworkDriver driver;

    //List of all conns
    private NativeList<NetworkConnection> conns;
    private bool isAlive = false;

    // Time out period
    private const float heartbeatLimit = 5.0f;
    private float lastHeartbeat;
    public Action connDropped;

    // Functions
    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);

        if(driver.Bind(endpoint) != 0) 
        {
            Debug.Log("Unable to bind on port: " + endpoint.Port);
        } else
        {
            driver.Listen();
        }

        // Creates list of conns, with a cap of two.
        conns = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        isAlive = true;
    }

    // Shut down server
    public void Shutdown() 
    {
        if (isAlive) 
        {
            Broadcast(new NetForfeit());
            driver.Dispose();
            conns.Dispose();
            isAlive = false;
        }

    }

    public void OnDestroy()
    {
        Shutdown();
    }

    public void Update()
    {
        if (!isAlive)
        {
            return;
        }

        Heartbeat();
        driver.ScheduleUpdate().Complete();
        Cleanup();
        Open();
        ReadMessage();
    }
    
    private void Heartbeat()
    {
        if (Time.time - lastHeartbeat > heartbeatLimit)
        {
            lastHeartbeat = Time.time;
            Broadcast(new NetHeartbeat());
        }
    }

    public void Cleanup() 
    {
        // Removes uncreated conns
        for (int i = 0; i < conns.Length; i++) 
        {
            if (!conns[i].IsCreated) 
            {
                conns.RemoveAtSwapBack(i);
                --i;
            }
        }
    }
    private void Open()
    {
        NetworkConnection conn;
        // While there is a connection that is not default.
        while ((conn = driver.Accept()) != default(NetworkConnection))
        {
            conns.Add(conn);
        }
    }
    private void ReadMessage()
    {
        // Reads the messages
        DataStreamReader stream;
        // For all conns
        for (int i = 0; i < conns.Length; i++)
        {
            NetworkEvent.Type cmd;
            while ((cmd = driver.PopEventForConnection(conns[i], out stream)) != NetworkEvent.Type.Empty)
            {
                // Recieved message is either data or a disconnect.
                // Catch message
                if (cmd == NetworkEvent.Type.Data)
                {
                    NetUtil.OnData(stream, conns[i], this);
                }
                // Catch disconnect
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    conns[i] = default(NetworkConnection);
                    connDropped?.Invoke();
                    // Kill server on disconnect
                    Shutdown();
                }
            }
        }
    }

    // Send to a single client
    public void ToClient(NetworkConnection connection, NetMessage msg)
    {
        DataStreamWriter writer;
        // Grabs packet and binds destination
        driver.BeginSend(connection, out writer);
        // Process Message
        msg.Pack(ref writer);
        // Send the messsage
        driver.EndSend(writer);
    }

    // Sends to all clients
    public void Broadcast(NetMessage msg)
    {
        // Iterates SendToClient for all clients
        for (int i = 0; i < conns.Length; i++)
        {
            if (conns[i].IsCreated) ToClient(conns[i], msg);
        }
    }
}