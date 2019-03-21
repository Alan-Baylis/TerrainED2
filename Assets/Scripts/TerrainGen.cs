using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TerrainLayer
{
    public TerrainLayer(string name, float min, float max, Color color, bool ignore = true)
    {
        Name = name;
        Min = min;
        Max = max;
        PreviewColor = color;
        IgnoreHeight = ignore;
    }

    public string Name;
    public float Min;
    public float Max;
    public Color PreviewColor;
    public bool IgnoreHeight;
}

public class TerrainGenDefinition
{

}
