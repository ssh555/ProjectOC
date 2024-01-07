namespace ProjectOC.ProductionNodeNS
{
    /// <summary>
    /// 生产节点类目
    /// </summary>
    public enum ProductionNodeCategory
    {
        /// <summary>
        /// 厨房
        /// 生产刁民食物
        /// </summary>
        Feed = 0,
        /// <summary>
        /// 酿造
        /// 生产酒，饮料，药品等，包含发酵的含义
        /// </summary>
        Brew = 1,
        /// <summary>
        /// 纺织
        /// 生产布艺和与外观相关的物品
        /// </summary>
        Textile = 2,
        /// <summary>
        /// 采矿
        /// 生产矿类物品
        /// </summary>
        Mine = 3,
        /// <summary>
        /// 冶炼
        /// 将矿类物品转化为金属制品
        /// </summary>
        Smelt = 4,
        /// <summary>
        /// 化工
        /// 生产工业产品，主要为现代工业材料
        /// </summary>
        Chemical = 5
    }
}

