using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ML.Engine.Animation
{
    public abstract class AnimationAssetBase : AnimancerTransitionAssetBase
    {
        /// <inheritdoc/>
        [Serializable]
        public new class UnShared<TAsset, TTransition, TState> : UnShared<TAsset>, ITransition<TState>
            where TAsset : AnimancerTransitionAssetBase
            where TTransition : ITransition<TState>, IHasEvents
            where TState : AnimancerState
        {

            protected override void OnSetBaseState()
            {
                base.OnSetBaseState();
                if (_State != BaseState)
                    _State = null;
            }

            private TState _State;
            public TState State
            {
                get
                {
                    if (_State == null)
                        _State = (TState)BaseState;

                    return _State;
                }
                protected set
                {
                    BaseState = _State = value;
                }
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override ref AnimancerEvent.Sequence.Serializable SerializedEvents
                => ref ((TTransition)Asset.GetTransition()).SerializedEvents;

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public virtual TState CreateState()
                => State = (TState)Asset.CreateState();

            /************************************************************************************************************************/
        }

    }


    //[CreateAssetMenu(menuName = Strings.MenuPrefix + "Animancer Transition", order = Strings.AssetMenuOrder + 0)]
    //public abstract class AnimationAssetBase<TTransition> : AnimationAssetBase
    //    where TTransition : ITransition
    //{

    //}

}
