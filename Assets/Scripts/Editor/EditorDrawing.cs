using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorTerrainDrawing
{
    public EditorTerrainDrawing(Texture2D noise)
    {
        _noiseBase = noise;
        LoadDefaultTLayerPref();
        CreateColorTex();
    }

    private TerrainLayer[] _terrainLayers;

    private Texture2D _noiseBase;
    private Texture2D _finalTex;

    public Texture2D NoiseBase { get => _noiseBase; set => _noiseBase = value; }
    public Texture2D FinalTex  { get => _finalTex;  set => _finalTex  = value; }

    public void Update ()
    {
        CreateColorTex();
    }

    private void CreateColorTex()
    {
        Color[] colorArr = new Color[_noiseBase.width * _noiseBase.height];
        int y = 0;

        _finalTex = new Texture2D(_noiseBase.width, _noiseBase.height);

        for (int x = 0; x < _noiseBase.width; x++)
        {
            for (y = 0; y < _noiseBase.height; y++)
            {
                float f = _noiseBase.GetPixel(x, y).grayscale;
                _finalTex.SetPixel(x,y,ColorFromHeight(f));
            }
        }
        Debug.Log(y);

        _finalTex.Apply();
    }

    private Color ColorFromHeight (float f)
    {
        f = Mathf.Clamp(f, 0f, 1f);
        foreach (TerrainLayer layer in _terrainLayers)
        {
            if (layer.Bottom <= f && layer.Top >= f)
            {
                if (layer.IgnoreHeight == true)
                    return layer.PreviewColor;
                else
                {
                    float ah = Mathf.InverseLerp(layer.Bottom - 0.3f, layer.Top, f);
                    
                    ah = Mathf.Clamp(ah, 0f, 1f);
                    Color newColor = layer.PreviewColor * (ah);
                    newColor.a = 1f;
                    return newColor;                    
                }
            }
        }

        Debug.LogWarning("No terrainlayer in range!");
        return Color.magenta;
    }

    

    private void LoadDefaultTLayerPref ()
    {
        _terrainLayers = LayerSerializer.DefaultTerrainLayers;
    }
}

