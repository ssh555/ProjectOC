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
    public AnimationAssetBase _clip;
    public AnimationAssetBase _clip2;

    public AnimancerComponent _animancer;

    public AvatarMask Mask;

    private void Awake()
    {
        //_clip.Events.AddCallback("TestEvent", DebugLog);
        _animancer.Layers[0].ApplyAnimatorIK = false;
        _animancer.Layers[0].ApplyFootIK = false;
        //_animancer.Layers[0].SetMask(Mask);
        _animancer.Play(_clip, 0.25f, FadeMode.FromStart);

        //Invoke("Play", 3);
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
        _animancer.Play(_clip2, 0.25f, FadeMode.FromStart);
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




