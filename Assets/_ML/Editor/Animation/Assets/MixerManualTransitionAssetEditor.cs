using ML.Engine.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;

namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(MixerManualTransitionAsset), true)]
    public class MixerManualTransitionAssetEditor : AnimationAssetBaseEditor
    {
        protected EventTrack eventTrack;

        protected MixerManualTransitionAsset asset;
        // �¼�
        protected SerializedProperty _endEventProperty;
        // [Animation Speed Weight Sync]
        private ReorderableList _AnimationList;
        public override void Init()
        {
            //var states = new ReorderableList(CurrentAnimations.serializedObject, CurrentAnimations)
            //{
            //    drawHeaderCallback = DoChildListHeaderGUI,
            //    elementHeightCallback = GetElementHeight,
            //    drawElementCallback = DoElementGUI,
            //    onAddCallback = OnAddElement,
            //    onRemoveCallback = OnRemoveElement,
            //    onReorderCallbackWithDetails = OnReorderList,
            //    drawFooterCallback = DoChildListFooterGUI,
            //};
            //states.serializedProperty = CurrentAnimations;


            asset = (MixerManualTransitionAsset)target;
            eventTrack = new EventTrack(asset);
            var p = serializedObject.FindProperty("transition");
            _endEventProperty = serializedObject.FindProperty("_EndEvent");
        }


        public override void DrawTrackGUI()
        {
            eventTrack.DrawTrackGUI();
        }

        private bool bShowFadeDuration = true;
        private bool bEndTime = true;
        public override void DrawInEditorWindow()
        {
            serializedObject.Update();

            float length = asset.Length;
            float frameRate = asset.FrameRate;
            float speed = asset.transition.Speed;

            // �ٶ�
            // Speed -> �����ٶ�
            DoSpeedGUI(asset.transition);

            #region ʱ���� -> ʹ������ʱ��
            EditorGUILayout.Space();
            DoAnimTimelineGUI(asset.transition, asset.EndEvent.NormalizedTime, length, frameRate);
            EditorGUILayout.Space();
            #endregion

            // ����ʱ��
            // Fade Duration -> ����ʱ��
            asset.transition.FadeDuration = DoFadeDurationGUI(asset.transition, length, frameRate, ref bShowFadeDuration);

            // ����ʱ��
            DoEndTimeGUI(_endEventProperty, length, frameRate, (float.IsNaN(speed) || speed >= 0) ? 1 : 0, ref bEndTime);

            // Animations[������Speed��Weight��Sync]
            // Animation -> ObjectField
            // Speed -> Toggle + FloatField
            // Weight -> FloatField[0, 1]
            // Sync -> Toggle
            // ��֤���鳤��һ�£��Զ���Ϊ׼
            int count = asset.transition.Animations.Length;

            serializedObject.ApplyModifiedProperties();
        }


    }

}
