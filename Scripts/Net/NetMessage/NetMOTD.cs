using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMOTD : NetMessage
{
    public int player { set; get; }
    public NetMOTD()
    {
        Code = OpCode.MOTD;
    }

    public NetMOTD(DataStreamReader reader)
    {
        Code = OpCode.MOTD;
        Unpack(reader);
    }

    public override void Pack(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(player);
    }
    public override void Unpack(DataStreamReader reader)
    {
        player = reader.ReadInt();
    }

    public override void GotOnClient()
    {
        NetUtil.CL_MOTD?.Invoke(this);
    }
    public override void GotOnServer(NetworkConnection conn)
    {
        NetUtil.SV_MOTD?.Invoke(this, conn);
    }
}
