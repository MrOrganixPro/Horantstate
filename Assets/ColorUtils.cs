using UnityEngine;

public static class ColorUtils
{
    public static Color Darken(Color color, float amount)
    {
        float h, s, v;

        Color.RGBToHSV(color, out h, out s, out v);

        v = Mathf.Max(0, v - amount);

        return Color.HSVToRGB(h, s, v);
    }
}
