using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Reflection.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;

namespace MineSystem
{
    [CustomEditor(typeof(TileMap))]
//CustomEditor 部分的数据，当跳转到其他Inspect面板后
    public class TileMapEditor : OCCustomEditor
    {
        private TileMap tileMap;
        private Event e;
        private GameObject startSelectGo, lastSelectGo, curGo;
        private List<GameObject> selectMine = new List<GameObject>();
        private BigMap bigMap;
        private bool showShortCut = false;
        public override void OnEnable()
        {
            tileMap = target as TileMap;
            tileMap.gridParentTransf = GameObject.Find("Editor/GridTransform").transform;
            tileMap.mineParentTransf = GameObject.Find("Editor/MineTransform").transform;
            tileMap.selectOutline = GameObject.Find("Editor/Others").transform.GetChild(0).gameObject;
            tileMap.TilePrefab = GameObject.Find("Editor/Others").transform.GetChild(1).gameObject;
            tileMap.MinePrefab = GameObject.Find("Editor/Others").transform.GetChild(2).gameObject;
            tileMap.brushIcon = GameObject.Find("Editor/Others").transform.GetChild(3).gameObject;
            bigMap = tileMap.transform.GetComponentInParent<BigMap>();

            CheckEditData();
            ChangeCurSelectdOption(tileMap.EditOption);
            //Scene部分
            base.OnEnable();
            
            ReGenerateSceneObject();
        }

        void CheckEditData()
        {
            if (tileMap.SmallMapEditData == null)
            {
                string[] nameStr = tileMap.gameObject.name.Split("_");
                int _index = int.Parse(nameStr[1]);
                tileMap.SmallMapEditData = bigMap.SmallMapEditDatas[_index];
            }

            tileMap.curMineBrush = bigMap.BigMapEditDatas.MineBrushDatas[0];
            foreach (var _mineBrush in bigMap.BigMapEditDatas.MineBrushDatas)
            {
                if ( !tileMap.SmallMapEditData.smallMapMineData.ContainsKey(_mineBrush))
                {
                    tileMap.SmallMapEditData.smallMapMineData[_mineBrush] = new List<Vector2>();  
                }
            }
            
            scaleWidth = tileMap.TileWidth;
            scaleHeight = tileMap.TileHeight;
        }
        





        #region Scene Override

        #region Unity

        /// <summary>
        /// Scene面板的Update
        /// </summary>
        private void OnSceneGUI()
        {
            ProcessInput();
            //tileMap.mineParentTransf.localScale = Vector3.one * tileMap.mineScale;
            if (tileMap.EditOption == 0)
            {
                DrawTileInspect();
            }
            else
            {
                DrawMine();
            }
        }

        private void ProcessInput()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            e = Event.current;

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Alpha1)
                    ChangeCurSelectdOption(0);
                else if (e.keyCode == KeyCode.Alpha2)
                    ChangeCurSelectdOption(1);
                else if (e.keyCode == KeyCode.Alpha3)
                    ChangeCurSelectdOption(2);
                else if (e.keyCode == KeyCode.LeftShift)
                {
                    tileMap.isShiftPressed = true;
                }

                if (tileMap.EditOption != 0 && e.keyCode == KeyCode.Tab)
                {
                    SwitchBrushIcon();
                }

                e.Use();
            }
            else if (e.type == EventType.KeyUp)
            {
                tileMap.isShiftPressed = false;
            }

            if (tileMap.isShiftPressed && e.type == EventType.ScrollWheel)
            {
                float scrollDelta = e.delta.x;
                curBrush.brushSize += Mathf.Sign(scrollDelta) * tileMap.brushSizeScale;
                //todo
                e.Use();
            }
        }

        void ChangeCurSelectdOption(int _index)
        {
            tileMap.EditOption = _index;
            //画块块
            if (_index == 0)
            {
                foreach (Transform _transf in tileMap.gridParentTransf)
                {
                    _transf.GetComponent<Collider2D>().enabled = true;
                }

                foreach (Transform _transf in tileMap.mineParentTransf)
                {
                    _transf.GetComponent<Collider2D>().enabled = false;
                }

                tileMap.brushIcon.SetActive(false);
                foreach (var _mine in selectMine)
                {
                    _mine.GetComponent<SpriteRenderer>().color = Color.white;
                }
                selectMine.Clear();
            }
            //画矿
            else
            {
                foreach (Transform _transf in tileMap.gridParentTransf)
                {
                    _transf.GetComponent<Collider2D>().enabled = false;
                }

                foreach (Transform _transf in tileMap.mineParentTransf)
                {
                    _transf.GetComponent<Collider2D>().enabled = true;
                }

                tileMap.brushIcon.SetActive(tileMap.brushTypeIsCircle);
                tileMap.selectOutline.SetActive(false);
            }
        }

        #endregion

        #region DrawTile

        private void DrawTileInspect()
        {
            bool pen = false;
            int _x, _y;

            //MouseDrag 不算 MouseMove
            //处理鼠标移动和拖拽的选中
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
                        //Debug.Log($"{debugLabel} text:{_x},{_y}");
                        Handles.Label(debugLabel, $"{_x},{_y}", scalelabelStyle);
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



            // (e.type != EventType.MouseDown&&e.type != EventType.MouseUp&& !(e.type == EventType.MouseDrag && e.button == 0 || e.button == 1)))
            //不是鼠标或者是中间拖拽
            if (!e.isMouse || e.type == EventType.MouseMove || (e.button == 2))
                return;
            if (e.button == 0)
            {
                pen = true;
            }
            else if (e.button == 1)
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
                if (startSelectGo != null && lastSelectGo != null && curGo != null && (curGo.name != lastSelectGo.name))
                {
                    SetSprite(startSelectGo, curGo, pen);
                }

                if (curGo != null)
                {
                    lastSelectGo = curGo;
                }
            }

            e.Use();

        }


        /// <summary>
        /// 修改数据 + 修改可视化画板颜色
        /// </summary>
        /// <param name="pen">true 绘制，false 擦除</param>
        void SetData(GameObject startGo, GameObject endGo, bool pen)
        {
            Vector2Int startPos, endPos;
            (startPos, endPos) = GameObjectsToRealRange(startGo, endGo);

            //清空外圈
            for (int i = startPos.x; i <= endPos.x; i++)
            {
                for (int j = startPos.y; j <= endPos.y; j++)
                {
                    tileMap.SmallMapEditData.gridData[i, j] = pen;
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
            int minX = Mathf.Min(startPos.x, Mathf.Min(endPos.x, lastPos.x));
            int maxX = Mathf.Max(startPos.x, Mathf.Max(endPos.x, lastPos.x));
            int minY = Mathf.Min(startPos.y, Mathf.Min(endPos.y, lastPos.y));
            int maxY = Mathf.Max(startPos.y, Mathf.Max(endPos.y, lastPos.y));
            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    try
                    {
                        tileMap.SetTileSprite(i, j, tileMap.SmallMapEditData.gridData[i, j]);
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
            bool[,] oldData = tileMap.SmallMapEditData.gridData;
            tileMap.SmallMapEditData.gridData = new bool[tileMap.TileWidth, tileMap.TileHeight];
            tileMap.tiles = new GameObject[tileMap.TileWidth, tileMap.TileHeight];
            // 默认设为false
            for (int i = 0; i < tileMap.TileWidth; i++)
            {
                for (int j = 0; j < tileMap.TileHeight; j++)
                {
                    tileMap.SmallMapEditData.gridData[i, j] = false;
                }
            }


            int oldRows = tileMap.TileWidth, oldCols = tileMap.TileHeight;
            if (oldData != null)
            {
                oldRows = Mathf.Min(oldData.GetLength(0), oldRows);
                oldCols = Mathf.Min(oldData.GetLength(1), oldCols);

                for (int i = 0; i < oldRows; i++)
                {
                    for (int j = 0; j < oldCols; j++)
                    {
                        tileMap.SmallMapEditData.gridData[i, j] = oldData[i, j];
                    }
                }
            }



            //生成可视化方块
            DestroyTransformChild(tileMap.gridParentTransf);

            for (int x = 0; x < tileMap.TileWidth; x++)
            {
                for (int y = 0; y < tileMap.TileHeight; y++)
                {
                    tileMap.SetTileSprite(x, y, tileMap.SmallMapEditData.gridData[x, y]);
                }
            }
        }

        void DestroyTransformChild(Transform _transf)
        {
            for (int i = _transf.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_transf.GetChild(i).gameObject);
            }
        }


        #endregion

        #region DrawMine

        private MineBigMapEditData.BrushData curBrush =>tileMap.curMineBrush.brushData;

        private void DrawMine()
        {
            Vector2 mouseWorldPos = Vector2.zero;
            if (e.isMouse)
            {
                Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                mouseWorldPos = worldRay.origin + worldRay.direction * 10;
                tileMap.brushIcon.transform.position = mouseWorldPos;
            }
            else
            {
                return;
            }

            //Update
            tileMap.brushIcon.SetActive(tileMap.brushTypeIsCircle);
            tileMap.brushIcon.transform.localScale = Vector3.one * curBrush.brushSize * 0.5f;

            if (tileMap.EditOption == 1)
            {
                if (e.button == 0 && e.type == EventType.MouseDown)
                {
                    SingleDrawer(mouseWorldPos, curBrush);
                }
            }
            else if (tileMap.EditOption == 2)
            {

                if (e.type == EventType.MouseMove)
                {
                    foreach (var _mine in selectMine)
                    {
                        _mine.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                    selectMine.Clear();
                    
                    //点
                    if (!tileMap.brushTypeIsCircle)
                    {
                        //复原
                        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray);
                        //if (Physics.Raycast(ray, out hit))
                        foreach (var _hit in hits)
                        {
                            if (_hit.collider != null)
                            {
                                if (_hit.collider.gameObject.tag == "EditorOnly")
                                {
                                    selectMine.Add(_hit.collider.gameObject);
                                }
                            }
                        }
                       
                    }
                    else
                    {
                        //根据距离算
                        foreach (Transform _mine in tileMap.mineParentTransf)
                        {
                            if (Vector2.Distance(_mine.transform.position, mouseWorldPos) < curBrush.brushSize * 0.5f)
                            {
                                selectMine.Add(_mine.gameObject);
                            }
                        }
                    }

                    foreach (var _mine in selectMine)
                    {
                        _mine.GetComponent<SpriteRenderer>().color = Color.cyan;
                    }
                }
                //删除矿物
                if (e.type == EventType.MouseDown)
                {
                    for (int i = selectMine.Count - 1; i >= 0; i--)
                    {
                        DestroyImmediate(selectMine[i]);
                    }
                    selectMine.Clear();

                    
                    //重新排列数据
                    Dictionary<string, MineBigMapEditData.MineBrushData> tempNameToBrush =
                        new Dictionary<string, MineBigMapEditData.MineBrushData>();
                    foreach (var _dic in tileMap.SmallMapEditData.smallMapMineData)
                    {
                        _dic.Value.Clear();
                        tempNameToBrush[_dic.Key.mineID]= _dic.Key;
                    }
                    
                    
                    for (int i = 0; i < tileMap.mineParentTransf.childCount; i++)
                    {
                        GameObject _mine = tileMap.mineParentTransf.GetChild(i).gameObject;
                        string _mineID =  _mine.name.Split("|")[0];
                        MineBigMapEditData.MineBrushData _mineData = tempNameToBrush[_mineID];
                        
                        tileMap.SmallMapEditData.smallMapMineData[_mineData].Add(new Vector2(_mine.transform.position.x,_mine.transform.position.y));
                        tileMap.mineParentTransf.GetChild(i).name = $"{_mineID}|{tileMap.SmallMapEditData.smallMapMineData[_mineData].Count}";
                    }
                    
                }
                

            }
        }

        private void SingleDrawer(Vector2 _mousePos, MineBigMapEditData.BrushData _brush)
        {
            //点
            if (!tileMap.brushTypeIsCircle)
            {
                Vector3 minePos = new Vector3(_mousePos.x, _mousePos.y, 0);
                // _mine.name = "";
                GameObject _mine = Instantiate(tileMap.MinePrefab, minePos, Quaternion.identity,
                    tileMap.mineParentTransf);
                ProcessMine(_mine, tileMap.curMineBrush,tileMap.SmallMapEditData.smallMapMineData[tileMap.curMineBrush].Count);
                tileMap.SmallMapEditData.smallMapMineData[tileMap.curMineBrush].Add(_mousePos);
            }
            //圈
            else
            {
                for (int i = 0; i < _brush.brushDensity; i++)
                {
                    float _angle = Random.Range(0f, 2 * Mathf.PI);
                    float _randomDistance = Random.Range(0f, 1f);
                    _randomDistance = Mathf.Pow(_randomDistance, _brush.brushHard + 1);
                    float distance = _randomDistance * _brush.brushSize * 0.5f;
                    float offsetX = Mathf.Cos(_angle) * distance;
                    float offsetY = Mathf.Sin(_angle) * distance;

                    Vector2 minePos2D = new Vector2(_mousePos.x + offsetX,_mousePos.y + offsetY);
                    Vector3 minePos = new Vector3(minePos2D.x,minePos2D.y , 0);

                    //Check在范围外
                    //if(minePos )

                    GameObject _mine = Instantiate(tileMap.MinePrefab, minePos, Quaternion.identity,
                        tileMap.mineParentTransf);

                    ProcessMine(_mine, tileMap.curMineBrush,tileMap.SmallMapEditData.smallMapMineData[tileMap.curMineBrush].Count);
                    tileMap.SmallMapEditData.smallMapMineData[tileMap.curMineBrush].Add(minePos2D);
                }
            }
        }
        void ProcessMine(GameObject _mineGo,MineBigMapEditData.MineBrushData _mineBrushData,int _index)
        {                
            _mineGo.SetActive(true);
            _mineGo.GetComponent<SpriteRenderer>().sprite = _mineBrushData.mineIcon;
            _mineGo.transform.localScale = Vector3.one * tileMap.mineScale;
            _mineGo.name = $"{_mineBrushData.mineID}|{_index}";
        }
        private void ReGenerateMine()
        {
            DestroyTransformChild(tileMap.mineParentTransf);
            foreach (var _singleMineData in tileMap.SmallMapEditData.smallMapMineData)
            {
                for (int i = 0; i < _singleMineData.Value.Count; i++)
                {
                    GameObject _mine = Instantiate(tileMap.MinePrefab, 
                        new Vector3(_singleMineData.Value[i].x,_singleMineData.Value[i].y,0), Quaternion.identity,
                        tileMap.mineParentTransf);
                    
                    ProcessMine(_mine, _singleMineData.Key,i);
                }
            }
            
        }
        private void ReGenerateSceneObject()
        {
            RegenerateMap();
            ReGenerateMine();
        }

        #endregion

        #endregion

        #region Inspect Override



        int btnSize = 100;
        int selectBordSize = 3;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            StyleCheck();

            GUILayout.Label("索引: 0", defaultTextStyle);
            tileMap.mineScale = EditorGUILayout.Slider("矿物与地块比例:", tileMap.mineScale, 0.01f, 1);

            GUILayout.Label("绘制工具", blackLabelStyle);
            
            showShortCut = EditorGUILayout.Foldout(showShortCut, "快捷键说明");
            if(showShortCut) {
                GUILayout.Label("1 2 3切换 地形、矿物绘制模式");
                GUILayout.Label("Tab 切换笔刷点圈模式");
                GUILayout.Label("长按Shift+鼠标滚轮 更改笔刷大小");
            }

            ToggleButton("地形绘制", (() => ChangeCurSelectdOption(0)), tileMap.EditOption == 0);


            GUILayout.BeginHorizontal();
            ToggleButton("项绘制", (() => ChangeCurSelectdOption(1)), tileMap.EditOption == 1);
            ToggleButton("项擦除", (() => ChangeCurSelectdOption(2)), tileMap.EditOption == 2);
            // ToggleButton("项擦除",(() => ChangeCurSelectdOption(3)),tileMap.EditOption == 3);
            GUILayout.EndHorizontal();

            if (tileMap.EditOption != 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("笔刷类型", blackLabelStyle);

                GUILayout.BeginHorizontal();
                ToggleButton("点", (() => SwitchBrushIcon(0)), !tileMap.brushTypeIsCircle, 30);
                ToggleButton("圈", (() => SwitchBrushIcon(1)), tileMap.brushTypeIsCircle, 30);
                GUILayout.EndHorizontal();

                curBrush.brushSize = EditorGUILayout.Slider("画笔大小:", curBrush.brushSize, curBrush.brushSizeMin,
                    curBrush.brushSizeMax);


                curBrush.brushHard = EditorGUILayout.Slider("画笔分散度:", curBrush.brushHard, curBrush.brushHardMin,
                    curBrush.brushHardMax);
                curBrush.brushDensity = EditorGUILayout.IntSlider("项密度:", curBrush.brushDensity,
                    curBrush.brushDensityMin, curBrush.brushDensityMax);

                DrawMineData();
            }

            GUILayout.Space(10);
            
            //设置矿物笔刷
            EditorGUILayout.BeginHorizontal();
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            int _buttonSize = 50;
            
            foreach (var _singleMineData in tileMap.SmallMapEditData.smallMapMineData)
            {
                MineBigMapEditData.MineBrushData _mineBrush = _singleMineData.Key;
                buttonStyle.normal.background = (Texture2D)_mineBrush.mineIcon.texture;
                //选中后切换CurBrush
                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("", buttonStyle, GUILayout.Width(_buttonSize), GUILayout.Height(_buttonSize)))
                {
                    tileMap.curMineBrush = _mineBrush;
                }
                EditorGUILayout.LabelField($"    {_singleMineData.Value.Count}");
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Label("Tile Selection", EditorStyles.boldLabel);

            if (GUILayout.Button("重新生成地图"))
            {
                RegenerateMap();
            }

            if (GUILayout.Button("检查合法性"))
            {
                CheckConfig();
            }
            
            //DrawDefault
            DrawDefaultInspector();
        }


        /// <summary>
        /// 0 点 1 圈
        /// </summary>
        /// <param name="type"></param>
        void SwitchBrushIcon(int type = -1)
        {
            if (type == 0)
                tileMap.brushTypeIsCircle = false;
            else if (type == 1)
                tileMap.brushTypeIsCircle = true;
            else
                tileMap.brushTypeIsCircle = !tileMap.brushTypeIsCircle;
            tileMap.brushIcon.SetActive(tileMap.brushTypeIsCircle);
        }

        void ToggleButton(string labelText, Action buttonAction, bool SelectCondition, int _buttonSize = -1)
        {
            if (_buttonSize == -1)
            {
                _buttonSize = btnSize;
            }

            if (GUILayout.Button(labelText, GUILayout.Height(_buttonSize), GUILayout.Width(_buttonSize)))
            {
                buttonAction();
            }

            if (SelectCondition)
            {
                DrawBorder(selectBordSize, new Color(1, 0, 0, 0.25f));
            }

            GUILayout.Space(20);
        }

        private void DrawBorder(float size, Color color)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.width += size * 2;
            rect.height += size * 2;
            rect.x -= size;
            rect.y -= size;

            EditorGUI.DrawRect(rect, color);
        }

        void DrawMineData()
        {
            //Draw
        }

        void CheckConfig()
        {
            //矿物距离太近，错误的位置
        }

        #endregion
    }
}