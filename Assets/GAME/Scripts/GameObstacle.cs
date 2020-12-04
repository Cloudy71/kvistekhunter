using System.Collections;
using Mirror;
using UnityEngine;

public abstract class GameObstacle : NetworkBehaviour {
    [SyncVar]
    public bool Active;

    [SyncVar]
    public bool Off;

    public GameObject[] ActivateTask;
    public bool         ActivateForVictim;
    public float        ActivateForVictimCooldown;
    public bool         ActivateForHunter;
    public float        ActivateForHunterCooldown;
    public GameObject[] DeactivateTask;
    public bool         DeActivateForVictim;
    public float        DeActivateForVictimCooldown;
    public bool         DeActivateForHunter;
    public float        DeActivateForHunterCooldown;

    private bool _oldActive;

    protected virtual void Awake() {
    }

    protected virtual void Start() {
        _oldActive = Active;
        OnActiveChange();
    }

    protected virtual void Update() {
        if (Active != _oldActive) {
            _oldActive = Active;
            OnActiveChange();
        }
    }

    private void OnActiveChange() {
        if (Active) OnActivate();
        else OnDeactivate();

        if (isServer) {
            int i = 0;
            if (Active && ActivateTask.Length > 0) {
                foreach (GameObject taskObject in ActivateTask) {
                    GameTask task = taskObject.GetComponent<GameTask>();
                    task.ActivatorObject = this;
                    // task.VictimActive = ActivateForVictim;
                    // task.HunterActive = ActivateForHunter;
                    if (!Off) {
                        task.VictimActive = false;
                        task.HunterActive = false;
                        if (ActivateForVictim)
                            StartCoroutine(doCooldown(ActivateForVictimCooldown, task, 0));
                        if (ActivateForHunter)
                            StartCoroutine(doCooldown(ActivateForHunterCooldown, task, 1));
                    }
                }
            }
            else if (!Active && DeactivateTask.Length > 0) {
                foreach (GameObject taskObject in DeactivateTask) {
                    GameTask task = taskObject.GetComponent<GameTask>();
                    task.ActivatorObject = this;
                    if (!Off) {
                        task.VictimActive = false;
                        task.HunterActive = false;
                        if (DeActivateForVictim)
                            StartCoroutine(doCooldown(DeActivateForVictimCooldown, task, 0));
                        if (DeActivateForHunter)
                            StartCoroutine(doCooldown(DeActivateForHunterCooldown, task, 1));
                    }
                }
            }
        }
    }

    private IEnumerator doCooldown(float time, GameTask task, byte var) {
        yield return new WaitForSeconds(time);
        if (Off)
            yield break;
        if (var == 0) {
            task.VictimActive = true;
        }
        else {
            task.HunterActive = true;
        }
    }

    protected virtual void OnActivate() {
    }

    protected virtual void OnDeactivate() {
    }
}