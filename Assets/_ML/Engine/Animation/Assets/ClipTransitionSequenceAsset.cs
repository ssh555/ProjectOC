using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Animation
{
    [CreateAssetMenu(menuName = "ML/Animation/Clip/Clip Transition Sequence", order = Strings.AssetMenuOrder + 0)]
    public class ClipTransitionSequenceAsset : AnimationAssetBase
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<ClipTransitionSequenceAsset, ClipTransitionSequence, ClipState>,
            ClipState.ITransition
        { }

        public ClipTransitionSequence clipTransitionSequence;
        public override ITransition GetTransition()
        {
            return clipTransitionSequence;
        }
    }

}
