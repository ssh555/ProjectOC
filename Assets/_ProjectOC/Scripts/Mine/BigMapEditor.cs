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
        
        if (GUILayout.Button("+������ͼ"))
        {
            //����ͼƬ������
        }
        controlRect = EditorGUILayout.GetControlRect(true, 20);
        bigMap.mineScale = int.Parse(EditorGUI.TextField(controlRect,"������",  bigMap.mineScale.ToString()));
        
        GUILayout.Space(20);
        GUILayout.Label("MapSmart��", blackLabelStyle);
        //todo List<TileMap>
        //todo ѡ�е�ʱ������Tile ����ͼƬ
        if (GUILayout.Button("+�½�С��ͼ"))
        {
            //�½�
        }
        
        GUILayout.Space(20);
        GUILayout.Label("��ˢ��", blackLabelStyle);
        //ѡ����ʲ�
        controlRect = EditorGUILayout.GetControlRect(true, 20);
        bigMap.mineID = EditorGUI.TextField(controlRect,"ID:", bigMap.mineID);
        bigMap.mineTex = EditorGUILayout.ObjectField("��ˢͼ��:",bigMap.mineTex,typeof(Texture),false) as Texture;
        
        
        if (GUILayout.Button("+�½���ˢ��"))
        {
            //�½���ˢ��洢ΪBrushData
        }
    }
}
