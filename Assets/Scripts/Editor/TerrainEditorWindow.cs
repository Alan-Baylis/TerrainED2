using UnityEngine;
using UnityEditor;
using System;

public class TerrainEditor : EditorWindow
{
    float myFloat = 0f;
    Vector2 Size = Vector2.zero;

    private GameObject _terrainObject;
    private string _seed;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Terrain Editor")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        TerrainEditor window = (TerrainEditor)GetWindow(typeof(TerrainEditor));
        window.Show();
    }

    void OnGUI()
    {
        myFloat = EditorGUILayout.Slider("Terrain height:", myFloat, -3f, 6f);
        Size = EditorGUILayout.Vector2Field("Terrain size: ", Size);
        _seed = EditorGUILayout.TextField("Seed (optional): ", _seed);

        Size = new Vector2((int)Size.x, (int)Size.y); //Make sure size is an even number
        
        if (_terrainObject == null)
        {
            if (GUILayout.Button("Create Terrain"))
            {
                CreateTerrainObject();
            }
        }
        else
        {
            if (GUILayout.Button("Update Terrain"))
            {
                UpdateTerrainObject();
            }
        }
    }

    private void UpdateTerrainObject()
    {        
        Selection.activeGameObject = _terrainObject;
        SceneView.FrameLastActiveSceneView();
    }

    void CreateTerrainObject ()
    {
        _terrainObject = new GameObject("Terrain");
        Selection.activeGameObject = _terrainObject;
        SceneView.FrameLastActiveSceneView();
    }
}