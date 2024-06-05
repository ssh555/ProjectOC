using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ML.Engine.Animation
{
    [Serializable, CreateAssetMenu(menuName = "ML/Animation/Clip/Clip Transition", order = Strings.AssetMenuOrder + 0)]
    public class ClipTransitionAsset : AnimationAssetBase, IAssetHasEvents
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<ClipTransitionAsset, ClipTransition, ClipState>,
            ClipState.ITransition
        { }

        public ClipTransition clipTransition;

        #region IAssetHasEvents
        [SerializeField]
        protected List<IAssetHasEvents.AssetEvent> _Events;
        public List<IAssetHasEvents.AssetEvent> Events => _Events;

        public float FrameLength => clipTransition.Clip.length * clipTransition.Clip.frameRate;

        [SerializeField]
        protected IAssetHasEvents.AssetEvent _EndEvent;
        public IAssetHasEvents.AssetEvent EndEvent => _EndEvent;

        #endregion

        public override ITransition GetTransition()
        {
            var events = ((IAssetHasEvents)this).GetEventsOptional();
            clipTransition.Events.CopyFrom(events);

            return clipTransition;
        }
    }

}
