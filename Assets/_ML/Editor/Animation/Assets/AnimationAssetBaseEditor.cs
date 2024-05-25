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
        public EventTrack(IAssetHasEvents transition)
        {
            TargetTransition = transition;
            // ����ʹ��foreach: Events��EventName�Ƿֿ���ĵ�
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
                // OnEndEvent��Ҫ�������� -> ��AnimationWindow��壬EventTrackֻ����������Event
            }

        }

        /// <summary>
        /// �༭��Ŀ�� Transition
        /// </summary>
        protected IAssetHasEvents TargetTransition;

        protected override void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("�½�Event"), false, () =>
            {
                var signal = this.CreateSignalOnMouse<EventTrackSignal>();
                var e = new AssetEvent();
                signal.Event = e;
                TargetTransition.Events.Add(e);
            });
        }


        internal class EventTrackSignal : TrackWindow.TrackSignal
        {
            public IAssetHasEvents _assets => ((EventTrack)AttachedTrack).TargetTransition;
            public override string Name { get => Event.Name; set => Event.Name = value; }
            public override float NormalizedTime { get => Event.NormalizedTime; set => Event.NormalizedTime = value; }
            public AssetEvent Event;

            public override void OnDelete()
            {
                _assets.Events.Remove(Event);
            }

            bool isFoldoutOpen = true;
            public override void DoSelectedGUI()
            {
                base.DoSelectedGUI();

                //// ���ƿ��۵���
                //isFoldoutOpen = EditorGUILayout.Foldout(isFoldoutOpen, "�¼�", true);
                //if (isFoldoutOpen)
                //{
                //    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //    EditorGUI.indentLevel++;

                //    // �༭����ʾ Name ����
                //    GUILayout.BeginHorizontal();
                //    EditorGUILayout.LabelField("����", GUILayout.Width(60));
                //    Name = EditorGUILayout.TextField(Name);
                //    GUILayout.EndHorizontal();


                //    // �༭����ʾ NormalizedTime ����
                //    GUILayout.BeginHorizontal();
                //    EditorGUILayout.LabelField("ʱ���", GUILayout.Width(60));
                //    NormalizedTime = EditorGUILayout.FloatField(NormalizedTime);
                //    GUILayout.EndHorizontal();

                //    //// �༭����ʾ Event.Event ����
                //    //SerializedObject serializedObject = new SerializedObject(this);
                //    //SerializedProperty eventProperty = serializedObject.FindProperty("Event.Event");

                //    //GUILayout.BeginHorizontal();
                //    ////EditorGUILayout.LabelField("ʱ���", GUILayout.Width(60));
                //    //EditorGUILayout.PropertyField(eventProperty, new GUIContent("�¼�"), true);
                //    //GUILayout.EndHorizontal();

                //    EditorGUI.indentLevel--;
                //    EditorGUILayout.EndVertical();
                //}
            }

        }
    }

}
