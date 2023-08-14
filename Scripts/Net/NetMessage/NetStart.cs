using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetStart : NetMessage
{
    public NetStart()
    {
        Code = OpCode.START;
    }

    public NetStart(DataStreamReader reader)
    {
        Code = OpCode.START;
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
        NetUtil.CL_START?.Invoke(this);
    }
    public override void GotOnServer(NetworkConnection conn)
    {
        NetUtil.SV_START?.Invoke(this, conn);
    }
}
