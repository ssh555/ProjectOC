using ML.Engine.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Animancer.Editor;
using UnityEditor.VersionControl;
using System.Linq;

namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(ClipTransitionAsset), true)]
    public class ClipTransitionAssetEditor : AnimationAssetBaseEditor
    {
        protected EventTrack eventTrack;

        protected ClipTransitionAsset asset;
        // ¶¯»­Property
        protected SerializedProperty _clipProperty;
        // ÊÂ¼þ
        protected SerializedProperty _endEventProperty;

        public override void Init()
        {
            asset = (ClipTransitionAsset)target;
            eventTrack = new EventTrack(asset);
            var p = serializedObject.FindProperty("transition");
            _clipProperty = p.FindPropertyRelative("_Clip");
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
            DoClipTransitionGUI(asset.transition, _clipProperty, _endEventProperty, ref bShowFadeDuration, ref bStartTime, ref bEndTime);
            serializedObject.ApplyModifiedProperties();
        }



    }

}
