using System;
using Sirenix.Reflection.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileMap))]
//CustomEditor 部分的数据，当跳转到其他Inspect面板后
public class TileMapEditor : Editor
{
    private TileMap tileMap;
    private int brushSize = 1;
    private GUIStyle labelStyle;
    
    public enum InputType
    {
        MouseLeft,
        MouseRight,
        None
    }
    private InputType curInputType;
    private Event e;
    private GameObject startSelectGo,lastSelectGo,curGo;
    public void OnEnable()
    {
        SceneView.duringSceneGui += DrawScale;
        
        tileMap = target as TileMap;
        tileMap.gridParentTransf = GameObject.Find("Editor/GridTransform").transform;
        tileMap.blockParentTransf = GameObject.Find("Editor/MineTransform").transform;
        tileMap.selectOutline = GameObject.Find("Editor").transform.GetChild(2).gameObject;
        
        labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.red; // 设置文本颜色为红色
        labelStyle.fontSize = 20; // 设置字体大小为20
    }

    public void OnDisable()
    {
        SceneView.duringSceneGui -= DrawScale;
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        serializedObject.UpdateIfRequiredOrScript();
        //serializedObject.Update();
        
        GUILayout.Space(10);
        GUILayout.Label("Brush Settings", EditorStyles.boldLabel);
        brushSize = EditorGUILayout.IntSlider("Brush Size", brushSize, 1, 10);

        GUILayout.Space(10);
        GUILayout.Label("Tile Selection", EditorStyles.boldLabel);

        if (GUILayout.Button("重新生成地图"))
        {
            RegenerateMap();
        }
    }

    private void OnSceneGUI()
    {
        ProcessInput();
        DrawInspect();
    }

    private void ProcessInput()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        e = Event.current;
        if (e.button == 0)
        {
            curInputType = InputType.MouseLeft;
        }
        else if(e.button == 1)
        {
            curInputType = InputType.MouseRight;
        }//考虑键盘
        else
        {
            curInputType = InputType.None;
        }
    }

    private void DrawInspect()
    {
        bool pen = false;
        int _x, _y;
        
        //MouseDrag 不算 MouseMove
        if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
            //if (Physics.Raycast(ray, out hit))
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.tag == "EditorOnly")
                {
                    curGo = hit.collider.gameObject;
                    string[] _pos = curGo.name.Split("_");
                    _x = int.Parse(_pos[0]);
                    _y = int.Parse(_pos[1]);
                    tileMap.selectOutline.SetActive(true);
                    tileMap.selectOutline.transform.position = curGo.transform.position;

                    Vector3 debugLabel = new Vector3(_x + 0.5f, _y + 0.5f, 0);
                    Debug.Log($"{debugLabel} text:{_x},{_y}");
                    Handles.Label( debugLabel, $"{_x},{_y}",labelStyle);
                    
                }
                else
                {
                    Debug.LogWarning($"Raycast get error collider: {hit.collider.gameObject.name}");
                }
            }
            else
            {
                tileMap.selectOutline.SetActive(false);
            }
        }
        
        if(curInputType == InputType.None 
           || (e.type != EventType.MouseDown&&e.type != EventType.MouseUp&&e.type != EventType.MouseDrag))
            return;
        
        if (curInputType == InputType.MouseLeft)
        {
            pen = true;
        }
        else if(curInputType == InputType.MouseRight)
        {
            pen = false;
        }
        if (e.type == EventType.MouseDown)
        {
            startSelectGo = curGo;
        }
        else if (e.type == EventType.MouseUp)
        {
            if (startSelectGo != null)
            {
                SetData(startSelectGo, curGo, pen);
            }
        }
        else if (e.type == EventType.MouseDrag)
        {
            //if (startSelectGo != null && lastSelectGo != null && (curGo != lastSelectGo))
            if (startSelectGo != null && lastSelectGo != null && curGo != null &&(curGo.name != lastSelectGo.name))
            {
                SetSprite(startSelectGo,curGo,pen);
            }

            if (curGo != null)
            {
                lastSelectGo = curGo;   
            }
        }
        
        e.Use();
            
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="startGo"></param>
    /// <param name="endGo"></param>
    /// <param name="pen">true 绘制，false 擦除</param>
    void SetData(GameObject startGo,GameObject endGo,bool pen)
    {
        Vector2Int startPos, endPos;
        (startPos, endPos) = GameObjectsToRealRange(startGo, endGo);
        
        //清空外圈
        for (int i = startPos.x; i <= endPos.x; i++)
        {
            for (int j = startPos.y; j <= endPos.y; j++)
            {
                tileMap.gridData[i, j] = pen;
                tileMap.SetTileSprite(i, j, pen);
            }
        }
    }

    void SetSprite(GameObject startGo, GameObject endGo, bool pen)
    {
        // Vector2Int startPos, endPos;
        // (startPos, endPos) = GameObjectsToRealRange(startGo, endGo);
        Vector2Int startPos = GameObjectToPos(startGo);
        Vector2Int endPos = GameObjectToPos(endGo);
        Vector2Int lastPos = GameObjectToPos(lastSelectGo);
        int minX = Mathf.Min(startPos.x, Mathf.Min(endPos.x,lastPos.x));
        int maxX = Mathf.Max(startPos.x, Mathf.Max(endPos.x,lastPos.x));
        int minY = Mathf.Min(startPos.y, Mathf.Min(endPos.y,lastPos.y));
        int maxY = Mathf.Max(startPos.y, Mathf.Max(endPos.y,lastPos.y));
        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                try
                {
                    tileMap.SetTileSprite(i, j, tileMap.gridData[i,j]);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"{i},{j}");
                    Console.WriteLine(exception);
                    throw;
                }
                
            }
        }
        (startPos, endPos) = GameObjectsToRealRange(startGo, endGo);
        for (int i = startPos.x; i <= endPos.x; i++)
        {
            for (int j = startPos.y; j <= endPos.y; j++)
            {
                tileMap.SetTileSprite(i, j, pen);
            }
        }
    }
    
    
    (Vector2Int, Vector2Int) GameObjectsToRealRange(GameObject _go1, GameObject _go2)
    {
        Vector2Int startPos = GameObjectToPos(_go1);
        Vector2Int endPos = GameObjectToPos(_go2);
        int xmin = Mathf.Min(startPos.x, endPos.x);
        int xmax = Mathf.Max(startPos.x, endPos.x);
        int ymin = Mathf.Min(startPos.y, endPos.y);
        int ymax = Mathf.Max(startPos.y, endPos.y);

        return (new Vector2Int(xmin, ymin), new Vector2Int(xmax, ymax));
    }
    Vector2Int GameObjectToPos(GameObject _go)
    {
        string[] _pos = _go.name.Split("_");
        int _x = int.Parse(_pos[0]);
        int _y = int.Parse(_pos[1]);
        return new Vector2Int(_x, _y);
    }


    private void RegenerateMap()
    {
        if (tileMap.TilePrefab == null)
        {
            Debug.LogWarning("Please select a tile to fill with.");
            return;
        }

        //复制旧数据
        bool[,] oldData = tileMap.gridData;
        tileMap.gridData = new bool[tileMap.Width, tileMap.Height];
        tileMap.tiles = new GameObject[tileMap.Width,tileMap.Height];

        int rows = Mathf.Min(oldData.GetLength(0), tileMap.gridData.GetLength(0));
        int cols = Mathf.Min(oldData.GetLength(1), tileMap.gridData.GetLength(1));
        
        // 默认设为false
        for (int i = 0; i < tileMap.gridData.GetLength(0); i++)
        {
            for (int j = 0; j < tileMap.gridData.GetLength(1); j++)
            {
                tileMap.gridData[i, j] = false;
            }
        }
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                tileMap.gridData[i, j] = oldData[i, j];
            }
        }
        
        
        //生成可视化方块
        for (int i = tileMap.gridParentTransf.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(tileMap.gridParentTransf.GetChild(i).gameObject);
        }
        
        for (int x = 0; x < tileMap.Width; x++)
        {
            for (int y = 0; y < tileMap.Height; y++)
            {
                tileMap.SetTileSprite(x, y, tileMap.gridData[x,y]);
            }
        }
    }

    void DrawScale(SceneView sceneView)
    {
        Handles.Label( new Vector3(0.5f,0.5f,0f), $"0,0",labelStyle);
        float scaleLineLength = 0.5f;
        float scaleLineThickness = 0.05f; // 设置线条的粗细
        Color rectangleColor = Color.red;
        Vector3 originPos = Vector3.zero;
        float stepSize = 1.0f;
        
        
        // Draw 水平 scale
        for (int i = 0; i < tileMap.Width; i++)
        {
            Vector3 start = originPos + new Vector3(i * stepSize, 0, 0);
            Vector3 end = start + new Vector3(0, scaleLineLength, 0);

            Vector3 thinkBuffer = Vector3.right * scaleLineThickness;
            Handles.DrawSolidRectangleWithOutline(
                new Vector3[] { start + thinkBuffer, start - thinkBuffer, end - thinkBuffer, end + thinkBuffer}, 
                rectangleColor, rectangleColor);
            // Label the row index
            Handles.Label(start + new Vector3(scaleLineLength, 0, 0), i.ToString());
        }

        // Draw 垂直 scale
        for (int j = 0; j < tileMap.Height; j++)
        {
            Vector3 start = originPos + new Vector3(0, j * stepSize, 0);
            Vector3 end = start + new Vector3(scaleLineLength, 0, 0);
            //Handles.DrawLine(start, end);
            Vector3 thinkBuffer = Vector3.up * scaleLineThickness;
            Handles.DrawSolidRectangleWithOutline(
                new Vector3[] { start + thinkBuffer, start - thinkBuffer, end - thinkBuffer, end + thinkBuffer}, 
                rectangleColor, rectangleColor);
            Handles.Label(start + new Vector3(0, scaleLineLength, 0), j.ToString());
        }
    }
    
    //不考虑z轴
    private void DrawThickLine(Vector3 start, Vector3 end,bool isHorizontal , float thickness)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = new Vector3(-direction.z, 0, direction.x);
        
        
        Vector3 p1 = start + perpendicular * (thickness / 2);
        Vector3 p2 = start - perpendicular * (thickness / 2);
        Vector3 p3 = end - perpendicular * (thickness / 2);
        Vector3 p4 = end + perpendicular * (thickness / 2);

        Handles.DrawSolidRectangleWithOutline(new Vector3[] { p1, p2, p3, p4 }, Color.red, Color.red);
    }
}
