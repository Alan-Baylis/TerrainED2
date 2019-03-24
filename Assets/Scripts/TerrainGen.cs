using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TerrainLayer : ICloneable
{
    public TerrainLayer(string name, float min, float max, Color color, bool ignore = true)
    {
        Name = name;
        Bottom = min;
        Top = max;
        PreviewColor = color;
        IgnoreHeight = ignore;
    }

    public string Name;
    public float Bottom;
    public float Top;
    public Color PreviewColor;
    public bool IgnoreHeight;

    public object Clone()
    {
        return new TerrainLayer(this.Name, this.Bottom, this.Top, this.PreviewColor, this.IgnoreHeight); //I acknowledge that this is not good code, and I shouldn't use ICloneable.
    }
}
