using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    public interface IEffectObj
    {
        public List<Effect> Effects { get; set; }
        public void ApplyEffect(Effect effect);
        public void RemoveEffect(Effect effect);
    }
}
