using ML.Engine.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(Mixer2DTransitionAsset), true)]
    public class Mixer2DTransitionAssetEditor : MixerParameterTransitionAssetEditor<Mixer2DTransitionAsset>
    {
        protected SerializedProperty _Type { get; set; }
        protected override float FadeDuration { get => asset.transition.FadeDuration; set => asset.transition.FadeDuration = value; }

        protected SerializedProperty DefaultValue { get; set; }
        private Timeline timeline { get; set; }

        public override void Init()
        {
            base.Init();
            var p = serializedObject.FindProperty("transition");
            _Type = p.FindPropertyRelative("_Type");
            DefaultValue = serializedObject.FindProperty("transition").FindPropertyRelative("_DefaultParameter");
            timeline = new Timeline(DefaultValue, CurrentWeights);
        }

        private Vector2 scrollPosition;
        public override void DrawInEditorWindow(EditorWindow window)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            serializedObject.Update();

            base.DrawInEditorWindow(window);

            DoThresholdGraphGUI(window);

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }

        public override void InitTransition()
        {
            transition = asset.transition;
        }

        protected void DoThresholdGraphGUI(EditorWindow window)
        {
            EditorGUILayout.PropertyField(DefaultValue, new GUIContent("Ĭ�ϲ���"));
            timeline.DrawTimelineGUI(window);
        }


        internal class Timeline
        {
            public Timeline(SerializedProperty defaultValue, SerializedProperty weights)
            {
                Weights = weights;
                DefaultValue = defaultValue;
            }

            protected SerializedProperty Weights { get; set; }
            protected SerializedProperty DefaultValue { get; set; }


            /// <summary>
            /// Ȩ����ֵ1��Ӧ���Ƶı���
            /// </summary>
            public const float weightRatio = 50;

            /// <summary>
            /// ʱ��������
            /// </summary>
            public Vector2 timelineScale = new Vector2(1, 1);

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

            private Color backgroundColor = new Color(0.2f, 0.8f, 0.6f, 0.4f);
            private Color timelineColor = new Color(0, 0.5f, 0.8f, 0.5f);

            private bool isInitScroll = false;
            private Vector2 scrollPositon = Vector2.zero;

            /// <summary>
            /// -2 -> None
            /// -1 -> Ĭ��ֵ
            /// >0 -> Ȩ��ֵ
            /// </summary>
            private int selected = -2;

            public void DrawTimelineGUI(EditorWindow window)
            {
                #region ��ʼ��������
                // ��������
                int size = Weights.arraySize;
                // Ȩ��ֵ
                Vector2[] weights = new Vector2[size];
                Vector2 minW = Vector2.one * float.MaxValue, maxW = Vector2.one * float.MinValue;
                for (int i = 0; i < size; ++i)
                {
                    weights[i] = Weights.GetArrayElementAtIndex(i).vector2Value;
                    minW.x = Mathf.Min(minW.x, weights[i].x);
                    maxW.x = Mathf.Max(maxW.x, weights[i].x);
                    minW.y = Mathf.Min(minW.y, weights[i].y);
                    maxW.y = Mathf.Max(maxW.y, weights[i].y);
                }
                // ����
                Vector2 timelineSize = (maxW - minW + Vector2.one * 2) * weightRatio * timelineScale * 5;

                scrollPositon = EditorGUILayout.BeginScrollView(scrollPositon, alwaysShowHorizontal: false, alwaysShowVertical: false, GUILayout.Height(window.position.width), GUILayout.ExpandWidth(true));

                // ����
                Rect area = GUILayoutUtility.GetRect(timelineSize.x, timelineSize.y);
                Vector2 showSize = window.position.size;
                showSize.y = showSize.x;
                if (area.width <= timelineSize.x)
                {
                    area = new Rect(area.center.x - timelineSize.x * 0.5f, area.y, timelineSize.x, area.height);
                }
                if(area.height <= timelineSize.y)
                {
                    area = new Rect(area.x, area.center.y - timelineSize.y * 0.5f, area.width, timelineSize.y);
                }
                area.width = area.height = Mathf.Max(area.width, area.height);
                EditorGUI.DrawRect(area, backgroundColor);
                // �������
                Vector2 center = area.center;

                #endregion

                #region ˮƽʱ����̶�
                int tickCount = Mathf.FloorToInt(area.width / (tickSpacing * timelineScale.x));
                float lastx = -tickInternal;
                // ��һ�λ��Ƶ�ֵ
                float lastTX = -tickTimeInternal / timelineScale .x- 2;
                // [-tickCount/2, +tickCount/2]
                int halftickCount = tickCount / 2;
                for (int i = -halftickCount; i <= halftickCount; ++i)
                {
                    float x = center.x + i * tickSpacing * timelineScale.x;
                    if (x - lastx < tickInternal)
                    {
                        continue;
                    }
                    float height = Mathf.Lerp(minTickHeight, maxTickHeight, timelineScale.x);

                    if (x - lastTX >= tickTimeInternal)
                    {
                        GUI.Label(new Rect(x, center.y - height - 10, 50, 20), (i * 0.1f).ToString("F1"));
                        timelineColor = Color.white;
                        lastTX = x;
                    }
                    else
                    {
                        height *= 0.5f;
                        timelineColor = new Color(1, 1, 1, 0.7f);
                    }
                    EditorGUI.DrawRect(new Rect(x, center.y - height, 1, height), timelineColor);
                    lastx = x;
                }

                #endregion

                #region ��ֱʱ����̶�
                tickCount = Mathf.FloorToInt(area.width / (tickSpacing * timelineScale.y));
                float lasty = -tickInternal;
                // ��һ�λ��Ƶ�ֵ
                float lastTY = -tickTimeInternal / timelineScale.y - 2;
                // [-tickCount/2, +tickCount/2]
                halftickCount = tickCount / 2;
                for (int i = -halftickCount; i <= halftickCount; ++i)
                {
                    float y = center.y + i * tickSpacing * timelineScale.y;
                    if (y - lasty < tickInternal)
                    {
                        continue;
                    }
                    float width = Mathf.Lerp(minTickHeight, maxTickHeight, timelineScale.y);

                    if (y - lastTY >= tickTimeInternal)
                    {
                        GUI.Label(new Rect(center.x + 10, y, 50, 20), (i * 0.1f).ToString("F1"));
                        timelineColor = Color.white;
                        lastTY = y;
                    }
                    else
                    {
                        width *= 0.5f;
                        timelineColor = new Color(1, 1, 1, 0.7f);
                    }
                    EditorGUI.DrawRect(new Rect(center.x, y, width, 1), timelineColor);
                    lasty = y;
                }


                #endregion

                #region Ȩ��ֵ|Ĭ��ֵ
                // ����ʱ������ -> Ĭ��ֵ
                Vector2 cursor = center + DefaultValue.vector2Value * tickSpacing * timelineScale * 10;
                if (isInitScroll == false)
                {
                    isInitScroll = true;
                    timelineScale = (maxW - minW + Vector2.one);
                    timelineScale.x = 1 / timelineScale.x;
                    timelineScale.y = 1 / timelineScale.y;
                    timelineSize = (maxW - minW + Vector2.one * 2) * weightRatio * timelineScale * 5;
                    scrollPositon = cursor;// + (timelineSize - showSize) * 0.5f;
                    scrollPositon.x += (Mathf.Max(timelineSize.x, timelineSize.y) - showSize.x) * 0.5f;
                    scrollPositon.y += (Mathf.Max(timelineSize.x, timelineSize.y) - showSize.y) * 0.5f;

                }
                Texture2D cursorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_ML/Editor/Animation/Window/TrackWindow/SquareCursor.png");
                Rect cursorRect = new Rect(cursor.x - 20 * Mathf.Max(timelineScale.x, timelineScale.y) * 0.5f, cursor.y - 20 * Mathf.Max(timelineScale.x, timelineScale.y) * 0.5f, 20 * Mathf.Max(timelineScale.x, timelineScale.y), 20 * Mathf.Max(timelineScale.x, timelineScale.y));
                timelineColor = GUI.color;
                GUI.color = selected == -1 ? Color.red : Color.white;
                GUI.DrawTexture(cursorRect, cursorTexture);
                // ��ѡ�У�����ʾ��ǰֵ
                if (selected == -1)
                {
                    GUI.color = Color.white;
                    Rect rect = new Rect(cursorRect.x + cursorRect.width, cursorRect.y - cursorRect.height, 100, 50);
                    EditorGUI.LabelField(rect, (DefaultValue.vector2Value).ToString("F2"));
                }
                GUI.color = timelineColor;

                // ����Ȩ��ֵ
                Texture2D weightTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_ML/Editor/Animation/Window/TrackWindow/CircleCursor.png");
                Rect[] weightRects = new Rect[size];
                for (int i = 0; i < size; ++i)
                {
                    Vector2 weight = center + Weights.GetArrayElementAtIndex(i).vector2Value * tickSpacing * timelineScale * 10;
                    Rect rect = new Rect(weight.x - 20 * Mathf.Max(timelineScale.x, timelineScale.y) * 0.5f, weight.y - 20 * Mathf.Max(timelineScale.x, timelineScale.y) * 0.5f, 20 * Mathf.Max(timelineScale.x, timelineScale.y), 20 * Mathf.Max(timelineScale.x, timelineScale.y));
                    weightRects[i] = rect;
                    timelineColor = GUI.color;
                    GUI.color = selected == i ? Color.red : Color.white;
                    GUI.DrawTexture(rect, weightTexture);
                    // ��ѡ�У�����ʾ��ǰֵ
                    if (selected == i)
                    {
                        GUI.color = Color.white;
                        rect = new Rect(rect.x + rect.width, rect.y - rect.height, 100, 50);
                        EditorGUI.LabelField(rect, (Weights.GetArrayElementAtIndex(i).vector2Value).ToString("F2"));
                    }
                    GUI.color = timelineColor;
                }
                #endregion

                #region ������Ӧ�¼�
                // ��������������
                if (Event.current.type == EventType.ScrollWheel)
                {
                    // ��סAltΪ��ֱ
                    if (Event.current.alt)
                    {
                        timelineScale.y -= Event.current.delta.y * 0.01f;
                        timelineScale.y = Mathf.Max(0.1f, timelineScale.y);
                    }
                    // ��סCtrlΪˮƽ
                    else if (Event.current.control)
                    {
                        timelineScale.x -= Event.current.delta.y * 0.01f;
                        timelineScale.x = Mathf.Max(0.1f, timelineScale.x);
                    }
                    window.Repaint();
                }

                Event currentEvent = Event.current;
                var mpos = Event.current.mousePosition;

                // �������϶��¼�
                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                {
                    if (cursorRect.Contains(mpos))
                    {
                        selected = -1;
                        currentEvent.Use();
                        window.Repaint();
                    }
                    else
                    {
                        for (int i = 0; i < size; ++i)
                        {
                            if (weightRects[i].Contains(mpos))
                            {
                                selected = i;
                                currentEvent.Use();
                                window.Repaint();
                            }
                        }
                    }
                }
                else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
                {
                    selected = -2;
                    currentEvent.Use();
                }
                else if (currentEvent.type == EventType.MouseDrag)
                {
                    // �϶�Ĭ��ֵ
                    if (selected == -1)
                    {
                        DefaultValue.vector2Value = (mpos - center) / timelineScale / tickSpacing / 10;
                        window.Repaint();
                        currentEvent.Use();
                    }
                    // �϶�Ȩ��ֵ
                    else if (selected >= 0)
                    {
                        Weights.GetArrayElementAtIndex(selected).vector2Value = (mpos - center) / timelineScale / tickSpacing / 10;
                        window.Repaint();
                        currentEvent.Use();
                    }
                }

                // ˫��������ͼ
                if (currentEvent.type == EventType.MouseDown && area.Contains(mpos) && currentEvent.clickCount >= 2)
                {
                    isInitScroll = false;
                }

                #endregion

                EditorGUILayout.EndScrollView();
            }

        }
    }
}

