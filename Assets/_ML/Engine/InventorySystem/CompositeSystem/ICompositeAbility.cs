using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem.CompositeSystem
{
    /// <summary>
    /// 拥有合成能力的接口
    /// </summary>
    public interface CompositeAbility
    {
        public CompositeAbility compositeAbility { get => (this as CompositeAbility); }

        /// <summary>
        /// 所用资源仓库
        /// </summary>
        public IInventory ResourceInventory { get;}

        /// <summary>
        /// 能否合成对应物品
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="compositonID"></param>
        /// <returns></returns>
        public bool CanComposite(string compositonID)
        {
            return CompositeManager.Instance.CanComposite(this.ResourceInventory, compositonID);
        }

        /// <summary>
        /// 合成物品
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="compositonID"></param>
        /// <returns></returns>
        public virtual CompositeManager.CompositionObjectType Composite(string compositonID, out IComposition composition)
        {
            return CompositeManager.Instance.Composite(this.ResourceInventory, compositonID, out composition);
        }

    }
}

