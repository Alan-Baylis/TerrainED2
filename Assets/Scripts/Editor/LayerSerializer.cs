using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class LayerSerializer
{
    public static string PrefPath
    {
        get { return Application.dataPath + "/TerrainPrefs/"; }
    }

    public static string DefaultJSONPath
    {
        get { return Application.dataPath + "/TerrainPrefs/DefaultTerrainLayers.json"; }
    }

    public static TerrainLayer[] DefaultTerrainLayers
    {
        get
        {
            List<string> jsonList = File.ReadAllLines(DefaultJSONPath).ToList();

            List<TerrainLayer> layers = new List<TerrainLayer>();

            foreach (string str in jsonList)
            {
                TerrainLayer layer = JsonUtility.FromJson<TerrainLayer>(str);
                layers.Add(layer);
            }

            return layers.ToArray();
        }
    }
}
