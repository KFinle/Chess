using System.Collections;
using System.Collections.Generic;
using System;
//using System.DateTime;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;


public class ChatManager : MonoBehaviour
{
    public GameObject chatBarContainer;
    [SerializeField] public InputField chatBar;
    [SerializeField] private Text chatWindow;
    [SerializeField] private ScrollRect scrollRect;
    private string textToSend;
    private string textReceived;
    public bool firstMessageSent = false;

    public string playerName = null;

    private Dictionary<string, string> chatCommands = new Dictionary<string, string>()
    {
        {"/name ", ": Sets the player name (/name {your_name})"},
        {"/commands", " : Displays all chat commands"}
    };



    private void Awake()
    {
        Register();
    }

    private void OnDestroy()
    {
        Unregister();
    }

    private void Update() {
        if (!chatBar.isFocused)
        {
            chatBar.ActivateInputField();
        }

        if (chatBarContainer.activeInHierarchy && Input.GetKeyUp(KeyCode.Return))
        {
            if (chatBar.text != string.Empty) 
            {
                textToSend = chatBar.text;
                if (!ParseForCommands(textToSend))
                {
                    if (!firstMessageSent)
                    {
                        //chatWindow.text = string.Empty;
                        firstMessageSent = true;
                    }
                    BroadcastMsg(textToSend);
                }


            }
            chatBar.text = string.Empty;
            chatBarContainer.SetActive(false);
        }
    }
    private void BroadcastMsg(string chatMsg)
    {
        NetChat nc = new NetChat();
        nc.player = playerName;
        nc.msg = chatMsg;
        Client.Instance.ToServer(nc);
    }
    public void SendText(string chatMsg, string sender)
    {
        if (playerName != null)
        {
            chatWindow.text += "\n[" + DateTime.Now.ToString("h:mm:ss tt") + "] " +  sender + ": " + chatMsg;
        }
        else 
        {
            chatWindow.text += "\n[" + DateTime.Now.ToString("h:mm:ss tt") + "] [Name Unset]: " + chatMsg;
        }
    }

    private bool ParseForCommands(string msg)
    {
        string strToParse = "";
        foreach (char c in msg)
        {
            strToParse += c;
            if (chatCommands.ContainsKey(strToParse))
            {
                ActivateCommand(msg);
                return true;
            }
        }

        return false;
    }

    private void SetName(string name)
    {
        playerName = name;
        SendCommandText("Name set: " + playerName);
    }

    private void DisplayCommands()
    {
        foreach(KeyValuePair<string, string> kv in chatCommands)
        {
            chatWindow.text += "\n[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + kv;
        }
    }


    private void SendCommandText(string msg)
    {
        chatWindow.text += "\n[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + msg; 
    }

    private void ActivateCommand(string cmd)
    {
       
            if(cmd ==  "/commands") DisplayCommands();
            if(cmd.StartsWith("/name ")) SetName(cmd.Substring(6));
            else Debug.Log("Error - command not recognized");
    }


    private void Register()
    {
        NetUtil.CL_CHAT += ClChat;
        NetUtil.SV_CHAT += ServChat;
    }

    private void Unregister()
    {
        NetUtil.CL_CHAT -= ClChat;
        NetUtil.SV_CHAT -= ServChat;
    }
    private void ServChat(NetMessage msg, NetworkConnection conn)
    {
        NetChat chat = msg as NetChat;
        Server.Instance.Broadcast(chat);
    }
    private void ClChat(NetMessage msg)
    {
        NetChat chat = msg as NetChat;
        SendText(chat.msg, chat.player);
    }
}
    





    





    










