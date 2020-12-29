using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumbersGraduallyLocalTask : GameLocalTask {
    private List<int> _positions;
    private int       _index;
    private bool      _error;

    private Texture2D _regularBackground;
    private Texture2D _unTickedBackground;
    private Texture2D _tickedBackground;

    protected override void Start() {
        base.Start();
        _positions = new List<int>();

        _regularBackground = AssetLoader.GetColor(200, 200, 200);
        _unTickedBackground = AssetLoader.GetColor(200, 64, 64);
        _tickedBackground = AssetLoader.GetColor(64, 200, 64);
    }

    public override void OnTaskOpenClient() {
        GenerateNumbers();
    }

    private void GenerateNumbers() {
        _positions.Clear();
        List<int> availablePositions = new List<int>();
        for (int i = 0; i < 10; ++i) {
            availablePositions.Add(i);
        }

        for (int i = 0; i < 10; ++i) {
            int posIndex = Random.Range(0, availablePositions.Count);
            _positions.Add(availablePositions[posIndex]);
            availablePositions.RemoveAt(posIndex);
        }

        _index = 0;
        _error = false;
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();
        GUI.BeginGroup(new Rect(Screen.width / 2f - 184f, Screen.height / 2f - 74f, 368f, 148f));
        GUI.Box(new Rect(0f, 0f, 368f, 148f), "");
        for (int i = 0; i < 2; ++i) {
            for (int j = 0; j < 5; ++j) {
                float x = 8f + j * 72f;
                float y = 8f + i * 72f;
                int index = i * 5 + j;
                Texture2D tex = _error ? _unTickedBackground : _index > _positions[index] ? _tickedBackground : _regularBackground;
                GUITaskUtils.SetBackground(tex);
                GUI.contentColor = tex == _tickedBackground ? Color.white : Color.black;
                if (GUI.Button(new Rect(x, y, 64f, 64f), (_positions[index] + 1).ToString())) {
                    if (!_error) {
                        if (_positions[index] == _index) {
                            _index++;
                            if (_index == 10) {
                                SendTaskFinish();
                            }
                        }
                        else {
                            _error = true;
                            StartCoroutine(ResetError());
                        }
                    }
                }
            }
        }

        GUI.contentColor = Color.black;
        GUITaskUtils.SetBackground(null);

        GUI.EndGroup();
    }

    private IEnumerator ResetError() {
        yield return new WaitForSeconds(1f);
        GenerateNumbers();
    }
}