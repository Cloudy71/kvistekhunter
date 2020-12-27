using System.Collections.Generic;
using UnityEngine;

public class MorseShortLocalTask : GameLocalTask {
    public AudioClip[] MorseSounds;
    public string[]    MorseChars;

    public int SequenceSize;

    private AudioSource _audioSource;
    private Texture2D   _red;

    private string   _sequence;
    private int      _index;
    private float    _currentLevel;
    private float[]  _data;
    private float    _speed;
    private bool     _isPlaying;
    private string[] _possibilities;

    protected override void Start() {
        base.Start();
        _audioSource = GetComponent<AudioSource>();
        _red = AssetLoader.GetColor(255, 0, 0);
        _data = new float[1024];
    }

    public override bool OnTaskOpen(Player player) {
        if (!base.OnTaskOpen(player))
            return false;

        return true;
    }

    public override void OnTaskOpenClient() {
        GenerateSequence();
    }

    public override void OnTaskUpdateClient() {
        base.OnTaskUpdateClient();
        if (_audioSource.clip != null && _audioSource.isPlaying) {
            _audioSource.clip.GetData(_data, _audioSource.timeSamples);
            _currentLevel = 0f;
            foreach (float f in _data) {
                _currentLevel += Mathf.Abs(f);
            }

            _currentLevel /= 1024f;
        }
    }

    private int GetCharIndex(char c) {
        if (c >= '0' && c <= '9') {
            return c - '0';
        }

        return 10 + c - 'a';
    }

    private void GenerateSequence() {
        _sequence = "";
        for (int i = 0; i < SequenceSize; ++i) {
            int rand = Random.Range(0, 36);
            _sequence += (char) (rand >= 0 && rand <= 25 ? 'a' + rand : '0' + (rand - 26));
        }

        _index = 0;
        _speed = 1f;
    }

    private void GeneratePossibilities() {
        int possibilies = 4;
        int correct = Random.Range(0, possibilies);
        List<string> possibilitiesList = new List<string>();
        char c = _sequence.Substring(_index, 1).ToCharArray()[0];
        for (int i = 0; i < possibilies; ++i) {
            string seq;
            if (i == correct) {
                seq = MorseChars[GetCharIndex(c)];
                if (!possibilitiesList.Contains(seq)) {
                    possibilitiesList.Add(seq);
                    continue;
                }
            }

            do {
                seq = MorseChars[Random.Range(0, MorseChars.Length)];
            } while (possibilitiesList.Contains(seq));

            possibilitiesList.Add(seq);
        }

        _possibilities = possibilitiesList.ToArray();
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();

        GUI.BeginGroup(new Rect(Screen.width / 2f - 300f, Screen.height / 2f - 200f, 600f, 400f));
        GUI.Box(new Rect(0f, 0f, 600f, 400f), "");
        GUI.Label(new Rect(8f, 8f, 584f, 20f), "Morse Message");
        GUI.Box(new Rect(28f, 36f, 288f, 200f), "");
        GUI.Label(new Rect(8f, 126f, 20f, 20f), "0");
        GUI.DrawTexture(new Rect(29f, 136f - _currentLevel * 300f, 286f, 2f), _red);
        if (GUI.Button(new Rect(320f, 36f, 128f, 20f), "PLAY")) {
            _audioSource.clip = MorseSounds[GetCharIndex(_sequence.Substring(_index, 1).ToCharArray()[0])];
            _audioSource.pitch = _speed;
            _audioSource.Play();
            _isPlaying = true;
            GeneratePossibilities();
        }

        GUI.Label(new Rect(456f, 36f, 128f, 20f), "Index: " + _index + " / " + (_sequence.Length - 1));
        GUI.Label(new Rect(320f, 64f, 64f, 20f), "Speed: ");
        if (GUI.Button(new Rect(384f, 64f, 20f, 20f), "-")) {
            _speed -= .1f;
            if (_speed < .1f) _speed = .1f;
        }

        GUI.Box(new Rect(404f, 64f, 64f, 20f), _speed.ToString("0.000"));
        if (GUI.Button(new Rect(468f, 64f, 20f, 20f), "+")) {
            _speed += .1f;
            if (_speed > 1.2f) _speed = 1.2f;
        }

        GUI.Label(new Rect(8f, 244f, 584f, 20f), "Message");
        GUI.Box(new Rect(8f, 264f, 584f, 48f), "");
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        GUI.skin.label.fontSize = 20;
        string finishedSequence = _sequence.Substring(0, _index);
        GUI.Label(new Rect(16f, 264f, 552f, 48f), finishedSequence);
        GUI.skin.label.fontSize = GUI.skin.font.fontSize;
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        GUI.Label(new Rect(8f, 320f, 584f, 20f), "Selection");
        GUI.Box(new Rect(8f, 340f, 584f, 52f), "");
        GUI.skin.button.fontSize = 20;
        if (_isPlaying && !_audioSource.isPlaying) {
            float size = 568f / _possibilities.Length;
            for (int i = 0; i < _possibilities.Length; ++i) {
                if (GUI.Button(new Rect(16f + size * i, 348f, size - 8f, 36f), _possibilities[i])) {
                    _isPlaying = false;
                    char c = _sequence.Substring(_index, 1).ToCharArray()[0];
                    if (_possibilities[i] == MorseChars[GetCharIndex(c)]) {
                        _index++;
                        if (_index == _sequence.Length) {
                            _index = 0;
                            SendTaskFinish();
                        }
                    }
                    else {
                        GenerateSequence();
                    }
                }
            }
        }

        GUI.skin.button.fontSize = GUI.skin.font.fontSize;

        GUI.EndGroup();
    }
}