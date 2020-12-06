using UnityEngine;

public class Utils {
    public static string TimeToString(float seconds) {
        int minutes = (int) Mathf.Floor(seconds / 60f);
        int secs = (int) Mathf.Floor(seconds - minutes * 60f);

        return (minutes < 10 ? "0" : "") + minutes + ":" + (secs < 10 ? "0" : "") + secs;
    }
}