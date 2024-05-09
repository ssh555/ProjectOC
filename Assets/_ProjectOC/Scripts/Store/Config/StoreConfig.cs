using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.StoreNS
{
    [LabelText("仓库配置数据"), System.Serializable]
    public struct StoreConfig
    {
        [LabelText("仓库最大等级")]
        public int LevelMax;
        [LabelText("每个级别的仓库容量")]
        public List<int> LevelCapacity;
        [LabelText("每个级别的仓库数据容量")]
        public List<int> LevelDataCapacity;
    }
}
