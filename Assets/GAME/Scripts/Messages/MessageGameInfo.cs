using Mirror;

public struct MessageGameInfo : NetworkMessage {
    public float VictimVision;
    public int   VictimLives;
    public float VictimSpeed;
    public float HunterVision;
    public float HunterKillCooldown;
    public float HunterSpeed;
}

public static class MessageGameInfoFunctions {
    public static void Serialize(this NetworkWriter writer, MessageGameInfo value) {
        writer.WriteSingle(value.VictimVision);
        writer.WriteInt32(value.VictimLives);
        writer.WriteSingle(value.VictimSpeed);
        writer.WriteSingle(value.HunterVision);
        writer.WriteSingle(value.HunterKillCooldown);
        writer.WriteSingle(value.HunterSpeed);
    }

    public static MessageGameInfo Deserialize(this NetworkReader reader) {
        MessageGameInfo value = new MessageGameInfo();
        value.VictimVision = reader.ReadSingle();
        value.VictimLives = reader.ReadInt32();
        value.VictimSpeed = reader.ReadSingle();
        value.HunterVision = reader.ReadSingle();
        value.HunterKillCooldown = reader.ReadSingle();
        value.HunterSpeed = reader.ReadSingle();
        return value;
    }
}