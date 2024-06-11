using Animancer.Editor;
using ML.Engine.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(IntegerMixerTransitionAsset), true)]
    public class IntegerMixerTransitionAssetEditor : AnimationAssetBaseEditor
    {
        protected SerializedProperty _transitionsProperty { get; set; }

        protected IntegerMixerTransitionAsset asset { get; set; }

        public override void Init()
        {
            asset = (IntegerMixerTransitionAsset)target;

            _transitionsProperty = serializedObject.FindProperty("transitions");

        }

        public override void DrawInEditorWindow(EditorWindow window)
        {
            serializedObject.Update();

            // Ĭ��ֵ
            asset.DefaultValue = EditorGUILayout.IntField("Ĭ��ֵ", asset.DefaultValue);
            asset.ClampDefaultValue();

            // ���ֵ
            asset.IsRandom = EditorGUILayout.Toggle("�Ƿ����", asset.IsRandom);

            // Animations
            EditorGUILayout.PropertyField(_transitionsProperty, new GUIContent("����"));

            serializedObject.ApplyModifiedProperties();
        }
     }

}
