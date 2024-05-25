using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;

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


        public abstract ITransition GetPreviewTransition();
    }


    //[CreateAssetMenu(menuName = Strings.MenuPrefix + "Animancer Transition", order = Strings.AssetMenuOrder + 0)]
    //public abstract class AnimationAssetBase<TTransition> : AnimationAssetBase
    //    where TTransition : ITransition
    //{

    //}

    //[Serializable]
    public interface IAssetHasEvents
    {
        [Serializable]
        public class AssetEvent
        {
            public string Name;
            public float NormalizedTime;
            public UnityEvent Event;
        }
        public List<AssetEvent> Events { get; }

        public AnimancerEvent.Sequence GetEventsOptional()
        {
            //Events.Sort((a, b) => a.NormalizedTime.CompareTo(b.NormalizedTime));

            var timeCount = Events.Count;
            if (timeCount == 0)
                return null;

            var _Events = new AnimancerEvent.Sequence(timeCount);

            for (int i = 0; i < timeCount; ++i)
            {
                Action callback = GetInvoker(Events[i].Event);
                _Events.Add(new AnimancerEvent(Events[i].NormalizedTime, callback));
                _Events.SetName(i, Events[i].Name);
            }

            return _Events;
        }

        public static Action GetInvoker(UnityEvent callback) => HasPersistentCalls(callback) ? callback.Invoke : AnimancerEvent.DummyCallback;

        public static bool HasPersistentCalls(UnityEvent callback)
        {
            if (callback == null)
                return false;

            // UnityEvents do not allow us to check if any dynamic calls are present.
            // But we are not giving runtime access to the events so it does not really matter.
            // UltEvents does allow it (via the HasCalls property), but we might as well be consistent.

#if ANIMANCER_ULT_EVENTS
                    var calls = callback.PersistentCallsList;
                    return calls != null && calls.Count > 0;
#else
            return callback.GetPersistentEventCount() > 0;
#endif
        }
    }

}
