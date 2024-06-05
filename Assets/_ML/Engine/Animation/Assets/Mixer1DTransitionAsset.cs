using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ML.Engine.Animation
{
    [CreateAssetMenu(menuName = "ML/Animation/Mixer/Mixer 1D Transition", order = Strings.AssetMenuOrder - 1)]
    public class Mixer1DTransitionAsset : AnimationAssetBase
    {
        [Serializable]
        public new class UnShared :
            UnShared<Mixer1DTransitionAsset, LinearMixerTransition, LinearMixerState>,
            LinearMixerState.ITransition
        { }


        public LinearMixerTransition transition;
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


