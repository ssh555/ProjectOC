using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Animancer.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Serialization = Unity.VisualScripting.Serialization;

namespace MineSystem
{
    [CustomEditor(typeof(BigMap))]
    public class BigMapEditor : OCCustomEditor
    {
        private BigMap bigMap;
        private Rect controlRect;
        private SerializedProperty smallMapList;
        
        
        public override void OnEnable()
        {
            base.OnEnable();
            bigMap = target as BigMap;
            bigMap.tileMaps = target.GetComponentsInChildren<TileMap>().ToList();
            smallMapList = serializedObject.FindProperty("tileMaps");
            //读取大地图和小地图所有资产
            bigMap.BigMapEditDatas = AssetDatabase.LoadAssetAtPath<MineBigMapEditData>(bigMapPath);
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
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            StyleCheck();

            if (GUILayout.Button("+导入大地图"))
            {
                //生成图片放上来
                
            }

            controlRect = EditorGUILayout.GetControlRect(true, 20);
            bigMap.mineScale = int.Parse(EditorGUI.TextField(controlRect, "比例尺", bigMap.mineScale.ToString()));

            GUILayout.Space(20);
            GUILayout.Label("MapSmart项", blackLabelStyle);
            // EditorGUILayout.PropertyField(smallMapList, new GUIContent("小地图"), true);
            for (int i = 0; i < smallMapList.arraySize; i++)
            {
                SerializedProperty gameObjectProperty = smallMapList.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(gameObjectProperty, new GUIContent($"小地图 {i}"));

                if (GUILayout.Button($"Select Element {i}"))
                {
                    TileMap selectedGameObject = gameObjectProperty.objectReferenceValue as TileMap;
                    if (selectedGameObject != null)
                    {
                        Debug.Log($"{selectedGameObject.gameObject.name}");
                    }
                }
            }
            
            
            
            
            
            //todo 选中的时候生成Tile 或者图片
            if (GUILayout.Button("+新建小地图"))
            {
                int _index = bigMap.SmallMapEditDatas.Count;
                MineSmallMapEditData _smallMapData = ScriptableObject.CreateInstance<MineSmallMapEditData>();
                CreateScriptableObjectAsset(_smallMapData, smallMapFoldPath, $"{smallMapDataNamePre}{_index}");
                bigMap.SmallMapEditDatas.Add(_smallMapData);
                _smallMapData.index = _index;
                GameObject _go = new GameObject($"MapSmart_{_index}");
                _go.transform.SetParent(bigMap.transform);
                TileMap _tileMap = _go.AddComponent<TileMap>();
                _tileMap.SmallMapEditData = _smallMapData;
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
            
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
                EditorUtility.SetDirty(bigMap.BigMapEditDatas);
                AssetDatabase.SaveAssets();
            }
        }

        #region ScriptObject数据处理
        private string bigMapPath = "Assets/_ProjectOC/OCResources/Mine/MineEditorData/BigMapData1.asset";
        private string smallMapFoldPath = "Assets/_ProjectOC/OCResources/Mine/MineEditorData";
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
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        #endregion

    }
}