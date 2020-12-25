using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

// Author: Cloudy
public class NumbersLocalTask : GameLocalTask {
    public int Amount;

    private List<byte> _numbers;
    private int        _nextIndex;
    private bool       _isTutorial;
    private bool       _error;

    private Texture2D _regularBackground;
    private Texture2D _unTickedBackground;
    private Texture2D _tickedBackground;

    protected override void Start() {
        base.Start();
        _regularBackground = AssetLoader.GetColor(200, 200, 200);
        _unTickedBackground = AssetLoader.GetColor(200, 64, 64);
        _tickedBackground = AssetLoader.GetColor(64, 200, 64);
    }

    public override bool OnTaskOpen(Player player) {
        if (!base.OnTaskOpen(player))
            return false;

        // int numbers = 0;
        // for (int i = 0; i < Amount; ++i) {
        //     int pos;
        //     do {
        //         pos = Random.Range(0, 30);
        //     } while (((numbers >> pos) & 0b1) == 1);
        //
        //     numbers |= 1 << pos;
        // }

        return true;
    }

    public override void OnTaskOpenClient() {
        GenerateNumbers();
        _isTutorial = true;
    }

    public override void OnTaskUpdateClient() {
        base.OnTaskUpdateClient();
        if (_nextIndex >= Amount) {
            if (_isTutorial) {
                _isTutorial = false;
                GenerateNumbers();
            }
            else {
                SendTaskFinish();
            }
        }
    }

    private void GenerateNumbers() {
        if (_numbers == null)
            _numbers = new List<byte>();
        _numbers.Clear();
        for (int i = 0; i < Amount; ++i) {
            byte pos;
            do {
                pos = (byte) Random.Range(0, 30);
            } while (_numbers.Contains(pos));

            _numbers.Add(pos);
        }

        _nextIndex = 0;
        _error = false;
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();

        GUI.BeginGroup(new Rect(Screen.width / 2f - 306f, Screen.height / 2f - 256f, 612f, 512f));

        for (int i = 0; i < 5; ++i) {
            for (int j = 0; j < 6; ++j) {
                float x = 8f + j * 99f;
                float y = 8f + i * 99f;
                int index = i * 6 + j;
                int num = _numbers.FindIndex(b => b == (byte) index);
                if (num == -1)
                    continue;
                Texture2D tex = _error ? _unTickedBackground : num >= _nextIndex ? _regularBackground : _tickedBackground;
                Color color = num >= _nextIndex && !_error ? Color.black : Color.white;
                GUITaskUtils.SetBackground(tex);
                GUI.contentColor = color;
                if (GUI.Button(new Rect(x, y, 90f, 90f), _isTutorial || _nextIndex == 0 ? (num + 1).ToString() : "")) {
                    if (!_error) {
                        if (_nextIndex == num) {
                            _nextIndex++;
                        }
                        else {
                            _error = true;
                            StartCoroutine(ResetError(1.5f));
                        }
                    }
                }
            }
        }

        GUI.contentColor = Color.black;
        GUITaskUtils.SetBackground(null);

        GUI.EndGroup();
    }

    private IEnumerator ResetError(float time) {
        yield return new WaitForSeconds(time);
        _isTutorial = true;
        GenerateNumbers();
    }
}