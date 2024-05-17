using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ML.Engine.Animation
{
    [CreateAssetMenu(menuName = "ML/Animation/Clip/Clip Transition", order = Strings.AssetMenuOrder + 0)]
    public class ClipTransitionAsset : AnimationAssetBase
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<ClipTransitionAsset, ClipTransition, ClipState>,
            ClipState.ITransition
        { }

        public ClipTransition clipTransition;
        public override ITransition GetTransition()
        {
            return clipTransition;
        }
    }

}
