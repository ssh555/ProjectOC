using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using ML.Engine.Animation;
using static ML.Engine.Animation.IAssetHasEvents;


namespace ML.Editor.Animation
{
    [UnityEditor.CustomEditor(typeof(AnimationAssetBase), true)]
    public partial class AnimationAssetBaseEditor : UnityEditor.Editor
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

        /// <summary>
        /// ������AnimationWindow�����л��Ƴ���������(������Track)
        /// </summary>
        public virtual void DrawInEditorWindow(EditorWindow window)
        {
            //Debug.LogWarning($"{this.target.GetType()} û��ʵ��Editor");
            DrawDefaultInspector();
        }

        /// <summary>
        /// �����Զ����Track
        /// ���ڴ˵���Track.DrawTrackGUI()
        /// </summary>
        public virtual void DrawTrackGUI()
        {
        }

        /// <summary>
        /// ��Ч��OnEnable -> ���ڴ��ڴ������ʲ�ʱ�ĳ�ʼ���༭��
        /// ���ڳ�ʼ����ʹ�õĹ��
        /// </summary>
        public virtual void Init()
        {

        }

        public override void OnInspectorGUI()
        {
            //DrawInEditorWindow();
            EditorGUILayout.LabelField("��˫���� Animation Window ���б༭��鿴");
            //base.OnInspectorGUI();
        }
    }

    [System.Serializable]
    public class EventTrack : TrackWindow.Track
    {
        public EventTrack(IAssetHasEvents transition) : base()
        {
            this.Name = "Event Track";
            
            this.Start = 0;
            this.End = transition.FrameLength;

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

        public override void DrawTrackGUI()
        {
            this.End = TargetTransition.FrameLength;
            base.DrawTrackGUI();
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
                TargetTransition.Events.Add(signal.Event);
                var asset = (TargetTransition as AnimationAssetBase);
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
                serializedObject.Update();
                _asset.Events.Remove(Event);
                serializedObject.ApplyModifiedProperties();
            }

            SerializedObject serializedObject;
            SerializedProperty eventProperty;
            void OnEnable()
            {
                serializedObject = new SerializedObject(this);
                eventProperty = serializedObject.FindProperty("Event.UnityEvents");
            }

            public override void DoSelectedGUI()
            {
                EditorGUI.BeginChangeCheck();
                base.DoSelectedGUI();

                // �༭����ʾ Event.Event ����

                GUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField("ʱ���", GUILayout.Width(60));
                serializedObject.Update();
                EditorGUILayout.PropertyField(eventProperty, new GUIContent("�¼�"), true);
                GUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }

}
