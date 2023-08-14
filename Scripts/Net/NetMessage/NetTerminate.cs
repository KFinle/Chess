using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetTerminate : NetMessage
{
    public NetTerminate()
    {
        Code = OpCode.TERMINATE;
    }

    public NetTerminate(DataStreamReader reader)
    {
        Code = OpCode.TERMINATE;
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
        NetUtil.CL_TERMINATE?.Invoke(this);
    }
    public override void GotOnServer(NetworkConnection conn)
    {
        NetUtil.SV_TERMINATE?.Invoke(this, conn);
    }
}
