using Animancer.Editor;
using ML.Engine.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(SlotClipTransitionAsset), true)]
    public class SlotClipTransitionAssetEditor : AnimationAssetBaseEditor
    {
        protected EventTrack eventTrack;

        protected SlotClipTransitionAsset asset;
        // ����Property
        protected SerializedProperty _clipProperty;
        // Slot
        protected SerializedProperty _slotProperty;
        // �¼�
        protected SerializedProperty _endEventProperty;

        public override void Init()
        {
            asset = (SlotClipTransitionAsset)target;
            eventTrack = new EventTrack(asset);
            var p = serializedObject.FindProperty("transition");
            _clipProperty = p.FindPropertyRelative("_Clip");
            _slotProperty = p.FindPropertyRelative("_slot");
            _endEventProperty = serializedObject.FindProperty("_EndEvent");
        }


        public override void DrawTrackGUI()
        {
            eventTrack.DrawTrackGUI();
        }

        private bool bShowFadeDuration = true;
        private bool bStartTime = true;
        private bool bEndTime = true;


        public override void DrawInEditorWindow()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            // ����ѡ��
            EditorGUILayout.PropertyField(_clipProperty, new GUIContent("����Ƭ��"), true);
            // AvatarMask
            EditorGUILayout.PropertyField(_slotProperty, new GUIContent("���|����"), true);
            if(EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (_clipProperty.objectReferenceValue != null)
            {
                float length = asset.transition.Clip.length;
                float frameRate = asset.transition.Clip.frameRate;
                float speed = asset.transition.Speed;
                // ֡��
                DoClipFrameGUI(asset.transition.Clip);

                // Speed -> �����ٶ�
                DoSpeedGUI(asset.transition);

                #region ʱ���� -> ʹ������ʱ��
                EditorGUILayout.Space();
                DoAnimTimelineGUI(asset.transition, _endEventProperty.GetValue<IAssetHasEvents.AssetEvent>().NormalizedTime, length, frameRate);
                EditorGUILayout.Space();
                #endregion

                // Fade Duration -> ����ʱ��
                asset.transition.FadeDuration = DoFadeDurationGUI(asset.transition, length, frameRate, ref bShowFadeDuration);

                // StartTime -> ��ʼʱ��
                DoStartTimeGUI(asset.transition, length, frameRate, (float.IsNaN(speed) || speed >= 0) ? 0 : 1, ref bStartTime);

                // End Time -> ����ʱ��
                DoEndTimeGUI(_endEventProperty, length, frameRate, (float.IsNaN(speed) || speed >= 0) ? 1 : 0, ref bEndTime);
            }

            serializedObject.ApplyModifiedProperties();
        }

    }

}
