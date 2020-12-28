using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameScreenDrawer : MonoBehaviour {
    public static GameScreenDrawer Instance;

    public enum ShaderType {
        ColorGrading_Filter,
        DepthOfField_Distance,
        ChromaticAberration_Intensity
    }

    public bool Intro;
    public bool Outro;

    private float                   _introStart;
    private float                   _outroStart;
    private float                   _imageY;
    private float                   _imageA;
    private float                   _bgA;
    private float                   _huntersHealthOnion;
    private float                   _victimsHealthOnion;
    private Camera                  _camera;
    private PostProcessVolume       _volume;
    private ColorGrading            _shaderColorGrading;
    private DepthOfField            _shaderDepthOfField;
    private ChromaticAberration     _shaderChromaticAberration;
    private List<ShaderValueBundle> _processingValues;

    private Texture2D _colorBlack;
    private Texture2D _colorRed;
    private Texture2D _colorGreen;
    private Texture2D _colorWhite;

    private void Start() {
        Instance = this;

        _introStart = 0f;
        _outroStart = 0f;
        _huntersHealthOnion = 1f;
        ReloadCamera();
        _processingValues = new List<ShaderValueBundle>();

        _colorBlack = AssetLoader.GetColor(0, 0, 0);
        _colorRed = AssetLoader.GetColor(255, 128, 128);
        _colorGreen = AssetLoader.GetColor(128, 255, 128);
        _colorWhite = AssetLoader.GetColor(255, 255, 255);

        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
    }

    private void SceneManagerOnsceneLoaded(Scene scene, LoadSceneMode mode) {
        ReloadCamera();
    }

    private void ReloadCamera() {
        _camera = Camera.main;
        _volume = _camera.GetComponent<PostProcessVolume>();
        _shaderColorGrading = _volume.profile.GetSetting<ColorGrading>();
        _shaderDepthOfField = _volume.profile.GetSetting<DepthOfField>();
        _shaderChromaticAberration = _volume.profile.GetSetting<ChromaticAberration>();
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

        for (var i = 0; i < _processingValues.Count; i++) {
            ShaderValueBundle valueBundle = _processingValues[i];
            ShaderValue value = valueBundle.Value;
            ShaderDefaultValue defaultValue = valueBundle.DefaultValue;

            if (Time.time >= valueBundle.StartTime && Time.time <= valueBundle.StartTime + value.TimeFull) {
                float time = Time.time - valueBundle.StartTime;
                float transition = time <= value.TimeIn ? time / value.TimeIn : time >= value.TimeFull - value.TimeOut ? 1f - (time - (value.TimeFull - value.TimeOut)) / value.TimeOut : 1f;
                Debug.Log(transition);
                if (value.ShaderType == ShaderType.ColorGrading_Filter) {
                    _shaderColorGrading.colorFilter.value = ColorTransition(defaultValue.DefaultColor, value.ColorValue, transition);
                }
                else if (value.ShaderType == ShaderType.DepthOfField_Distance) {
                    _shaderDepthOfField.focusDistance.value = FloatTransition(defaultValue.DefaultFloat, value.FloatValue, transition);
                }
                else if (value.ShaderType == ShaderType.ChromaticAberration_Intensity) {
                    _shaderChromaticAberration.intensity.value = FloatTransition(defaultValue.DefaultFloat, value.FloatValue, transition);
                }
            }
            else {
                if (value.ShaderType == ShaderType.ColorGrading_Filter) {
                    _shaderColorGrading.colorFilter.value = defaultValue.DefaultColor;
                }
                else if (value.ShaderType == ShaderType.DepthOfField_Distance) {
                    _shaderDepthOfField.focusDistance.value = defaultValue.DefaultFloat;
                }
                else if (value.ShaderType == ShaderType.ChromaticAberration_Intensity) {
                    _shaderChromaticAberration.intensity.value = defaultValue.DefaultFloat;
                }

                _processingValues.RemoveAt(i);
                i--;
            }
        }
    }

    private Color ColorTransition(Color defaultColor, Color targetColor, float transition) {
        return new Color(
            Mathf.MoveTowards(defaultColor.r, targetColor.r, transition),
            Mathf.MoveTowards(defaultColor.g, targetColor.g, transition),
            Mathf.MoveTowards(defaultColor.b, targetColor.b, transition),
            Mathf.MoveTowards(defaultColor.a, targetColor.a, transition));
    }

    private float FloatTransition(float defaultFloat, float targetFloat, float transition) {
        return Mathf.MoveTowards(defaultFloat, targetFloat, transition * Mathf.Abs(targetFloat - defaultFloat));
    }

    private void OnGUI() {
        if (Player.GetLocal == null)
            return;

        GUI.skin.label.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(Screen.width - 80f, Screen.height - 20f, 72f, 20f), "Ping: " + Mathf.Round((float) NetworkTime.rtt * 1000f));
        GUI.skin.label.alignment = TextAnchor.UpperLeft;

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

    private void SetShaderValueInternal(ShaderValue value) {
        ShaderDefaultValue defaultValue;
        if (_processingValues.Any(bundle => bundle.Value.ShaderType == value.ShaderType)) {
            defaultValue = _processingValues.First(bundle => bundle.Value.ShaderType == value.ShaderType).DefaultValue;
        }
        else {
            float defaultFloat = 0f;
            Color defaultColor = Color.black;

            if (value.ShaderType == ShaderType.ColorGrading_Filter) {
                defaultColor = _shaderColorGrading.colorFilter.value;
            }
            else if (value.ShaderType == ShaderType.DepthOfField_Distance) {
                defaultFloat = _shaderDepthOfField.focusDistance.value;
            }
            else if (value.ShaderType == ShaderType.ChromaticAberration_Intensity) {
                defaultFloat = _shaderChromaticAberration.intensity.value;
            }

            defaultValue = new ShaderDefaultValue {
                                                      DefaultFloat = defaultFloat,
                                                      DefaultColor = defaultColor
                                                  };
        }

        _processingValues.Add(new ShaderValueBundle {
                                                        StartTime = Time.time,
                                                        Value = value,
                                                        DefaultValue = defaultValue
                                                    });
    }

    public static void SetShaderValue(ShaderValue value) {
        Instance.SetShaderValueInternal(value);
    }
}