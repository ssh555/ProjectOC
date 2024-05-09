using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.StoreNS
{
    [LabelText("�ֿ���������"), System.Serializable]
    public struct StoreConfig
    {
        [LabelText("�ֿ����ȼ�")]
        public int LevelMax;
        [LabelText("ÿ������Ĳֿ�����")]
        public List<int> LevelCapacity;
        [LabelText("ÿ������Ĳֿ���������")]
        public List<int> LevelDataCapacity;

        public StoreConfig(StoreConfig config)
        {
            LevelMax = config.LevelMax;
            LevelCapacity = new List<int>();
            LevelCapacity.AddRange(config.LevelCapacity);
            LevelDataCapacity = new List<int>();
            LevelDataCapacity.AddRange(config.LevelDataCapacity);
        }
    }
}
