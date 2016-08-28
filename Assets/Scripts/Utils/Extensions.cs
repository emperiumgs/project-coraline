using UnityEngine;

public static class Extensions
{
    public static string Colored(this string s, Color32 c)
    {
        string hex = c.r.ToString("X2") + c.g.ToString("X2") + c.b.ToString("X2");
        return "<color=#" + hex + ">" + s + "</color>";
    }
}