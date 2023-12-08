using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.ItemNS
{
    /// <summary>
    /// 配方
    /// </summary>
    [System.Serializable]
    public class Recipe
    {
        /// <summary>
        /// ID，Recipe_类型_序号
        /// </summary>
        public string ID = "";
        /// <summary>
        /// 类目
        /// </summary>
        public ItemCategory Category;
        /// <summary>
        /// 原料
        /// </summary>
        public Dictionary<string, int> RawItems = new Dictionary<string, int>();
        /// <summary>
        /// 成品
        /// </summary>
        public string ProductItems = "";
        /// <summary>
        /// 时间消耗，进行1次生产需要多少秒
        /// </summary>
        public int TimeCost = 1;
        /// <summary>
        /// 配方经验，该配方制作完成时可获得的经验值
        /// </summary>
        public int ExpRecipe;
        public Recipe(string id)
        {
            // TODO: 读表拿数据
        }
    }
}
