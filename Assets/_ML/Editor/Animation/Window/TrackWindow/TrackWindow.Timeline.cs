using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ML.Editor.Animation
{
    partial class TrackWindow
    {
        internal class Timeline 
        {
            /// <summary>
            /// ʱ����߶�
            /// </summary>
            private const float timelineHeight = 50f;
            /// <summary>
            /// ʱ����λ��
            /// </summary>
            private float timelinePosition => Framelength * NormalizedTime;
            /// <summary>
            /// ʱ��������
            /// </summary>
            public float timelineScale = 1f;
            /// <summary>
            /// ��С�̶ȸ߶�
            /// </summary>
            private const float minTickHeight = 20f;
            /// <summary>
            /// ���̶ȸ߶�
            /// </summary>
            private const float maxTickHeight = 40f;
            /// <summary>
            /// �̶ȼ��
            /// </summary>
            public const float tickSpacing = 20f;
            /// <summary>
            /// ���ź�Ŀ̶ȼ��С�ڴ�ֵ������
            /// </summary>
            private const float tickInternal = 5f;
            /// <summary>
            /// ���ź�Ŀ̶ȼ��ÿ����ֵ����ʾһ��֡��
            /// </summary>
            private const float tickTimeInternal = 50f;
            ///// <summary>
            ///// ����λ��
            ///// </summary>
            //private Vector2 scrollPosition;

            /// <summary>
            /// ������֡��
            /// </summary>
            public float FrameRate => PreviewWindow.Instance.GetAnimations.FrameRate;

            /// <summary>
            /// Ԥ���ĳ���
            /// </summary>
            public float Framelength => PreviewWindow.Instance.GetAnimations.Length * FrameRate;

            /// <summary>
            /// ���ù��λ��λ�õĹ淶������[0, 1]
            /// </summary>
            public float NormalizedTime
            {
                get => PreviewWindow.Instance.GetAnimations.NormalizedTime;
                set => PreviewWindow.Instance.GetAnimations.NormalizedTime = value;
            }

            private bool isdragging = false;
            public void DrawTimelineGUI()
            {
                //scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(timelineHeight));

                // ����ʱ���ᱳ��
                Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(timelineHeight));
                rect.x += 10;
                //EditorGUI.DrawRect(rect, Color.gray);
                // ���Ƶ�ǰʱ���᳤��
                //Rect lengthRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(false), GUILayout.Height(20), GUILayout.Width((tickSpacing * timelineScale) * framelength));
                Rect lengthRect = new Rect(rect.x, rect.y + rect.height * 0.75f, (tickSpacing * timelineScale) * Framelength, rect.height * 0.25f);
                var color = new Color(0, 0.5f, 0.8f, 0.5f);
                EditorGUI.DrawRect(lengthRect, color);

                // ����ʱ����̶�
                int tickCount = Mathf.FloorToInt(rect.width / (tickSpacing * timelineScale));
                float lastx = -tickInternal;
                // ��һ�λ���֡����ֵ
                float lastTX = -tickTimeInternal / timelineScale - 2;
                for (int i = 0; i <= tickCount; i++)
                {
                    float x = rect.x + i * tickSpacing * timelineScale;
                    if (x - lastx < tickInternal)
                    {
                        continue;
                    }
                    float height = Mathf.Lerp(minTickHeight, maxTickHeight, timelineScale);

                    if (x - lastTX >= tickTimeInternal)
                    {
                        GUI.Label(new Rect(x, rect.y + rect.height - height - 10, 50, 20), Instance.ShowTimeWay ? i.ToString() : (i / FrameRate).ToString("F3"));
                        color = Color.white;
                        lastTX = x;
                    }
                    else
                    {
                        height *= 0.5f;
                        color = new Color(1, 1, 1, 0.7f);
                    }
                    EditorGUI.DrawRect(new Rect(x, rect.y + rect.height - height, 1, height), color);
                    lastx = x;
                }

                // ����ʵ�ʹ��
                AnimationWindow.Instance.AssetEditor.DrawTrackGUI();

                // ����ʱ������
                float cursorX = rect.x + timelinePosition * tickSpacing * timelineScale;
                EditorGUI.DrawRect(new Rect(cursorX, rect.y, 2, Instance.position.height), Color.white);
                //EditorGUI.DrawRect(new Rect(cursorX - 5, rect.y, 10, 10), Color.white);
                Texture2D cursorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_ML/Editor/Animation/Window/TrackWindow/Cursor.png");
                color = GUI.color;
                GUI.color = Color.red;
                GUI.DrawTexture(new Rect(cursorX - 4, rect.y, 10, 15), cursorTexture);
                GUI.color = color;

                // ��������������
                
                //if (rect.Contains(Event.current.mousePosition))
                if (TrackWindow.Instance.hasFocus)
                {
                    if (Event.current.type == EventType.ScrollWheel)
                    {
                        timelineScale -= Event.current.delta.y * 0.01f;
                        timelineScale = Mathf.Max(0.1f, timelineScale);
                        Instance.Repaint();
                    }
                }

                Event currentEvent = Event.current;

                // �������϶��¼�
                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(Event.current.mousePosition))
                {
                    isdragging = true;
                    currentEvent.Use();
                    NormalizedTime = Mathf.Max((Event.current.mousePosition.x - rect.x), 0) / timelineScale / tickSpacing / Framelength;
                    Instance.Repaint();
                }
                else if (isdragging && currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
                {
                    currentEvent.Use();
                }
                else if (isdragging && currentEvent.type == EventType.MouseDrag)
                {
                    NormalizedTime = Mathf.Max((Event.current.mousePosition.x - rect.x), 0) / timelineScale / tickSpacing / Framelength;
                    Instance.Repaint();
                    currentEvent.Use();
                }
                //EditorGUILayout.EndScrollView();



            }

        }
    }
}

