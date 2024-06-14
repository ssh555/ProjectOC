using Codice.Client.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static ML.Editor.Animation.DetailWindow;

namespace ML.Editor.Animation
{
    partial class TrackWindow
    {
        public abstract class Track
        {
            public const float TrackHeight = 40;

            /// <summary>
            /// ��ʼλ��
            /// </summary>
            public float Start = 0;
            /// <summary>
            /// ����λ��
            /// </summary>
            public float End = 60;

            /// <summary>
            /// �����ʾ����
            /// </summary>
            public string Name = "Track";

            /// <summary>
            /// �������ɫ
            /// </summary>
            public Color MainColor = new Color(0.3f, 0.3f, 0.3f, 1);
            /// <summary>
            /// ����ײ���ɫ
            /// </summary>
            public Color BotColor = new Color(0, 0.2f, 0.8f, 0.5f);
            /// <summary>
            /// ������ɫ
            /// </summary>
            public Color TextColor = new Color(1, 1, 1, 1);
            /// <summary>
            /// ������
            /// </summary>
            private List<TrackSignal> Siganls = new List<TrackSignal>();

            public Rect PaintRect { get; private set; }

            private Vector2 mousepos;
            public virtual void DrawTrackGUI()
            {
                // ����Track
                Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(TrackHeight));
                rect.x += 10;
                rect.y += 2;
                PaintRect = rect;
                // ������ɫ
                Rect mainRect = new Rect(rect.x + Start, rect.y, (Timeline.tickSpacing * Instance._timeline.timelineScale) * (End - Start), rect.height);
                EditorGUI.DrawRect(mainRect, MainColor);
                // �ײ���ɫ
                Rect botRect = new Rect(rect.x + Start, rect.y + rect.height * 0.85f, (Timeline.tickSpacing * Instance._timeline.timelineScale) * (End - Start), rect.height * 0.15f);
                EditorGUI.DrawRect(botRect, BotColor);

                // �ھ����ϻ����ַ���
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.alignment = TextAnchor.MiddleLeft;
                labelStyle.normal.textColor = TextColor;

                // ȷ���ַ����ھ��ε����Ļ���
                Rect textRect = mainRect;
                EditorGUI.LabelField(textRect, Name, labelStyle);

                // ����Signal
                foreach (var signal in Siganls.ToArray())
                {
                    signal.DrawGUI(rect);
                }

                // ��������Ҽ����
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
                {
                    bool IsContain = false;
                    foreach(var signal in Siganls)
                    {
                        if(signal.PaintRect.Contains(Event.current.mousePosition))
                        {
                            signal.ShowContextMenu();
                            IsContain = true;
                            break;
                        }
                    }
                    if(!IsContain)
                    {
                        mousepos = Event.current.mousePosition;
                        ShowContextMenu();
                    }
                }

                //if(Event.current.control && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S)
                //{
                //    SaveData();
                //}
            }

            /// <summary>
            /// �Ҽ��˵���
            /// </summary>
            private void ShowContextMenu()
            {
                // ����һ���µĲ˵�
                GenericMenu menu = new GenericMenu();

                AddItemsToMenu(menu);

                // ��ʾ�˵�
                menu.ShowAsContext();
            }

            protected virtual void AddItemsToMenu(GenericMenu menu)
            {

            }

            public T CreateSignal<T>() where T : TrackSignal
            {
                T signal = ScriptableObject.CreateInstance<T>();
                this.Siganls.Add(signal);
                signal.AttachedTrack = this;
                return signal;

            }

            public T CreateSignalOnMouse<T>() where T : TrackSignal, new()
            {
                return CreateSignal<T>(Mathf.Max((mousepos.x - 10), 0) / Instance._timeline.timelineScale / Timeline.tickSpacing / Instance._timeline.Framelength);
            }

            public T CreateSignalOnMouse<T>(out float time) where T : TrackSignal, new()
            {
                time = Mathf.Max((mousepos.x - 10), 0) / Instance._timeline.timelineScale / Timeline.tickSpacing / Instance._timeline.Framelength;
                return CreateSignal<T>();
            }

            public T CreateSignal<T>(float time) where T : TrackSignal, new()
            {
                var signal = CreateSignal<T>();
                signal.NormalizedTime = time;
                return signal;
            }

            public void DeleteSignal(TrackSignal signal)
            {
                if (this.Siganls.Remove(signal))
                {
                    signal.OnDelete();
                }
            }
        }

        /// <summary>
        /// ������
        /// </summary>
        [System.Serializable]
        public class TrackSignal : ScriptableObject, ISelection
        {
            /// <summary>
            /// �������
            /// </summary>
            public virtual string Name { get; set; } = "New Signal";

            /// <summary>
            /// λ�ڹ�����ĸ�λ��
            /// </summary>
            public virtual float NormalizedTime { get; set; } = 0;

            /// <summary>
            /// ������ɫ
            /// </summary>
            public Color MainColor = Color.white;
            /// <summary>
            /// ��ѡ��ʱ����ɫ
            /// </summary>
            public Color SelectedColor = Color.red;
            private bool IsSelected = false;

            /// <summary>
            /// �������
            /// </summary>
            public Track AttachedTrack;

            public Rect PaintRect;

            private bool isdragging = false;

            /// <summary>
            /// ����Signal
            /// </summary>
            public void DrawGUI(Rect rect)
            {
                float cursorX = rect.x + NormalizedTime * Timeline.tickSpacing * Instance._timeline.timelineScale * Instance._timeline.Framelength;
                Texture2D cursorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_ML/Editor/Animation/Window/TrackWindow/Cursor.png");
                var color = GUI.color;
                GUI.color = IsSelected ? SelectedColor : MainColor;
                PaintRect = new Rect(cursorX - 4, rect.y, 10, 30);
                GUI.DrawTexture(PaintRect, cursorTexture);
                GUI.color = color;

                if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && PaintRect.Contains(Event.current.mousePosition))
                {
                    DetailWindow.Instance.CurSelection = this;
                }
                else if(Event.current.type == EventType.MouseDown && !PaintRect.Contains(Event.current.mousePosition))
                {
                    Instance.Repaint();
                    DetailWindow.Instance.CurSelection = null;
                }

                // ��ѡ��
                if (this.IsSelected)
                {
                    Event currentEvent = Event.current;

                    // �������϶��¼�
                    if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(Event.current.mousePosition))
                    {
                        isdragging = true;
                        currentEvent.Use();
                        NormalizedTime = Mathf.Max((Event.current.mousePosition.x - AttachedTrack.PaintRect.x), 0) / Instance._timeline.timelineScale / Timeline.tickSpacing / Instance._timeline.Framelength;
                        Instance.Repaint();
                        DetailWindow.Instance.Repaint();
                    }
                    else if (isdragging && currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
                    {
                        currentEvent.Use();
                    }
                    else if (isdragging && currentEvent.type == EventType.MouseDrag)
                    {
                        NormalizedTime = Mathf.Max((Event.current.mousePosition.x - AttachedTrack.PaintRect.x), 0) / Instance._timeline.timelineScale / Timeline.tickSpacing / Instance._timeline.Framelength;
                        Instance.Repaint();
                        DetailWindow.Instance.Repaint();
                        currentEvent.Use();
                    }

                    // ����Delete��ɾ��
                    if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
                    {
                        AttachedTrack.DeleteSignal(this);
                        Instance.Repaint();
                        DetailWindow.Instance.Repaint();
                    }
                }
            }

            public virtual void DoSelectedGUI()
            {
                GUILayout.BeginVertical();

                // �༭����ʾ Name ���ԣ����ƿ��
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("����", GUILayout.Width(60));
                Name = EditorGUILayout.TextField(Name);
                GUILayout.EndHorizontal();

                // �༭����ʾ Time ���ԣ����ƿ��
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ʱ���", GUILayout.Width(60));
                NormalizedTime = EditorGUILayout.FloatField(NormalizedTime);
                GUILayout.EndHorizontal();

                // �༭����ʾ MainColor ���ԣ����ƿ��
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("������ɫ", GUILayout.Width(60));
                MainColor = EditorGUILayout.ColorField(MainColor);
                GUILayout.EndHorizontal();

                // �༭����ʾ SelectedColor ���ԣ����ƿ��
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ѡ����ɫ", GUILayout.Width(60));
                SelectedColor = EditorGUILayout.ColorField(SelectedColor);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }



            /// <summary>
            /// �Ҽ��˵���
            /// </summary>
            public void ShowContextMenu()
            {
                // ����һ���µĲ˵�
                GenericMenu menu = new GenericMenu();

                AddItemsToMenu(menu);

                // ��ʾ�˵�
                menu.ShowAsContext();
            }

            protected virtual void AddItemsToMenu(GenericMenu menu)
            {
                menu.AddItem(new GUIContent("ȡ��ѡ��"), false, () =>
                {
                    DetailWindow.Instance.CurSelection = null;
                });
                menu.AddItem(new GUIContent("ɾ��"), false, () =>
                {
                    AttachedTrack.DeleteSignal(this);
                });
            }

            /// <summary>
            /// ��ɾ��ʱ�Ļص�
            /// </summary>
            public virtual void OnDelete()
            {

            }

            public virtual void OnSelected()
            {
                IsSelected = true;
                Instance.Repaint();
            }

            public virtual void OnDeselected()
            {
                IsSelected = false;
            }
        }
    }
}

