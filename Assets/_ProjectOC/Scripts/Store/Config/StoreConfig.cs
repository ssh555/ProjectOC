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
    }
}
