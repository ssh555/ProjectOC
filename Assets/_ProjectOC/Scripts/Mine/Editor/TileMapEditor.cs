using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Reflection.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;

namespace ProjectOC.MineSystem
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
        private int tempMapWidht, tempMapHeight,copyMapIndex = -1;

        private bool ShowProgrammerData = false;
        public override void OnEnable()
        {
            tileMap = target as TileMap;

            bigMap = tileMap.transform.GetComponentInParent<BigMap>();
            tileMap.SceneInit();
            tileMap.textMeshTransf.gameObject.SetActive(true);
            
            CheckEditData();
       
            ReGenerateSceneObject();
            ChangeCurSelectdOption(tileMap.EditOption);
            base.OnEnable();
        }

        public override void OnDisable()
        {
            tileMap.textMeshTransf.gameObject.SetActive(false);
            base.OnDisable();
            SaveAssetData();
        }
    
        void CheckEditData()
        {
            foreach (var _mineBrush in bigMap.BigMapEditDatas.MineBrushDatas)
            {
                bool newData = true;
                foreach (var _mineData in tileMap.SmallMapEditData.mineData)
                {
                    if (_mineData.MineID == _mineBrush.mineID)
                    {
                        newData = false;
                        break;
                    }
                }
                if (newData)
                {
                    tileMap.SmallMapEditData.mineData.Add(new MineSmallMapEditData.SingleMineData(_mineBrush.mineID));
                }
            }

            tempMapWidht = tileMap.TileWidth;
            tempMapHeight = tileMap.TileHeight;
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
                float scrollDelta = -e.delta.x;
                curBrush.brushSize += Mathf.Sign(scrollDelta) * tileMap.brushSizeScale;
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
                tileMap.textMeshTransf.gameObject.SetActive(true);
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
                tileMap.textMeshTransf.gameObject.SetActive(false);
            }

            if (_index != 2)
            {
                foreach (var _mine in selectMine)
                {
                    _mine.GetComponent<SpriteRenderer>().color = Color.white;
                }
                selectMine.Clear();
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
                        tileMap.textMeshTransf.GetComponent<TextMesh>().text = $"{_x},{_y}";
                        tileMap.textMeshTransf.position = new Vector3(_x, _y, -50f);
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
                    tileMap.SmallMapEditData.gridData[i + j *tileMap.TileWidth] = pen;
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
                        tileMap.SetTileSprite(i, j, tileMap.SmallMapEditData.gridData[i + j *tileMap.TileWidth]);
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
            bool[] oldData = tileMap.SmallMapEditData.gridData;
            int oldRows = tileMap.TileWidth, oldCols = tileMap.TileHeight;

            tileMap.TileWidth = tempMapWidht;
            tileMap.TileHeight = tempMapHeight;
            tileMap.SmallMapEditData.gridData = new bool[tileMap.TileWidth* tileMap.TileHeight];
            
            // 默认设为false
            for (int i = 0; i < tileMap.SmallMapEditData.gridData.Length; i++)
            {
                tileMap.SmallMapEditData.gridData[i] = false;
            }

            if (oldData != null)
            {
                oldRows = Mathf.Min(tileMap.TileWidth, oldRows);
                oldCols = Mathf.Min(tileMap.TileHeight, oldCols);
            
                for (int i = 0; i < oldRows; i++)
                {
                    for (int j = 0; j < oldCols; j++)
                    {
                        tileMap.SmallMapEditData.gridData[i+j*tileMap.TileWidth] = oldData[i+j*oldRows];
                    }
                }
            }
            
            tileMap.ReGenerateTileGo();
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
                    SingleDrawer(mouseWorldPos, tileMap.curMineBrush);
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
                if (e.button == 0 && e.type == EventType.MouseDown)
                {
                    for (int i = selectMine.Count - 1; i >= 0; i--)
                    {
                        DestroyImmediate(selectMine[i]);
                    }
                    selectMine.Clear();

                    
                    //重新排列数据
                    Dictionary<string, MineSmallMapEditData.SingleMineData> tempNameToBrush =
                        new Dictionary<string, MineSmallMapEditData.SingleMineData>();
                    foreach (var _singleMineData in tileMap.SmallMapEditData.mineData)
                    {
                        _singleMineData.MinePoses.Clear();
                        tempNameToBrush.Add(_singleMineData.MineID,_singleMineData);
                    }
                    
                    
                    for (int i = 0; i < tileMap.mineParentTransf.childCount; i++)
                    {
                        GameObject _mine = tileMap.mineParentTransf.GetChild(i).gameObject;
                        string _mineID =  _mine.name.Split("|")[0];

                        tileMap.mineParentTransf.GetChild(i).name = $"{_mineID}|{tempNameToBrush[_mineID].MinePoses.Count}";
                        tempNameToBrush[_mineID].MinePoses.Add(new Vector2(_mine.transform.position.x,_mine.transform.position.y));
                    }
                    
                }
                

            }
        }

        private void SingleDrawer(Vector2 _mousePos, MineBigMapEditData.MineBrushData _mineBrush)
        {
            MineBigMapEditData.BrushData _brush = _mineBrush.brushData;
            MineSmallMapEditData.SingleMineData _curMineData = null;
            foreach (var _singleMineData in tileMap.SmallMapEditData.mineData)
            {
                if (_singleMineData.MineID == _mineBrush.mineID)
                {
                    _curMineData = _singleMineData;   
                    break;
                }
            }
            
            //点
            if (!tileMap.brushTypeIsCircle)
            {
                Vector3 minePos = new Vector3(_mousePos.x, _mousePos.y, 0);
                // 判断有没有在格子上
                Vector2Int _grid = new Vector2Int(Mathf.FloorToInt(minePos.x),Mathf.FloorToInt(minePos.y));
                if(!JudgeDataValid(_grid))
                    return;
                
                GameObject _mine = Instantiate(tileMap.MinePrefab, minePos, Quaternion.identity,
                    tileMap.mineParentTransf);
                tileMap.ProcessMine(_mine, tileMap.curMineBrush,_curMineData.MinePoses.Count);
                _curMineData.MinePoses.Add(new Vector2(_mousePos.x,_mousePos.y));

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
                    // 判断有没有在格子上
                    Vector2Int _grid = new Vector2Int(Mathf.FloorToInt(minePos2D.x),Mathf.FloorToInt(minePos2D.y));
                    
                    if(!JudgeDataValid(_grid))
                        continue;
                    
                    
                    Vector3 minePos = new Vector3(minePos2D.x,minePos2D.y , 0);

                    //Check在范围外
                    //if(minePos )

                    GameObject _mine = Instantiate(tileMap.MinePrefab, minePos, Quaternion.identity,
                        tileMap.mineParentTransf);
                    tileMap.ProcessMine(_mine, tileMap.curMineBrush,_curMineData.MinePoses.Count);
                    _curMineData.MinePoses.Add(minePos2D);
                }
            }

            
        }
        bool JudgeDataValid(Vector2Int _grid)
        {
            bool res = (_grid.x >= 0 && _grid.x < tileMap.SmallMapEditData.width
                                     && _grid.y >= 0 && _grid.y < tileMap.SmallMapEditData.height) 
                       &&(tileMap.SmallMapEditData.gridData[_grid.x + _grid.y * tileMap.TileWidth]);
            return res;
        }
        
        private void ReGenerateSceneObject()
        {
            RegenerateMap();
            tileMap.ReGenerateMine();
        }

        #endregion

        #endregion

        #region Inspect Override
        
        private int brushIndex = 0;
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            StyleCheck();

            // GUILayout.Label($"索引: {tileMap.SmallMapEditData.index}", defaultTextStyle);
            GUILayout.Label($"索引: {tileMap.gameObject.name.Split("_")[1]}", defaultTextStyle);
            

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
            }

            GUILayout.Space(10);
            
            //设置矿物笔刷
            EditorGUILayout.BeginHorizontal();
            int _buttonSize = 50;
            int iCounter = 0;
            foreach (var _singleMineData in tileMap.SmallMapEditData.mineData)
            {
                int _index = iCounter++;
                if (_index % 8 == 0)  //8、16 24
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }

                MineBigMapEditData.MineBrushData _mineBrush = bigMap.BigMapEditDatas.IDToMineBrushData(_singleMineData.MineID);
                //从Sprite获取Texture，直接Sprite.tex 获取的是图集
                string spritePath = AssetDatabase.GetAssetPath(_mineBrush.mineIcon);
                TextureImporter textureImporter = AssetImporter.GetAtPath(spritePath) as TextureImporter;
                Texture2D originalTexture = null;
                if (textureImporter != null)
                {
                    originalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureImporter.assetPath);
                }
                //选中后切换CurBrush
                EditorGUILayout.BeginVertical();
                ToggleButton("", () =>
                {
                    brushIndex = _index;
                    tileMap.curMineBrush = _mineBrush;
                }, brushIndex == _index ,_buttonSize, originalTexture);

                EditorGUILayout.LabelField($"    {_singleMineData.MinePoses.Count}",GUILayout.Width(_buttonSize));
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("地图大小", blackLabelStyle);
            
            tempMapWidht = EditorGUILayout.IntField("地图长", tempMapWidht);
            tempMapHeight = EditorGUILayout.IntField("地图高", tempMapHeight);
            if (GUILayout.Button("重新生成地图"))
            {
                RegenerateMap();
            }

            if (GUILayout.Button("检查合法性"))
            {
                // SaveAssetData();
                CheckConfig();
                tileMap.ReGenerateMine();
            }
            GUILayout.BeginHorizontal();
            copyMapIndex = EditorGUILayout.IntField("拷贝地图下标", copyMapIndex);
            if (GUILayout.Button("复制地图"))
            {
                //要拷贝[1],至少要有 2个
                if (bigMap.SmallMapEditDatas.Count > copyMapIndex && copyMapIndex >= 0)
                {
                    MineSmallMapEditData _targetData = bigMap.SmallMapEditDatas[copyMapIndex];
                    tileMap.SmallMapEditData.width = _targetData.width;
                    tileMap.SmallMapEditData.height = _targetData.height;
                    tempMapWidht = _targetData.width;
                    tempMapHeight = _targetData.height;
                    
                    tileMap.SmallMapEditData.gridData = _targetData.gridData;
                    tileMap.ReGenerateTileGo();
                }
                else
                {
                    Debug.LogError("没有小地图" + copyMapIndex);
                }
            }
            GUILayout.EndHorizontal();
            
            ShowProgrammerData =GUILayout.Toggle(ShowProgrammerData,"程序Debug参数，策划勿动");
            if (ShowProgrammerData)
            {
                DrawDefaultInspector(); 
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        void SaveAssetData()
        {
            EditorUtility.SetDirty(target);
            EditorUtility.SetDirty(tileMap.SmallMapEditData);
            EditorUtility.SetDirty(bigMap.BigMapEditDatas); //笔刷的数据
            //EditorUtility.SetDirty(bigMap.SmallMapEditDatas[tileMap.SmallMapEditData.index]);
            AssetDatabase.SaveAssets();
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
        

        
        

        void CheckConfig()
        {
            //矿物距离太近，错误的位置
            float minDistance = 0.1f;
            // 使用双重循环检查每一对元素的距离
            foreach (var _singleMineData in tileMap.SmallMapEditData.mineData)
            {
                for(int i = _singleMineData.MinePoses.Count-1; i >= 0; i--)
                {
                    Vector2Int _minePos = new Vector2Int(Mathf.FloorToInt(_singleMineData.MinePoses[i].x),Mathf.FloorToInt(_singleMineData.MinePoses[i].y));
                    if(!JudgeDataValid(_minePos))
                    {
                        _singleMineData.MinePoses.RemoveAt(i);
                    }
                }
                
                for (int i = 0; i < _singleMineData.MinePoses.Count; i++)
                {
                    for (int j = i + 1; j < _singleMineData.MinePoses.Count; j++)
                    {
                        if (Vector2.Distance(_singleMineData.MinePoses[i], _singleMineData.MinePoses[j]) < minDistance)
                        {
                            _singleMineData.MinePoses.RemoveAt(j);
                            j--; // 调整索引以检查移除后新的第 j 个元素
                        }
                    }
                }
            }
        }

        #endregion
    }
}