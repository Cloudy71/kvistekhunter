using Mirror;
using UnityEngine;

public struct MessageGameInfo : NetworkMessage {
    public float VictimVision;
    public int   VictimLives;
    public float VictimSpeed;
    public float HunterVision;
    public float HunterKillCooldown;
    public float HunterSpeed;
    public int   Hunters;
    public bool  DisplayHunters;
    public float VictimTaskDistance;
    public float HunterKillDistance;
    public float HunterVisionOnCooldown;
    public int   VictimCommonTasks;
    public int   VictimLongTasks;
    public int   VictimShortTasks;
    public float TimeLimit;
    public Color DefaultColor;
    public bool  TasksBalancedDamage;
}

public static class MessageGameInfoFunctions {
    public static void Serialize(this NetworkWriter writer, MessageGameInfo value) {
        writer.WriteSingle(value.VictimVision);
        writer.WriteInt32(value.VictimLives);
        writer.WriteSingle(value.VictimSpeed);
        writer.WriteSingle(value.HunterVision);
        writer.WriteSingle(value.HunterKillCooldown);
        writer.WriteSingle(value.HunterSpeed);
        writer.WriteInt32(value.Hunters);
        writer.WriteBoolean(value.DisplayHunters);
        writer.WriteSingle(value.VictimTaskDistance);
        writer.WriteSingle(value.HunterKillDistance);
        writer.WriteSingle(value.HunterVisionOnCooldown);
        writer.WriteInt32(value.VictimCommonTasks);
        writer.WriteInt32(value.VictimLongTasks);
        writer.WriteInt32(value.VictimShortTasks);
        writer.WriteSingle(value.TimeLimit);
        writer.WriteColor(value.DefaultColor);
        writer.WriteBoolean(value.TasksBalancedDamage);
    }

    public static MessageGameInfo Deserialize(this NetworkReader reader) {
        MessageGameInfo value = new MessageGameInfo();
        value.VictimVision = reader.ReadSingle();
        value.VictimLives = reader.ReadInt32();
        value.VictimSpeed = reader.ReadSingle();
        value.HunterVision = reader.ReadSingle();
        value.HunterKillCooldown = reader.ReadSingle();
        value.HunterSpeed = reader.ReadSingle();
        value.Hunters = reader.ReadInt32();
        value.DisplayHunters = reader.ReadBoolean();
        value.VictimTaskDistance = reader.ReadSingle();
        value.HunterKillDistance = reader.ReadSingle();
        value.HunterVisionOnCooldown = reader.ReadSingle();
        value.VictimCommonTasks = reader.ReadInt32();
        value.VictimLongTasks = reader.ReadInt32();
        value.VictimShortTasks = reader.ReadInt32();
        value.TimeLimit = reader.ReadSingle();
        value.DefaultColor = reader.ReadColor();
        value.TasksBalancedDamage = reader.ReadBoolean();
        return value;
    }
}