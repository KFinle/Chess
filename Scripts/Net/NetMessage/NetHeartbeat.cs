using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetHeartbeat : NetMessage
{
    // out
    public NetHeartbeat()
    {
        Code = OpCode.HEARTBEAT;
    }
    // in
    public NetHeartbeat(DataStreamReader reader)
    {
        Code = OpCode.HEARTBEAT;
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
        NetUtil.CL_HEARTBEAT?.Invoke(this);
    }
    public override void GotOnServer(NetworkConnection conn)
    {
        NetUtil.SV_HEARTBEAT?.Invoke(this, conn);
    }
}
