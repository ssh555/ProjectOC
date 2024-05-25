using Animancer.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ML.Editor.Animation
{
    public class DetailWindow : EditorWindow
    {
        #region Instance
        public static DetailWindow Instance
        {
            get;
            private set;
        }
        #endregion

        private static readonly string[] TabNames = { "Ԥ��", "����" };
        private const int PreviewTab = 0, SettingsTab = 1;

        [SerializeField]
        private int _CurrentTab;
        /// <summary>
        /// ���ڻ��� AnimancerPlable
        /// </summary>
        private readonly AnimancerPlayableDrawer PlayableDrawer = new AnimancerPlayableDrawer();

        public PreviewWindow Target => PreviewWindow.Instance;

        /// <summary>
        /// ��ǰѡ��
        /// </summary>
        public ISelection _curSelection;
        public ISelection CurSelection
        {
            get => _curSelection;
            set
            {
                _curSelection?.OnDeselected();
                _curSelection = value;
                _curSelection?.OnSelected();
                this.Repaint();
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(AnimancerGUI.StandardSpacing * 2);

            // ��������
            if (Event.current.type == EventType.MouseDown)
                Target.ShowTab();

            _CurrentTab = GUILayout.Toolbar(_CurrentTab, TabNames);
            _CurrentTab = Mathf.Clamp(_CurrentTab, 0, TabNames.Length - 1);

            switch (_CurrentTab)
            {
                case PreviewTab: DoPreviewInspectorGUI(); break;
                case SettingsTab: PreviewWindow.Settings.DoInspectorGUI(); break;
                default: GUILayout.Label("Tab index is out of bounds"); break;
            }
        }

        private void DoPreviewInspectorGUI()
        {
            if (Target._SelectedAsset == null)
            {
                EditorGUILayout.HelpBox("ѡ�е�Animation�ʲ�ΪNull", MessageType.Info, true);
                return;
            }

            Target.GetAnimations.DoGUI();

            var animancer = Target.GetScene.Animancer;
            if (animancer != null)
            {
                PlayableDrawer.DoGUI(animancer.Component);
                if (animancer.IsGraphPlaying)
                    GUI.changed = true;
            }
            EditorGUILayout.Separator();
            PreviewWindow.Settings.DoHierarchyGUI();

            // ѡ�е�ϸ�����
            EditorGUILayout.Separator();
            AnimancerGUI.BeginVerticalBox(GUIStyle.none);
            EditorGUILayout.LabelField("ϸ�����");
            CurSelection?.DoSelectedGUI();
            AnimancerGUI.EndVerticalBox(GUIStyle.none);
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }

        public interface ISelection
        {
            public virtual void DoSelectedGUI()
            {
                EditorGUILayout.LabelField("δʵ��ϸ����ʾ");
            }

            public virtual void OnSelected()
            {

            }

            public virtual void OnDeselected()
            {

            }
        }
    }
}
