using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem.CompositeSystem
{
    /// <summary>
    /// �ϳ����� -> ͳһ����ֵ
    /// </summary>
    public interface IComposition
    {
        /// <summary>
        /// �ϳ��� ID
        /// </summary>
        public string ID { get; set; }
    }
}

