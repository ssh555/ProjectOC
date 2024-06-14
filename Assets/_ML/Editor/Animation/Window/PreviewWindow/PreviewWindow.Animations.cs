using Animancer;
using Animancer.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Animancer.Editor.AnimancerGUI;

namespace ML.Editor.Animation
{
    partial class PreviewWindow
    {
        [Serializable]
        public class Animations
        {
            public const string PreviousAnimationKey = "Previous Animation";
            public const string NextAnimationKey = "Next Animation";

            /// <summary>
            /// Ԥ��ģ���ϵ�����AnimationClip
            /// </summary>
            [NonSerialized] private AnimationClip[] _OtherAnimations;

            [SerializeField]
            private AnimationClip _PreviousAnimation;
            public AnimationClip PreviousAnimation => _PreviousAnimation;

            [SerializeField]
            private AnimationClip _NextAnimation;
            public AnimationClip NextAnimation => _NextAnimation;


            /// <summary>
            /// �������ŵĹ�Լ��ʱ��
            /// </summary>
            [SerializeField]
            private float _NormalizedTime;
            /// <summary>
            /// �������ŵĹ�Լ��ʱ��
            /// </summary>
            public float NormalizedTime
            {
                get => _NormalizedTime;
                set
                {
                    if (!value.IsFinite())
                        return;

                    _NormalizedTime = value;
                    
                    // ���Բ��ŵ�ǰ����
                    if (!TryShowTransitionPaused(out var animancer, out var transition, out var state))
                        return;

                    // ��������
                    var length = state.Length;
                    // �����ٶ�
                    var speed = state.Speed;
                    // ��ǰ����ʱ���
                    var time = value * length;
                    // ƽ������ʱ��
                    var fadeDuration = transition.FadeDuration * Math.Abs(speed);

                    // ������ʼʱ��
                    var startTime = TimelineGUI.GetStartTime(transition.NormalizedStartTime, speed, length);
                    // ��Լ���Ľ���ʱ��
                    var normalizedEndTime = state.NormalizedEndTime;
                    // ��������ʱ��
                    var endTime = normalizedEndTime * length;
                    // ƽ�����ɽ���ʱ��
                    var fadeOutEnd = TimelineGUI.GetFadeOutEnd(speed, endTime, length);

                    // ���򲥷�
                    if (speed < 0)
                    {
                        time = length - time;
                        startTime = length - startTime;
                        value = 1 - value;
                        normalizedEndTime = 1 - normalizedEndTime;
                        endTime = length - endTime;
                        fadeOutEnd = length - fadeOutEnd;
                    }

                    // ���� Previous Animation
                    if (time < startTime)
                    {
                        if (_PreviousAnimation != null)
                        {
                            PlayOther(PreviousAnimationKey, _PreviousAnimation, value);
                            value = 0;
                        }
                    }
                    // ƽ������ Fade from previous animation to the target
                    else if (time < startTime + fadeDuration)
                    {
                        if (_PreviousAnimation != null)
                        {
                            // ����ǰһ������
                            var fromState = PlayOther(PreviousAnimationKey, _PreviousAnimation, value);
                            // ���ò���
                            state.IsPlaying = true;
                            // ����Ȩ�� -> ƽ������
                            state.Weight = (time - startTime) / fadeDuration;
                            fromState.Weight = 1 - state.Weight;
                        }
                    }
                    // ���� Next Animation
                    else if (_NextAnimation != null)
                    {
                        if (value < normalizedEndTime)
                        {
                            // Just the main state.
                        }
                        // ƽ������
                        else
                        {
                            var toState = PlayOther(NextAnimationKey, _NextAnimation, value - normalizedEndTime);
                            // Fade from the target transition to the next animation.
                            if (time < fadeOutEnd)
                            {
                                state.IsPlaying = true;
                                toState.Weight = (time - endTime) / (fadeOutEnd - endTime);
                                state.Weight = 1 - toState.Weight;
                            }
                            // Else just the next animation.
                        }
                    }

                    // ���򲥷�
                    if (speed < 0)
                    {
                        value = 1 - value;
                    }

                    state.NormalizedTime = state.Weight > 0 ? value : 0;
                    animancer.Evaluate();

                    RepaintEverything();
                }
            }

            public float Length
            {
                get
                {
                    var state = Instance._Scene.Animancer.States.Current;
                    float length = state.Clip == null ? 1 : state.Clip.length;
                    for (int i = 0; i < state.ChildCount; ++i)
                    {
                        float tmp = state.GetChild(i).Clip == null ? 1 : state.GetChild(i).Clip.length;
                        if (tmp > length)
                        {
                            length = tmp;
                        }
                    }
                    return length;
                }
            }

            public float FrameRate
            {
                get
                {
                    var state = Instance._Scene.Animancer.States.Current;
                    float frameRate = state.Clip == null ? 1 : state.Clip.frameRate;
                    for(int i = 0; i < state.ChildCount; ++i)
                    {
                        float tmp = state.GetChild(i).Clip == null ? 1 : state.GetChild(i).Clip.frameRate;
                        if(tmp > frameRate)
                        {
                            frameRate = tmp;
                        }
                    }
                    return frameRate;
                }
            }


            /// <summary>
            /// ����Ԥ��ϸ��GUI
            /// </summary>
            public void DoGUI()
            {
                GUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Preview Details", "(Not Serialized)");

                // ģ��ѡ��GUI
                DoModelGUI();
                // Animatorѡ��GUI -> ֻ��һ��Animator�Ͳ���ʾ
                DoAnimatorSelectorGUI();

                // ��������GUI
                // ǰһ������ Previous Animation
                using (ObjectPool.Disposable.AcquireContent(out var label, "Previous Animation",
                    "The animation for the preview to play before the target transition"))
                {
                    // ��ʾ Previous Animation Clip -> �ɱ༭
                    DoAnimationFieldGUI(label, ref _PreviousAnimation, (clip) => _PreviousAnimation = clip);
                }

                // ��ǰԤ���Ķ���
                var animancer = Instance._Scene.Animancer;
                DoCurrentAnimationGUI(animancer);

                // ��һ������ Next Animation
                using (ObjectPool.Disposable.AcquireContent(out var label, "Next Animation",
                    "The animation for the preview to play after the target transition"))
                {
                    // ��ʾ Next Animation Clip -> �ɱ༭
                    DoAnimationFieldGUI(label, ref _NextAnimation, (clip) => _NextAnimation = clip);
                }

                // �������ſ���
                if (animancer != null)
                {
                    using (new EditorGUI.DisabledScope(!Transition.IsValid()))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        // ���ڲ���
                        if (animancer.IsGraphPlaying)
                        {
                            // ��ͣ����
                            if (CompactMiniButton(PauseButtonContent))
                                animancer.PauseGraph();
                        }
                        // ��δ����
                        else
                        {
                            // ����һ֡
                            if (CompactMiniButton(StepBackwardButtonContent))
                                StepBackward();

                            // ���²���
                            if (CompactMiniButton(PlayButtonContent))
                                PlaySequence(animancer);

                            // ǰ��һ֡
                            if (CompactMiniButton(StepForwardButtonContent))
                                StepForward();
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndVertical();
            }

            /// <summary>
            /// ѡ��ģ��
            /// </summary>
            private void DoModelGUI()
            {
                // ѡ��ģ�͵ĸ�����(����Instance)
                var root = Instance._Scene.OriginalRoot;
                var model = root != null ? root.gameObject : null;

                EditorGUI.BeginChangeCheck();

                // model warning ��Ϣ�ַ���
                var warning = GetModelWarning(model);
                // ������Ϣ��ɫ
                var color = GUI.color;
                if (warning != null)
                    GUI.color = WarningFieldColor;

                using (ObjectPool.Disposable.AcquireContent(out var label, "Model"))
                {
                    // ģ��ѡ�������б�
                    if (DoDropdownObjectField(label, true, ref model, SpacingMode.After))
                    {
                        // �����˵�
                        var menu = new GenericMenu();

                        // Ĭ������ģ��
                        menu.AddItem(new GUIContent("Default Humanoid"), Settings.IsDefaultHumanoid(model), () => Instance._Scene.OriginalRoot = Settings.DefaultHumanoid.transform);
                        // ģ��2D Sprite
                        menu.AddItem(new GUIContent("Default Sprite"), Settings.IsDefaultSprite(model), () => Instance._Scene.OriginalRoot = Settings.DefaultSprite.transform);

                        // �����ģ��
                        var persistentModels = Settings.Models;
                        var temporaryModels = TemporarySettings.PreviewModels;
                        if (persistentModels.Count == 0 && temporaryModels.Count == 0)
                        {
                            menu.AddDisabledItem(new GUIContent("No model prefabs have been used yet"));
                        }
                        else
                        {
                            AddModelSelectionFunctions(menu, persistentModels, model);
                            AddModelSelectionFunctions(menu, temporaryModels, model);
                        }

                        // ��ʾ�˵������б�
                        menu.ShowAsContext();
                    }
                }


                // ��ģ��ѡ���޸�
                if (EditorGUI.EndChangeCheck())
                    Instance._Scene.OriginalRoot = model != null ? model.transform : null;

                // ��ʾ����
                if (warning != null)
                    EditorGUILayout.HelpBox(warning, MessageType.Warning, true);

                GUI.color = color;
            }

            private void DoAnimatorSelectorGUI()
            {
                // Ԥ��ģ�͵�ʵ���ϵ����е�Amimator
                var instanceAnimators = Instance._Scene.InstanceAnimators;
                // ֻ��һ��Animator�Ͳ���ʾ
                if (instanceAnimators == null ||
                    instanceAnimators.Length <= 1)
                    return;

                var area = LayoutSingleLineRect(SpacingMode.After);
                var labelArea = StealFromLeft(ref area, EditorGUIUtility.labelWidth, StandardSpacing);
                GUI.Label(labelArea, nameof(Animator));

                // ��ǰѡ�е�Animator
                var selectedAnimator = Instance._Scene.SelectedInstanceAnimator;

                // ѡ��Animator�����б�
                using (ObjectPool.Disposable.AcquireContent(out var label, selectedAnimator != null ? selectedAnimator.name : "None"))
                {
                    var clicked = EditorGUI.DropdownButton(area, label, FocusType.Passive);

                    if (!clicked)
                        return;

                    // �����˵�
                    var menu = new GenericMenu();

                    for (int i = 0; i < instanceAnimators.Length; i++)
                    {
                        var animator = instanceAnimators[i];
                        var index = i;
                        menu.AddItem(new GUIContent(animator.name), animator == selectedAnimator, () =>
                        {
                            // ѡ��ѡ���Animator
                            Instance._Scene.SetSelectedAnimator(index);
                            // ������ʱ������Ϊ0
                            NormalizedTime = 0;
                        });
                    }
                    // ��ʾ�˵�
                    menu.ShowAsContext();
                }
            }

            /// <summary>
            /// ��ȡmodel��warning��Ϣ�ַ���
            /// </summary>
            /// <param name="model"></param>
            /// <returns></returns>
            private string GetModelWarning(GameObject model)
            {
                if (model == null)
                    return "No Model is selected so nothing can be previewed.";

                if (Instance._Scene.Animancer == null)
                    return "The selected Model has no Animator component.";

                return null;
            }

            /// <summary>
            /// �����б�
            /// </summary>
            /// <typeparam name="T">DropDown -> TObject</typeparam>
            /// <param name="label">DropDownText</param>
            /// <param name="showDropdown">�Ƿ�Ϊ�����б�</param>
            /// <param name="obj">DropDownSelectObject</param>
            /// <param name="spacingMode"></param>
            /// <returns></returns>
            private static bool DoDropdownObjectField<T>(GUIContent label, bool showDropdown, ref T obj, SpacingMode spacingMode = SpacingMode.None) where T : UnityEngine.Object
            {
                var area = LayoutSingleLineRect(spacingMode);

                var labelWidth = EditorGUIUtility.labelWidth;

                labelWidth += 2;
                area.xMin -= 1;

                var spacing = StandardSpacing;
                var labelArea = StealFromLeft(ref area, labelWidth - spacing, spacing);

                // ��ǰѡ�е�model
                obj = (T)EditorGUI.ObjectField(area, obj, typeof(T), true);

                // �����б�
                if (showDropdown)
                {
                    return EditorGUI.DropdownButton(labelArea, label, FocusType.Passive);
                }
                // Label
                else
                {
                    GUI.Label(labelArea, label);
                    return false;
                }
            }

            /// <summary>
            /// ��menu���������ѡ��
            /// </summary>
            /// <param name="menu"></param>
            /// <param name="models"></param>
            /// <param name="selected"></param>
            private static void AddModelSelectionFunctions(GenericMenu menu, List<GameObject> models, GameObject selected)
            {
                for (int i = models.Count - 1; i >= 0; i--)
                {
                    var model = models[i];
                    var path = AssetDatabase.GetAssetPath(model);
                    // �ʲ�
                    if (!string.IsNullOrEmpty(path))
                        path = path.Replace('/', '\\');
                    // ����������
                    else
                        path = model.name;

                    // �������ѡ��
                    menu.AddItem(new GUIContent(path), model == selected, () => Instance._Scene.OriginalRoot = model.transform);
                }
            }

            /// <summary>
            /// ���Բ��ŵ�ǰ��������ͣ
            /// </summary>
            /// <param name="animancer">���ڲ��Ŷ�����AnimancerPlayable</param>
            /// <param name="transition">���ŵ�Ԥ������</param>
            /// <param name="state">���Ŷ�����AnimancerState</param>
            /// <returns></returns>
            private bool TryShowTransitionPaused(out AnimancerPlayable animancer, out ITransitionDetailed transition, out AnimancerState state)
            {
                // ���ڲ���Ԥ��������Animancer
                animancer = Instance._Scene.Animancer;
                // Ԥ���Ķ���
                transition = Transition as ITransitionDetailed;

                // ��ǰ����Ԥ������
                if (animancer == null || !transition.IsValid())
                {
                    state = null;
                    return false;
                }

                // ���ŵ�ǰ����
                state = animancer.Play(transition, 0);
                OnPlayAnimation();
                animancer.PauseGraph();
                return true;
            }

            /// <summary>
            /// �½���������Playable,���Event(Ԥ��ʱ��ִ��)
            /// </summary>
            public void OnPlayAnimation()
            {
                var animancer = Instance._Scene.Animancer;
                if (animancer == null || animancer.States.Current == null)
                    return;

                var state = animancer.States.Current;

                state.RecreatePlayableRecursive();

                if (state.HasEvents)
                {
                    // ����
                    var warnings = OptionalWarning.UnsupportedEvents.DisableTemporarily();
                    //var normalizedEndTime = state.Events.NormalizedEndTime;
                    var end = state.Events.EndEvent;
                    // ����¼� -> Ԥ������ʱ��ִ��Event
                    state.Events = null;
                    // �ָ���ǰ����ʱ��
                    //state.Events.NormalizedEndTime = normalizedEndTime;
                    state.Events.EndEvent = end;
                    warnings.Enable();
                }
            }

            /// <summary>
            /// ���Ŷ��� -> ���ڲ���ǰһ�������ͺ�һ������
            /// </summary>
            /// <param name="key"></param>
            /// <param name="animation"></param>
            /// <param name="normalizedTime"></param>
            /// <param name="fadeDuration"></param>
            /// <returns></returns>
            private AnimancerState PlayOther(object key, AnimationClip animation, float normalizedTime, float fadeDuration = 0)
            {
                var animancer = Instance._Scene.Animancer;
                // ���Ŷ�����AnimancerState
                var state = animancer.States.GetOrCreate(key, animation, true);
                // ���Ŷ���
                state = animancer.Play(state, fadeDuration);
                // Ԥ���Ķ������Ŵ���
                OnPlayAnimation();

                // state ��ʼ���ŵ�ʵ��ʱ��
                normalizedTime *= state.Length;
                state.Time = normalizedTime.IsFinite() ? normalizedTime : 0;

                return state;
            }

            /// <summary>
            /// ��ʾ Animation Clip Field GUI
            /// ��OtherAnimations��Ϊnull����ɽ���������ѡ��
            /// </summary>
            /// <param name="label"></param>
            /// <param name="clip"></param>
            /// <param name="setClip"></param>
            private void DoAnimationFieldGUI(GUIContent label, ref AnimationClip clip, Action<AnimationClip> setClip)
            {
                // �Ƿ���ʾ�����б�-> ��������Ϊnull����ʾ
                var showDropdown = !_OtherAnimations.IsNullOrEmpty();

                if (DoDropdownObjectField(label, showDropdown, ref clip))
                {
                    var menu = new GenericMenu();

                    menu.AddItem(new GUIContent("None"), clip == null, () => setClip(null));

                    for (int i = 0; i < _OtherAnimations.Length; i++)
                    {
                        var animation = _OtherAnimations[i];
                        menu.AddItem(new GUIContent(animation.name), animation == clip, () => setClip(animation));
                    }

                    menu.ShowAsContext();
                }
            }

            /// <summary>
            /// ��ʾ��ǰ������GUI
            /// </summary>
            /// <param name="animancer"></param>
            private void DoCurrentAnimationGUI(AnimancerPlayable animancer)
            {
                string text;

                if (animancer != null)
                {
                    // ��ǰ����
                    var transition = Transition;
                    if (transition.IsValid() && transition.Key != null)
                        text = animancer.States.GetOrCreate(transition).ToString();
                    else
                        text = transition.ToString();
                }
                else
                {
                    text = Instance._selectedAsset.name;
                }

                if (text != null)
                    EditorGUILayout.LabelField("Current Animation", text);
            }

            /// <summary>
            /// ���˶���һ֡
            /// </summary>
            public void StepBackward()
                => StepTime(-(1 / FrameRate));

            /// <summary>
            /// ǰ������һ֡
            /// </summary>
            public void StepForward()
                => StepTime((1 / FrameRate));

            /// <summary>
            /// ���ڵ�ǰ��������ʱ�䲽��
            /// </summary>
            /// <param name="timeOffset"></param>
            private void StepTime(float timeOffset)
            {
                // ���Բ���
                if (!TryShowTransitionPaused(out _, out _, out var state))
                    return;

                var length = state.Length;
                // ����Ϊ��Լ��ʱ��
                if (length != 0)
                    timeOffset /= length;

                // Ӧ�õ�ǰTimeOffset
                NormalizedTime += timeOffset;
            }

            /// <summary>
            /// �ص���һ֡
            /// </summary>
            public void StepToFirstKey()
            {
                // ���Բ���
                if (!TryShowTransitionPaused(out _, out _, out var state))
                    return;

                // Ӧ�õ�ǰTimeOffset
                NormalizedTime = 0;
            }

            /// <summary>
            /// �����һ֡
            /// </summary>
            public void StepToLastKey()
            {
                // ���Բ���
                if (!TryShowTransitionPaused(out _, out _, out var state))
                    return;

                // Ӧ�õ�ǰTimeOffset
                NormalizedTime = 1;
            }

            /// <summary>
            /// ���� Previous Animation ���ŵ�ǰ����
            /// </summary>
            /// <param name="animancer"></param>
            public void PlaySequence(AnimancerPlayable animancer)
            {
                if (_PreviousAnimation != null && _PreviousAnimation.length > 0)
                {
                    // ��ͣ��ǰ����
                    Instance._Scene.Animancer.Stop();
                    // Previous Animancer State
                    var fromState = animancer.States.GetOrCreate(PreviousAnimationKey, _PreviousAnimation, true);
                    // ���� Previous
                    animancer.Play(fromState);
                    OnPlayAnimation();
                    fromState.TimeD = 0;

                    var warnings = OptionalWarning.UnsupportedEvents.DisableTemporarily();
                    // Previous ���Ž���ʱ���ŵ�ǰ����
                    fromState.Events.EndEvent = new AnimancerEvent(1 / fromState.Length, PlayTransition);
                    warnings.Enable();
                }
                // ֱ�Ӳ��ŵ�ǰ����
                else
                {
                    PlayTransition();
                }

                // ����
                Instance._Scene.Animancer.UnpauseGraph();
            }

            /// <summary>
            /// ���ŵ�ǰ����
            /// </summary>
            private void PlayTransition()
            {
                var transition = Transition;
                var animancer = Instance._Scene.Animancer;
                // ��ȡ�ɵĲ���״̬
                animancer.States.TryGet(transition, out var oldState);

                // ���پ�״̬
                if (oldState != null)
                    oldState.Destroy();

                // �����µĶ�����״̬
                var targetState = animancer.Play(transition);
                if (_PreviousAnimation == null || _PreviousAnimation.length == 0)
                {
                    targetState.Weight = 1;
                }

                OnPlayAnimation();


                // ����
                var warnings = OptionalWarning.UnsupportedEvents.DisableTemporarily();
                //targetState.NormalizedTime = 0;
                // ���Ž����¼�
                //targetState.NormalizedEndTime = 1;
                targetState.Events.OnEnd = () =>
                {
                    // ������һ������ Next Animation
                    if (_NextAnimation != null)
                    {
                        var fadeDuration = AnimancerEvent.GetFadeOutDuration(targetState, AnimancerPlayable.DefaultFadeDuration);
                        PlayOther(NextAnimationKey, _NextAnimation, 0, fadeDuration);
                        OnPlayAnimation();
                    }
                    else
                    {
                        //// TODO: ���ɷ���
                        //animancer.Layers[0].IncrementCommandCount();
                        animancer.PauseGraph();
                    }
                };
                warnings.Enable();
            }
            /// <summary>
            /// �� OriginalRoot �������ռ� AnimationClip
            /// ������Ĭ�ϵ� Previous || Next Animation
            /// </summary>
            public void GatherAnimations()
            {
                AnimationGatherer.GatherFromGameObject(Instance._Scene.OriginalRoot.gameObject, ref _OtherAnimations, true);

                if (_OtherAnimations.Length > 0 && (_PreviousAnimation == null || _NextAnimation == null))
                {
                    var defaultClip = _OtherAnimations[0];
                    var defaultClipIsIdle = false;

                    for (int i = 0; i < _OtherAnimations.Length; i++)
                    {
                        var clip = _OtherAnimations[i];

                        if (defaultClipIsIdle && clip.name.Length > defaultClip.name.Length)
                            continue;

                        if (clip.name.IndexOf("idle", StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            defaultClip = clip;
                            break;
                        }
                    }

                    if (_PreviousAnimation == null)
                        _PreviousAnimation = defaultClip;
                    if (_NextAnimation == null)
                        _NextAnimation = defaultClip;
                }
            }

            /// <summary>
            /// �õ�ǰ״̬�Ĳ���ʱ����Animations�еĹ�Լ��ʱ��ƥ��
            /// </summary>
            internal class WindowMatchStateTime : Key, IUpdatable
            {
                public static readonly WindowMatchStateTime Instance = new WindowMatchStateTime();

                /// <summary>
                /// ÿ֡����
                /// </summary>
                void IUpdatable.Update()
                {
                    //if (PreviewWindow.Instance == null ||
                    //    !AnimancerPlayable.Current.IsGraphPlaying)
                    //    return;
                    if (PreviewWindow.Instance == null)
                        return;

                    var transition = Transition;
                    if (transition == null)
                        return;

                    if (AnimancerPlayable.Current.States.TryGet(transition, out var state))
                    {
                        PreviewWindow.Instance._Animations._NormalizedTime = state.NormalizedTime;
                        if(state.IsPlaying)
                        {
                            TrackWindow.Instance.Repaint();
                        }
                    }

                }
            }
        }
    }
}

