using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMove : NetMessage
{
    public int currentX, currentY, finalX, finalY, player;
    public NetMove()
    {
        Code = OpCode.MOVE;
    }

    public NetMove(DataStreamReader reader)
    {
        Code = OpCode.MOVE;
        Unpack(reader);
    }

    public override void Pack(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(player);
        writer.WriteInt(currentX);
        writer.WriteInt(currentY);
        writer.WriteInt(finalX);
        writer.WriteInt(finalY);
    }
    public override void Unpack(DataStreamReader reader)
    {
        player = reader.ReadInt();
        currentX = reader.ReadInt();
        currentY = reader.ReadInt();
        finalX = reader.ReadInt();
        finalY = reader.ReadInt();
    }

    public override void GotOnClient()
    {
        NetUtil.CL_MOVE?.Invoke(this);
    }
    public override void GotOnServer(NetworkConnection conn)
    {
        NetUtil.SV_MOVE?.Invoke(this, conn);
    }
}
