using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using ML.Engine.Animation;
using static ML.Engine.Animation.IAssetHasEvents;
using UnityEngine.Events;
using Animancer.Editor;
using System.Diagnostics.Eventing.Reader;

namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(AnimationAssetBase), true)]
    public class AnimationAssetBaseEditor : UnityEditor.Editor
    {
        [OnOpenAsset(0)]
        public static bool ClickAction(int instanceID, int line)
        {
            AnimationAssetBase asset = EditorUtility.InstanceIDToObject(instanceID) as AnimationAssetBase;
            if (asset == null)
            {
                return false;
            }

            AnimationWindow.OpenWithAsset(asset);

            return true;
        }

        public virtual void DrawInEditorWindow()
        {
            //Debug.LogWarning($"{this.target.GetType()} 没有实现Editor");
            DrawDefaultInspector();
        }

        /// <summary>
        /// 绘制自定义的Track
        /// 默认只绘制 EventTrack
        /// </summary>
        public virtual void DrawTrackGUI()
        {
        }

        public virtual void Init()
        {

        }


    }

    [System.Serializable]
    public class EventTrack : TrackWindow.Track
    {
        public EventTrack(IAssetHasEvents transition)
        {
            TargetTransition = transition;
            // 不能使用foreach: Events和EventName是分开存的的
            if(transition.Events != null)
            {
                int length = transition.Events.Count;
                //transition.Events[1].normalizedTime

                for (int i = 0; i < length; ++i)
                {
                    var signal = CreateSignal<EventTrackSignal>();
                    signal.Event = transition.Events[i];
                    //Debug.Log(i + " " + transition.Events[i]);
                }
                // OnEndEvent需要单独处理 -> 在AnimationWindow面板，EventTrack只处理其他的Event
            }

        }

        /// <summary>
        /// 编辑的目标 Transition
        /// </summary>
        protected IAssetHasEvents TargetTransition;

        protected override void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("新建Event"), false, () =>
            {
                var signal = this.CreateSignalOnMouse<EventTrackSignal>();
                var e = new AssetEvent();
                signal.Event = e;
                TargetTransition.Events.Add(e);
            });
        }


        [System.Serializable]
        internal class EventTrackSignal : TrackWindow.TrackSignal
        {
            public IAssetHasEvents _asset => ((EventTrack)AttachedTrack).TargetTransition;
            public override string Name { get => Event.Name; set => Event.Name = value; }
            public override float NormalizedTime { get => Event.NormalizedTime; set => Event.NormalizedTime = value; }
            public AssetEvent Event;

            public override void OnDelete()
            {
                _asset.Events.Remove(Event);
            }

            SerializedObject serializedObject;
            SerializedProperty eventProperty;
            void OnEnable()
            {
                serializedObject = new SerializedObject(this);
                eventProperty = serializedObject.FindProperty("Event.Event");
            }

            public override void DoSelectedGUI()
            {
                EditorGUI.BeginChangeCheck();
                base.DoSelectedGUI();

                // 编辑并显示 Event.Event 属性

                GUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField("时间点", GUILayout.Width(60));
                serializedObject.Update();
                EditorGUILayout.PropertyField(eventProperty, new GUIContent("事件"), true);
                GUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_asset as ScriptableObject);
                }
            }
        }
    }

}
