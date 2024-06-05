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


        public MixerTransition2D mixerTransition2D;
        public override ITransition GetTransition()
        {
            return mixerTransition2D;
        }
        public override ITransition GetPreviewTransition()
        {
            return mixerTransition2D;
        }
    }

}


