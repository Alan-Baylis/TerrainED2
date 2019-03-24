using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Windows.Input;

public class TerrainLayerEditor : EditorWindow
{
    public string PrefPath;

    private List<TerrainLayer> _editLayers;
    private TerrainLayer _currentEditable;
    private int _curEditInt;

    private Event _event;
    private int _mouseOriginY;

    TerrainLayer minLayer;
    TerrainLayer maxLayer;

    private Rect _sideRect = new Rect(2, 120, 38, _sideViewHeight + 6);
    private Texture2D _sideCutView;
    private const int _sideViewHeight = 360;

    [MenuItem("Window/K-Terrain/Edit terrain layers")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        TerrainLayerEditor window = (TerrainLayerEditor)GetWindow(typeof(TerrainLayerEditor));
        window.Show();
        
        window.PrefPath = Application.dataPath + "/TerrainPrefs/";
        //window.GenerateGenericJSON();
        window.wantsMouseMove = true;
    }

    void OnGUI()
    {
        GeneralMouseEvent();

        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.Label("Edit terrain layers", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();

        GUILayout.Space(20f);

        #region Loading/saving buttons
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Load default", EditorStyles.miniButtonLeft))
            LoadDefault();

        if (GUILayout.Button("Save default", EditorStyles.miniButtonMid))
            SaveDefault();

        if (GUILayout.Button("Browse",       EditorStyles.miniButtonMid))
            Browse();

        if (GUILayout.Button("Save as",      EditorStyles.miniButtonRight))
            SaveAs();

        if (GUILayout.Button("debug",        EditorStyles.miniButtonRight))
            GenerateGenericJSON();
        #endregion 

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        #region Iterating through terrainlayer list
        if (_editLayers == null)
            return;
        
        GUILayout.BeginVertical(GUILayout.Width(30f));

        for (int i = 0; i < _editLayers.Count; i++)
        {
            TerrainLayer layer = _editLayers[i];
            if (GUILayout.Button(layer.Name, EditorStyles.miniButton))
            {
                _currentEditable = layer;
                _curEditInt = i;
            }
            
        }

        if (_sideCutView == null)
            _sideCutView = SideCutTexture(GetLayerInfo());

        GUILayout.EndVertical();

        GUILayout.BeginArea(_sideRect);
        SideCutMouseEvent(); //Updating the event here actually provides some "funky"/unexpected results, event.mouseposition returns the mousepositions relative to the _sideRect area
        GUILayout.Box(_sideCutView);
        GUILayout.EndArea();

        #endregion

        #region Current editable

        if (_currentEditable == null)
            return;

        GUILayout.BeginVertical();

        GUILayout.Label(_currentEditable.Name, EditorStyles.boldLabel);

        #region Calculating min and max values
        float curMin;
        float curMax;
        if (_curEditInt == 0)                          //First one
        {
            curMin = 0f;
            curMax = _editLayers[_curEditInt + 1].Bottom;
        }
        else if (_curEditInt == _editLayers.Count - 1) //Last one 
        {
            curMin = _editLayers[_curEditInt - 1].Top;
            curMax = 1.0f;
        }
        else                                           //In the middle
        {
            curMin = _editLayers[_curEditInt - 1].Top;
            curMax = _editLayers[_curEditInt + 1].Bottom;
        }
        #endregion

        #region Min slider
        GUILayout.BeginHorizontal();

        GUILayout.Label("Min: ", EditorStyles.largeLabel, GUILayout.Width(30f));
        
        _currentEditable.Bottom = (float)Math.Round(
                                                GUILayout.HorizontalSlider(_currentEditable.Bottom, curMin, curMax - 0.1f)
                                                , 2);

        GUILayout.Label(_currentEditable.Bottom.ToString(), EditorStyles.miniLabel, GUILayout.Width(30f));

        GUILayout.EndHorizontal();
        #endregion

        #region Max slider
        GUILayout.BeginHorizontal();

        GUILayout.Label("Max: ", EditorStyles.largeLabel, GUILayout.Width(30f));
        _currentEditable.Top = (float)Math.Round(
                                                GUILayout.HorizontalSlider(_currentEditable.Top, curMin + 0.1f, curMax)
                                                , 2);
        GUILayout.Label(_currentEditable.Top.ToString(), EditorStyles.miniLabel, GUILayout.Width(30f));

        GUILayout.EndHorizontal();
        #endregion

        GUILayout.EndVertical();

        #endregion

        GUILayout.EndHorizontal();

    }

    private void GeneralMouseEvent()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseUp && _editLayers != null)
        {
            _sideCutView = SideCutTexture(GetLayerInfo());
        }
    }

    private List<KeyValuePair<Color, int>> GetLayerInfo ()
    {
        List<KeyValuePair<Color, int>> layersInfo = new List<KeyValuePair<Color, int>>(); //What color to draw and until which coordinates

        for (int i = 0; i < _editLayers.Count; i++)
        {
            TerrainLayer layer = _editLayers[i];
            KeyValuePair<Color, int> kvp = new KeyValuePair<Color, int>(layer.PreviewColor, (int)(layer.Top * _sideViewHeight));
            layersInfo.Add(kvp);
        }

        return layersInfo;
    }

    private void SideCutMouseEvent()
    {
        _event = Event.current;

        int y = (int)(_sideRect.height - _event.mousePosition.y); //This is relative to the _sideRect, since we update the event "inside" the guiutility area

        float yInRange = (float)Math.Round(y / _sideRect.height, 3); //Mouse position scaled to 0.0f - 1.0f range

        //Debug.Log(Math.Round(y / _sideRect.height,3));

        if (_event.type == EventType.MouseDown)
        {
            int min = y - 6;
            int max = y + 3;

            for (int i = 1; i < _editLayers.Count; i++)
            {
                TerrainLayer tlayer = _editLayers[i];

                int minInPixels = (int)(tlayer.Bottom * _sideViewHeight);
                Debug.Log("min of " + tlayer.Name + " is " + minInPixels + ". Mouse y = " + y);
                if (minInPixels > min && minInPixels < max)
                {
                    minLayer = tlayer;
                    maxLayer = _editLayers[i - 1];
                }
            }
        }

        if (_event.type == EventType.MouseDrag && minLayer != null)
        {
            _sideCutView = SideCutTexture(GetLayerInfo());
            DrawDragLine(y);
            Repaint();
        }

        if (_event.type == EventType.MouseUp)
        {
            if (minLayer != null) // Use a boolean "selected" instead
            {
                float layerMinHeight = 0.1f;
                float maxValue = Math.Min(minLayer.Top - layerMinHeight, yInRange); //That sure is a whole lotta min and max
                float minValue = Math.Max(maxLayer.Bottom + layerMinHeight, yInRange); //Oh and these are for ensuring the min and max values don't pass other layers

                Debug.Log(minLayer.Top - layerMinHeight);

                minLayer.Bottom = minValue;
                maxLayer.Top = maxValue;
                minLayer = null; //You can't null these you fucking dip
                maxLayer = null;
            }

            _sideCutView = SideCutTexture(GetLayerInfo());
            Repaint();
        }

    }

    void DrawDragLine (float height)
    {
        if (minLayer == null)
            return;

        int lineWidth = 30;
        int y = (int)(height);

        Color[] arr = _sideCutView.GetPixels();


        for (int ii = lineWidth * y; ii < lineWidth * y + lineWidth; ii++)
        {
            arr[ii] = Color.red;
        }

        _sideCutView.SetPixels(arr);
        _sideCutView.Apply();
    }

    Texture2D SideCutTexture (List<KeyValuePair<Color, int>> layersInfo)
    {
        Texture2D newTex = new Texture2D(30, _sideViewHeight);

        int oldY = 0;
        foreach (KeyValuePair<Color,int> kvp in layersInfo)
        {
            int newY = kvp.Value;
            for (int y = oldY; y < newY; y++)
            {
                for (int x = 0; x < 30; x++)
                {
                    
                    if (y == newY - 1)
                    {
                        newTex.SetPixel(x, y, Color.black);
                    } else
                    {
                        newTex.SetPixel(x, y, kvp.Key);
                    }

                }
                    

                
            }
            oldY = newY;
        }
        newTex.Apply();
        Repaint();
        return newTex;
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

    private void LoadDefault() //Change current editable layerpref to DefaultTerrainLayers
    {
        List<string> jsonList = File.ReadAllLines(PrefPath + "DefaultTerrainLayers.json").ToList();

        _currentEditable = null;
        _editLayers = new List<TerrainLayer>();

        foreach (string str in jsonList)
        {
            TerrainLayer layer = JsonUtility.FromJson<TerrainLayer>(str);
            _editLayers.Add(layer);
            Debug.Log(layer.Name);
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

