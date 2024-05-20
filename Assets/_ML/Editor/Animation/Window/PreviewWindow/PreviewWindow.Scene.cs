using Animancer;
using Animancer.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ML.Editor.Animation
{
    partial class PreviewWindow
    {
        public static Scene InstanceScene => Instance != null ? Instance._Scene : null;

        [Serializable]
        public class Scene
        {
            #region Fields and Properties
            private const HideFlags HideAndDontSave = HideFlags.HideInHierarchy | HideFlags.DontSave;

            /// <summary>
            /// Preview Scene
            /// </summary>
            [SerializeField]
            private UnityEngine.SceneManagement.Scene _Scene;

            /// <summary>
            /// Ԥ�������еĸ�����
            /// </summary>
            public Transform PreviewSceneRoot { get; private set; }

            /// <summary>
            /// Ԥ��������ģ�͵ĸ�����
            /// <see cref="PreviewSceneRoot">��������
            /// </summary>
            public Transform InstanceRoot { get; private set; }

            /// <summary>
            /// <see cref="Settings.SceneEnvironment"/>��ʵ������
            /// <see cref="PreviewSceneRoot"/>��������
            /// </summary>
            public GameObject EnvironmentInstance { get; private set; }

            [SerializeField]
            private Transform _OriginalRoot;

            /// <summary>
            /// ����ʵ����<see cref="InstanceRoot"/> ��ԭģ��
            /// </summary>
            public Transform OriginalRoot
            {
                get => _OriginalRoot;
                set
                {
                    _OriginalRoot = value;
                    InstantiateModel();

                    if (value != null)
                    {
                        Settings.AddModel(value.gameObject);
                    }
                }
            }

            /// <summary>
            /// <see cref="InstanceRoot"/> �������������е�Animator
            /// </summary>
            public Animator[] InstanceAnimators { get; private set; }

            /// <summary>
            /// ��ǰѡ�е�Animator��Index
            /// </summary>
            [SerializeField] private int _SelectedInstanceAnimator;
            /// <summary>
            /// ѡ�е�ģ�Ͷ���������
            /// </summary>
            [NonSerialized] private AnimationType _SelectedInstanceType;

            /// <summary>
            /// ��ǰѡ�е�Animator
            /// </summary>
            public Animator SelectedInstanceAnimator
            {
                get
                {
                    if (InstanceAnimators == null ||
                        InstanceAnimators.Length == 0)
                        return null;

                    if (_SelectedInstanceAnimator > InstanceAnimators.Length)
                        _SelectedInstanceAnimator = InstanceAnimators.Length;

                    return InstanceAnimators[_SelectedInstanceAnimator];
                }
            }

            [NonSerialized]
            private AnimancerPlayable _Animancer;
            /// <summary>
            /// ����Ԥ������
            /// </summary>
            public AnimancerPlayable Animancer
            {
                get
                {
                    if ((_Animancer == null || !_Animancer.IsValid) &&
                        InstanceRoot != null)
                    {
                        // ��ǰѡ�е�Animator
                        var animator = SelectedInstanceAnimator;
                        if (animator != null)
                        {
                            AnimancerPlayable.SetNextGraphName($"{animator.name} (Animancer Preview)");
                            _Animancer = AnimancerPlayable.Create();
                            _Animancer.CreateOutput(
                                new AnimancerEditorUtilities.DummyAnimancerComponent(animator, _Animancer));
                            _Animancer.RequirePostUpdate(Animations.WindowMatchStateTime.Instance);
                            Instance._Animations.NormalizedTime = Instance._Animations.NormalizedTime;
                        }
                    }

                    return _Animancer;
                }
            }

            #endregion

            #region Initialization
            /// <summary>
            /// ��ʼ������ -> �ֶ�����
            /// </summary>
            public void OnEnable()
            {
                // �³�����ʱ����
                EditorSceneManager.sceneOpening += OnSceneOpening;
                // ����ģʽ�޸�ʱ����
                EditorApplication.playModeStateChanged += OnPlayModeChanged;

                // ���Ƴ���GUIʱ����
                //duringSceneGui += DoCustomGUI;

                // ��������
                CreateScene();

                // ѡ����ѵ�Ԥ��ģ��
                if (OriginalRoot == null)
                    OriginalRoot = Settings.TrySelectBestModel();
            }

            /// <summary>
            /// ����Ԥ������
            /// </summary>
            private void CreateScene()
            {
                _Scene = EditorSceneManager.NewPreviewScene();
                _Scene.name = "Transition Preview";
                Instance.customScene = _Scene;

                // ����Ԥ��������
                PreviewSceneRoot = EditorUtility.CreateGameObjectWithHideFlags(
                    $"Preview Scene Root", HideAndDontSave).transform;
                // �� PreviewSceneRoot �ƶ���Ԥ��������
                SceneManager.MoveGameObjectToScene(PreviewSceneRoot.gameObject, _Scene);
                Instance.customParentForDraggedObjects = PreviewSceneRoot;

                OnEnvironmentPrefabChanged();
            }

            /// <summary>
            /// ʵ�����µ�ģ��
            /// </summary>
            private void InstantiateModel()
            {
                // ���پ�ģ��
                DestroyModelInstance();

                if (_OriginalRoot == null)
                {
                    return;
                }

                // ʵ�����µ�ģ��ΪInstanceRoot
                PreviewSceneRoot.gameObject.SetActive(false);
                InstanceRoot = Instantiate(_OriginalRoot, PreviewSceneRoot);
                InstanceRoot.localPosition = default;
                InstanceRoot.name = _OriginalRoot.name;

                // ����Ԥ��ʵ��ģ���ϵĲ���Ҫ���
                DisableUnnecessaryComponents(InstanceRoot.gameObject);

                // ʵ����ģ���ϵ�����Animators
                InstanceAnimators = InstanceRoot.GetComponentsInChildren<Animator>();
                for (int i = 0; i < InstanceAnimators.Length; i++)
                {
                    var animator = InstanceAnimators[i];
                    // ����
                    animator.enabled = false;
                    // ���ǲ���
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    // ������ʱ��
                    animator.fireEvents = false;
                    // ��������ģʽ
                    animator.updateMode = AnimatorUpdateMode.Normal;
                }

                // Active Ԥ�������ĸ�����
                PreviewSceneRoot.gameObject.SetActive(true);

                // ����ѡ���Animator
                SetSelectedAnimator(_SelectedInstanceAnimator);
                // �۽�������ڵ�ǰģ��ʵ��
                FocusCamera();
                // �ռ���ǰԤ��ģ���ϵĶ���
                Instance._Animations.GatherAnimations();
            }

            private static void DisableUnnecessaryComponents(GameObject root)
            {
                var behaviours = root.GetComponentsInChildren<Behaviour>();
                for (int i = 0; i < behaviours.Length; i++)
                {
                    var behaviour = behaviours[i];

                    // Other undesirable components aren't Behaviors anyway: Transform, MeshFilter, Renderer
                    if (behaviour is Animator)
                        continue;

                    var type = behaviour.GetType();
                    if (type.IsDefined(typeof(ExecuteAlways), true) ||
                        type.IsDefined(typeof(ExecuteInEditMode), true))
                        continue;

                    behaviour.enabled = false;
                    behaviour.hideFlags |= HideFlags.NotEditable;
                }
            }

            /// <summary>
            /// ����֮ǰ�Ļ���ʵ���������µĻ���ʵ��
            /// </summary>
            internal void OnEnvironmentPrefabChanged()
            {
                DestroyImmediate(EnvironmentInstance);

                var prefab = Settings.SceneEnvironment;
                if (prefab != null)
                    EnvironmentInstance = Instantiate(prefab, PreviewSceneRoot);
            }

            /// <summary>
            /// ��ѡ����ʲ��ı�ʱ����
            /// </summary>
            public void OnSelectedAssetChanged()
            {
                _SelectedInstanceAnimator = 0;
                if (_ExpandedHierarchy != null)
                    _ExpandedHierarchy.Clear();

                // ֱ��Ѱ�����Ԥ��ģ��
                //OriginalRoot = AnimancerEditorUtilities.FindRoot(Instance._TransitionProperty.TargetObject);
                if (OriginalRoot == null)
                    OriginalRoot = Settings.TrySelectBestModel();

                // ���ù�Լ��ʱ��
                Instance._Animations.NormalizedTime = 0;

                // ���ݶ�����������PreviewScene������ʾmode
                Instance.in2DMode = _SelectedInstanceType == AnimationType.Sprite;
            }

            /// <summary>
            /// ����ѡ�е�Animator
            /// </summary>
            /// <param name="index"></param>
            public void SetSelectedAnimator(int index)
            {
                // ����֮ǰ��Animancerʵ��
                DestroyAnimancerInstance();

                // ȡ��֮ǰѡ�е�Animator
                var animator = SelectedInstanceAnimator;
                if (animator != null && animator.enabled)
                {
                    animator.Rebind();
                    animator.enabled = false;
                    // ΪʲôҪreturn
                    //return;
                }

                // ѡ�е�ǰѡ���Animator
                _SelectedInstanceAnimator = index;

                animator = SelectedInstanceAnimator;
                if (animator != null)
                {
                    animator.enabled = true;
                    _SelectedInstanceType = AnimationBindings.GetAnimationType(animator);
                    Instance.in2DMode = _SelectedInstanceType == AnimationType.Sprite;
                }
            }

            /// <summary>
            /// ��������� -> �۽��ڵ�ǰ��ģ��ʵ��
            /// </summary>
            private void FocusCamera()
            {
                // ʵ��ģ�͵�RendererBound
                var bounds = CalculateBounds(InstanceRoot);

                var rotation = Instance.in2DMode ? Quaternion.identity : Quaternion.Euler(35, 135, 0);

                var size = bounds.extents.magnitude * 1.5f;
                if (size == float.PositiveInfinity)
                    return;
                else if (size == 0)
                    size = 10;

                Instance.LookAt(bounds.center, rotation, size, Instance.in2DMode, true);
            }

            /// <summary>
            /// ���� Transform �� RendererBound -> ���е�RendererBound.Grow
            /// </summary>
            /// <param name="transform"></param>
            /// <returns></returns>
            private static Bounds CalculateBounds(Transform transform)
            {
                var renderers = transform.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                    return default;

                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                return bounds;
            }

            #endregion

            #region Cleanup
            /// <summary>
            /// PreviewWindow �ֶ�����
            /// </summary>
            public void OnDisable()
            {
                EditorSceneManager.sceneOpening -= OnSceneOpening;
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;

                //duringSceneGui -= DoCustomGUI;

                DestroyAnimancerInstance();

                EditorSceneManager.ClosePreviewScene(_Scene);
            }

            /// <summary>
            /// PreviewWindow �ֶ�����
            /// </summary>
            public void OnDestroy()
            {
                if (PreviewSceneRoot != null)
                {
                    DestroyImmediate(PreviewSceneRoot.gameObject);
                    PreviewSceneRoot = null;
                }
            }


            public void DestroyModelInstance()
            {
                DestroyAnimancerInstance();

                if (InstanceRoot == null)
                    return;

                DestroyImmediate(InstanceRoot.gameObject);
                InstanceRoot = null;
                InstanceAnimators = null;
            }

            private void DestroyAnimancerInstance()
            {
                if (_Animancer == null)
                    return;

                _Animancer.CancelPostUpdate(Animations.WindowMatchStateTime.Instance);
                _Animancer.DestroyGraph();
                _Animancer = null;
            }
            #endregion

            #region Execution
            [SerializeField]
            private List<Transform> _ExpandedHierarchy;

            /// <summary>
            /// A list of all objects with their child hierarchy expanded.
            /// </summary>
            public List<Transform> ExpandedHierarchy
            {
                get
                {
                    if (_ExpandedHierarchy == null)
                        _ExpandedHierarchy = new List<Transform>();
                    return _ExpandedHierarchy;
                }
            }

            /// <summary>
            /// GUI���� -> �ֶ�����
            /// </summary>
            public void OnGUI()
            {
                // ʵ����Ԥ��ģ��
                if (!AnimancerEditorUtilities.IsChangingPlayMode && InstanceRoot == null)
                    InstantiateModel();

                if (_Animancer != null && _Animancer.IsGraphPlaying)
                    AnimancerGUI.RepaintEverything();

                // ���� ALT + F ���¾۽���Ԥ��ģ��
                if (Event.current.alt && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F)
                    FocusCamera();
            }

            /// <summary>
            /// �³�����ʱ����
            /// </summary>
            /// <param name="path">�³�����·��</param>
            /// <param name="mode">�򿪷�ʽ</param>
            private void OnSceneOpening(string path, OpenSceneMode mode)
            {
                if (mode == OpenSceneMode.Single)
                    DestroyModelInstance();
            }

            /// <summary>
            /// ������ģʽ�ı�ʱ����
            /// </summary>
            /// <param name="change"></param>
            private void OnPlayModeChanged(PlayModeStateChange change)
            {
                switch (change)
                {
                    // �˳��༭ģʽ | �˳�����ģʽ
                    case PlayModeStateChange.ExitingEditMode:
                    case PlayModeStateChange.ExitingPlayMode:
                        DestroyModelInstance();
                        break;
                }
            }

            ///// <summary>
            ///// ��SceneGUI�ϻ��� Transition ���Զ���GUI
            ///// </summary>
            ///// <param name="sceneView"></param>
            //private void DoCustomGUI(SceneView sceneView)
            //{
            //    var animancer = Animancer;
            //    if (animancer != null &&
            //        sceneView is TransitionPreviewWindow instance &&
            //        AnimancerUtilities.TryGetWrappedObject(Transition, out ITransitionGUI gui) &&
            //        instance._TransitionProperty != null)
            //    {
            //        EditorGUI.BeginChangeCheck();

            //        using (TransitionDrawer.DrawerContext.Get(instance._TransitionProperty))
            //        {
            //            try
            //            {
            //                gui.OnPreviewSceneGUI(new TransitionPreviewDetails(animancer));
            //            }
            //            catch (Exception exception)
            //            {
            //                Debug.LogException(exception);
            //            }
            //        }

            //        if (EditorGUI.EndChangeCheck())
            //            AnimancerGUI.RepaintEverything();
            //    }
            //}

            /// <summary>
            /// �Ƿ���Ԥ�������е�PreviewSceneRoot�µ�����
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public bool IsSceneObject(UnityEngine.Object obj)
            {
                return obj is GameObject gameObject && gameObject.transform.IsChildOf(PreviewSceneRoot);
            }
            #endregion
        }
    }
}
