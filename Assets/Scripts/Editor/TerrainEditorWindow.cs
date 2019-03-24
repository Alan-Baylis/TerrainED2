using UnityEngine;
using UnityEditor;
using System;
using LibNoise.Generator;

public class TerrainEditor : EditorWindow
{
    float myFloat = 0f;
    public static Vector2 MinSize = new Vector2(420, 320);

    private GameObject _terrainObject;

    //TerrainParams
    private string _seed;
    private float _noiseFreq = 1f; //TODO: make a class that stores perlinnoise values, and make it serializable so you can reuse terrain gen parameters
    private double _noiseLac = 1.0;
    private int _noiseOct = 2;

    private Texture2D _previewTex;
    private EditorTerrainDrawing _terDrawing;

    //Layout and styling related stuff
    private readonly Rect _previewArea = new Rect(5f, 150f, 400f, 400f);

    bool _showChunkHelp = false;
    private Vector2 _mouseOffset;
    private Event _event;

    private bool LeftMouseClicked
    {
        get
        {
            
            if (_event.type == EventType.MouseDown)
            {
                if (_event.button == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    [MenuItem("Window/K-Terrain/Terrain generation preferences")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        TerrainEditor window = (TerrainEditor)GetWindow(typeof(TerrainEditor));
        window.Show();
        window.EditorInit();
    }

    public void EditorInit()
    {
        _previewTex = new Texture2D(320, 320);
        _terDrawing = new EditorTerrainDrawing(_previewTex);
        wantsMouseMove = true;
        minSize = MinSize;
    }

    void OnGUI()
    {
        GUILayout.Label("You can edit K-Terrain parameters in this window", EditorStyles.boldLabel); //I thought it was funny to "brand" this as K-Terrain. It's just noise, I know
        GUILayout.Space(20);

        _seed = EditorGUILayout.TextField("Seed (optional): ", _seed);
        _noiseFreq = EditorGUILayout.FloatField("Frequency: ", _noiseFreq);
        _noiseLac = EditorGUILayout.Slider("Complexity: ",(float)_noiseLac, 1.0f, 3.4f);
        _noiseOct = EditorGUILayout.IntSlider("Octaves: ", _noiseOct, 1, 10);

        if (GUI.changed)
        {
            UpdateTerrainPreview();
        }

        if (GUILayout.Button("Update Terrain"))
        {
            UpdateTerrainPreview();
        }

        GUILayout.Space(20);

        //Preview
        GUILayout.BeginArea(_previewArea);

        GUILayout.Label("Preview:");

        if (_terDrawing.FinalTex != null)
            GUILayout.Box(_terDrawing.FinalTex);

        if (_showChunkHelp)
            GUILayout.Label("Use arrow keys to preview other chunks", EditorStyles.miniBoldLabel); //False advertising for now, TODO: this
        
        GUILayout.EndArea();
        //Preview Ends



        UpdateMouseEvent();
    }

    private void UpdateMouseEvent()
    {
        _event = Event.current;

        if (LeftMouseClicked)
        {
            _showChunkHelp = false;
            bool mouseIsInside = _previewArea.Overlaps(new Rect(_event.mousePosition, Vector2.one));
            if (mouseIsInside)
            {
                _showChunkHelp = true;
                
            }

            Repaint();
        }
    }

    public void UpdateTerrainPreview()
    {
        Perlin baseNoise = new Perlin() { Frequency = _noiseFreq, Lacunarity = _noiseLac, OctaveCount = _noiseOct};

        if (_seed != "")
            baseNoise.Seed = Convert.ToInt32(_seed);
        else
            baseNoise.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            

        LibNoise.Noise2D noise2D = new LibNoise.Noise2D(_previewTex.width, baseNoise);

        float mOff = 0.01f;

        noise2D.GeneratePlanar(-1 - _mouseOffset.y * mOff, 1 - _mouseOffset.y * mOff, -1 - _mouseOffset.x * mOff, 1 - _mouseOffset.x * mOff);

        _previewTex = noise2D.GetTexture();
        _terDrawing = new EditorTerrainDrawing(_previewTex);
        _terDrawing.Update();


        _previewTex.Apply();

    }
}