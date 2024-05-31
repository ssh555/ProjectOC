using ML.Engine.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(ClipTransitionAsset), true)]
    public class ClipTransitionAssetEditor : AnimationAssetBaseEditor
    {
        protected EventTrack eventTrack;

        protected ClipTransitionAsset asset;
        // 动画Property
        protected SerializedProperty _clipProperty;

        public override void Init()
        {
            asset = (ClipTransitionAsset)target;
            eventTrack = new EventTrack(asset);
            _clipProperty = serializedObject.FindProperty("clipTransition").FindPropertyRelative("_Clip");
        }

        public override void DrawTrackGUI()
        {
            eventTrack.DrawTrackGUI();
        }

        public override void DrawInEditorWindow()
        {
            serializedObject.Update();

            // 动画选择
            EditorGUILayout.PropertyField(_clipProperty, new GUIContent("动画片段"), true);

            if(_clipProperty.objectReferenceValue != null)
            {
                EditorGUILayout.LabelField("QWQ");
                // Fade Duration -> 过渡时间

                // Speed -> 播放速度

                // StartTime -> 开始时间

                // End Time -> 结束时间

                // End Callback -> OnEnd 结束事件回调
            }

            serializedObject.ApplyModifiedProperties();
        }



    }

}
