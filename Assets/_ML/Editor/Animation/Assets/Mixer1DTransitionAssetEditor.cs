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

            // 默认参数值
            asset.transition.DefaultParameter = EditorGUILayout.FloatField("默认参数值", asset.transition.DefaultParameter);
            EditorGUILayout.Space();

            // 速度
            // Speed -> 播放速度
            DoSpeedGUI(asset.transition);

            // Extrapolate Speed -> 启用，则参数值超过了阈值，会增加混合的速度
            asset.transition.ExtrapolateSpeed = EditorGUILayout.Toggle(new GUIContent("超高速度", "是否应将Parameter设置为高于最高阈值，以按比例增加混合器的Speed？"), asset.transition.ExtrapolateSpeed);

            #region 时间轴 -> 使用秒数时间
            EditorGUILayout.Space();
            DoAnimTimelineGUI(asset.transition, asset.EndEvent.NormalizedTime, length, frameRate);
            EditorGUILayout.Space();
            #endregion

            // 过渡时间
            // Fade Duration -> 过渡时间
            asset.transition.FadeDuration = DoFadeDurationGUI(asset.transition, length, frameRate, ref bShowFadeDuration);

            // 结束时间
            DoEndTimeGUI(_endEventProperty, length, frameRate, (float.IsNaN(speed) || speed >= 0) ? 1 : 0, ref bEndTime);

            // Animations[动画、Speed、Weight、Sync]
            // Animation -> ObjectField
            // Speed -> Toggle + FloatField
            // Weight -> FloatField[0, 1]
            // Sync -> Toggle
            // 保证数组长度一致，以动画为准
            _AnimationList.DoLayoutList();

            DoThresholdGraphGUI();

            serializedObject.ApplyModifiedProperties();
        }

        // TODO : 绘制一个一维图，在上面拖动设置默认值以及更改动画设置的阈值
        protected void DoThresholdGraphGUI()
        {

        }
    }

}
