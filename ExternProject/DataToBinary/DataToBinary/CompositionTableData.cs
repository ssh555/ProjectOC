using ML.Engine.InventorySystem.CompositeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Engine.InventorySystem.CompositeSystem
{
    [System.Serializable]
    public class CompositionTableData
    {
        /// <summary>
        /// 合成对象 -> Item | 建筑物 ID 引用
        /// </summary>
        public string id;

        /// <summary>
        /// 合成物名称
        /// </summary>
        public TextContent.TextContent name;

        /// <summary>
        /// 标签分级
        /// 1级|2级|3级
        /// </summary>
        public string[] tag; // Category

        /// <summary>
        /// 合成公式
        /// 没有 num 则默认为 1
        /// num 目前仅限 item
        /// num = <1,2> | <1,2>
        /// 1 -> ID 
        /// 2 -> Num
        /// </summary>
        public Formula[] formula;
        /// <summary>
        /// 一次可合成数量
        /// </summary>
        public int compositionnum;

        public string texture2d;

        public List<string> usage;
    }

}
