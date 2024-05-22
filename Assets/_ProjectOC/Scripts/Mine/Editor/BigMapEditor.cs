using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Animancer.Editor;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Serialization = Unity.VisualScripting.Serialization;

namespace ProjectOC.MineSystem
{
    [CustomEditor(typeof(BigMap))]
    public class BigMapEditor : OCCustomEditor
    {
        #region Unity

        private BigMap bigMap;
        private Rect controlRect;
        private ReorderableList reorderableList;

        private Transform mapPreview;
        private bool ShowProgrammerData = false;
        private float tempMineScale;
        private int brushIndex;
        private string tempMineIDData;
        private Sprite tempMineTexData;
        private bool showMineEdit = false;
        public override void OnEnable()
        {
            base.OnEnable();
            bigMap = target as BigMap;
            mapPreview = GameObject.Find("Editor/Others").transform.GetChild(4); 
            tempMineScale = bigMap.mineToTileScale;
            brushIndex = 0;

            CheckAssetAndData();
            ReorderableListInit();
        }

        void ReorderableListInit()
        {
            reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("tileMaps")
                ,true,true,true,true);
            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "小地图");
            };

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                float buttonWidth = 50;
                float labelWidth = 50;
                float fieldWidth = rect.width - buttonWidth - labelWidth - 15;
                
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight),
                    element, GUIContent.none);

                if (GUI.Button(new Rect(rect.x + fieldWidth + 5, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight), "显示"))
                {
                    TileMap selectedTileMap = element.objectReferenceValue as TileMap;
                    if (selectedTileMap != null)
                    {
                        selectedTileMap.ReGenerateTileGo();
                        selectedTileMap.ReGenerateMine();
                    }
                }
                EditorGUI.LabelField(new Rect(rect.x + fieldWidth + buttonWidth + 10, rect.y, labelWidth, EditorGUIUtility.singleLineHeight), index.ToString());
            };
            reorderableList.onReorderCallback = (ReorderableList list) =>
            {
                serializedObject.ApplyModifiedProperties();
            };
        }

        void CheckAssetAndData()
        {
            //读取大地图和小地图所有资产
            bigMap.BigMapEditDatas = AssetDatabase.LoadAssetAtPath<MineBigMapEditData>(bigMapPath);
            ReloadSmallMapAsset();
            if (bigMap.transform.childCount != bigMap.SmallMapEditDatas.Count)
            {
                TileMap.DestroyTransformChild(bigMap.transform);

                for (int i = 0; i < bigMap.SmallMapEditDatas.Count; i++)
                {
                    GameObject _go = new GameObject($"MapSmart_{i}");
                    _go.transform.SetParent(bigMap.transform);
                    TileMap _tileMap = _go.AddComponent<TileMap>();
                    _tileMap.SceneInit();
                }
                bigMap.tileMaps = target.GetComponentsInChildren<TileMap>().ToList();
            }
            
            
            for (int i = 0; i < bigMap.tileMaps.Count; i++)
            {
                if (bigMap.tileMaps[i].SmallMapEditData == null)
                {
                    bigMap.tileMaps[i].SmallMapEditData = bigMap.SmallMapEditDatas[i];
                }
                if (bigMap.tileMaps[i].bigMap == null)
                {
                    bigMap.tileMaps[i].bigMap = bigMap;
                }
            }

            // if (bigMap.bigMapRegionColor.Count == 0)
            // {
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //0
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //1
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //2
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //3
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //4
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //5
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //6
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //7
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //8
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //9
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //10
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //11
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //12
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //13
            //     bigMap.bigMapRegionColor.Add(new Color32(217,217,217,255));    //14
            //     bigMap.bigMapRegionColor.Add(new Color32(243,84,26,255));    //15
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //16
            //     bigMap.bigMapRegionColor.Add(new Color32(242,220,219,255));    //17
            //     bigMap.bigMapRegionColor.Add(new Color32(243,84,26,255));    //18
            //     bigMap.bigMapRegionColor.Add(new Color32(217,217,217,255));    //19
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //20
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //21
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //22
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //23
            //     bigMap.bigMapRegionColor.Add(new Color32(148,138,84,255));    //24
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //25
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //26
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //27
            //     bigMap.bigMapRegionColor.Add(new Color32(89,89,89,255));    //28
            // }
        }

        void ReloadSmallMapAsset()
        {
            string[] assetGUIDs = AssetDatabase.FindAssets("t:" + typeof(MineSmallMapEditData).Name, new[] { smallMapFoldPath });
            foreach (string guid in assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                MineSmallMapEditData _asset = AssetDatabase.LoadAssetAtPath<MineSmallMapEditData>(assetPath);
                if (_asset != null && !bigMap.SmallMapEditDatas.Contains(_asset))
                {
                    bigMap.SmallMapEditDatas.Add(_asset);
                }
            }
            bigMap.SmallMapEditDatas.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        }
        public override void OnDisable()
        {
            base.OnDisable();
            SaveAssetData();
            
            mapPreview.gameObject.SetActive(false);
        }
        void SaveAssetData()
        {
            if (bigMap.BigMapEditDatas != null)
            {
                EditorUtility.SetDirty(target);
                EditorUtility.SetDirty(bigMap.BigMapEditDatas);
                //更改小地图排序数据
                foreach (var _data in bigMap.SmallMapEditDatas)
                {
                    EditorUtility.SetDirty(_data);
                }
                AssetDatabase.SaveAssets();
            }
        }
        #endregion

        #region OverWrite
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            StyleCheck();

            if (GUILayout.Button("+导入大地图"))
            {
                //生成图片放上来
                
            }

            controlRect = EditorGUILayout.GetControlRect(true, 20);
            tempMineScale = EditorGUILayout.Slider("矿物与地块比例:", tempMineScale, 0.01f, 1);
            GUILayout.Space(20);
            GUILayout.Label("MapSmart项", blackLabelStyle);
            
            reorderableList.DoLayoutList();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("复原"))
            {
                bigMap.tileMaps = target.GetComponentsInChildren<TileMap>().ToList();
            }
            if (GUILayout.Button("应用"))
            {
                //先改成别的，以防后面重名
                for (int i = 0; i < bigMap.tileMaps.Count; i++)
                {
                    MineSmallMapEditData _smallMapEditData = bigMap.tileMaps[i].SmallMapEditData;
                    string assetPath = AssetDatabase.GetAssetPath(_smallMapEditData);
                    AssetDatabase.RenameAsset(assetPath, i.ToString());
                }

                for (int i = 0; i < bigMap.tileMaps.Count; i++)
                {
                    MineSmallMapEditData _smallMapEditData = bigMap.tileMaps[i].SmallMapEditData;
                    _smallMapEditData.index = i;
                    //Debug.Log($"{i} {bigMap.tileMaps[i].name} Index:{_smallMapEditData.index}");
                    string assetPath = AssetDatabase.GetAssetPath(_smallMapEditData);
                    string newName = $"SmallMapEditData_{i}";
                    AssetDatabase.RenameAsset(assetPath, newName);
                    bigMap.tileMaps[i].gameObject.name = $"MapSmart_{i}";
                }
                //排序
                SortTransformByName(bigMap.tileMaps[0].transform.parent);

                ReloadSmallMapAsset();
        
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            GUILayout.EndHorizontal();
            
            
            if (GUILayout.Button("+新建小地图"))
            {
                int _index = bigMap.SmallMapEditDatas.Count;
                MineSmallMapEditData _smallMapData = ScriptableObject.CreateInstance<MineSmallMapEditData>();
                CreateScriptableObjectAsset(_smallMapData, smallMapFoldPath, $"{smallMapDataNamePre}{_index}");
                bigMap.SmallMapEditDatas.Add(_smallMapData);
                _smallMapData.index = _index;
                GameObject _go = new GameObject($"MapSmart_{_index}");
                _go.transform.SetParent(bigMap.transform);
                
                //TileMap处理
                TileMap _tileMap = _go.AddComponent<TileMap>();
                _tileMap.SceneInit();
                bigMap.tileMaps.Add(_tileMap);
            }

            GUILayout.Space(20);
            GUILayout.Label("笔刷项", blackLabelStyle);
            //选择表资产
            controlRect = EditorGUILayout.GetControlRect(true, 20);
            bigMap.mineID = EditorGUI.TextField(controlRect, "ID:", bigMap.mineID);
            bigMap.mineTex = EditorGUILayout.ObjectField("笔刷图标:", bigMap.mineTex, typeof(Sprite), false) as Sprite;


            if (GUILayout.Button("+新建笔刷项"))
            {
                bool valid = true;
                //新建笔刷项，存储为BrushData
                foreach (var _mineBrushData in bigMap.BigMapEditDatas.MineBrushDatas)
                {
                    if (_mineBrushData.mineID == bigMap.mineID)
                    {
                        valid = false;
                        Debug.LogError("新笔刷矿物ID重复");
                        break;
                    }
                }

                if (valid)
                {
                    MineBigMapEditData.MineBrushData mineBrush = new MineBigMapEditData.MineBrushData(bigMap.mineID,bigMap.mineTex);
                    bigMap.BigMapEditDatas.MineBrushDatas.Add(mineBrush);
                }
            }
            showMineEdit = EditorGUILayout.Foldout(showMineEdit, "矿物笔刷二次编辑");
            if (showMineEdit)
            {
                EditorGUILayout.BeginHorizontal();
                int _buttonSize = 50;
                int iCounter = 0;
                foreach (var _singleBrush in bigMap.BigMapEditDatas.MineBrushDatas)
                {
                    //从Sprite获取Texture，直接Sprite.tex 获取的是图集
                    string spritePath = AssetDatabase.GetAssetPath(_singleBrush.mineIcon);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(spritePath) as TextureImporter;
                    Texture2D originalTexture = null;
                    if (textureImporter != null)
                    {
                        originalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureImporter.assetPath);
                    }

                    //选中后切换CurBrush
                    int _index = iCounter++;
                    ToggleButton("", () =>
                    {
                        brushIndex = _index;
                        tempMineIDData = bigMap.BigMapEditDatas.MineBrushDatas[_index].mineID;
                        tempMineTexData = bigMap.BigMapEditDatas.MineBrushDatas[_index].mineIcon;
                    }, brushIndex == _index, _buttonSize, originalTexture);
                }

                EditorGUILayout.EndHorizontal();
                controlRect = EditorGUILayout.GetControlRect(true, 20);
                tempMineIDData = EditorGUI.TextField(controlRect, "ID:", tempMineIDData);
                tempMineTexData =
                    EditorGUILayout.ObjectField("笔刷图标:", tempMineTexData, typeof(Sprite), false) as Sprite;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("复原"))
                {
                    tempMineIDData = bigMap.BigMapEditDatas.MineBrushDatas[brushIndex].mineID;
                    tempMineTexData = bigMap.BigMapEditDatas.MineBrushDatas[brushIndex].mineIcon;
                }

                if (GUILayout.Button("应用"))
                {
                    //修改笔刷
                    //更新SmallMapData的MineID
                    foreach (var _smallMapData in bigMap.SmallMapEditDatas)
                    {
                        foreach (var _mineData in _smallMapData.mineData)
                        {
                            if (_mineData.MineID == bigMap.BigMapEditDatas.MineBrushDatas[brushIndex].mineID)
                            {
                                _mineData.MineID = tempMineIDData;
                                break;
                            }
                        }
                    }
                    bigMap.BigMapEditDatas.MineBrushDatas[brushIndex].mineID = tempMineIDData;
                    bigMap.BigMapEditDatas.MineBrushDatas[brushIndex].mineIcon = tempMineTexData;
                }

                GUILayout.EndHorizontal();
            }

            
            
            ShowProgrammerData =GUILayout.Toggle(ShowProgrammerData,"程序Debug参数，策划勿动");
            if (ShowProgrammerData)
            {
                if (GUILayout.Button("重新生成资产"))
                {
                    ReGenerateAsset();
                }
                DrawDefaultInspector(); 
            }
            serializedObject.ApplyModifiedProperties();
        }

        void SortTransformByName(Transform _parent,int _indexPos = 1)
        {
            //排序
            Transform[] childTransforms = new Transform[_parent.childCount];
            for (int i = 0; i < _parent.childCount; i++)
            {
                childTransforms[i] = _parent.GetChild(i);
            }
            System.Array.Sort(childTransforms, (x, y) => 
                (int.Parse(x.name.Split("_")[_indexPos]).CompareTo( int.Parse(y.name.Split("_")[_indexPos]))));
            for (int i = 0; i < childTransforms.Length; i++)
            {
                childTransforms[i].transform.SetSiblingIndex(i);
            }
        }
        private void OnSceneGUI()
        {
            if (tempMineScale != bigMap.mineToTileScale)
            {
                Transform transf = GameObject.Find("Editor/MineTransform").transform;
                bigMap.mineToTileScale = tempMineScale;
                foreach (Transform _mine in transf)
                {
                    _mine.localScale = Vector3.one * bigMap.mineToTileScale;
                }
            }
        }
        
        

        #endregion

        
        #region ScriptObject数据处理
        
        
        private string bigMapPath = "Assets/_ProjectOC/OCResources/MineSystem/MineEditorData/BigMapData1.asset";
        private string smallMapFoldPath = "Assets/_ProjectOC/OCResources/MineSystem/MineEditorData";
        private string smallMapDataNamePre = "SmallMapEditData_";
        
        void CreateScriptableObjectAsset<T>(T asset,string _path,string _assetName) where T : ScriptableObject
        {
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(_path + "/"+ _assetName+ ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            // EditorUtility.FocusProjectWindow();
            // Selection.activeObject = asset;
        }



        void ReGenerateAsset()
        {
            GenerateBigMapPrefab();
            
            for (int i = 0; i < bigMap.SmallMapEditDatas.Count; i++)
            {
                GenerateSmallMapPrefab(i);
            }
        }

        private string bigMapDataJson = "Assets/_ProjectOC/OCResources/Json/TableData/WorldMap.json";
        private string bigMapPrefabPath = "Assets/_ProjectOC/OCResources/MineSystem/Prefabs/UIPrefab/Prefab_MineSystem_UI_BigMap_copy.prefab";
        private string smallMapTexPath = "Assets/_ProjectOC/OCResources/MineSystem/Texture2D/SmallMapTex";
        
        
        void GenerateBigMapPrefab()
        {
            //读取二维Json数据和Prefab
            string _jsonData = File.ReadAllText(bigMapDataJson);
            GameObject _prefabData = GameObject.Instantiate( AssetDatabase.LoadAssetAtPath<GameObject>(bigMapPrefabPath));
            GameObject _regionTemplate = _prefabData.transform.Find("BlockRegion/MapRegion_Block").gameObject;
            Transform normalRegionTransf = _prefabData.transform.Find("NormalRegion");
            TileMap.DestroyTransformChild(normalRegionTransf);
            
            //修改部分: 生成对应的Tex和 Collider2D
            ProcessMapJsonData(_jsonData);
            // _prefabData.transform.SetParent(GameObject.Find("Canvas").transform);
            //PrefabUtility.ApplyPrefabInstance(_prefabData, InteractionMode.UserAction);
            //DestroyImmediate(_prefabData);
            
            int _width = 0;
            int _height = 0;
            //处理二维大地图数据
            void ProcessMapJsonData(string _data)
            {
                Dictionary<int, List<Vector2>> _regions = new Dictionary<int, List<Vector2>>();
                int[,] data = JsonConvert.DeserializeObject<int[,]>(_data);
                _width = data.GetLength(0);
                _height = data.GetLength(1);
                
 
                // for (int x = 0; x < _height; x++)
                // {
                //     for (int y = 0; y < _width; y++)
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        int label = data[x, y];
                        if (label != -1)
                        {
                            if (!_regions.ContainsKey(label))
                            {
                                // Debug.Log($"New  {x},{y}  Label{label}");
                                _regions[label] = new List<Vector2>();
                            }
                            
                            _regions[label].Add(new Vector2(x, y));
                        }
                    }
                }

                foreach (var _region in _regions)
                {
                    CreateSpriteForRegion(_region.Key, _region.Value);
                }
                SortTransformByName(normalRegionTransf);

            }

            //处理每个小区域数据
            void CreateSpriteForRegion(int _lable, List<Vector2> _positions)
            {
                if (_lable == -1)       //空白区
                {
                    return;
                }

                Texture2D texture = new Texture2D(_width, _height);
                Color[] pixels = new Color[_width * _height];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.clear;
                }
                texture.SetPixels(pixels); 
                
                //左下角为起点，右上为终点
                foreach (var pos in _positions)
                {
                    // texture.SetPixel((int)pos.x, (int)pos.y, Color.white);
                    texture.SetPixel((int)pos.y, _width-1-(int)pos.x, Color.white);
                }
                texture.Apply();
                string PATH = $"{smallMapTexPath}/Tex_MineBigMap_{_lable}.png";
                SaveTextureAsPNG(texture,PATH);
                SetSpriteTextureAsset(PATH);
                Sprite _sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PATH);
                
                GameObject newPrefab = null;
                
                if(_lable == 0)    //石头区域
                {
                    newPrefab = _regionTemplate;
                    Image _uiImage = newPrefab.GetComponent<Image>();
                    _uiImage.sprite = _sprite;
                }
                else
                {
                    newPrefab = Instantiate(_regionTemplate);
                    newPrefab.name = $"MapRegion_{_lable}";
                    Image _uiImage = newPrefab.GetComponent<Image>();
                    _uiImage.sprite = _sprite;
                    newPrefab.transform.SetParent(normalRegionTransf);
                    (newPrefab.transform as RectTransform).anchoredPosition = Vector2.zero;
                    
                    
                    
                    // float _randomValue = Random.Range(0f,1f);
                    // Color randomColor = new Color(_randomValue, _randomValue, _randomValue);
                    Color randomColor = Color.HSVToRGB(Random.Range(0f,1f),Random.Range(0f,0.3f),Random.Range(0.15f,0.9f));
                    _uiImage.color = randomColor;

                }
            }
        }

        
        
        void GenerateSmallMapPrefab(int _index)
        {
            MineSmallMapEditData _smallMapEditData = bigMap.SmallMapEditDatas[_index];
            Texture2D _resTex = CreateTextureFromData(_smallMapEditData.gridData, _smallMapEditData.width,
                _smallMapEditData.height);
            string PATH = $"{smallMapTexPath}/Tex_MineSmallMap_{_index}.png";
            SaveTextureAsPNG(_resTex,PATH);
            SetSpriteTextureAsset(PATH);
        }
        public Texture2D CreateTextureFromData(bool[] texData, int texWidth, int texHeight)
        {
            if (texData.Length != texWidth * texHeight)
            {
                Debug.LogError("texData length does not match the specified width and height.");
                return null;
            }

            Texture2D texture = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);

            for (int y = 0; y < texHeight; y++)
            {
                for (int x = 0; x < texWidth; x++)
                {
                    int index = y * texWidth + x;
                    Color color = (texData[x +y*texWidth]? bigMap.dataTileColor:bigMap.emptyTileColor);
                    texture.SetPixel(x, y, color);
                }
            }

            texture.filterMode = FilterMode.Point;
            
            texture.Apply();
            return texture;
        }
        public void SaveTextureAsPNG(Texture2D texture, string path)
        {
            byte[] pngData = texture.EncodeToPNG();
            if (pngData != null)
            {
                File.WriteAllBytes(path, pngData);
                // Debug.Log("Texture saved to " + path);
            }
            else
            {
                Debug.LogError("Failed to encode texture to PNG.");
            }
        }
        public void SetSpriteTextureAsset(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
        }

        #endregion

    }
}