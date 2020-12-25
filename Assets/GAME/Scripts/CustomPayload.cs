using Mirror;
using UnityEngine;

public class CustomPayload {
    public object[] Data;

    public CustomPayload(params object[] data) {
        Data = data;
    }

    public T Get<T>(int index) {
        return (T) Data[index];
    }

    public object this[int i] {
        get => Data[i];
        set => Data[i] = value;
    }
}

public static class TaskPayloadSerializer {
    private static byte TypeByte(object obj) {
        if (obj is int) return 0;
        else if (obj is float) return 1;
        else if (obj is bool) return 2;
        else if (obj is GameObject) return 3;
        else if (obj is string) return 4;
        else if (obj is byte[]) return 5;
        return 255;
    }

    private static void WriteObject(NetworkWriter writer, object obj, byte typeByte) {
        if (typeByte == 0) writer.WriteInt32((int) obj);
        else if (typeByte == 1) writer.WriteSingle((float) obj);
        else if (typeByte == 2) writer.WriteBoolean((bool) obj);
        else if (typeByte == 3) writer.WriteGameObject((GameObject) obj);
        else if (typeByte == 4) writer.WriteString((string) obj);
        else if (typeByte == 5) {
            writer.WriteInt32(((byte[]) obj).Length);
            writer.WriteBytes((byte[]) obj, 0, ((byte[]) obj).Length);
        }
    }

    private static object ReadObject(NetworkReader reader, byte typeByte) {
        if (typeByte == 0) return reader.ReadInt32();
        if (typeByte == 1) return reader.ReadSingle();
        if (typeByte == 2) return reader.ReadBoolean();
        if (typeByte == 3) return reader.ReadGameObject();
        if (typeByte == 4) return reader.ReadString();
        if (typeByte == 5) {
            int len = reader.ReadInt32();
            return reader.ReadBytes(len);
        }

        return null;
    }

    public static void WriteTaskPayload(this NetworkWriter writer, CustomPayload payload) {
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

    public static CustomPayload ReadTaskPayload(this NetworkReader reader) {
        CustomPayload payload = new CustomPayload();
        int size = reader.ReadInt32();
        payload.Data = new object[size];
        for (int i = 0; i < size; ++i) {
            byte typeByte = reader.ReadByte();
            payload.Data[i] = ReadObject(reader, typeByte);
        }

        return payload;
    }
}