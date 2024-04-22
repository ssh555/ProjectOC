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
            // 将 configs 中与 b 对应的 [i, j] 数据拷贝到 b 中
            if (editortarget.configs != null)
            {
                for (int i = 0; i < Math.Min(b.GetLength(0), editortarget.configs.GetLength(0)); ++i)
                {
                    for (int j = 0; j < Math.Min(b.GetLength(1), editortarget.configs.GetLength(1)); ++j)
                    {
                        // 获取当前配置项的值，并赋值给 b 数组
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
            // 显示配置
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("建筑点面配置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            //GUIStyle style = new GUIStyle();
            //style.alignment = TextAnchor.MiddleRight;
            //style.normal.textColor = new Color(1, 1, 1, 1);

            // 显示枚举标签
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(200); // 空出标签的位置
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
            EditorGUILayout.BeginVertical(); // 垂直布局
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

            // 应用修改
            if (GUI.changed)
            {
                EditorUtility.SetDirty(editortarget);
            }
        }


        public static void DrawBorder(Rect rect, Color borderColor, int borderWidth)
        {
            // 绘制左边框
            GUI.DrawTexture(new Rect(rect.x, rect.y, borderWidth, rect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, borderColor, 0, 0);

            // 绘制右边框
            GUI.DrawTexture(new Rect(rect.x + rect.width - borderWidth, rect.y, borderWidth, rect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, borderColor, 0, 0);

            // 绘制上边框
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, borderWidth), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, borderColor, 0, 0);

            // 绘制下边框
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - borderWidth, rect.width, borderWidth), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, borderColor, 0, 0);
        }
    }
#endif

}
