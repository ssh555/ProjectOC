using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem.CompositeSystem
{
    /// <summary>
    /// 合成物标记 -> 统一返回值
    /// </summary>
    public interface IComposition
    {
        /// <summary>
        /// 合成物 ID
        /// </summary>
        public string ID { get; set; }
    }
}

