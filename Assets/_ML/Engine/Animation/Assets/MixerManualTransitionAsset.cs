using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ML.Engine.Animation
{
    [CreateAssetMenu(menuName = "ML/Animation/Mixer/Mixer Manual Transition", order = Strings.AssetMenuOrder + 1)]
    public class MixerManualTransitionAsset : AnimationAssetBase
    {
        [Serializable]
        public new class UnShared :
            UnShared<MixerManualTransitionAsset, ManualMixerTransition, ManualMixerState>,
            ManualMixerState.ITransition
        { }


        public ManualMixerTransition manualMixerTransition;
        public override ITransition GetTransition()
        {
            return manualMixerTransition;
        }
    }

}



