using ML.Engine.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(Mixer2DTransitionAsset), true)]
    public class Mixer2DTransitionAssetEditor : MixerParameterTransitionAssetEditor<Mixer2DTransitionAsset>
    {
        protected SerializedProperty _Type { get; set; }

        public override void Init()
        {
            base.Init();
            var p = serializedObject.FindProperty("transition");
            _Type = p.FindPropertyRelative("_Type");
        }

        public override void DrawInEditorWindow()
        {
            serializedObject.Update();

            float length = asset.Length;
            float frameRate = asset.FrameRate;
            float speed = asset.transition.Speed;

            // Ĭ�ϲ���ֵ
            asset.transition.DefaultParameter = EditorGUILayout.Vector2Field("Ĭ�ϲ���ֵ", asset.transition.DefaultParameter);
            EditorGUILayout.Space();

            // �ٶ�
            // Speed -> �����ٶ�
            DoSpeedGUI(asset.transition);

            // Ӧ������
            // Gradient Band Interpolation -> ���Բ�ֵ
            // Polar Gradient Band Interpolation -> �������ֵ
            EditorGUILayout.PropertyField(_Type, new GUIContent("��ֵ��ʽ", "Cartesian -> Gradient Band Interpolation[���Բ�ֵ]\nDirectional -> Polar Gradient Band Interpolation[�������ֵ]"));

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
            _AnimationList.DoLayoutList();

            DoThresholdGraphGUI();

            serializedObject.ApplyModifiedProperties();
        }

        // TODO : ����һ����άͼ���������϶�����Ĭ��ֵ�Լ����Ķ������õ���ֵ
        protected void DoThresholdGraphGUI()
        {

        }
    }
}

