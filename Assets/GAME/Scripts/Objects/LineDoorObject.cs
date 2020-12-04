using Mirror;
using UnityEngine;

public class LineDoorObject : GameObstacle {
    private LineRenderer _line0;
    private LineRenderer _line1;
    private Light        _light;
    private MeshRenderer _mask;
    private BoxCollider  _collider;

    protected override void Awake() {
        base.Awake();
        _line0 = transform.Find("Line0").GetComponent<LineRenderer>();
        _line1 = transform.Find("Line1").GetComponent<LineRenderer>();
        _light = transform.Find("Light").GetComponent<Light>();
        _mask = transform.Find("Mask").GetComponent<MeshRenderer>();
        _collider = GetComponent<BoxCollider>();
    }

    protected override void Update() {
        base.Update();
        if (!Active) return;

        _mask.enabled = Player.GetLocal.IsHunter;
    }

    protected override void OnActivate() {
        base.OnActivate();
        _line0.enabled = true;
        _line1.enabled = true;
        _light.enabled = true;
        _mask.enabled = Player.GetLocal.IsHunter;
        _collider.enabled = true;
    }

    protected override void OnDeactivate() {
        base.OnDeactivate();
        _line0.enabled = false;
        _line1.enabled = false;
        _light.enabled = false;
        _mask.enabled = false;
        _collider.enabled = false;
    }
}