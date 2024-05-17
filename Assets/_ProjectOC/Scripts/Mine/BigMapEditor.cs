using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Animancer.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

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
        bigMap.mineScale = int.Parse(EditorGUI.TextField(controlRect,"比例尺",  bigMap.mineScale.ToString()));
        
        GUILayout.Space(20);
        GUILayout.Label("MapSmart项", blackLabelStyle);
        //todo List<TileMap>
        //todo 选中的时候生成Tile 或者图片
        if (GUILayout.Button("+新建小地图"))
        {
            //新建
        }
        
        GUILayout.Space(20);
        GUILayout.Label("笔刷项", blackLabelStyle);
        //选择表资产
        controlRect = EditorGUILayout.GetControlRect(true, 20);
        bigMap.mineID = EditorGUI.TextField(controlRect,"ID:", bigMap.mineID);
        bigMap.mineTex = EditorGUILayout.ObjectField("笔刷图标:",bigMap.mineTex,typeof(Texture),false) as Texture;
        
        
        if (GUILayout.Button("+新建笔刷项"))
        {
            //新建笔刷项，存储为BrushData
        }
    }
}
