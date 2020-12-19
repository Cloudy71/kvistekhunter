using System.Collections.Generic;
using Mirror;
using UnityEngine;

public struct MessageUsedColors : NetworkMessage {
    public List<Color> UsedColors;
}

public static class MessageUsedColorsFunctions {
    public static void Serialize(this NetworkWriter writer, MessageUsedColors value) {
        writer.WriteList(value.UsedColors);
    }

    public static MessageUsedColors Deserialize(this NetworkReader reader) {
        MessageUsedColors value = new MessageUsedColors();
        value.UsedColors = reader.ReadList<Color>();
        return value;
    }
}