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

    private readonly Rect _previewArea = new Rect(70f, 100f, 220f, 240f);

    private Vector2 _mouseOrigin; //Origin of mouseDrag
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

    [MenuItem("Window/Terrain Editor")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        TerrainEditor window = (TerrainEditor)GetWindow(typeof(TerrainEditor));
        window.Show();
        window.EditorInit();
        window.wantsMouseMove = true;
        window.minSize = MinSize;
    }

    public void EditorInit()
    {
        _previewTex = new Texture2D(200, 200);
    }

    void OnGUI()
    {
        GUILayout.Label("You can edit KTerrain parameters in this window");
        GUILayout.Space(20);
        myFloat = EditorGUILayout.Slider("Terrain height:", myFloat, -3f, 6f);
        _seed = EditorGUILayout.TextField("Seed (optional): ", _seed);
        
        if (GUILayout.Button("Update Terrain"))
        {
            UpdateTerrainPreview();
        }

        GUILayout.Space(20);

        GUILayout.BeginArea(_previewArea);

        GUILayout.Label("Preview:");
        GUILayout.Box(_previewTex);
        
    
        GUILayout.EndArea();

        DrawChunkLoadButtons();

        CheckDragEvent();
    }

    private void DrawChunkLoadButtons()
    {
        int bWidth = 40;

        GUILayout.BeginHorizontal();
        GUILayout.Button("Up",GUILayout.Width(bWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Button("Left",  GUILayout.Width(bWidth));
        GUILayout.Button("Down",  GUILayout.Width(bWidth));
        GUILayout.Button("Right", GUILayout.Width(bWidth));
        GUILayout.EndHorizontal();
    }

    private void CheckDragEvent() //Maybe just make buttons that load the next chunk, le    ft right up down
    {
        _event = Event.current;

        if (LeftMouseClicked)
        {
            Debug.Log("Click");
            _mouseOrigin = _event.mousePosition;
        }

        if (_event.type == EventType.MouseDrag)
        {
            bool mouseIsInside = _previewArea.Overlaps(new Rect(_event.mousePosition, Vector2.one));
            if (mouseIsInside)
            {
                _mouseOffset = _mouseOrigin - _event.mousePosition;
                UpdateTerrainPreview(); //TODO: Make an approximation for the preview, rathen than updating the texture at full resolution, OR sample at slower intervalls
            }
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