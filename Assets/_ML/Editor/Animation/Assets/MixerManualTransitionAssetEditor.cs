using ML.Engine.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(MixerManualTransitionAsset), true)]
    public class MixerManualTransitionAssetEditor : AnimationAssetBaseEditor
    {
        protected EventTrack eventTrack;

        protected MixerManualTransitionAsset asset;
        // 事件
        protected SerializedProperty _endEventProperty;

        public override void Init()
        {
            asset = (MixerManualTransitionAsset)target;
            eventTrack = new EventTrack(asset);
            var p = serializedObject.FindProperty("manualMixerTransition");
            _endEventProperty = serializedObject.FindProperty("_EndEvent");
        }


        public override void DrawTrackGUI()
        {
            eventTrack.DrawTrackGUI();
        }


        public override void DrawInEditorWindow()
        {
            serializedObject.Update();


            // 速度

            // 时间轴

            // 过渡时间

            // 结束时间

            // Animations[动画、Speed、Weight]

            serializedObject.ApplyModifiedProperties();
        }


    }

}
