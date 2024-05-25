using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Animancer.Editor;
using static Animancer.Editor.AnimancerGUI;

namespace ML.Editor.Animation
{
    public partial class TrackWindow : EditorWindow, IHasCustomMenu
    {
        public static TrackWindow Instance
        {
            get;
            private set;
        }

        private static GUIContent _FirstKeyButtonContent, _LastKeyButtonContent;
        public static GUIContent FirstKeyButtonContent => IconContent(ref _FirstKeyButtonContent, "Animation.FirstKey");
        public static GUIContent LastKeyButtonContent => IconContent(ref _LastKeyButtonContent, "Animation.LastKey");

        /// <summary>
        /// true => Frame
        /// false => Seconds
        /// </summary>
        public bool ShowTimeWay = true;

        /// <summary>
        /// ʱ����
        /// </summary>
        private Timeline _timeline = new Timeline();

        private void OnGUI()
        {
            if (AnimationWindow.Instance != null && AnimationWindow.Instance.AssetEditor != null)
            {
                DrawTrackMenuGUI();
            }
            else
            {
                EditorGUILayout.LabelField("û��ѡ�ж����ʲ�");
            }
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }

        private void DrawTrackMenuGUI()
        {
            var animancer = PreviewWindow.Instance.GetScene.Animancer;
            var anims = PreviewWindow.Instance.GetAnimations;

            // �������ſ���
            if (animancer != null)
            {
                using (new EditorGUI.DisabledScope(!PreviewWindow.Transition.IsValid()))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    #region TrackMenu
                    // ����һ֡
                    if (CompactMiniButton(FirstKeyButtonContent))
                        anims.StepToFirstKey();

                    // ����һ֡
                    if (CompactMiniButton(StepBackwardButtonContent))
                        anims.StepBackward();

                    // ���²��� | ��ͣ����
                    if (animancer.IsGraphPlaying)
                    {
                        // ��ͣ����
                        if (CompactMiniButton(PauseButtonContent))
                            animancer.PauseGraph();
                    }
                    else
                    {
                        if (CompactMiniButton(PlayButtonContent))
                            anims.PlaySequence(animancer);
                    }

                    // ǰ��һ֡
                    if (CompactMiniButton(StepForwardButtonContent))
                        anims.StepForward();

                    // �����һ֡
                    if (CompactMiniButton(LastKeyButtonContent))
                        anims.StepToLastKey();

                    // ��ǰ����֡��
                    // Frame
                    EditorGUI.BeginChangeCheck();
                    float num = anims.NormalizedTime * anims.Length * (ShowTimeWay ? anims.FrameRate : 1);
                    num = ShowTimeWay ? (int)num : (int)(num * 10000) * 0.0001f;
                    num = EditorGUILayout.FloatField(num, GUILayout.Width(60)) / anims.Length / (ShowTimeWay ? anims.FrameRate : 1);
                    if (EditorGUI.EndChangeCheck())
                    {
                        anims.NormalizedTime = num;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    #endregion

                    #region Track Timeline
                    // ʱ���� ��ǰCursor
                    _timeline.DrawTimelineGUI();
                    #endregion
                }

            }
        }


        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("ʱ����ʾ��ʽ/Frame"), ShowTimeWay, () =>
            {
                ShowTimeWay = true;
            });
            menu.AddItem(new GUIContent("ʱ����ʾ��ʽ/Seconds"), !ShowTimeWay, () =>
            {
                ShowTimeWay = false;
            });
        }
    }



}