using ML.Engine.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(BooleanMixerTransitionAsset), true)]
    public class BooleanMixerTransitionAssetEditor : AnimationAssetBaseEditor
    {
        protected SerializedProperty _FalseAnimationProperty { get; set; }
        protected SerializedProperty _TrueAnimationProperty { get; set; }

        protected SerializedProperty _BooleanProperty { get; set; }


        public override void Init()
        {
            _BooleanProperty = serializedObject.FindProperty("Boolean");

            _FalseAnimationProperty = serializedObject.FindProperty("falseTransition");
            _TrueAnimationProperty = serializedObject.FindProperty("trueTransition");
        }
        public override void DrawInEditorWindow(EditorWindow window)
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_BooleanProperty, new GUIContent("�������ֵ", "��סCTRL���Է����ʲ�����"));
            // Boolean == false
            EditorGUILayout.ObjectField(_FalseAnimationProperty, new GUIContent("False ����"));
            // Boolean == true
            EditorGUILayout.ObjectField(_TrueAnimationProperty, new GUIContent("True ����"));


            serializedObject.ApplyModifiedProperties();
        }

    }
}

