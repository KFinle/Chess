using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;


public class NetMessage
{
    public OpCode Code { set; get; }
    // Put meaning to message
    public virtual void Pack(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }
    // Unpack message
    public virtual void Unpack(DataStreamReader reader)
    {

    }
    // Recieve as client
    public virtual void GotOnClient()
    {

    }
    // Recieve as server, holding on to the source
    public virtual void GotOnServer(NetworkConnection conn)
    {

    }

}
