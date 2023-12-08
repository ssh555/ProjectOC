using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ItemNS
{
    /// <summary>
    /// 物品
    /// </summary>
    [System.Serializable]
    public class Item
    {
        /// <summary>
        /// 物品ID，Item_类型_序号
        /// </summary>
        public string ID = "";
        /// <summary>
        /// 类目
        /// </summary>
        public ItemCategory Category;
        /// <summary>
        /// 重量，影响搬运
        /// </summary>
        public int Weight;
        /// <summary>
        /// 物品描述
        /// </summary>
        public string ItemDescription = "";
        /// <summary>
        /// 效果描述
        /// </summary>
        public string EffectsDescription = "";
        public Item(string id)
        {
            // TODO: 读表
        }
    }
}
