using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetRematch : NetMessage
{
    public NetRematch()
    {
        Code = OpCode.REMATCH;
    }

    public NetRematch(DataStreamReader reader)
    {
        Code = OpCode.REMATCH;
        Unpack(reader);
    }

    public override void Pack(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }
    public override void Unpack(DataStreamReader reader)
    {
    }

    public override void GotOnClient()
    {
        NetUtil.CL_REMATCH?.Invoke(this);
    }
    public override void GotOnServer(NetworkConnection conn)
    {
        NetUtil.SV_REMATCH?.Invoke(this, conn);
    }
}
