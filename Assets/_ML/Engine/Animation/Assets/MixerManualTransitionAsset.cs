using Animancer;
using Animancer.Editor;
using Animancer.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;


namespace ML.Engine.Animation
{
    [CreateAssetMenu(menuName = "ML/Animation/Mixer/Mixer Manual Transition", order = Strings.AssetMenuOrder + 1)]
    public class MixerManualTransitionAsset : AnimationAssetBase, IAssetHasEvents
    {
        [Serializable]
        public new class UnShared :
            UnShared<MixerManualTransitionAsset, ManualMixerTransition, ManualMixerState>,
            ManualMixerState.ITransition
        { }


        public WeightMixerTransition manualMixerTransition;
        public override ITransition GetTransition()
        {
            var events = ((IAssetHasEvents)this).GetEventsOptional();
            manualMixerTransition.Events.CopyFrom(events);

            return manualMixerTransition;
        }

        public override ITransition GetPreviewTransition()
        {
            return manualMixerTransition;
        }


        #region IAssetHasEvents
        [SerializeField]
        protected List<IAssetHasEvents.AssetEvent> _Events;
        public List<IAssetHasEvents.AssetEvent> Events => _Events;

        public float FrameRate
        {
            get
            {
                if (manualMixerTransition.Animations == null)
                    return 0;

                var framerate = 0f;

                for (int i = manualMixerTransition.Animations.Length - 1; i >= 0; i--)
                {
                    if (!TryGetFrameRate(manualMixerTransition.Animations[i], out var frame))
                        continue;

                    if (framerate < frame)
                        framerate = frame;
                }

                return framerate;
            }
        }

        public float Length
        {
            get
            {
                if (manualMixerTransition.Animations == null)
                    return 0;

                var duration = 0f;

                for (int i = manualMixerTransition.Animations.Length - 1; i >= 0; i--)
                {
                    if (!AnimancerUtilities.TryGetLength(manualMixerTransition.Animations[i], out var length))
                        continue;

                    if (duration < length)
                        duration = length;
                }

                return duration;
            }
        }

        public float FrameLength => Length * FrameRate;

        [SerializeField]
        protected IAssetHasEvents.AssetEvent _EndEvent;
        public IAssetHasEvents.AssetEvent EndEvent => _EndEvent;

        #endregion

    }

    [Serializable]
    public class WeightMixerTransition : ManualMixerTransition
    {
        [SerializeField]
        [DefaultValue(0f, 1f)]
        private float[] _Weights;

        /// <summary>[<see cref="SerializeField"/>]
        /// The <see cref="AnimancerNode.Speed"/> to use for each state in the mixer.
        /// </summary>
        /// <remarks>If the size of this array doesn't match the <see cref="Animations"/>, it will be ignored.</remarks>
        public ref float[] Weights => ref _Weights;

        public override void InitializeState()
        {
            base.InitializeState();
            var mixer = State;
            var childCount = mixer.ChildCount;
            for (int i = 0; i < childCount; ++i)
            {
                mixer.GetChild(i).Weight = (_Weights != null && i < _Weights.Length) ? _Weights[i] : 1f / childCount;
            }

        }

        public void CopyFrom(WeightMixerTransition copyFrom)
        {
            CopyFrom((ManualMixerTransition<ManualMixerState>)copyFrom);
            Array.Copy(copyFrom.Weights, this._Weights, copyFrom.Weights.Length);
        }
    }

}



