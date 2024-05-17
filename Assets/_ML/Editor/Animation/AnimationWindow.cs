using Animancer;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using ML.Editor;
using ML.Engine.Animation;

namespace ML.Editor.Animation
{
    public class AnimancerTransitionAssetBaseWindow : EditorWindow, IHasCustomMenu
    {
        private static AnimancerTransitionAssetBaseWindow _instance;
        public static AnimancerTransitionAssetBaseWindow Instance => _instance;

        private static object _container;

        public PreviewWindow previewWindow;
        public TrackWindow trackWindow;
        public DetailWindow detailWindow;


        private AnimationAssetBase selectedAsset;
        private AnimationAssetBase SelectedAsset
        {
            get => selectedAsset;
            set
            {
                if(!locked)
                {
                    selectedAsset = value;
                }
            }
        }
        private AnimationAssetBaseEditor assetEditor;

        [MenuItem("Window/ML/AnimancerAssetEditor", priority = 0)]
        public static object ShowWindow()
        {
            if (_container != null)
            {
                return _container;
            }
            // 创建最外层容器
            _container = ML.Editor.EditorContainerWindow.CreateInstance();
            // 创建分屏容器
            object splitViewInstance = EditorSplitView.CreateInstance();
            // 设置根容器
            EditorContainerWindow.SetRootView(_container, splitViewInstance);

            // 创建window容器
            object windowDockAreaInstance = EditorDockArea.CreateInstance();
            EditorDockArea.SetPosition(windowDockAreaInstance, new Rect(0, 0, 200, 800));
            _instance = (AnimancerTransitionAssetBaseWindow)ScriptableObject.CreateInstance(typeof(AnimancerTransitionAssetBaseWindow));
            EditorWindow window = _instance;
            window.titleContent = new GUIContent("AnimancerAsset");
            EditorDockArea.AddTab(windowDockAreaInstance, window);
            // 加入Window容器
            EditorSplitView.AddChild(splitViewInstance, windowDockAreaInstance);

            object previewtrackSplitViewInstance = EditorSplitView.CreateInstance();
            EditorSplitView.SetPosition(previewtrackSplitViewInstance, new Rect(200, 0, 800, 800));
            EditorSplitView.SetVertical(previewtrackSplitViewInstance, true);
            EditorSplitView.AddChild(splitViewInstance, previewtrackSplitViewInstance);
            // 创建Preview
            object previewDockAreaInstance = EditorDockArea.CreateInstance();
            EditorDockArea.SetPosition(previewDockAreaInstance, new Rect(200, 0, 800, 600));
            _instance.previewWindow = (PreviewWindow)ScriptableObject.CreateInstance(typeof(PreviewWindow));
            _instance.previewWindow.titleContent = new GUIContent("Preview");
            EditorDockArea.AddTab(previewDockAreaInstance, _instance.previewWindow);
            // 加入Window容器
            EditorSplitView.AddChild(previewtrackSplitViewInstance, previewDockAreaInstance);

            // 创建Track
            object trackDockAreaInstance = EditorDockArea.CreateInstance();
            EditorDockArea.SetPosition(trackDockAreaInstance, new Rect(200, 600, 800, 200));
            _instance.trackWindow = (TrackWindow)ScriptableObject.CreateInstance(typeof(TrackWindow));
            _instance.trackWindow.titleContent = new GUIContent("Track");
            EditorDockArea.AddTab(trackDockAreaInstance, _instance.trackWindow);
            // 加入Window容器
            EditorSplitView.AddChild(previewtrackSplitViewInstance, trackDockAreaInstance);
            //EditorSplitView.SetVertical(splitViewInstance, false);


            // 创建Detail
            object detailDockAreaInstance = EditorDockArea.CreateInstance();
            EditorDockArea.SetPosition(detailDockAreaInstance, new Rect(1000, 600, 200, 800));
            _instance.detailWindow = (DetailWindow)ScriptableObject.CreateInstance(typeof(DetailWindow));
            _instance.detailWindow.titleContent = new GUIContent("Detail");
            EditorDockArea.AddTab(detailDockAreaInstance, _instance.detailWindow);
            // 加入Window容器
            EditorSplitView.AddChild(splitViewInstance, detailDockAreaInstance);


            EditorEditorWindow.MakeParentsSettingsMatchMe(window);

            EditorContainerWindow.SetPosition(_container, new Rect(100, 100, 1200, 800));
            EditorSplitView.SetPosition(splitViewInstance, new Rect(0, 0, 1200, 800));


            EditorContainerWindow.Show(_container, 0, true, false, true);
            EditorContainerWindow.OnResize(_container);

            return _container;
        }

        public static AnimancerTransitionAssetBaseWindow OpenWithAsset(AnimationAssetBase asset)
        {
            ShowWindow();
            if (locked == false)
            {
                _instance.SelectedAsset = asset;
            }
            return _instance;
        }

        private void OnGUI()
        {
            // 选中的资产
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("选中的资产", GUILayout.Width(100));
            SelectedAsset = (AnimationAssetBase)EditorGUILayout.ObjectField(SelectedAsset, typeof(AnimationAssetBase), false);
            EditorGUILayout.EndHorizontal();

            // 显示选中的资产的数据
            if (SelectedAsset != null && (assetEditor == null || assetEditor.target != SelectedAsset))
            {
                assetEditor = UnityEditor.Editor.CreateEditor(SelectedAsset) as AnimationAssetBaseEditor;
                Debug.Log(UnityEditor.Editor.CreateEditor(SelectedAsset).GetType() + " " + SelectedAsset.GetType());
            }
            if (assetEditor != null)
            {
                assetEditor.DrawInEditorWindow();
            }
            else
            {
                Debug.LogWarning($"{SelectedAsset.GetType()} 没有对应的Editor");
            }


        }
        public void OnSelectionChange()
        {
            SelectedAsset = Selection.activeObject as AnimationAssetBase;
            Repaint();
        }

        private void OnDestroy()
        {
            _container = null;
            if (_instance == this)
            {
                _instance = null;
            }
        }


        #region ContextMenu
        /// <summary>
        /// Keep local copy of lock button style for efficiency.
        /// </summary>
        [System.NonSerialized]
        GUIStyle lockButtonStyle;
        /// <summary>
        /// Indicates whether lock is toggled on/off.
        /// </summary>
        [System.NonSerialized]
        private static bool locked = false;

        /// <summary>
        /// Magic method which Unity detects automatically.
        /// </summary>
        /// <param name="position">Position of button.</param>
        void ShowButton(Rect position)
        {
            if (lockButtonStyle == null)
                lockButtonStyle = "IN LockButton";
            // 如果没有选中资产，则禁用按钮
            EditorGUI.BeginDisabledGroup(SelectedAsset == null);
            if (SelectedAsset == null)
            {
                locked = false;
            }
            locked = GUI.Toggle(position, locked, GUIContent.none, lockButtonStyle);
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Adds custom items to editor window context menu.
        /// </summary>
        /// <remarks>
        /// <para>This will only work for Unity 4.x+</para>
        /// </remarks>
        /// <param name="menu">Context menu.</param>
        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            // 如果没有选中资产，则禁用菜单项
            bool disabled = SelectedAsset == null;
            if (disabled)
            {
                menu.AddDisabledItem(new GUIContent("Lock"));
                locked = false;
            }
            else
            {
                menu.AddItem(new GUIContent("Lock"), locked, () =>
                {
                    locked = !locked;
                });
            }
        }
        #endregion
    }

    public class PreviewWindow : EditorWindow
    {
        public static PreviewWindow Instance
        {
            get => AnimancerTransitionAssetBaseWindow.Instance != null ? AnimancerTransitionAssetBaseWindow.Instance.previewWindow : null;
            set
            {
                if (AnimancerTransitionAssetBaseWindow.Instance != null)
                {
                    AnimancerTransitionAssetBaseWindow.Instance.previewWindow = value;
                }
            }
        }

        public static void GetWindow()
        {
            GetWindow<PreviewWindow>("Window1");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }

    public class TrackWindow : EditorWindow
    {
        public static TrackWindow Instance
        {
            get => AnimancerTransitionAssetBaseWindow.Instance != null ? AnimancerTransitionAssetBaseWindow.Instance.trackWindow : null;
            set
            {
                if (AnimancerTransitionAssetBaseWindow.Instance != null)
                {
                    AnimancerTransitionAssetBaseWindow.Instance.trackWindow = value;
                }
            }
        }

        public static void GetWindow()
        {
            GetWindow<TrackWindow>("Window2");
        }
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }

    public class DetailWindow : EditorWindow
    {
        public static DetailWindow Instance
        {
            get => AnimancerTransitionAssetBaseWindow.Instance != null ? AnimancerTransitionAssetBaseWindow.Instance.detailWindow : null;
            set
            {
                if (AnimancerTransitionAssetBaseWindow.Instance != null)
                {
                    AnimancerTransitionAssetBaseWindow.Instance.detailWindow = value;
                }
            }
        }

        public static void GetWindow()
        {
            GetWindow<DetailWindow>("Window3");
        }
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }


}

