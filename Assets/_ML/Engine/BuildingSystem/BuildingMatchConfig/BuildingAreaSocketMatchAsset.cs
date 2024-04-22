using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace ML.Engine.BuildingSystem.Config
{
    [CreateAssetMenu(fileName = "AreaSocketMatchConfig", menuName = "ML/BuildingSystem/AreaSocketMatchConfig", order = 1)]
    public sealed class BuildingAreaSocketMatchAsset : SerializedScriptableObject
    {
        /// <summary>
        /// [socket][area]
        /// </summary>
        [SerializeField]
        public bool[,] configs;

        public bool IsMatch(BuildingSocket.BuildingSocketType socket, BuildingArea.BuildingAreaType area)
        {
            if(socket == 0 || area == 0 || (int)socket > configs.GetLength(0) || (int)area > configs.GetLength(1))
            {
                return false;
            }
            return configs[(int)socket - 1, (int)area - 1];
        }
    }

    #if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(BuildingAreaSocketMatchAsset))]
    public class BuildingAreaSocketMatchAssetEditor : Sirenix.OdinInspector.Editor.OdinEditor
    {
        private BuildingAreaSocketMatchAsset editortarget;


        private BuildingSocket.BuildingSocketType[] sockets;
        private BuildingArea.BuildingAreaType[] areas;

        protected override void OnEnable()
        {
            base.OnEnable();
            editortarget = (BuildingAreaSocketMatchAsset)base.target;

            sockets = Enum.GetValues(typeof(BuildingSocket.BuildingSocketType))
                .Cast<BuildingSocket.BuildingSocketType>()
                .Where(x => x != BuildingSocket.BuildingSocketType.None)
                .ToArray();

            areas = Enum.GetValues(typeof(BuildingArea.BuildingAreaType))
                .Cast<BuildingArea.BuildingAreaType>()
                .Where(x => x != BuildingArea.BuildingAreaType.None)
                .ToArray();

            var b = new bool[sockets.Length, areas.Length];
            // �� configs ���� b ��Ӧ�� [i, j] ���ݿ����� b ��
            if (editortarget.configs != null)
            {
                for (int i = 0; i < Math.Min(b.GetLength(0), editortarget.configs.GetLength(0)); ++i)
                {
                    for (int j = 0; j < Math.Min(b.GetLength(1), editortarget.configs.GetLength(1)); ++j)
                    {
                        // ��ȡ��ǰ�������ֵ������ֵ�� b ����
                        b[i, j] = editortarget.configs[i, j];
                    }
                }
            }
            editortarget.configs = b;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            // ��ʾ����
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("������������", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            //GUIStyle style = new GUIStyle();
            //style.alignment = TextAnchor.MiddleRight;
            //style.normal.textColor = new Color(1, 1, 1, 1);

            // ��ʾö�ٱ�ǩ
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(200); // �ճ���ǩ��λ��
            for (int j = 0; j < areas.Length; ++j)
            {
                //var rect = GUILayoutUtility.GetRect(0, 80);
                //DrawBorder(rect, Color.white, 1);
                //var pos = rect.position;
                //pos.x += 10;
                //pos.y += 90;
                //EditorGUIUtility.RotateAroundPivot(90, pos);
                //EditorGUILayout.LabelField(areas[j].ToString(), style, GUILayout.Height(20), GUILayout.Width(150));
                //EditorGUIUtility.RotateAroundPivot(-90, pos);
                EditorGUILayout.LabelField(areas[j].ToString(), GUILayout.Width(80));
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(); // ��ֱ����
            for (int i = 0; i < sockets.Length; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(sockets[i].ToString(), GUILayout.Width(200));
                for (int j = 0; j < areas.Length; ++j)
                {
                    editortarget.configs[i, j] =
                        EditorGUILayout.Toggle(editortarget.configs[i, j], GUILayout.Width(80));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            // Ӧ���޸�
            if (GUI.changed)
            {
                EditorUtility.SetDirty(editortarget);
            }
        }


        public static void DrawBorder(Rect rect, Color borderColor, int borderWidth)
        {
            // ������߿�
            GUI.DrawTexture(new Rect(rect.x, rect.y, borderWidth, rect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, borderColor, 0, 0);

            // �����ұ߿�
            GUI.DrawTexture(new Rect(rect.x + rect.width - borderWidth, rect.y, borderWidth, rect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, borderColor, 0, 0);

            // �����ϱ߿�
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, borderWidth), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, borderColor, 0, 0);

            // �����±߿�
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - borderWidth, rect.width, borderWidth), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, borderColor, 0, 0);
        }
    }
#endif

}
