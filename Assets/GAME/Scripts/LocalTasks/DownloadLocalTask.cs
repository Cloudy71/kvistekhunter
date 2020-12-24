using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class DownloadLocalTask : GameLocalTask {
    [Serializable]
    public class Entry {
        public bool        IsFile;
        public string      Name;
        public int         Size;
        public List<Entry> Entries;

        public override string ToString() {
            return JsonUtility.ToJson(this);
        }

        public override bool Equals(object obj) {
            if (obj is Entry entry) {
                return IsFile == entry.IsFile && Name == entry.Name && Size == entry.Size;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return 17 * (23 + IsFile.GetHashCode()) * (23 + Name.GetHashCode()) * (23 + Size.GetHashCode());
        }

        public static bool operator ==(Entry entry1, Entry entry2) {
            return Equals(entry1, entry2);
        }

        public static bool operator !=(Entry entry1, Entry entry2) {
            return !Equals(entry1, entry2);
        }
    }

    [SyncVar]
    public string IpAddress;

    public Entry Main;

    public Entry Target;

    private string[] _folderNames;
    private string[] _fileNames;

    private Coroutine _currentCoroutine;

    private string _ipAddress;
    private Entry  _main;
    private Entry  _target;
    private Entry  _current;
    private bool   _isDownloading;
    private int    _step;
    private int    _speed;
    private float  _lastStep;

    protected override void Start() {
        base.Start();
        _folderNames = new[] {
                                 "DATA", "SYSTEM", "WEBSERVER", "DATASERVER", "LOGINSERVER", "METIN2",
                                 "KVISTEK", "LOL", "FORHONOR", "COMPANY", "DATA2", "SYNOLOGY", "APACHE", "MYSQL",
                                 "POSTGRES", "ORACLE", "SQL", "MSSQL", "NOSQL", "DRWHO", "COLORS", "STATUS", "GAME",
                                 "UINT32", "_HIDDEN"
                             };
        _fileNames = new[] {
                               "A.BIN", "B.BIN", "C.BIN", "D.BIN", "E.BIN", "F.BIN",
                               "Q.BIN", "U.BIN", "V.BIN", "W.BIN", "X.BIN", "Y.BIN", "Z.BIN",
                               "DB.DUMP", "IMPORT.SQL", "._", ".GITIGNORE", ".GIT"
                           };
    }

    public override bool OnTaskOpen(Player player) {
        if (!base.OnTaskOpen(player))
            return false;

        // TODO: Send by parts, MTU is 1200 bytes, which is too much.

        if (_currentCoroutine != null) {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }

        IpAddress = Random.Range(0, 256) + "." + Random.Range(0, 256) + "." + Random.Range(0, 256) + "." + Random.Range(0, 256);

        Main = GenerateEntry("SRV", 1, 6, 1, 6);
        List<Entry> fileEntries = new List<Entry>();
        Traverse(fileEntries, Main);
        Target = fileEntries[Random.Range(0, fileEntries.Count)];
        _currentCoroutine = StartCoroutine(SendTarget(player, Main.ToString(), Target.ToString(), 2f));

        return true;
    }

    private IEnumerator SendTarget(Player player, string fileSystem, string target, float time) {
        yield return new WaitForSeconds(time);
        SendTaskResponse(player, 0, fileSystem, target);
        _currentCoroutine = null;
    }

    private Entry GenerateEntry(string entryName, int fmin, int fmax, int ffmin, int ffmax) {
        Entry entry = new Entry {
                                    Name = entryName,
                                    Size = Random.Range(1, 100000),
                                    IsFile = false,
                                    Entries = new List<Entry>()
                                };
        int folders = Random.Range(fmin, fmax);
        int files = Random.Range(ffmin, ffmax);
        List<string> availableFolderNames = new List<string>(_folderNames);
        List<string> availableFileNames = new List<string>(_fileNames);
        for (int i = 0; i < folders; ++i) {
            string folderName = availableFolderNames[Random.Range(0, availableFolderNames.Count)];
            availableFolderNames.Remove(folderName);
            entry.Entries.Add(GenerateEntry(folderName, 0, fmax - 2, 0, ffmax - 2));
        }

        for (int i = 0; i < files; ++i) {
            Entry fileEntry = new Entry {
                                            Name = availableFileNames[Random.Range(0, availableFileNames.Count)],
                                            Size = Random.Range(1, 1025),
                                            IsFile = true,
                                            Entries = null
                                        };
            availableFileNames.Remove(fileEntry.Name);
            entry.Entries.Add(fileEntry);
        }

        return entry;
    }

    private void Traverse(List<Entry> returnList, Entry select) {
        returnList.AddRange(select.Entries.Where(entry => entry.IsFile));
        foreach (Entry entry1 in @select.Entries.Where(entry => !entry.IsFile)) {
            Traverse(returnList, entry1);
        }
    }

    public override void OnTaskStep(Player player, params object[] data) {
        base.OnTaskStep(player, data);
        int id = (int) data[0];
        if (id == 1) {
            Entry target = JsonUtility.FromJson<Entry>((string) data[1]);
            if (target.Name == Target.Name && target.Size == Target.Size) {
                SendTaskResponse(player, 2);
            }
        }
    }

    public override bool OnTaskFinish(Player player, params object[] data) {
        base.OnTaskFinish(player, data);
        foreach (GameTask gameTask in ActivateOnFinish) {
            if (gameTask is UploadLocalTask uploadLocalTask) {
                uploadLocalTask.Entries.Add(player, Target);
            }
        }

        return true;
    }

    public override void OnTaskResponseClient(params object[] data) {
        base.OnTaskResponseClient(data);
        int id = (int) data[0];

        if (id == 0) {
            _main = JsonUtility.FromJson<Entry>((string) data[1]);
            _target = JsonUtility.FromJson<Entry>((string) data[2]);
            _current = _main;
        }
        else if (id == 2) {
            _isDownloading = true;
            _step = 0;
            _lastStep = Time.time;
        }
    }

    public override void OnTaskOpenClient() {
        _ipAddress = IpAddress;
        _main = null;
        _target = null;
        _isDownloading = false;
    }

    public override void OnTaskUpdateClient() {
        base.OnTaskUpdateClient();
        if (_ipAddress != IpAddress)
            return;
        if (_isDownloading) {
            if (Time.time >= _lastStep + 1f && _step < _target.Size) {
                _lastStep = Time.time;
                _step += _speed;
                if (_step >= _target.Size) {
                    _step = _target.Size;
                    SendTaskFinish();
                }

                _speed = Random.Range(10, 60);
            }
        }
    }

    private Entry GetParent(Entry main, Entry entry) {
        foreach (Entry mainEntry in main.Entries) {
            if (entry == mainEntry) {
                return main;
            }

            if (mainEntry.IsFile)
                continue;
            Entry retEntry = GetParent(mainEntry, entry);
            if (retEntry != null)
                return retEntry;
        }

        return null;
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();

        GUI.BeginGroup(new Rect(Screen.width / 2f - 512f, Screen.height / 2f - 300f, 1024f, 600f));

        GUI.Box(new Rect(0f, 0f, 1024f, 600f), "");
        if (_ipAddress == IpAddress) {
            if (_main == null) {
                GUI.contentColor = new Color32(128, 255, 128, 255);
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(8f, 290f, 1008f, 20f), "Fetching data from server, please wait...");
                GUI.skin.label.alignment = TextAnchor.UpperLeft;
                GUI.contentColor = Color.white;
            }
            else {
                if (_isDownloading) {
                    GUI.contentColor = new Color32(128, 255, 128, 255);
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(new Rect(8f, 290f, 1008f, 40f), "Downloading " + _target.Name + "...\n" + _step + "kB / " + _target.Size + "kB ... " + _speed + "kB/s");
                    GUI.skin.label.alignment = TextAnchor.UpperLeft;
                    GUI.contentColor = Color.white;
                }
                else {
                    float y = _current == _main ? 8f : 36f;
                    if (_current != _main) {
                        if (GUI.Button(new Rect(8f, 8f, 1008f, 20f), "..")) {
                            _current = GetParent(_main, _current);
                        }
                    }

                    foreach (Entry currentEntry in _current.Entries) {
                        if (GUI.Button(new Rect(8f, y, 1008f, 20f), currentEntry.Name + (currentEntry.IsFile ? " [" + currentEntry.Size + "kB]" : ""))) {
                            if (currentEntry.IsFile) {
                                if (currentEntry == _target) {
                                    SendTaskStep(1, _target.ToString());
                                }
                            }
                            else {
                                _current = currentEntry;
                            }
                        }

                        y += 28f;
                    }

                    GUI.Label(new Rect(8f, 572f, 1008f, 20f), "Find: " + _target.Name + " [" + _target.Size + "kB]");
                }
            }
        }
        else {
            GUI.contentColor = new Color32(255, 128, 128, 255);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(8f, 290f, 1008f, 20f), "Connection has been lost. Please retry.");
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.contentColor = Color.white;
        }

        GUI.EndGroup();
    }
}