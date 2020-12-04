using System.Linq;
using Mirror;
using UnityEngine;

public struct MessagePlayerInfo : NetworkMessage {
    public string Name;
}

public static class MessagePlayerInfoFunctions {
    public static void Serialize(this NetworkWriter writer, MessagePlayerInfo value) {
        writer.WriteString(value.Name);
    }

    public static MessagePlayerInfo Deserialize(this NetworkReader reader) {
        MessagePlayerInfo value = new MessagePlayerInfo();
        value.Name = reader.ReadString();
        return value;
    }
}