using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Windows.Input;

public class TerrainLayerEditor : EditorWindow
{
    public string PrefPath
    {
        get { return Application.dataPath + "/TerrainPrefs/"; }
    }

    private List<TerrainLayer> _editLayers;
    private TerrainLayer _currentEditable;
    private int _curEditInt;

    private Event _event;
    private int _mouseOriginY;

    private TerrainLayer bottomLayerDragged;
    private TerrainLayer topLayerDraggedt;
    private int topDrag;    //The position in the array of the layer of which .top    value we are modifying
    private int bottomDrag; //The position in the array of the layer of which .bottom value we are modifying

    private Rect _sideRect = new Rect(2, 120, 38, _sideViewHeight + 6);
    private Texture2D _sideCutView;
    private const int _sideViewHeight = 360;

    [MenuItem("Window/K-Terrain/Edit terrain layers")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        TerrainLayerEditor window = (TerrainLayerEditor)GetWindow(typeof(TerrainLayerEditor));
        window.Show();

        //window.GenerateGenericJSON();
        window.wantsMouseMove = true;
    }

    void OnGUI() //TODO: Make new functions instead of regions
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

        for (int i = _editLayers.Count - 1; i >= 0; i--)
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

        #region Color preview field
        GUILayout.BeginHorizontal();

        GUILayout.Label("PreviewColor: ");
        _currentEditable.PreviewColor = EditorGUILayout.ColorField(_currentEditable.PreviewColor);
        _currentEditable.IgnoreHeight = GUILayout.Toggle(_currentEditable.IgnoreHeight,"Ignore Height:");

        GUILayout.EndHorizontal();
        #endregion Color preview field
        GUILayout.EndVertical();

        #endregion Current editable

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

        float yInRange = (float)Math.Round(y / _sideRect.height, 3); //Mouse position.y scaled to 0.0f - 1.0f range

        if (_event.type == EventType.MouseDown)
        {

            #region Drag line hit test
            int lineHitMin = y - 6;
            int lineHitMax = y + 3;

            for (int i = 1; i < _editLayers.Count; i++)
            {
                TerrainLayer tlayer = _editLayers[i];

                int minInPixels = (int)(tlayer.Bottom * _sideViewHeight);
                Debug.Log("min of " + tlayer.Name + " is " + minInPixels + ". Mouse y = " + y);
                if (minInPixels > lineHitMin && minInPixels < lineHitMax)
                {
                    bottomLayerDragged = tlayer.Clone() as TerrainLayer;
                    topLayerDraggedt = _editLayers[i - 1].Clone() as TerrainLayer;

                    bottomDrag = i;
                    topDrag = i - 1;
                    return;
                }
            }
            #endregion

            #region Select layer click test



            #endregion
        }

        if (_event.type == EventType.MouseDrag && bottomLayerDragged != null)
        {
            _sideCutView = SideCutTexture(GetLayerInfo());
            DrawDragLine(y);
            Repaint();
        }

        if (_event.type == EventType.MouseUp)
        {
            if (bottomLayerDragged != null)
            {
                float layerMinHeight = 0.05f;
                //minLayer.Top - layerMinHeight, yInRange);

                if (yInRange < topLayerDraggedt.Bottom + layerMinHeight || yInRange > bottomLayerDragged.Top - layerMinHeight)
                {
                    bottomLayerDragged = null;
                    topLayerDraggedt = null;
                    return;
                }


                Debug.Log(bottomLayerDragged.Top - layerMinHeight);

                bottomLayerDragged.Bottom = yInRange;
                topLayerDraggedt.Top = yInRange;

                _editLayers[bottomDrag] = bottomLayerDragged;
                _editLayers[topDrag] = topLayerDraggedt;
                //Debug.Log(realLayerMin.Bottom);
                bottomLayerDragged = null;
                topLayerDraggedt = null;
            }

            _sideCutView = SideCutTexture(GetLayerInfo());
            Repaint();
        }

    }

    private TerrainLayer FindTerrain(string name)
    {
        TerrainLayer match = null;

        for (int i = 0; i < _editLayers.Count; i++)
        {
            if (_editLayers[i].Name == name)
                match = _editLayers[i];
        }
        Debug.Log(match.Name);
        return match;
    }

    void DrawDragLine (float height)
    {
        if (bottomLayerDragged == null)
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
        List<string> jsonList = new List<string>();

        foreach(TerrainLayer layer in _editLayers)
        {
            jsonList.Add(JsonUtility.ToJson(layer));
        }

        try
        {
            File.WriteAllLines(LayerSerializer.DefaultJSONPath, jsonList);
        }
        catch (Exception e)
        {
            
        }

        TerrainEditor window = (TerrainEditor)GetWindow(typeof(TerrainEditor));
        window.Show();
        window.UpdateTerrainPreview();
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

