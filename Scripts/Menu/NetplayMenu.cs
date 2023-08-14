 using System.Net;
 using System.Net.NetworkInformation;
 using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Networking.Transport;

public class NetplayMenu : MonoBehaviour
{
    [SerializeField] private Server server;
    [SerializeField] private Client client;
    [SerializeField] private InputField hostIPDisplay;
    [SerializeField] private InputField ipInput;
    [SerializeField] private InputField portInput;
    [SerializeField] private InputField portToConnectField;
    [SerializeField] private GameObject netplayHomeScreen;
    [SerializeField] private GameObject hostScreen;
    [SerializeField] private GameObject connectScreen;
    [SerializeField] private GameObject awaitScreen;

    private string hostIP;
    private string connectIP;
    private string hostPort;
    private string portToConnect;

    private int playerCount = -1;
    private int currentPlayer = -1;



    private void Awake()
    {
        Client.Instance.Shutdown();
        Server.Instance.Shutdown();
        Register();
    }

    private void OnDestroy()
    {
        Unregister();
    }
    public void HostGameMenu()
    {
        netplayHomeScreen.SetActive(false);
        hostScreen.SetActive(true);
        hostIPDisplay.text = GetLocalIPAddress();
    }


    public void ConnectToGameMenu()
    {
        connectScreen.SetActive(true);
        netplayHomeScreen.SetActive(false);
    }
    private void ActivateConnections()
    {
        connectScreen.SetActive(false);
        hostScreen.SetActive(false);
        awaitScreen.SetActive(true);
    }
    public void StartHostServer()
    {
        hostPort = portInput.text;
        hostIP = hostIPDisplay.text;
        Server.Instance.Init(hostIP, Convert.ToUInt16(hostPort));
        Client.Instance.Init(hostIP, Convert.ToUInt16(hostPort));
        ActivateConnections();
    }

    public void ConnectToHost()
    {
        connectIP = ipInput.text;
        portToConnect = portToConnectField.text;
        Client.Instance.Init(connectIP, Convert.ToUInt16(portToConnect));
        ActivateConnections();
    }

    public void BackToNetplayScreen()
    {
        connectScreen.SetActive(false);
        hostScreen.SetActive(false);
        netplayHomeScreen.SetActive(true);
    }

    public void CancelConnection()
    {
        awaitScreen.SetActive(false);
        Client.Instance.Shutdown();
        Server.Instance.Shutdown();
        playerCount = -1;
        currentPlayer = -1;
        BackToNetplayScreen();
    }

    public string GetLocalIPAddress()
     {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
     }


    //Networking game start


    private void Register()
    {
        NetUtil.SV_MOTD += ServOnMOTD;
        NetUtil.CL_MOTD += ClOnMOTD;
        NetUtil.CL_START += ClStart;
        NetUtil.SV_START += ServStart;
    }
    private void Unregister()
    {
        NetUtil.SV_MOTD -= ServOnMOTD;
        NetUtil.CL_MOTD -= ClOnMOTD;
        NetUtil.CL_START -= ClStart;
        NetUtil.SV_START -= ServStart;
    }

    //SV
    private void ServOnMOTD(NetMessage msg, NetworkConnection conn)
    {
        //Assign team - pong client
        NetMOTD motd = msg as NetMOTD;

        motd.player = ++playerCount;
        Server.Instance.ToClient(conn, motd);
    }

    private void ServStart(NetMessage msg, NetworkConnection conn)
    {
        NetStart start = msg as NetStart;
        Server.Instance.Broadcast(start);
    }

    //CL
    private void ClOnMOTD(NetMessage msg)
    {
        //Assign team
        NetMOTD motd = msg as NetMOTD;

        currentPlayer = motd.player;
        Client.Instance.player = currentPlayer;
        if (currentPlayer > 0)
        {
            Client.Instance.ToServer(new NetStart());
        }
    }
    private void ClStart(NetMessage msg)
    {
        currentPlayer = -1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("NetplayGame");
    }
}
