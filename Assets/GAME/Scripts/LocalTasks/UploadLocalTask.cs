using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Cloudy
public class UploadLocalTask : GameLocalTask {
    public Dictionary<Player, DownloadLocalTask.Entry> Entries;

    private DownloadLocalTask.Entry _entry;
    private int                     _step;
    private int                     _speed;
    private float                   _lastStep;

    protected override void Start() {
        base.Start();
        Entries = new Dictionary<Player, DownloadLocalTask.Entry>();
    }

    public override bool OnTaskOpen(Player player) {
        if (!base.OnTaskOpen(player))
            return false;
        if (!Entries.ContainsKey(player))
            return false;

        RegisterCoroutine(player,
                          StartCoroutine(SendEntry(player, Entries[player].ToString(), 1f)));
        return true;
    }

    private IEnumerator SendEntry(Player player, string entry, float time) {
        yield return new WaitForSeconds(time);
        SendTaskResponse(player, 0, entry);
    }

    public override bool OnTaskFinish(Player player, params object[] data) {
        base.OnTaskFinish(player, data);
        Entries.Remove(player);
        return true;
    }

    public override void OnTaskResponseClient(params object[] data) {
        base.OnTaskResponseClient(data);
        int id = (int) data[0];
        if (id == 0) {
            _entry = DownloadLocalTask.Entry.GetFromString((string) data[1]);
            _step = 0;
            _speed = 0;
            _lastStep = Time.time;
        }
    }

    public override void OnTaskOpenClient() {
        _entry = null;
    }

    public override void OnTaskUpdateClient() {
        base.OnTaskUpdateClient();
        if (_entry == null)
            return;
        if (Time.time >= _lastStep + 1f && _step < _entry.Size) {
            _lastStep = Time.time;
            _step += _speed;
            if (_step >= _entry.Size) {
                _step = _entry.Size;
                SendTaskFinish();
            }

            _speed = Random.Range(10, 60);
        }
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();

        GUI.BeginGroup(new Rect(Screen.width / 2f - 512f, Screen.height / 2f - 300f, 1024f, 600f));
        GUI.Box(new Rect(0f, 0f, 1024f, 600f), "");
        if (_entry != null) {
            GUI.contentColor = new Color32(128, 255, 128, 255);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(8f, 290f, 1008f, 40f), "Uploading " + _entry.Name + "...\n" + _step + "kB / " + _entry.Size + "kB ... " + _speed + "kB/s");
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.contentColor = Color.white;
        }
        else {
            GUI.contentColor = new Color32(128, 255, 128, 255);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(8f, 290f, 1008f, 20f), "Preparing for upload...");
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.contentColor = Color.white;
        }

        GUI.EndGroup();
    }
}