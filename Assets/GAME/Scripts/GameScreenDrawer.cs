using System;
using Mirror;
using UnityEngine;

public class GameScreenDrawer : MonoBehaviour {
    public static GameScreenDrawer Instance;

    public bool Intro;
    public bool Outro;

    private float _introStart;
    private float _outroStart;
    private float _imageY;
    private float _imageA;
    private float _bgA;
    private float _huntersHealthOnion;
    private float _victimsHealthOnion;

    private Texture2D _colorBlack;
    private Texture2D _colorRed;
    private Texture2D _colorGreen;
    private Texture2D _colorWhite;

    private void Start() {
        Instance = this;

        _introStart = 0f;
        _outroStart = 0f;
        _huntersHealthOnion = 1f;

        _colorBlack = AssetLoader.GetColor(0, 0, 0);
        _colorRed = AssetLoader.GetColor(255, 128, 128);
        _colorGreen = AssetLoader.GetColor(128, 255, 128);
        _colorWhite = AssetLoader.GetColor(255, 255, 255);
    }

    private void Update() {
        if (Intro) {
            if (_introStart.Equals(0f))
                _introStart = Time.time;

            float perc = (Time.time - _introStart) / 5f;
            if (perc <= .5f) {
                _imageY = perc / .5f;
                _bgA = 1f;
                _imageA = 1f;
            }
            else {
                _imageY = 1f;
                _bgA = 1f - (perc - .5f) / .5f;
                _imageA = _bgA;
            }

            if (perc >= 1f) {
                Intro = false;
                _introStart = 0f;
            }
        }

        float desiredHealthOnion = GameManager.Instance.StatusHuntersHealth / (float) GameManager.Instance.StatusHuntersMaxHealth;
        if (_huntersHealthOnion > desiredHealthOnion)
            _huntersHealthOnion = Mathf.MoveTowards(_huntersHealthOnion, desiredHealthOnion, Time.deltaTime * .5f);
        else
            _huntersHealthOnion = desiredHealthOnion;

        desiredHealthOnion = GameManager.Instance.StatusVictimsHealth / (float) GameManager.Instance.StatusVictimsMaxHealth;
        if (_victimsHealthOnion > desiredHealthOnion)
            _victimsHealthOnion = Mathf.MoveTowards(_victimsHealthOnion, desiredHealthOnion, Time.deltaTime * .5f);
        else
            _victimsHealthOnion = desiredHealthOnion;
    }

    private void OnGUI() {
        if (Player.GetLocal == null)
            return;

        if (GameManager.Instance.GameStarted) {
            GUI.skin.label.fontSize = 36;
            GUI.contentColor = Color.white;
            string time = Utils.TimeToString(GameManager.Instance.TimeLimit - ((float) NetworkTime.time - GameManager.Instance.StatusStartTime));
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(time));
            GUI.Label(new Rect(Screen.width / 2f - size.x / 2f, Screen.height - size.y, size.x, size.y), time);
            GUI.skin.label.fontSize = GUI.skin.font.fontSize;

            GUI.DrawTexture(new Rect(Screen.width - 260f - 8f, 8f, 260f, 20f), _colorBlack);
            GUI.DrawTexture(new Rect(Screen.width - 260f - 6f, 10f, _huntersHealthOnion * 256f, 16f), _colorWhite);
            GUI.DrawTexture(new Rect(Screen.width - 260f - 6f, 10f, GameManager.Instance.StatusHuntersHealth / (float) GameManager.Instance.StatusHuntersMaxHealth * 256f, 16f), _colorRed);

            GUI.DrawTexture(new Rect(Screen.width - 260f - 8f, 36f, 260f, 20f), _colorBlack);
            GUI.DrawTexture(new Rect(Screen.width - 260f - 6f, 38f, _victimsHealthOnion * 256f, 16f), _colorWhite);
            GUI.DrawTexture(new Rect(Screen.width - 260f - 6f, 38f, GameManager.Instance.StatusVictimsHealth / (float) GameManager.Instance.StatusVictimsMaxHealth * 256f, 16f), _colorGreen);
        }

        if (Intro) {
            float y = 64f - _imageY * 64f;
            Texture2D tex = Player.GetLocal.IsHunter ? GameAssets.Instance.IntroHunterSprite.texture : GameAssets.Instance.IntroVictimSprite.texture;
            GUI.color = new Color(1f, 1f, 1f, _bgA);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _colorBlack);
            GUI.color = new Color(1f, 1f, 1f, _imageA);
            GUI.DrawTexture(new Rect(Screen.width / 2f - 320f, Screen.height / 2f - 43f - y, 640f, 86f), tex);
            GUI.color = Color.white;
        }
    }
}