using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetForfeit : NetMessage
{
    public NetForfeit()
    {
        Code = OpCode.FORFEIT;
    }

    public NetForfeit(DataStreamReader reader)
    {
        Code = OpCode.FORFEIT;
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
        NetUtil.CL_FORFEIT?.Invoke(this);
    }
    public override void GotOnServer(NetworkConnection conn)
    {
        NetUtil.SV_FORFEIT?.Invoke(this, conn);
    }
}
