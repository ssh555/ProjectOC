using Animancer;
using Animancer.Editor;
using ML.Engine.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ML.Editor.Animation
{
    public class MixerParameterTransitionAssetEditor<T> : MixerTransitionAssetEditor<T>
        where T : AnimationAssetBase, IAssetHasEvents
    {
        protected override string GetWeightsField() => "_Thresholds";
    }

    [UnityEditor.CustomEditor(typeof(Mixer1DTransitionAsset), true)]
    public class Mixer1DTransitionAssetEditor : MixerParameterTransitionAssetEditor<Mixer1DTransitionAsset>
    {
        public override void DrawInEditorWindow()
        {
            serializedObject.Update();

            float length = asset.Length;
            float frameRate = asset.FrameRate;
            float speed = asset.transition.Speed;

            // Ĭ�ϲ���ֵ
            asset.transition.DefaultParameter = EditorGUILayout.FloatField("Ĭ�ϲ���ֵ", asset.transition.DefaultParameter);
            EditorGUILayout.Space();

            // �ٶ�
            // Speed -> �����ٶ�
            DoSpeedGUI(asset.transition);

            // Extrapolate Speed -> ���ã������ֵ��������ֵ�������ӻ�ϵ��ٶ�
            asset.transition.ExtrapolateSpeed = EditorGUILayout.Toggle(new GUIContent("�����ٶ�", "�Ƿ�Ӧ��Parameter����Ϊ���������ֵ���԰��������ӻ������Speed��"), asset.transition.ExtrapolateSpeed);

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

        // TODO : ����һ��һάͼ���������϶�����Ĭ��ֵ�Լ����Ķ������õ���ֵ
        protected void DoThresholdGraphGUI()
        {

        }
    }

}
