using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    //Singleton Implement.
    public static Client Instance { set; get; }

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
    private NetworkConnection self;
    public bool isAlive = false;
    public int player = -1;

    public Action connectionDropped;

    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);
        endpoint.Port = port;

        self = driver.Connect(endpoint);
        isAlive = true;
        Register();
    }

    // Shut down server
    public void Shutdown()
    {
        if (isAlive)
        {
            Unregister();
            driver.Dispose();
            isAlive = false;
            player = -1;
            self = default(NetworkConnection);
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
        driver.ScheduleUpdate().Complete();
        CheckAlive();
        ReadMessage();
    }
    private void CheckAlive()
    {
        if (!self.IsCreated && isAlive)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            connectionDropped?.Invoke();
            Shutdown();
        }
    }


    private void ReadMessage()
    {
        DataStreamReader stream;
        
        NetworkEvent.Type cmd;
        while ((cmd = self.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                ToServer(new NetMOTD());
            }
            else if (cmd == NetworkEvent.Type.Data)
                NetUtil.OnData(stream, default(NetworkConnection));
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                self = default(NetworkConnection);
                connectionDropped?.Invoke();
                Shutdown();
            }
        }
    }

    public void ToServer(NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(self, out writer);
        msg.Pack(ref writer);
        driver.EndSend(writer);
    }

    // Events
    private void Register()
    {
        NetUtil.CL_HEARTBEAT += OnHeartbeat;
    }
    private void Unregister()
    {
        NetUtil.CL_HEARTBEAT -= OnHeartbeat;
    }
    private void OnHeartbeat(NetMessage nm)
    {
        ToServer(nm);
    }
}