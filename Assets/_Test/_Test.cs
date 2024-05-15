using Animancer;
using Animancer.Editor;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class _Test : MonoBehaviour
{
    [SerializeField]
    public AnimancerTransitionAssetBase _clip;

    public AnimancerComponent _animancer;

    private void Awake()
    {
        _animancer.Play(_clip, 0, FadeMode.FromStart);
    }

    [Button("Play")]
    private void Play()
    {
        //var state = _clip.CreateState();

        // Actually play the animation.
        _animancer.Play(_clip).NormalizedTime = 0;
    }
}
