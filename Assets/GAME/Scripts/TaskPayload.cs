using Mirror;
using UnityEngine;

public class TaskPayload {
    public object[] Data;

    public TaskPayload(params object[] data) {
        Data = data;
    }
}

public static class TaskPayloadSerializer {
    private static byte TypeByte(object obj) {
        if (obj is int) return 0;
        else if (obj is float) return 1;
        else if (obj is bool) return 2;
        else if (obj is GameObject) return 3;
        return 255;
    }

    private static void WriteObject(NetworkWriter writer, object obj, byte typeByte) {
        if (typeByte == 0) writer.WriteInt32((int) obj);
        else if (typeByte == 1) writer.WriteSingle((float) obj);
        else if (typeByte == 2) writer.WriteBoolean((bool) obj);
        else if (typeByte == 3) writer.WriteGameObject((GameObject) obj);
    }

    private static object ReadObject(NetworkReader reader, byte typeByte) {
        if (typeByte == 0) return reader.ReadInt32();
        else if (typeByte == 1) return reader.ReadSingle();
        else if (typeByte == 2) return reader.ReadBoolean();
        else if (typeByte == 3) return reader.ReadGameObject();
        return null;
    }

    public static void WriteTaskPayload(this NetworkWriter writer, TaskPayload payload) {
        if (payload == null) {
            writer.WriteInt32(0);
            return;
        }
        writer.WriteInt32(payload.Data.Length);
        for (var i = 0; i < payload.Data.Length; ++i) {
            byte typeByte = TypeByte(payload.Data[i]);
            writer.WriteByte(typeByte);
            WriteObject(writer, payload.Data[i], typeByte);
        }
    }

    public static TaskPayload ReadTaskPayload(this NetworkReader reader) {
        TaskPayload payload = new TaskPayload();
        int size = reader.ReadInt32();
        payload.Data = new object[size];
        for (int i = 0; i < size; ++i) {
            byte typeByte = reader.ReadByte();
            payload.Data[i] = ReadObject(reader, typeByte);
        }

        return payload;
    }
}