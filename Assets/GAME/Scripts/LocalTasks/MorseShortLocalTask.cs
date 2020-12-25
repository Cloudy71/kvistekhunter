using UnityEngine;

public class MorseShortLocalTask : GameLocalTask {
    public AudioClip[] MorseSounds;
    public string[]    MorseChars;

    public int SequenceSize;

    public override bool OnTaskOpen(Player player) {
        if (!base.OnTaskOpen(player))
            return false;

        return true;
    }

    public override void OnTaskOpenClient() {
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();
    }
}