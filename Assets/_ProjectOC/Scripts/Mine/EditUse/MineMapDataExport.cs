using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectOC.MineSystem
{
    public class MineMapDataExport : MonoBehaviour
    {
        #region 检测岛屿区域
        [Button("检测岛屿区域")]
        void CheckButtonRegion()
        {
            //Transform
            RectTransform referenceRectTransform;
            Transform island1, island2;
            referenceRectTransform = GameObject.Find("Canvas2/Prefab_MineSystem_UI_BigMap/NormalRegion").transform as RectTransform;
            island1 = GameObject.Find("Canvas2/Prefab_MineSystem_UI_BigMap/IslandPos1").transform;
            island2 = GameObject.Find("Canvas2/Prefab_MineSystem_UI_BigMap/IslandTransf/IslandPos2").transform;
            
            //策划大地图数据
            string _jsonData = File.ReadAllText(bigMapDataJson);
            int[,] data = JsonConvert.DeserializeObject<int[,]>(_jsonData);
            
            
            GetTransformPos(island1,"Island1:  ");
            GetTransformPos(island2,"Island2:  ");
            
            //从Transform转为局部坐标，从左下到右上 0,0 ->1,1
            void GetTransformPos(Transform _transf,string debugText)
            {
                Vector3 worldPosition = _transf.position;
                Vector2 localPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(referenceRectTransform, worldPosition, null, out localPosition);
                Vector2 referenceSize = referenceRectTransform.rect.size;
                
                Vector2 anchorPosition = new Vector2(localPosition.x / referenceSize.x + 0.5f, localPosition.y / referenceSize.y + 0.5f);
                anchorPosition = new Vector2(anchorPosition.x,1-anchorPosition.y);
                
                //float [0-1]  -> int[0 - width-1]
                int width = data.GetLength(0);
                Vector2Int gridPos = new Vector2Int(
                    Mathf.Clamp((int)(anchorPosition.x * (width)) , 0,width-1),
                    Mathf.Clamp((int)(anchorPosition.y * (width)) , 0,width-1));
                //注意GridPos的Y和X是一致的
                Debug.Log($"{debugText}{anchorPosition} Region:({gridPos.y},{gridPos.x})   {data[gridPos.y,gridPos.x]}");
            }

            
        }
        

        #endregion

        #region 导出数据
        List<MineSmallMapEditData> SmallMapEditDatas = new List<MineSmallMapEditData>();
        Color dataTileColor = new Color32(44, 46, 47,255);
        Color emptyTileColor = new Color32(161, 162, 166,255);
        [Button("导出数据")]
        void ReGenerateAsset()
        {
            ReloadMineData();
            
            GenerateBigMapPrefab();

            for (int i = 0; i < SmallMapEditDatas.Count; i++)
            {
                GenerateSmallMapPrefab(i);
            }
        }

        private string smallMapFoldPath = "Assets/_ProjectOC/OCResources/MineSystem/MineEditorData";
        private string bigMapDataJson = "Assets/_ProjectOC/OCResources/Json/TableData/WorldMap.json";
        private string bigMapPrefabPath =
            "Assets/_ProjectOC/OCResources/MineSystem/Prefabs/UIPrefab/Prefab_MineSystem_UI_BigMap.prefab";
        private string smallMapTexPath = "Assets/_ProjectOC/OCResources/MineSystem/Texture2D/SmallMapTex";
        private string bigMapTexPath = "Assets/_ProjectOC/OCResources/MineSystem/Texture2D/BigMapTex";
        void ReloadMineData()
        {
            string[] assetGUIDs = AssetDatabase.FindAssets("t:" + typeof(MineSmallMapEditData).Name, new[] { smallMapFoldPath });
            foreach (string guid in assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                MineSmallMapEditData _asset = AssetDatabase.LoadAssetAtPath<MineSmallMapEditData>(assetPath);
                if (_asset != null && !SmallMapEditDatas.Contains(_asset))
                {
                    SmallMapEditDatas.Add(_asset);
                }
            }

            SmallMapEditDatas.Sort((a, b) =>
                (int.Parse(a.name.Split("_")[1]).CompareTo(int.Parse(b.name.Split("_")[1]))));

        }
        void GenerateBigMapPrefab()
        {
            //读取二维Json数据和Prefab
            string _jsonData = File.ReadAllText(bigMapDataJson);
            GameObject _prefabData =
                GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(bigMapPrefabPath));
            _prefabData.name = "Prefab_MineSystem_UI_BigMap";
            GameObject _regionTemplate = _prefabData.transform.Find("BlockRegion/MapRegion_Block").gameObject;
            Transform normalRegionTransf = _prefabData.transform.Find("NormalRegion");
            TileMap.DestroyTransformChild(normalRegionTransf);

            //修改部分: 生成对应的Tex和 Collider2D
            ProcessMapJsonData(_jsonData);
            Transform _canvas = GameObject.Find("Canvas").transform;
            TileMap.DestroyTransformChild(_canvas);
            _prefabData.transform.SetParent(_canvas);
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
            //处理每个小区域数据
            void CreateSpriteForRegion(int _lable, List<Vector2> _positions)
            {
                if (_lable == -1) //空白区
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
                    texture.SetPixel((int)pos.y, _width - 1 - (int)pos.x, Color.white);
                }

                texture.Apply();
                string PATH = $"{bigMapTexPath}/Tex_MineBigMap_{_lable}.png";
                SaveTextureAsPNG(texture, PATH);
                SetSpriteTextureAsset(PATH);
                Sprite _sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PATH);

                GameObject newPrefab = null;

                if (_lable == 0) //石头区域
                {
                    newPrefab = _regionTemplate;
                }
                else
                {
                    newPrefab = Instantiate(_regionTemplate);
                    newPrefab.name = $"MapRegion_{_lable}";
                    newPrefab.transform.SetParent(normalRegionTransf);
                    (newPrefab.transform as RectTransform).anchoredPosition = Vector2.zero;
                    (newPrefab.transform as RectTransform).localScale = Vector3.one;
                    // float _randomValue = Random.Range(0f,1f);
                    // Color randomColor = new Color(_randomValue, _randomValue, _randomValue);
                }
                
                Image[] images = newPrefab.GetComponentsInChildren<Image>(true);
                foreach (var image in images)
                {
                    image.sprite = _sprite;
                    if(image.name != "Locked" && _lable != 0)
                    {
                        Color randomColor = Color.HSVToRGB(Random.Range(0f, 1f), Random.Range(0f, 0.3f),
                        Random.Range(0.15f, 0.9f));
                        image.color = randomColor;
                    }
                }
            }
        }



        void GenerateSmallMapPrefab(int _index)
        {
            MineSmallMapEditData _smallMapEditData = SmallMapEditDatas[_index];
            Texture2D _resTex = CreateTextureFromData(_smallMapEditData.gridData, _smallMapEditData.width,
                _smallMapEditData.height);
            string PATH = $"{smallMapTexPath}/Tex_MineSmallMap_{_index}.png";
            SaveTextureAsPNG(_resTex, PATH);
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
                    Color color = (texData[x + y * texWidth] ? dataTileColor : emptyTileColor);
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