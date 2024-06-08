using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Animation
{
    [Serializable, CreateAssetMenu(menuName = "ML/Animation/Clip/Slot Transition", order = Strings.AssetMenuOrder + 0)]
    public class SlotClipTransitionAsset : AnimationAssetBase, IAssetHasEvents
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared :
            UnShared<ClipTransitionAsset, SlotClipTransition, ClipState>,
            ClipState.ITransition
        { }

        public SlotClipTransition transition;
        public override ITransition GetTransition()
        {
            var events = ((IAssetHasEvents)this).GetEventsOptional();
            transition.Events.CopyFrom(events);

            return transition;
        }

        public override ITransition GetPreviewTransition()
        {
            return transition;
        }

        #region IAssetHasEvents
        [SerializeField]
        protected List<IAssetHasEvents.AssetEvent> _Events;
        public List<IAssetHasEvents.AssetEvent> Events => _Events;

        public override float FrameRate
        {
            get
            {
                return transition.Clip == null ? 24 : transition.Clip.frameRate;
            }
        }

        public override float Length
        {
            get
            {
                return transition.Clip == null ? 0.01f : transition.Clip.length;
            }
        }


        [SerializeField]
        protected IAssetHasEvents.AssetEvent _EndEvent;
        public IAssetHasEvents.AssetEvent EndEvent => _EndEvent;

        #endregion
    }

    [Serializable]
    public class SlotClipTransition : ClipTransition
    {
        [SerializeField]
        private AvatarMask _slot;
        public ref AvatarMask Slot => ref _slot;

        public override void Apply(AnimancerState state)
        {
            base.Apply(state);
            BaseState.Layer.SetMask(_slot);
            BaseState.Events.OnEnd += OnSlotEnd;
        }

        private void OnSlotEnd()
        {
            BaseState.Layer.SetMask(null);
        }
    }

}
