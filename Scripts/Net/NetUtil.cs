using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Networking.Transport;
using UnityEngine;

public enum OpCode
{
    HEARTBEAT = 1,
    MOTD = 2,
    START = 3,
    MOVE = 4,
    REMATCH = 5,
    FORFEIT = 6,
    TERMINATE = 7,
    CHAT = 8
}

public class NetUtil : MonoBehaviour
{
    // Net Messages
    public static Action<NetMessage> CL_HEARTBEAT;
    public static Action<NetMessage> CL_MOTD;
    public static Action<NetMessage> CL_START;
    public static Action<NetMessage> CL_MOVE;
    public static Action<NetMessage> CL_REMATCH;
    public static Action<NetMessage> CL_FORFEIT;
    public static Action<NetMessage> CL_TERMINATE;
    public static Action<NetMessage> CL_CHAT;

    public static Action<NetMessage, NetworkConnection> SV_HEARTBEAT;
    public static Action<NetMessage, NetworkConnection> SV_MOTD;
    public static Action<NetMessage, NetworkConnection> SV_START;
    public static Action<NetMessage, NetworkConnection> SV_MOVE;
    public static Action<NetMessage, NetworkConnection> SV_REMATCH;
    public static Action<NetMessage, NetworkConnection> SV_FORFEIT;
    public static Action<NetMessage, NetworkConnection> SV_TERMINATE;
    public static Action<NetMessage, NetworkConnection> SV_CHAT;

    // Open data message
    public static void OnData(DataStreamReader stream, NetworkConnection conn, Server sv = null)
    {
        NetMessage msg = null;
        var opCode = (OpCode)stream.ReadByte();
        switch (opCode)
        {
            case OpCode.HEARTBEAT: msg = new NetHeartbeat(stream); break;
            case OpCode.MOTD: msg = new NetMOTD(stream); break;
            case OpCode.START: msg = new NetStart(stream); break;
            case OpCode.MOVE: msg = new NetMove(stream); break;
            case OpCode.REMATCH: msg = new NetRematch(stream); break;
            case OpCode.FORFEIT: msg = new NetForfeit(stream); break;
            case OpCode.TERMINATE: msg = new NetTerminate(stream); break;
            case OpCode.CHAT: msg = new NetChat(stream); break;
            default:
                Debug.LogError("Bad message");
                break;
        }
        if (sv != null)
        {
            msg.GotOnServer(conn);
        } else
        {
            msg.GotOnClient();
        }
    }
}
