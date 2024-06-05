using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Animation
{
    [CreateAssetMenu(menuName = "ML/Animation/Mixer/Mixer 2D Transition", order = Strings.AssetMenuOrder + 0)]
    public class Mixer2DTransitionAsset : AnimationAssetBase
    {
        [Serializable]
        public new class UnShared :
            UnShared<Mixer2DTransitionAsset, MixerTransition2D, MixerState<Vector2>>,
            ManualMixerState.ITransition2D
        { }


        public MixerTransition2D transition;
        public override ITransition GetTransition()
        {
            return transition;
        }
        public override ITransition GetPreviewTransition()
        {
            return transition;
        }
    }

}


