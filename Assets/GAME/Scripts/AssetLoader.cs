using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/*
 * Version: 1.0 official
 */
public class AssetLoader {
    public enum GradientSide {
        LEFT   = 0,
        RIGHT  = 1,
        TOP    = 2,
        BOTTOM = 3
    }

    private static Dictionary<string, Object> objects = new Dictionary<string, Object>();

    public static Texture GetTexture(string name) {
        Object ret = null;

        if (objects.TryGetValue(name, out ret)) return (Texture) ret;

        ret = (Texture) Resources.Load(name);
        objects.Add(name, ret);

        return (Texture) ret;
    }

    public static Object GetObject(string name) {
        Object ret = null;

        if (objects.TryGetValue(name, out ret)) return ret;

        ret = Resources.Load(name);
        objects.Add(name, ret);

        return ret;
    }

    public static Texture2D GetColor(int r, int g, int b, int a = 255) {
        return GetColor(new Color(r / 255f, g / 255f, b / 255f, a / 255f));
    }

    public static Texture2D GetColor(Color color) {
        Object ret;
        string name = "_c_" + color.r + "_" + color.g + "_" + color.b + "_" + color.a;

        if (objects.TryGetValue(name, out ret)) return (Texture2D) ret;

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        objects.Add(name, tex);

        return tex;
    }

    private static Color[] CalculateGradient(Color startColor, Color endColor, int steps) {
        Color[] colors = new Color[steps];
        float rDec = (startColor.r - endColor.r) / (steps - 1);
        float gDec = (startColor.g - endColor.g) / (steps - 1);
        float bDec = (startColor.b - endColor.b) / (steps - 1);
        float aDec = (startColor.a - endColor.a) / (steps - 1);

        for (int i = 0; i < steps; i++) {
            colors[i] = new Color(startColor.r - rDec * i, startColor.g - gDec * i, startColor.b - bDec * i,
                                  startColor.a - aDec * i);
        }

        return colors;
    }

    public static Texture2D GetGradient(int sR, int sG, int sB, int eR, int eG, int eB, GradientSide gradientSide,
                                        int steps) {
        return GetGradient(new Color(sR / 255f, sG / 255f, sB / 255f, 1f),
                           new Color(eR / 255f, eG / 255f, eB / 255f, 1f), gradientSide, steps);
    }

    public static Texture2D GetGradient(int          sR, int sG, int sB, int sA, int eR, int eG, int eB, int eA,
                                        GradientSide gradientSide,
                                        int          steps) {
        return GetGradient(new Color(sR / 255f, sG / 255f, sB / 255f, sA / 255f),
                           new Color(eR / 255f, eG / 255f, eB / 255f, eA / 255f), gradientSide, steps);
    }

    public static Texture2D GetGradient(Color startColor, Color endColor, GradientSide gradientSide, int steps) {
        Object ret;
        string name = "_g_" +
                      startColor.r +
                      "_" +
                      startColor.g +
                      "_" +
                      startColor.b +
                      "_" +
                      startColor.a +
                      "_" +
                      endColor.r +
                      "_" +
                      endColor.g +
                      "_" +
                      endColor.b +
                      "_" +
                      endColor.a +
                      "_" +
                      gradientSide +
                      "_" +
                      steps;
        if (objects.TryGetValue(name, out ret)) return (Texture2D) ret;

        Texture2D tex =
            new Texture2D(gradientSide == GradientSide.LEFT || gradientSide == GradientSide.RIGHT ? steps : 1,
                          gradientSide == GradientSide.TOP || gradientSide == GradientSide.BOTTOM ? steps : 1);
        Color[] colors = CalculateGradient(startColor, endColor, steps);
        if (gradientSide == GradientSide.TOP || gradientSide == GradientSide.BOTTOM) {
            for (int i = 0; i < colors.Length; i++) {
                tex.SetPixel(0, gradientSide == GradientSide.TOP ? i : colors.Length - i - 1, colors[i]);
            }
        }
        else {
            for (int i = 0; i < colors.Length; i++) {
                tex.SetPixel(gradientSide == GradientSide.LEFT ? i : colors.Length - i - 1, 0, colors[i]);
            }
        }

        tex.Apply();
        objects.Add(name, tex);
        return tex;
    }
}