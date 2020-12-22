using Mirror;

public struct MessageGameStatusData : NetworkMessage {
    public int HuntersMaxHealth;
    public int HuntersHealth;
    public int VictimsMaxHealth;
    public int VictimsHealth;
}

public static class MessageGameStatusDataFunctions {
    public static void Serialize(this NetworkWriter writer, MessageGameStatusData value) {
        writer.WriteInt32(value.HuntersMaxHealth);
        writer.WriteInt32(value.HuntersHealth);
        writer.WriteInt32(value.VictimsMaxHealth);
        writer.WriteInt32(value.VictimsHealth);
    }

    public static MessageGameStatusData Deserialize(this NetworkReader reader) {
        MessageGameStatusData message = new MessageGameStatusData();
        message.HuntersMaxHealth = reader.ReadInt32();
        message.HuntersHealth = reader.ReadInt32();
        message.VictimsMaxHealth = reader.ReadInt32();
        message.VictimsHealth = reader.ReadInt32();
        return message;
    }
}