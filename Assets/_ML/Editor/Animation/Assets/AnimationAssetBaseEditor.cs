using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using ML.Engine.Animation;

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
            //Debug.LogWarning($"{this.target.GetType()} û��ʵ��Editor");
            DrawDefaultInspector();
        }

        /// <summary>
        /// �����Զ����Track
        /// Ĭ��ֻ���� EventTrack
        /// </summary>
        public virtual void DrawTrackGUI()
        {
        }

        public virtual void Init()
        {

        }


    }

    public class EventTrack : TrackWindow.Track
    {
        public EventTrack(ITransitionWithEvents transition)
        {
            TargetTransition = transition;
            // ����ʹ��foreach: Events��EventName�Ƿֿ���ĵ�
            int length = transition.Events.Names.Length;
            //transition.Events[1].normalizedTime
            
            for(int i = 0; i < length; ++i)
            {
                var signal = CreateSignal<EventTrackSignal>();
                signal.EventIndex = i;
            }
            // OnEndEvent��Ҫ�������� -> ��AnimationWindow��壬EventTrackֻ����������Event
        }

        /// <summary>
        /// �༭��Ŀ�� Transition
        /// </summary>
        protected ITransitionWithEvents TargetTransition;

        protected override void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("�½�Event"), false, () =>
            {
                this.CreateSignalOnMouse<EventTrackSignal>();
            });
        }


        internal class EventTrackSignal : TrackWindow.TrackSignal
        {
            public ITransitionWithEvents transition => ((EventTrack)AttachedTrack).TargetTransition;
            public override string Name { get => transition.Events.GetName(EventIndex); set => transition.Events.SetName(EventIndex, value); }
            public override float NormalizedTime { get => transition.Events[EventIndex].normalizedTime;
                set
                {
                    //transition.Events.SetShouldNotModifyReason(null);
                    transition.Events.SetNormalizedTime(EventIndex, value);
                }
            }
            public int EventIndex { get; set; }

        }
    }

}
