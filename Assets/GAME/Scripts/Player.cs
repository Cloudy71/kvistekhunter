using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Mirror;
using UnityEngine;

// TODO: CmdKill cannot be called from Server, refactor...
public class Player : NetworkBehaviour {
    public static Player Local;
    public static Player HunterPlayer;

    public static Player GetLocal => Local == null ? HunterPlayer : Local;

    [SyncVar]
    public string Name;

    [SyncVar]
    public bool IsHunter;

    [SyncVar]
    public int Lives;

    [SyncVar]
    public float Speed;

    [SyncVar]
    public float Vision;

    [SyncVar]
    public int Points;

    [SyncVar]
    public float Distance;

    [SyncVar]
    public float Cooldown;

    [SyncVar]
    public float LastAction;

    [SyncVar]
    public bool SeesEveryone;

    public GameTask CurrentTask;

    private Rigidbody    _rigidbody;
    private Light        _light;
    private MeshRenderer _model;
    private Texture2D    _nameGradient;
    private LineRenderer _lineRenderer;
    private float        _visionTransition;
    private Animator     _animator;

    private Vector3 _oldVelocity;
    private byte    _syncMovementKeys;

    private void Awake() {
        Vision = 10f;

        _rigidbody = GetComponent<Rigidbody>();
        _model = transform.GetChild(0).GetComponent<MeshRenderer>();
        _light = transform.GetChild(1).GetComponent<Light>();
        _lineRenderer = GetComponent<LineRenderer>();
        _animator = transform.Find("Model").GetComponent<Animator>();
        _syncMovementKeys = 0b0000;
        _oldVelocity = Vector3.zero;
    }

    // Start is called before the first frame update
    void Start() {
        if (isServer) {
            RefreshStats();
            Points = 0;
            LastAction = -1000f;
        }

        DontDestroyOnLoad(gameObject);

        _light.enabled = isLocalPlayer;

        if (isLocalPlayer) Local = this;

        CurrentTask = null;

        _nameGradient = AssetLoader.GetGradient(0, 0, 0, 0, 0, 0, 0, 255, AssetLoader.GradientSide.BOTTOM, 16);
        VisibilityController();
    }

    public void RefreshStats() {
        if (!isServer)
            return;
        Lives = GameManager.Instance.VictimLives;
        Speed = IsHunter ? GameManager.Instance.HunterSpeed : GameManager.Instance.VictimSpeed;
        Vision = IsHunter ? GameManager.Instance.HunterVision : GameManager.Instance.VictimVision;
        Distance = IsHunter ? GameManager.Instance.HunterKillDistance : GameManager.Instance.VictimTaskDistance;
        Cooldown = IsHunter ? GameManager.Instance.HunterKillCooldown : 0f;
        LastAction = (float) NetworkTime.time - Cooldown + 10f;
    }

    // Update is called once per frame
    void Update() {
        if (isServer) {
            Vision = IsHunter
                         ? LastAction + Cooldown - NetworkTime.time > 0f || GameStatus.Instance.LightsOff ? GameManager.Instance.HunterVisionOnCooldown : GameManager.Instance.HunterVision
                         : GameManager.Instance.VictimVision;
        }

        gameObject.layer = IsHunter ? 8 : 9;

        SyncMovement();
        VisibilityController();
        if (!isLocalPlayer) return;
        Movement();
        CameraMovement();
        VisionController();
        HunterController();
    }

    private void SyncMovement() {
        if (isLocalPlayer)
            return;

        Vector3 velocity = Vector3.zero;

        if ((_syncMovementKeys & 0b1) == 1) {
            velocity += new Vector3(1f, 0f, 0f);
        }

        if (((_syncMovementKeys >> 1) & 0b1) == 1) {
            velocity += new Vector3(-1f, 0f, 0f);
        }

        if (((_syncMovementKeys >> 2) & 0b1) == 1) {
            velocity += new Vector3(0f, 0f, 1f);
        }

        if (((_syncMovementKeys >> 3) & 0b1) == 1) {
            velocity += new Vector3(0f, 0f, -1f);
        }

        if (velocity != Vector3.zero) {
            velocity *= Speed;
        }

        velocity.y = _rigidbody.velocity.y;

        _rigidbody.velocity = velocity;

        // if (Input.GetKey(KeyCode.W)) {
        //     velocity += new Vector3(1f, 0f, 0f);
        //     keys |= 0b1;
        // }
        //
        // if (Input.GetKey(KeyCode.S)) {
        //     velocity += new Vector3(-1f, 0f, 0f);
        //     keys |= 0b10;
        // }
        //
        // if (Input.GetKey(KeyCode.A)) {
        //     velocity += new Vector3(0f, 0f, 1f);
        //     keys |= 0b100;
        // }
        //
        // if (Input.GetKey(KeyCode.D)) {
        //     velocity += new Vector3(0f, 0f, -1f);
        //     keys |= 0b1000;
        // }
    }

    private void Movement() {
        if (Lives == 0)
            return;

        Vector3 velocity = Vector3.zero;
        byte keys = 0b0000;

        if (CurrentTask == null) {
            if (Input.GetKey(KeyCode.W)) {
                velocity += new Vector3(1f, 0f, 0f);
                keys |= 0b1;
            }

            if (Input.GetKey(KeyCode.S)) {
                velocity += new Vector3(-1f, 0f, 0f);
                keys |= 0b10;
            }

            if (Input.GetKey(KeyCode.A)) {
                velocity += new Vector3(0f, 0f, 1f);
                keys |= 0b100;
            }

            if (Input.GetKey(KeyCode.D)) {
                velocity += new Vector3(0f, 0f, -1f);
                keys |= 0b1000;
            }
        }

        if (velocity != Vector3.zero) {
            velocity *= Speed;
        }

        if (keys != _syncMovementKeys || velocity == Vector3.zero && _oldVelocity != velocity) {
            CmdSyncMovement(transform.position, keys);
        }

        _oldVelocity = velocity;

        velocity.y = _rigidbody.velocity.y;

        _rigidbody.velocity = velocity;

        if (Input.GetKeyDown(KeyCode.Space) && CurrentTask == null) {
            GameTask task = PhysicsUtils.GetNearestObjectHit<GameTask>(Physics.SphereCastAll(transform.position, Distance, Vector3.up, Distance), transform);
            if (task != null && (task.HunterActive && IsHunter || task.VictimActive && !IsHunter))
                CmdTaskOpen(task.gameObject);
        }

        if (Input.GetKeyDown(KeyCode.Escape) && CurrentTask != null && CurrentTask.Closeable) {
            CurrentTask.OnTaskCloseClient();
            CmdTaskClose();
        }
    }

    private void CameraMovement() {
        Transform cameraTransform = GameManager.GetCamera().transform;
        Vector3 position = transform.position;

        if (Lives > 0) {
            cameraTransform.position = position + new Vector3(-5f, 10f, 0f);
            cameraTransform.LookAt(position);
        }
        else {
            if (Input.GetKey(KeyCode.W)) {
                cameraTransform.transform.position += new Vector3(1f, 0f, 0f) * (Time.deltaTime * Speed * 1.5f);
            }

            if (Input.GetKey(KeyCode.S)) {
                cameraTransform.transform.position += new Vector3(-1f, 0f, 0f) * (Time.deltaTime * Speed * 1.5f);
            }

            if (Input.GetKey(KeyCode.A)) {
                cameraTransform.transform.position += new Vector3(0f, 0f, 1f) * (Time.deltaTime * Speed * 1.5f);
            }

            if (Input.GetKey(KeyCode.D)) {
                cameraTransform.transform.position += new Vector3(0f, 0f, -1f) * (Time.deltaTime * Speed * 1.5f);
            }
        }
    }

    private void VisionController() {
        if (!GameManager.Instance.GameStarted || Lives == 0) {
            _light.range = 0f;
            RenderSettings.ambientLight = Color.white;
            return;
        }

        RenderSettings.ambientLight = new Color32(51, 51, 51, 255);
        if (!_visionTransition.Equals(Vision)) {
            _visionTransition = Mathf.MoveTowards(_visionTransition, Vision, Time.deltaTime * 10f);
        }

        _light.range = _visionTransition;
    }

    private void VisibilityController() {
        _model.material =
            isLocalPlayer || !GameManager.Instance.GameStarted ? IsHunter ? GameAssets.Instance.MaterialPlayerHunter : GameAssets.Instance.MaterialPlayerLocal :
            GameManager.Instance.DisplayHunters && IsHunter    ? GameAssets.Instance.MaterialPlayerHunter : GameAssets.Instance.MaterialPlayerOther;
    }

    private void HunterController() {
        if (!IsHunter)
            return;

        if (NetworkTime.time >= LastAction + Cooldown) {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, Distance, Vector3.up, Distance);
            Transform nearest = PhysicsUtils.GetNearestPlayerHit(hits, transform);
            if (!(nearest is null)) {
                RaycastHit[] hits1 = Physics.RaycastAll(transform.position + new Vector3(0f, 1f, 0f), (nearest.position + new Vector3(0f, 1f, 0f)) - (transform.position + new Vector3(0f, 1f, 0f)),
                                                        Distance);
                Transform nearest1 = PhysicsUtils.GetNearestHit(hits1, transform);
                if (nearest1 == nearest) {
                    _lineRenderer.positionCount = 2;
                    _lineRenderer.SetPositions(new[] {
                                                         transform.position + new Vector3(0f, 1f, 0f),
                                                         nearest.position + new Vector3(0f, 1f, 0f)
                                                     });

                    if (Input.GetKeyDown(KeyCode.Q)) {
                        CmdKill(nearest.gameObject);
                    }

                    return;
                }
            }
        }

        _lineRenderer.positionCount = 0;
    }

    [Command]
    public void CmdSyncMovement(Vector3 position, byte keys) {
        transform.position = position;
        _syncMovementKeys = keys;
        RpcSyncMovement(position, keys);
    }

    [ClientRpc]
    private void RpcSyncMovement(Vector3 position, byte keys) {
        // string bitString = Convert.ToString(keys, 2);
        // int len = bitString.Length;
        // for (int i = 0; i < 4 - len; ++i) {
        //     bitString = "0" + bitString;
        // }
        //
        // Debug.Log("MOVE " + name + " => " + bitString);
        if (isLocalPlayer)
            return;
        transform.position = position;
        _syncMovementKeys = keys;
    }

    public void Kill(GameObject target) {
        if (isServer) KillInternal(target);
        else CmdKill(target);
    }

    private void KillInternal(GameObject target) {
        if (NetworkTime.time < LastAction + Cooldown)
            return;
        // RaycastHit[] hits = Physics.SphereCastAll(transform.position, Distance, Vector3.up, Distance);
        // Transform nearest = PhysicsUtils.GetNearestPlayerHit(hits, transform);
        // if (nearest == null || nearest.gameObject != target)
        //     return;
        Player p = target.GetComponent<Player>();
        if (p.CurrentTask != null && p.CurrentTask.UnkillableInTask)
            return;
        if (p.CurrentTask != null) {
            p.CurrentTask.GetComponent<GameTask>().OnTaskClose(p);
            p.CurrentTask = null;
            p.TargetSetTask(p.connectionToClient, null);
        }

        int lives = p.Lives /* - 1*/;
        p.Lives = 0;
        LastAction = (float) NetworkTime.time;
        Vector3 force = (target.transform.position - transform.position) * 300f;
        p.RpcSendForce(force);
        p._rigidbody.constraints = RigidbodyConstraints.None;
        p._rigidbody.AddForce(force, ForceMode.Force);
        if (lives > 0) {
            p.StartCoroutine(p.waitToRespawn(8f, lives));
        }

        RpcSetPosition(p.transform.position);
    }

    [Command]
    private void CmdKill(GameObject target) {
        KillInternal(target);
    }

    [Command]
    public void CmdTaskOpen(GameObject task) {
        GameTask t = task.GetComponent<GameTask>();
        if (NetworkTime.time < (IsHunter ? t.LastHunterOpened + t.HunterCooldown : t.LastVictimOpened + t.VictimCooldown))
            return;
        if (t.OnTaskOpen(this)) {
            CurrentTask = t;
            TargetSetTask(connectionToClient, task);
        }
    }

    [Command]
    public void CmdTaskFinish(TaskPayload payload) {
        if (CurrentTask == null)
            return;
        GameTask t = CurrentTask.GetComponent<GameTask>();
        if (IsHunter && !t.HunterActive || !IsHunter && !t.VictimActive)
            return;
        t.OnTaskFinish(this, payload.Data);
        if (IsHunter)
            t.LastHunterOpened = (float) NetworkTime.time;
        else
            t.LastVictimOpened = (float) NetworkTime.time;
        CurrentTask = null;
        TargetSetTask(connectionToClient, null);
    }

    [Command]
    public void CmdTaskClose() {
        if (CurrentTask == null)
            return;
        GameTask t = CurrentTask.GetComponent<GameTask>();
        if (!t.Closeable)
            return;
        t.OnTaskClose(this);
        CurrentTask = null;
        TargetSetTask(connectionToClient, null);
    }

    [Command]
    public void CmdTaskStep(TaskPayload payload) {
        if (CurrentTask == null)
            return;
        GameTask t = CurrentTask.GetComponent<GameTask>();
        if (t.SetCooldownOnStep)
            if (IsHunter)
                t.LastHunterOpened = (float) NetworkTime.time;
            else
                t.LastVictimOpened = (float) NetworkTime.time;
        t.OnTaskStep(this, payload.Data);
    }

    [ClientRpc]
    public void RpcSendForce(Vector3 force) {
        _rigidbody.constraints = RigidbodyConstraints.None;
        _rigidbody.AddForce(force, ForceMode.Force);
    }

    [ClientRpc]
    public void RpcResetRigidbody() {
        transform.rotation = Quaternion.identity;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    [ClientRpc]
    public void RpcSetPosition(Vector3 position) {
        transform.position = position;
    }

    [TargetRpc]
    public void TargetSetTask(NetworkConnection conn, GameObject taskObject) {
        if (taskObject == null) {
            CurrentTask = null;
            return;
        }

        CurrentTask = taskObject.GetComponent<GameTask>();
    }

    [ClientRpc]
    public void RpcPlayAnimation(string animation) {
        _animator.Play(animation);
    }

    private IEnumerator waitToRespawn(float time, int lives) {
        yield return new WaitForSeconds(time);
        Lives = lives;
        transform.position = GameManager.Instance.GetStartPosition().transform.position;
        RpcSetPosition(transform.position);
        RpcResetRigidbody();
        RpcPlayAnimation("Idle");
        transform.rotation = Quaternion.identity;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnGUI() {
        if (!_model.isVisible || GameManager.GetCamera() == null)
            return;

        Vector3 pos = GameManager.GetCamera().WorldToScreenPoint(transform.position + new Vector3(0f, 2.8f, 0f));

        GUI.contentColor = Color.white;
        float width = GUI.skin.label.CalcSize(new GUIContent(Name)).x;
        GUI.DrawTexture(new Rect(pos.x - width / 2f, Screen.height - pos.y - 8f, width, 16f), _nameGradient);
        GUI.Label(new Rect(pos.x - width / 2f, Screen.height - pos.y - 8f, width, 20f), Name);
        GUI.contentColor = Color.white;

        if (isLocalPlayer) {
            if (IsHunter) {
                float cd = Mathf.Ceil(LastAction + Cooldown - (float) NetworkTime.time);
                if (cd > 0f) {
                    GUI.contentColor = Color.black;
                    GUI.Label(new Rect(pos.x - 8f, Screen.height - pos.y + 12f, 16f, 18f), cd.ToString(CultureInfo.InvariantCulture));
                    GUI.contentColor = new Color32(200, 128, 200, 255);
                    GUI.Label(new Rect(pos.x - 9f, Screen.height - pos.y + 11f, 16f, 18f), cd.ToString(CultureInfo.InvariantCulture));
                    GUI.contentColor = Color.white;
                }
            }

            if (CurrentTask != null)
                CurrentTask.OnTaskGUI();
        }
    }
}