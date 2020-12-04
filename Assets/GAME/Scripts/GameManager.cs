using System;
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

    private Transform _panelGameInfo;
    private Button    _startButton;
    private Camera    _camera;

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
    }

    public static Camera GetCamera() {
        return Instance._camera;
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
                                                       HunterSpeed = HunterSpeed
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
            VictimLives = _panelGameInfo.Find("VictimLives").GetComponent<Dropdown>().value + 1;
            float.TryParse(_panelGameInfo.Find("VictimSpeed").GetComponent<InputField>().text.Replace(",", "."), out VictimSpeed);
            float.TryParse(_panelGameInfo.Find("HunterVision").GetComponent<InputField>().text.Replace(",", "."), out HunterVision);
            float.TryParse(_panelGameInfo.Find("HunterKillCooldown").GetComponent<InputField>().text.Replace(",", "."), out HunterKillCooldown);
            float.TryParse(_panelGameInfo.Find("HunterSpeed").GetComponent<InputField>().text.Replace(",", "."), out HunterSpeed);
            int.TryParse(_panelGameInfo.Find("Hunters").GetComponent<InputField>().text.Replace(",", "."), out Hunters);
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
                                                             GameStarted = true
                                                         };
        NetworkClient.Send(status);
    }

    public void RefreshGameInfo() {
        bool admin = IsAdmin;
        _panelGameInfo.Find("VictimVision").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("VictimLives").GetComponent<Dropdown>().interactable = admin;
        _panelGameInfo.Find("VictimSpeed").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("HunterVision").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("HunterKillCooldown").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("HunterSpeed").GetComponent<InputField>().interactable = admin;
        _panelGameInfo.Find("Hunters").GetComponent<InputField>().interactable = admin;
        // _panelGameInfo.Find("ButtonChange").GetComponent<Button>().interactable = IsAdmin;
        _startButton.gameObject.SetActive(admin);

        _panelGameInfo.Find("VictimVision").GetComponent<InputField>().text = VictimVision.ToString(CultureInfo.InvariantCulture);
        _panelGameInfo.Find("VictimLives").GetComponent<Dropdown>().value = VictimLives - 1;
        _panelGameInfo.Find("VictimSpeed").GetComponent<InputField>().text = VictimSpeed.ToString(CultureInfo.InvariantCulture);
        _panelGameInfo.Find("HunterVision").GetComponent<InputField>().text = HunterVision.ToString(CultureInfo.InvariantCulture);
        _panelGameInfo.Find("HunterKillCooldown").GetComponent<InputField>().text = HunterKillCooldown.ToString(CultureInfo.InvariantCulture);
        _panelGameInfo.Find("HunterSpeed").GetComponent<InputField>().text = HunterSpeed.ToString(CultureInfo.InvariantCulture);
        _panelGameInfo.Find("Hunters").GetComponent<InputField>().text = Hunters.ToString();
    }

    private void UpdateGameInfoData(MessageGameInfo msg) {
        VictimVision = msg.VictimVision;
        VictimLives = msg.VictimLives;
        VictimSpeed = msg.VictimSpeed;
        HunterVision = msg.HunterVision;
        HunterKillCooldown = msg.HunterKillCooldown;
        HunterSpeed = msg.HunterSpeed;
    }

    private void MessageGameInfoClientHandler(MessageGameInfo msg) {
        UpdateGameInfoData(msg);
        RefreshGameInfo();
    }

    private void MessageGameStatusClientHandler(MessageGameStatus msg) {
        GameStarted = msg.GameStarted;
        Admin = msg.Admin.GetComponent<Player>();
        if (!GameStarted)
            RefreshGameInfo();
        else {
            GameScreenDrawer.Instance.Intro = true;
            GameScreenDrawer.Instance.Outro = false;
        }
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
        msg.Admin = Admin.gameObject;
        NetworkServer.SendToAll(msg);
        List<int> connectionIds = new List<int>();
        foreach (int connectionsKey in NetworkServer.connections.Keys) {
            connectionIds.Add(connectionsKey);
        }

        int[] hunters = new int[Hunters];
        for (int i = 0; i < Hunters; ++i) {
            hunters[i] = -1;
            int id = Random.Range(0, connectionIds.Count);
            if (hunters.Contains(id)) {
                i--;
                continue;
            }

            hunters[i] = id;
        }

        Player.HunterPlayer = null;

        foreach (KeyValuePair<int, NetworkConnectionToClient> player in NetworkServer.connections) {
            Player p = player.Value.identity.GetComponent<Player>();
            p.IsHunter = false;
            foreach (int hunter in hunters) {
                if (player.Key == connectionIds[hunter]) {
                    p.IsHunter = true;
                    if (Player.HunterPlayer == null)
                        Player.HunterPlayer = p;
                }
            }

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

        foreach (KeyValuePair<int, NetworkConnectionToClient> player in NetworkServer.connections) {
            Player p = player.Value.identity.GetComponent<Player>();
            Transform startPosition = GetStartPosition();

            if (!p.IsHunter) {
                foreach (GameLocalTask.LocalTaskType taskType in typedTasks.Keys) {
                    if (taskType == GameLocalTask.LocalTaskType.Common) {
                        foreach (GameLocalTask selectedCommonTask in selectedCommonTasks) {
                            p.TaskList.Add(selectedCommonTask);
                        }

                        continue;
                    }

                    List<GameLocalTask> availableTasks = new List<GameLocalTask>(typedTasks[taskType]);
                    int req = taskType == GameLocalTask.LocalTaskType.Common ? VictimCommonTasks : taskType == GameLocalTask.LocalTaskType.Long ? VictimLongTasks : VictimShortTasks;
                    total = availableTasks.Count;
                    Debug.Log(taskType + " => " + total);
                    for (int i = 0; i < (req <= total ? req : total); ++i) {
                        int taskIndex = Random.Range(0, availableTasks.Count);
                        p.TaskList.Add(availableTasks[taskIndex]);
                        availableTasks.RemoveAt(taskIndex);
                    }
                }

                p.SynchronizeTaskList();
            }

            if (startPosition == null)
                continue;
            p.transform.position = startPosition.position;
            p.RpcSetPosition(p.transform.position);
        }
    }
}