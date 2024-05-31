using Animancer;
using ML.Engine.Animation;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class _Test : MonoBehaviour
{
    [SerializeField]
    public ML.Engine.Animation.ClipTransitionAsset.UnShared _clip;
    public ML.Engine.Animation.ClipTransitionAsset _modifyclip;

    public AnimancerComponent _animancer;

    private void Awake()
    {
        _clip.Events.AddCallback("TestEvent", DebugLog);
        _animancer.Play(_clip, 0, FadeMode.FromStart);
    }

    [Button("Play")]
    private void Play()
    {
        ////var state = _clip.CreateState();

        //// Actually play the animation.
        //var state = _animancer.Play(_clip);
        //state.NormalizedTime = 0;
        //state.Events.OnEnd += () =>
        //{
        //    _animancer.Playable.DestroyGraph();
        //};

        _modifyclip.clipTransition.Events.SetNormalizedTime(0, 0.23f);


    }


    public void DebugLog()
    {
        if(_animancer && _animancer.States != null && _animancer.States.Current != null)
        {
            Debug.Log(_animancer.States.Current.NormalizedTime);
        }
        else
        {
            Debug.Log("QWQ");
        }
    }
}




