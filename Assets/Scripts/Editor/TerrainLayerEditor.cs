using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TerrainLayerEditor : EditorWindow
{
    public string PrefPath;
    private List<TerrainLayer> _editLayers;

    [MenuItem("Window/K-Terrain/Edit terrain layers")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        TerrainLayerEditor window = (TerrainLayerEditor)GetWindow(typeof(TerrainLayerEditor));
        window.Show();
        
        window.PrefPath = Application.dataPath + "/TerrainPrefs/";
        window.GenerateGenericJSON();
    }

    void OnGUI()
    {
        GUILayout.Label("Edit terrain layers", EditorStyles.boldLabel);

        GUILayout.Space(20f);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Load default", EditorStyles.miniButtonLeft))
            LoadDefault();

        if (GUILayout.Button("Save default", EditorStyles.miniButtonMid))
            SaveDefault();

        if (GUILayout.Button("Browse",       EditorStyles.miniButtonMid))
            Browse();

        if (GUILayout.Button("Save as",      EditorStyles.miniButtonRight))
            SaveAs();

        GUILayout.EndHorizontal();
    }

    private void SaveAs()
    {
        throw new NotImplementedException();
    }

    private void Browse()
    {
        throw new NotImplementedException();
    }

    private void SaveDefault()
    {
        throw new NotImplementedException();
    }

    private void LoadDefault()
    {
        List<string> jsonList = File.ReadAllLines(PrefPath + "DefaultTerrainLayers.json").ToList();

        _editLayers = new List<TerrainLayer>();

        foreach (string str in jsonList)
        {
            TerrainLayer layer = JsonUtility.FromJson<TerrainLayer>(str);
            _editLayers.Add(layer);
        }
    }

    private void GenerateGenericJSON() //For testing purposes
    {
        TerrainLayer[] arr = new TerrainLayer[2];

        TerrainLayer terrainLayer1 = new TerrainLayer("Water",0.0f, 0.3f, Color.blue);
        TerrainLayer terrainLayer2 = new TerrainLayer("Sand" ,0.3f, 0.4f, Color.yellow);
        TerrainLayer terrainLayer3 = new TerrainLayer("Grass",0.4f, 1.0f, Color.green, false);

        string json1 = JsonUtility.ToJson(terrainLayer1);
        string json2 = JsonUtility.ToJson(terrainLayer2);
        string json3 = JsonUtility.ToJson(terrainLayer3);

        List<string> jsonList = new List<string>
        {
            json1,
            json2,
            json3
        };

        Debug.Log("first json: " + json1);

        try
        {
            File.WriteAllLines(PrefPath + "DefaultTerrainLayers.json", jsonList);
        } catch
        {

        }
    }
}

