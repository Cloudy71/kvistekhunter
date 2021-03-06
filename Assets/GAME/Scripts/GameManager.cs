using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : NetworkManager {
    public static GameManager Instance;

    public bool   GameStarted = false;
    public Player Admin       = null;

    public bool IsAdmin => NetworkClient.connection != null && Admin != null && Admin == NetworkClient.connection.identity.GetComponent<Player>();

    public float VictimVision           = 7.5f;
    public int   VictimLives            = 1;
    public float VictimSpeed            = 6f;
    public float HunterVision           = 25f;
    public float HunterKillCooldown     = 10f;
    public float HunterSpeed            = 8f;
    public int   Hunters                = 1;
    public bool  DisplayHunters         = false;
    public float VictimTaskDistance     = 1f;
    public float HunterKillDistance     = 2.5f;
    public float HunterVisionOnCooldown = 3f;
    public int   VictimCommonTasks      = 1;
    public int   VictimLongTasks        = 1;
    public int   VictimShortTasks       = 1;
    public float TimeLimit              = 600f;
    public Color DefaultColor           = Color.gray;
    public bool  TasksBalancedDamage    = false;

    public Color[] PreCreatedColors = {
                                          Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.white,
                                          new Color32(255, 128, 255, 255), new Color32(160, 0, 192, 255),
                                          new Color32(160, 82, 45, 255), new Color32(0, 150, 0, 255)
                                      };

    [HideInInspector]
    public float StatusStartTime;

    [HideInInspector]
    public int StatusHuntersMaxHealth;

    [HideInInspector]
    public int StatusHuntersHealth;

    [HideInInspector]
    public int StatusVictimsMaxHealth;

    [HideInInspector]
    public int StatusVictimsHealth;

    private Transform   _panelGameInfo;
    private Button      _startButton;
    private Camera      _camera;
    private List<Color> _usedColors;

    public override void Awake() {
        base.Awake();
        Instance = this;
    }

    public override void Start() {
        base.Start();

        // if (networkSceneName == "Lobby" || networkSceneName.Length == 0) {
        _panelGameInfo = GameObject.Find("Canvas/PanelGameInfo").transform;
        _startButton = _panelGameInfo.Find("ButtonStart").GetComponent<Button>();
        _panelGameInfo.gameObject.SetActive(false);
        _usedColors = new List<Color>();
        // }
        // else {
        //     GameStarted = true;
        //     IsAdmin = NetworkServer.active;
        //     VictimVision = OldInstance.VictimVision;
        //     VictimLives = OldInstance.VictimLives;
        //     VictimSpeed = OldInstance.VictimSpeed;
        //     HunterVision = OldInstance.HunterVision;
        //     HunterKillCooldown = OldInstance.HunterKillCooldown;
        //     HunterSpeed = OldInstance.HunterSpeed;
        //     // foreach (KeyValuePair<int, NetworkConnectionToClient> player in NetworkServer.connections) {
        //     //     Player p = player.Value.identity.GetComponent<Player>();
        //     //     p.Name = (string) PlayerDataMap[p.netId][0];
        //     // }
        // }
    }

    private void Update() {
        if (_camera == null)
            _camera = Camera.main;

        if (NetworkServer.active && GameStarted) {
            TimeController();
        }
    }

    public static Camera GetCamera() {
        return Instance._camera;
    }

    private void TimeController() {
    }

    public override void OnStartServer() {
        base.OnStartServer();
        NetworkServer.RegisterHandler<MessagePlayerInfo>(MessagePlayerInfoServerHandler, false);
        NetworkServer.RegisterHandler<MessageGameInfo>(MessageGameInfoServerHandler, false);
        NetworkServer.RegisterHandler<MessageGameStatus>(MessageGameStatusServerHandler, false);

        Admin = null;
        GameStarted = false;

        if (_panelGameInfo != null) {
            _panelGameInfo.gameObject.SetActive(true);
            RefreshGameInfo();
        }
    }

    public override void OnStopServer() {
        base.OnStopServer();
        NetworkServer.ClearHandlers();

        Admin = null;
        GameStarted = false;
        if (_panelGameInfo != null)
            _panelGameInfo.gameObject.SetActive(false);
    }

    public override void OnStartClient() {
        base.OnStartClient();
        NetworkClient.RegisterHandler<MessageGameInfo>(MessageGameInfoClientHandler, false);
        NetworkClient.RegisterHandler<MessageGameStatus>(MessageGameStatusClientHandler, false);
        NetworkClient.RegisterHandler<MessageUsedColors>(MessageUsedColorsClientHandler, false);
        NetworkClient.RegisterHandler<MessageGameStatusData>(MessageGameStatusDataClientHandler, false);

        _panelGameInfo.gameObject.SetActive(true);

        RefreshGameInfo();
    }

    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);
        NetworkClient.UnregisterHandler<MessageGameInfo>();
        NetworkClient.UnregisterHandler<MessageGameStatus>();
        Admin = null;
        GameStarted = false;
        if (_panelGameInfo != null)
            _panelGameInfo.gameObject.SetActive(false);
    }

    public override void OnServerConnect(NetworkConnection conn) {
        base.OnServerConnect(conn);
        if (GameStarted) {
            conn.Disconnect();
            return;
        }

        // ResendGameInfoData(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn) {
        base.OnServerAddPlayer(conn);
        if (GameStarted)
            return;
        Player p = conn.identity.GetComponent<Player>();
        p.Name = "Player";

        ResendGameInfoData(conn);
        if (Admin == null) {
            Admin = p;
            conn.Send(new MessageGameStatus {
                                                GameStarted = false,
                                                Admin = conn.identity.gameObject
                                            });
        }
    }

    public void ResendGameInfoData(NetworkConnection conn = null) {
        MessageGameInfo info = new MessageGameInfo {
                                                       VictimVision = VictimVision,
                                                       VictimLives = VictimLives,
                                                       VictimSpeed = VictimSpeed,
                                                       HunterVision = HunterVision,
                                                       HunterKillCooldown = HunterKillCooldown,
                                                       HunterSpeed = HunterSpeed,
                                                       Hunters = Hunters,
                                                       DisplayHunters = DisplayHunters,
                                                       VictimTaskDistance = VictimTaskDistance,
                                                       HunterKillDistance = HunterKillDistance,
                                                       HunterVisionOnCooldown = HunterVisionOnCooldown,
                                                       VictimCommonTasks = VictimCommonTasks,
                                                       VictimLongTasks = VictimLongTasks,
                                                       VictimShortTasks = VictimShortTasks,
                                                       TimeLimit = TimeLimit,
                                                       DefaultColor = DefaultColor,
                                                       TasksBalancedDamage = TasksBalancedDamage
                                                   };
        if (NetworkServer.active) {
            if (conn != null)
                conn.Send(info);
            else
                NetworkServer.SendToAll(info);
        }
        else {
            NetworkClient.Send(info);
        }
    }

    public void UIChangeGameInfo() {
        if (IsAdmin) {
            float.TryParse(_panelGameInfo.Find("VictimVision").GetComponent<InputField>().text.Replace(",", "."), out VictimVision);
            int.TryParse(_panelGameInfo.Find("VictimLives").GetComponent<InputField>().text, out VictimLives);
            float.TryParse(_panelGameInfo.Find("VictimSpeed").GetComponent<InputField>().text.Replace(",", "."), out VictimSpeed);
            float.TryParse(_panelGameInfo.Find("HunterVision").GetComponent<InputField>().text.Replace(",", "."), out HunterVision);
            float.TryParse(_panelGameInfo.Find("HunterKillCooldown").GetComponent<InputField>().text.Replace(",", "."), out HunterKillCooldown);
            float.TryParse(_panelGameInfo.Find("HunterSpeed").GetComponent<InputField>().text.Replace(",", "."), out HunterSpeed);
            int.TryParse(_panelGameInfo.Find("Hunters").GetComponent<InputField>().text, out Hunters);
            int.TryParse(_panelGameInfo.Find("TasksCommon").GetComponent<InputField>().text, out VictimCommonTasks);
            int.TryParse(_panelGameInfo.Find("TasksLong").GetComponent<InputField>().text, out VictimLongTasks);
            int.TryParse(_panelGameInfo.Find("TasksShort").GetComponent<InputField>().text, out VictimShortTasks);
            ResendGameInfoData();
        }

        string playerName = _panelGameInfo.Find("PlayerName").GetComponent<InputField>().text;
        if (playerName.Length > 4 && playerName.Length <= 16) {
            NetworkClient.Send(new MessagePlayerInfo {
                                                         Name = playerName
                                                     });
        }
        else {
            _panelGameInfo.Find("PlayerName").GetComponent<InputField>().text = "";
        }
    }

    public void UIStartGame() {
        if (!IsAdmin)
            return;

        // if (Hunters > NetworkServer.connections.Count / 2)
        //     return;

        MessageGameStatus status = new MessageGameStatus {
                                                             GameStarted = true,
                                                             GameFinished = false
                                                         };
        NetworkClient.Send(status);
    }

    public void RefreshGameInfo() {
        bool admin = IsAdmin;
        _panelGameInfo.Find("VictimVision").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("VictimLives").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("VictimSpeed").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("HunterVision").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("HunterKillCooldown").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("HunterSpeed").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("Hunters").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("TasksCommon").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("TasksLong").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("TasksShort").GetComponent<InputField>().interactable = admin;
        // _panelGameInfo.Find("ButtonChange").GetComponent<Button>().interactable = IsAdmin;
        _startButton.gameObject.SetActive(admin);

        _panelGameInfo.Find("VictimVision").GetComponent<InputField>().text = VictimVision.ToString(CultureInfo.InvariantCulture);
        _panelGameInfo.Find("VictimLives").GetComponent<InputField>().text = VictimLives.ToString(CultureInfo.InvariantCulture);
        _panelGameInfo.Find("VictimSpeed").GetComponent<InputField>().text = VictimSpeed.ToString(CultureInfo.InvariantCulture);
        _panelGameInfo.Find("HunterVision").GetComponent<InputField>().text = HunterVision.ToString(CultureInfo.InvariantCulture);
        _panelGameInfo.Find("HunterKillCooldown").GetComponent<InputField>().text = HunterKillCooldown.ToString(CultureInfo.InvariantCulture);
        _panelGameInfo.Find("HunterSpeed").GetComponent<InputField>().text = HunterSpeed.ToString(CultureInfo.InvariantCulture);
        _panelGameInfo.Find("Hunters").GetComponent<InputField>().text = Hunters.ToString();
        _panelGameInfo.Find("TasksCommon").GetComponent<InputField>().text = VictimCommonTasks.ToString();
        _panelGameInfo.Find("TasksLong").GetComponent<InputField>().text = VictimLongTasks.ToString();
        _panelGameInfo.Find("TasksShort").GetComponent<InputField>().text = VictimShortTasks.ToString();
    }

    private void UpdateGameInfoData(MessageGameInfo msg) {
        VictimVision = msg.VictimVision;
        VictimLives = msg.VictimLives;
        VictimSpeed = msg.VictimSpeed;
        HunterVision = msg.HunterVision;
        HunterKillCooldown = msg.HunterKillCooldown;
        HunterSpeed = msg.HunterSpeed;
        Hunters = msg.Hunters;
        DisplayHunters = msg.DisplayHunters;
        VictimTaskDistance = msg.VictimTaskDistance;
        HunterKillDistance = msg.HunterKillDistance;
        HunterVisionOnCooldown = msg.HunterVisionOnCooldown;
        VictimCommonTasks = msg.VictimCommonTasks;
        VictimLongTasks = msg.VictimLongTasks;
        VictimShortTasks = msg.VictimShortTasks;
        TimeLimit = msg.TimeLimit;
        DefaultColor = msg.DefaultColor;
        TasksBalancedDamage = msg.TasksBalancedDamage;
    }

    private void MessageGameInfoClientHandler(MessageGameInfo msg) {
        UpdateGameInfoData(msg);
        RefreshGameInfo();
    }

    private void MessageGameStatusClientHandler(MessageGameStatus msg) {
        GameStarted = msg.GameStarted;
        Admin = msg.Admin.GetComponent<Player>();
        if (!GameStarted) {
            if (msg.GameFinished) {
            }
            else {
                RefreshGameInfo();
            }
        }
        else {
            StatusStartTime = (float) NetworkTime.time;
            GameScreenDrawer.Instance.Intro = true;
            GameScreenDrawer.Instance.Outro = false;
            _usedColors.Clear();
        }
    }

    private void MessageUsedColorsClientHandler(MessageUsedColors msg) {
        _usedColors = msg.UsedColors;
    }

    private void MessageGameStatusDataClientHandler(MessageGameStatusData msg) {
        StatusHuntersHealth = msg.HuntersHealth;
        StatusHuntersMaxHealth = msg.HuntersMaxHealth;
        StatusVictimsHealth = msg.VictimsHealth;
        StatusVictimsMaxHealth = msg.VictimsMaxHealth;
    }

    private void MessagePlayerInfoServerHandler(NetworkConnection conn, MessagePlayerInfo msg) {
        Player player = conn.identity.GetComponent<Player>();
        player.Name = msg.Name;
    }

    private void MessageGameInfoServerHandler(NetworkConnection conn, MessageGameInfo msg) {
        UpdateGameInfoData(msg);
        ResendGameInfoData();
    }

    private void MessageGameStatusServerHandler(NetworkConnection conn, MessageGameStatus msg) {
        if (Admin != conn.identity.GetComponent<Player>())
            return;

        if (!msg.GameStarted)
            return;

        GameStarted = true;
        _usedColors.Clear();
        msg.Admin = Admin.gameObject;
        NetworkServer.SendToAll(msg);
        StatusStartTime = (float) NetworkTime.time;
        List<int> connectionIds = new List<int>();
        foreach (int connectionsKey in NetworkServer.connections.Keys) {
            connectionIds.Add(connectionsKey);
        }

        int huntersCount = connectionIds.Count >= Hunters ? Hunters : connectionIds.Count;
        Debug.Log("HUNTERS = " + huntersCount);
        int[] hunters = new int[huntersCount];
        for (int i = 0; i < huntersCount; ++i) {
            hunters[i] = -100;
            int id = Random.Range(0, connectionIds.Count);
            if (hunters.Contains(id)) {
                i--;
                continue;
            }

            hunters[i] = id;
        }

        Player.HunterPlayer = null;

        List<Color> availableColors = new List<Color>(PreCreatedColors);
        foreach (KeyValuePair<int, NetworkConnectionToClient> player in NetworkServer.connections) {
            Player p = player.Value.identity.GetComponent<Player>();
            p.IsHunter = false;
            Debug.Log("Color = " + p.Color);
            if (p.Name == "HUNTER") {
                p.IsHunter = true;
                Player.HunterPlayer = p;
            }

            foreach (int hunter in hunters) {
                if (player.Key == connectionIds[hunter]) {
                    p.IsHunter = true;
                    if (Player.HunterPlayer == null)
                        Player.HunterPlayer = p;
                }
            }

            int colorIndex = Random.Range(0, availableColors.Count);
            p.SetColor(availableColors[colorIndex]);
            availableColors.RemoveAt(colorIndex);
            p.RefreshStats();
        }

        ServerChangeScene("SampleScene");
    }

    public override void OnServerSceneChanged(string sceneName) {
        base.OnServerSceneChanged(sceneName);
        if (!GameStarted)
            return;

        GameLocalTask[] tasks = FindObjectsOfType<GameLocalTask>();
        Dictionary<GameLocalTask.LocalTaskType, GameLocalTask[]> typedTasks = new Dictionary<GameLocalTask.LocalTaskType, GameLocalTask[]>();
        typedTasks.Add(GameLocalTask.LocalTaskType.Common, tasks.Where(task => task.Type == GameLocalTask.LocalTaskType.Common).ToArray());
        typedTasks.Add(GameLocalTask.LocalTaskType.Long, tasks.Where(task => task.Type == GameLocalTask.LocalTaskType.Long).ToArray());
        typedTasks.Add(GameLocalTask.LocalTaskType.Short, tasks.Where(task => task.Type == GameLocalTask.LocalTaskType.Short).ToArray());

        List<GameLocalTask> selectedCommonTasks = new List<GameLocalTask>();
        List<GameLocalTask> availableCommonTasks = new List<GameLocalTask>(typedTasks[GameLocalTask.LocalTaskType.Common]);
        int total = VictimCommonTasks <= availableCommonTasks.Count ? VictimCommonTasks : availableCommonTasks.Count;
        for (int i = 0; i < total; ++i) {
            int taskIndex = Random.Range(0, availableCommonTasks.Count);
            selectedCommonTasks.Add(availableCommonTasks[taskIndex]);
            availableCommonTasks.RemoveAt(taskIndex);
        }

        int huntersHealth = 0;
        int victimsHealth = 0;
        int huntersCount = 0;
        foreach (KeyValuePair<int, NetworkConnectionToClient> player in NetworkServer.connections) {
            Player p = player.Value.identity.GetComponent<Player>();
            Transform startPosition = GetStartPosition();

            if (!p.IsHunter) {
                foreach (GameLocalTask.LocalTaskType taskType in typedTasks.Keys) {
                    if (taskType == GameLocalTask.LocalTaskType.Common) {
                        // Debug.Log("COMMON => " + selectedCommonTasks.Count);
                        foreach (GameLocalTask selectedCommonTask in selectedCommonTasks) {
                            p.TaskList.Add(selectedCommonTask);
                        }

                        continue;
                    }

                    List<GameLocalTask> availableTasks = new List<GameLocalTask>(typedTasks[taskType]);
                    int req = taskType == GameLocalTask.LocalTaskType.Long ? VictimLongTasks : VictimShortTasks;
                    total = availableTasks.Count;
                    // Debug.Log(taskType + " => " + total);
                    for (int i = 0; i < (req <= total ? req : total); ++i) {
                        int taskIndex = Random.Range(0, availableTasks.Count);
                        p.TaskList.Add(availableTasks[taskIndex]);
                        availableTasks.RemoveAt(taskIndex);
                    }
                }

                foreach (GameLocalTask gameLocalTask in p.TaskList) {
                    huntersHealth += gameLocalTask.Damage;
                    foreach (GameTask gameTask in gameLocalTask.ActivateOnFinish) {
                        if (gameTask is GameLocalTask gameLocalTask1) {
                            huntersHealth += gameLocalTask1.Damage;
                        }
                    }
                }

                victimsHealth += p.Lives;

                p.SynchronizeTaskList();
            }
            else {
                huntersCount++;
            }

            if (startPosition == null)
                continue;
            p.transform.position = startPosition.position;
            p.RpcSetPosition(p.transform.position);
        }

        foreach (KeyValuePair<int, NetworkConnectionToClient> player in NetworkServer.connections) {
            player.Value.identity.RebuildObservers(false);
        }

        // StartCoroutine(SendTaskList(1f));

        huntersHealth *= huntersCount;
        StatusHuntersMaxHealth = huntersHealth;
        SetHuntersHealth(huntersHealth, true);
        StatusVictimsMaxHealth = victimsHealth;
        SetVictimsHealth(victimsHealth);
    }

    public override void OnClientSceneChanged(NetworkConnection conn) {
        base.OnClientSceneChanged(conn);
        Debug.Log("CL CHANGE");
        conn.identity.GetComponent<Player>().SynchronizeTaskList();
    }

    // private IEnumerator SendTaskList(float time) {
    //     yield return new WaitForSeconds(time);
    //
    //     foreach (KeyValuePair<int, NetworkConnectionToClient> networkConnectionToClient in NetworkServer.connections) {
    //         networkConnectionToClient.Value.identity.GetComponent<Player>().SynchronizeTaskList();
    //     }
    // }

    public List<Color> GetUsedColors() {
        return _usedColors;
        // if (_usedColors != null)
        //     return _usedColors;
        //
        // List<Color> usedColors = new List<Color>();
        // usedColors.Add(Player.Local.Color);
        // Player[] players = GameObject.FindObjectsOfType<Player>();
        // foreach (Player player in players) {
        //     if (usedColors.Contains(player.Color))
        //         continue;
        //     usedColors.Add(player.Color);
        // }
        //
        // _usedColors = usedColors.ToArray();
        // return _usedColors;
    }

    public void ReplaceUsedColor(Color oldColor, Color newColor, bool dontDelete = false) {
        if (_usedColors == null)
            return;
        bool changed = false;
        if (!dontDelete)
            for (var i = 0; i < _usedColors.Count; i++) {
                if (_usedColors[i] == oldColor) {
                    _usedColors[i] = newColor;
                    changed = true;
                    break;
                }
            }

        if (!changed) {
            if (!_usedColors.Contains(newColor))
                _usedColors.Add(newColor);
        }

        NetworkServer.SendToAll(new MessageUsedColors {UsedColors = _usedColors});
    }

    public void SendCurrentGameStatusData() {
        NetworkServer.SendToAll(new MessageGameStatusData {
                                                              HuntersMaxHealth = StatusHuntersMaxHealth,
                                                              HuntersHealth = StatusHuntersHealth,
                                                              VictimsMaxHealth = StatusVictimsMaxHealth,
                                                              VictimsHealth = StatusVictimsHealth
                                                          });
    }

    public void SetHuntersHealth(int health, bool skipSendInfo = false) {
        StatusHuntersHealth = health;
        if (!skipSendInfo) SendCurrentGameStatusData();
    }

    public void SetVictimsHealth(int health, bool skipSendInfo = false) {
        StatusVictimsHealth = health;
        if (!skipSendInfo) SendCurrentGameStatusData();
    }
}