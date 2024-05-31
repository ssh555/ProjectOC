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
        // ����Property
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

            // ����ѡ��
            EditorGUILayout.PropertyField(_clipProperty, new GUIContent("����Ƭ��"), true);

            if(_clipProperty.objectReferenceValue != null)
            {
                EditorGUILayout.LabelField("QWQ");
                // Fade Duration -> ����ʱ��

                // Speed -> �����ٶ�

                // StartTime -> ��ʼʱ��

                // End Time -> ����ʱ��

                // End Callback -> OnEnd �����¼��ص�
            }

            serializedObject.ApplyModifiedProperties();
        }



    }

}
