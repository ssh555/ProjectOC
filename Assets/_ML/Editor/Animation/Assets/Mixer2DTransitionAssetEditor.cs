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

            // 默认参数值
            asset.transition.DefaultParameter = EditorGUILayout.Vector2Field("默认参数值", asset.transition.DefaultParameter);
            EditorGUILayout.Space();

            // 速度
            // Speed -> 播放速度
            DoSpeedGUI(asset.transition);

            // 应用类型
            // Gradient Band Interpolation -> 线性插值
            // Polar Gradient Band Interpolation -> 极坐标插值
            EditorGUILayout.PropertyField(_Type, new GUIContent("插值方式", "Cartesian -> Gradient Band Interpolation[线性插值]\nDirectional -> Polar Gradient Band Interpolation[极坐标插值]"));

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

        // TODO : 绘制一个二维图，在上面拖动设置默认值以及更改动画设置的阈值
        protected void DoThresholdGraphGUI()
        {

        }
    }
}

