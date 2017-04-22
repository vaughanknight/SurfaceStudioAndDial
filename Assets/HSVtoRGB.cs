using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the HSV color format
/// </summary>
public struct HSV
{
    public HSV(float h, float s, float v)
    {
        this.h = h;
        this.s = s;
        this.v = v;
    }

    public float h;
    public float s;
    public float v;
}

/// <summary>
/// Static helper class for Unity to translate between the HSV and RGB color spaces.  
/// Converts from HSV to RGB and vice versa.
/// NOTE: The maths is well known and all over the web.  No magic here.
/// </summary>
public static class ColorSpace
{
    public static Color HSVtoRGB(HSV hsv)
    {
        var h = hsv.h;
        var s = hsv.s;
        var v = hsv.v;

        float r, g, b;

        if (s == 0)
        {
            r = v;
            g = v;
            b = v;
        }
        else
        {
            float varH = h * 6;
            float varI = Mathf.Floor(varH);
            float var1 = v * (1 - s);
            float var2 = v * (1 - (s * (varH - varI)));
            float var3 = v * (1 - (s * (1 - (varH - varI))));

            if (varI == 0)
            {
                r = v;
                g = var3;
                b = var1;
            }
            else if (varI == 1)
            {
                r = var2;
                g = v;
                b = var1;
            }
            else if (varI == 2)
            {
                r = var1;
                g = v;
                b = var3;
            }
            else if (varI == 3)
            {
                r = var1;
                g = var2;
                b = v;
            }
            else if (varI == 4)
            {
                r = var3;
                g = var1;
                b = v;
            }
            else
            {
                r = v;
                g = var1;
                b = var2;
            }
        }

        return new Color(r, g, b);
    }

    public static HSV RGBtoHSV(Color rgb)
    {
        var r = rgb.r;
        var g = rgb.g;
        var b = rgb.b;
        float h, s, v;

        float varMin = Mathf.Min(r, Mathf.Min(g, b));
        float varMax = Mathf.Max(r, Mathf.Max(g, b));
        float delMax = varMax - varMin;

        v = varMax;

        if (delMax == 0)
        {
            h = 0;
            s = 0;
        }
        else
        {
            float delR = (((varMax - r) / 6) + (delMax / 2)) / delMax;
            float delG = (((varMax - g) / 6) + (delMax / 2)) / delMax;
            float delB = (((varMax - b) / 6) + (delMax / 2)) / delMax;

            s = delMax / varMax;

            if (r == varMax)
            {
                h = delB - delG;
            }
            else if (g == varMax)
            {
                h = (1.0f / 3f) + delR - delB;
            }
            else //// if (b == varMax) 
            {
                h = (2.0f / 3f) + delG - delR;
            }

            if (h < 0)
            {
                h += 1;
            }

            if (h > 1)
            {
                h -= 1;
            }
        }
        return new HSV(h, s, v);
    }
}