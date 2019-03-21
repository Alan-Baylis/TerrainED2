using UnityEngine;
using UnityEditor;
using System;
using LibNoise.Generator;

public class TerrainEditor : EditorWindow
{
    float myFloat = 0f;
    public static Vector2 MinSize = new Vector2(420, 320);

    private GameObject _terrainObject;
    private string _seed;
    private Texture2D _previewTex;

    //Layout and styling related stuff
    private readonly Rect _previewArea = new Rect(5f, 100f, 220f, 260f);

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
        _previewTex = new Texture2D(200, 200);
        wantsMouseMove = true;
        minSize = MinSize;
    }

    void OnGUI()
    {
        GUILayout.Label("You can edit K-Terrain parameters in this window", EditorStyles.boldLabel);
        GUILayout.Space(20);
        myFloat = EditorGUILayout.Slider("Terrain height:", myFloat, -3f, 6f);
        _seed = EditorGUILayout.TextField("Seed (optional): ", _seed);
        
        if (GUILayout.Button("Update Terrain"))
        {
            UpdateTerrainPreview();
        }

        GUILayout.Space(20);

        //Preview
        GUILayout.BeginArea(_previewArea);

        GUILayout.Label("Preview:");
        GUILayout.Box(_previewTex);
        if (_showChunkHelp)
            GUILayout.Label("Use arrow keys to preview other chunks", EditorStyles.miniBoldLabel);
        
        GUILayout.EndArea();
        //Preview Ends

        UpdateMouseEvent();
    }

    private void DrawChunkLoadButtons() //Unused
    {
       
        int bWidth = 40;

        GUILayout.BeginHorizontal();
        GUILayout.Button("Up", EditorStyles.miniButton, GUILayout.Width(bWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Button("Left", EditorStyles.miniButtonLeft, GUILayout.Width(bWidth));
        GUILayout.Button("Down", EditorStyles.miniButtonMid, GUILayout.Width(bWidth));
        GUILayout.Button("Right",EditorStyles.miniButtonRight, GUILayout.Width(bWidth));
        GUILayout.EndHorizontal();
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

    private void UpdateTerrainPreview()
    {
        Perlin baseNoise = new Perlin();

        if (_seed != "")
            baseNoise.Seed = Convert.ToInt32(_seed);
        else
            baseNoise.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue); //Dont generate new seed unless we pressed the button
            

        LibNoise.Noise2D noise2D = new LibNoise.Noise2D(200, baseNoise);

        float mOff = 0.01f;

        noise2D.GeneratePlanar(-1 - _mouseOffset.y * mOff, 1 - _mouseOffset.y * mOff, -1 - _mouseOffset.x * mOff, 1 - _mouseOffset.x * mOff);

        _previewTex = noise2D.GetTexture();
        
        _previewTex.Apply();

    }
}