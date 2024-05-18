using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Animancer.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MineSystem
{
    [CustomEditor(typeof(BigMap))]
    public class BigMapEditor : OCCustomEditor
    {
        private BigMap bigMap;
        private Rect controlRect;
        
        
        
        public override void OnEnable()
        {
            base.OnEnable();
            bigMap = target as BigMap;
            bigMap.tileMaps = target.GetComponentsInChildren<TileMap>().ToList();
            
            //��ȡ���ͼ��С��ͼ�����ʲ�
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

            if (GUILayout.Button("+������ͼ"))
            {
                //����ͼƬ������
                
            }

            controlRect = EditorGUILayout.GetControlRect(true, 20);
            bigMap.mineScale = int.Parse(EditorGUI.TextField(controlRect, "������", bigMap.mineScale.ToString()));

            GUILayout.Space(20);
            GUILayout.Label("MapSmart��", blackLabelStyle);
            //todo List<TileMap>
            //todo ѡ�е�ʱ������Tile ����ͼƬ
            if (GUILayout.Button("+�½�С��ͼ"))
            {
                int _index = bigMap.SmallMapEditDatas.Count;
                MineSmallMapEditData _smallMapData = ScriptableObject.CreateInstance<MineSmallMapEditData>();
                CreateScriptableObjectAsset(_smallMapData, smallMapFoldPath, $"{smallMapDataNamePre}{_index}");
                bigMap.SmallMapEditDatas.Add(_smallMapData);
                
                GameObject _go = new GameObject($"MapSmart_{_index}");
                _go.transform.SetParent(bigMap.transform);
                TileMap _tileMap = _go.AddComponent<TileMap>();
                _tileMap.SmallMapEditData = _smallMapData;
            }

            GUILayout.Space(20);
            GUILayout.Label("��ˢ��", blackLabelStyle);
            //ѡ����ʲ�
            controlRect = EditorGUILayout.GetControlRect(true, 20);
            bigMap.mineID = EditorGUI.TextField(controlRect, "ID:", bigMap.mineID);
            bigMap.mineTex = EditorGUILayout.ObjectField("��ˢͼ��:", bigMap.mineTex, typeof(Sprite), false) as Sprite;


            if (GUILayout.Button("+�½���ˢ��"))
            {
                bool valid = true;
                //�½���ˢ��洢ΪBrushData
                foreach (var _mineBrushData in bigMap.BigMapEditDatas.MineBrushDatas)
                {
                    if (_mineBrushData.mineID == bigMap.mineID)
                    {
                        valid = false;
                        Debug.LogError("�±�ˢ����ID�ظ�");
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
        }

        #region ScriptObject���ݴ���
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