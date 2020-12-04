using Mirror;
using UnityEngine;

public struct MessageGameStatus : NetworkMessage {
    public bool       GameStarted;
    public GameObject Admin;
}

public static class MessageGameStatusFunctions {
    public static void Serialize(this NetworkWriter writer, MessageGameStatus value) {
        writer.WriteBoolean(value.GameStarted);
        writer.WriteGameObject(value.Admin);
    }

    public static MessageGameStatus Deserialize(this NetworkReader reader) {
        MessageGameStatus value = new MessageGameStatus();
        value.GameStarted = reader.ReadBoolean();
        value.Admin = reader.ReadGameObject();
        return value;
    }
}