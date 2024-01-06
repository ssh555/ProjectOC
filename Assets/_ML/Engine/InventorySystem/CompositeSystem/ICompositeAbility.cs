using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem.CompositeSystem
{
    /// <summary>
    /// ӵ�кϳ������Ľӿ�
    /// </summary>
    public interface CompositeAbility
    {
        public CompositeAbility compositeAbility { get => (this as CompositeAbility); }

        /// <summary>
        /// ������Դ�ֿ�
        /// </summary>
        public IInventory ResourceInventory { get;}

        /// <summary>
        /// �ܷ�ϳɶ�Ӧ��Ʒ
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="compositonID"></param>
        /// <returns></returns>
        public bool CanComposite(string compositonID)
        {
            return CompositeManager.Instance.CanComposite(this.ResourceInventory, compositonID);
        }

        /// <summary>
        /// �ϳ���Ʒ
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

