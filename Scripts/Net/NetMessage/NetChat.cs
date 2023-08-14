using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetChat : NetMessage
{
    public string msg, player;
    public NetChat()
    {
        Code = OpCode.CHAT;
    }

    public NetChat(DataStreamReader reader)
    {
        Code = OpCode.CHAT;
        Unpack(reader);
    }

    public override void Pack(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteFixedString32(player);
        writer.WriteFixedString4096(msg);
    }
    public override void Unpack(DataStreamReader reader)
    {
        player = reader.ReadFixedString32().ToString();
        msg = reader.ReadFixedString4096().ToString();
    }

    public override void GotOnClient()
    {
        NetUtil.CL_CHAT?.Invoke(this);
    }
    public override void GotOnServer(NetworkConnection conn)
    {
        NetUtil.SV_CHAT?.Invoke(this, conn);
    }
}
