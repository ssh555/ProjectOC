using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Animation
{
    [CreateAssetMenu(menuName = "ML/Animation/Clip/Directional Clip Transition", order = Strings.AssetMenuOrder + 0)]
    public class DirectionalClipTransitionAsset : AnimationAssetBase
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<DirectionalClipTransitionAsset, ClipTransition, ClipState>,
            ClipState.ITransition
        { }

        public DirectionalClipTransition directionalClipTransition;
        public override ITransition GetTransition()
        {
            return directionalClipTransition;
        }

        public override ITransition GetPreviewTransition()
        {
            return directionalClipTransition;
        }
    }

}


