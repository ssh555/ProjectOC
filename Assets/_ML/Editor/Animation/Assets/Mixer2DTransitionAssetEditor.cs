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

        public override void DrawInEditorWindow(EditorWindow window)
        {
            serializedObject.Update();

            base.DrawInEditorWindow(window);

            DoThresholdGraphGUI(window);

            serializedObject.ApplyModifiedProperties();
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
                scrollPositon = EditorGUILayout.BeginScrollView(scrollPositon, alwaysShowHorizontal: false, alwaysShowVertical: false, GUILayout.Height(65), GUILayout.ExpandWidth(true));

                // ��������
                int size = Weights.arraySize;
                // Ȩ��ֵ
                float[] weights = new float[size];
                float minW = float.MaxValue, maxW = float.MinValue;
                for (int i = 0; i < size; ++i)
                {
                    weights[i] = Weights.GetArrayElementAtIndex(i).floatValue;
                    minW = Mathf.Min(minW, weights[i]);
                    maxW = Mathf.Max(maxW, weights[i]);
                }
                // ����
                float timelineWidth = (maxW - minW + 2) * weightRatio * timelineScale * 5;
                // ����
                Rect area = GUILayoutUtility.GetRect(timelineWidth, 50);
                EditorGUI.DrawRect(area, backgroundColor);
                float showWidth = window.position.width;
                if (area.width <= timelineWidth)
                {
                    area = new Rect(area.center.x - timelineWidth * 0.5f, area.y, timelineWidth, 50);
                }
                // ���������
                Vector2 topcenter = area.center;
                topcenter.y = area.y;


                // ����ʱ����̶� -> ����С��
                int tickCount = Mathf.FloorToInt(area.width / (tickSpacing * timelineScale));
                float lastx = -tickInternal;
                // ��һ�λ��Ƶ�ֵ
                float lastTX = -tickTimeInternal / timelineScale - 2;
                // [-tickCount/2, +tickCount/2]
                int halftickCount = tickCount / 2;
                for (int i = -halftickCount; i <= halftickCount; ++i)
                {
                    float x = topcenter.x + i * tickSpacing * timelineScale;
                    if (x - lastx < tickInternal)
                    {
                        continue;
                    }
                    float height = Mathf.Lerp(minTickHeight, maxTickHeight, timelineScale);

                    if (x - lastTX >= tickTimeInternal)
                    {
                        GUI.Label(new Rect(x, topcenter.y + area.height - height - 10, 50, 20), (i * 0.1f).ToString("F1"));
                        timelineColor = Color.white;
                        lastTX = x;
                    }
                    else
                    {
                        height *= 0.5f;
                        timelineColor = new Color(1, 1, 1, 0.7f);
                    }
                    EditorGUI.DrawRect(new Rect(x, topcenter.y + area.height - height, 1, height), timelineColor);
                    lastx = x;
                }

                // ����ʱ������ -> Ĭ��ֵ
                float cursorX = topcenter.x + DefaultValue.floatValue * tickSpacing * timelineScale * 10;
                if (isInitScroll == false)
                {
                    isInitScroll = true;
                    scrollPositon.x = cursorX + (timelineWidth - showWidth) * 0.5f;
                }
                Rect cursorRect = new Rect(cursorX - 4, topcenter.y, 10, 30);
                Texture2D cursorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_ML/Editor/Animation/Window/TrackWindow/Cursor.png");
                timelineColor = GUI.color;
                GUI.color = selected == -1 ? Color.red : Color.white;
                GUI.DrawTexture(cursorRect, cursorTexture);
                // ��ѡ�У�����ʾ��ǰֵ
                if (selected == -1)
                {
                    GUI.color = Color.white;
                    Rect rect = new Rect(cursorX, topcenter.y, 30, 10);
                    EditorGUI.LabelField(rect, (DefaultValue.floatValue).ToString("F2"));
                }
                GUI.color = timelineColor;

                // ����Ȩ��ֵ
                Texture2D weightTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_ML/Editor/Animation/Window/TrackWindow/TriangleCursor.png");
                Rect[] weightRects = new Rect[size];
                for (int i = 0; i < size; ++i)
                {
                    float weightX = topcenter.x + Weights.GetArrayElementAtIndex(i).floatValue * tickSpacing * timelineScale * 10;
                    Rect rect = new Rect(weightX - 4, topcenter.y, 10, 20);
                    weightRects[i] = rect;
                    timelineColor = GUI.color;
                    GUI.color = selected == i ? Color.red : Color.white;
                    GUI.DrawTexture(rect, weightTexture);
                    // ��ѡ�У�����ʾ��ǰֵ
                    if (selected == i)
                    {
                        GUI.color = Color.white;
                        rect = new Rect(weightX, topcenter.y, 30, 10);
                        EditorGUI.LabelField(rect, (Weights.GetArrayElementAtIndex(i).floatValue).ToString("F2"));
                    }
                    GUI.color = timelineColor;
                }

                // ��������������
                if (Event.current.type == EventType.ScrollWheel)
                {
                    timelineScale -= Event.current.delta.y * 0.01f;
                    timelineScale = Mathf.Max(0.1f, timelineScale);
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
                        DefaultValue.floatValue = (mpos.x - topcenter.x) / timelineScale / tickSpacing / 10;
                        window.Repaint();
                        currentEvent.Use();
                    }
                    // �϶�Ȩ��ֵ
                    else if (selected >= 0)
                    {
                        Weights.GetArrayElementAtIndex(selected).floatValue = (mpos.x - topcenter.x) / timelineScale / tickSpacing / 10;
                        window.Repaint();
                        currentEvent.Use();
                    }
                }

                EditorGUILayout.EndScrollView();
            }

        }
    }
}

