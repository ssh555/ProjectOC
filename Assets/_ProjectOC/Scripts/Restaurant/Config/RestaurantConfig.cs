using Sirenix.OdinInspector;

namespace ProjectOC.RestaurantNS
{
    [LabelText("餐厅配置数据"), System.Serializable]
    public struct RestaurantConfig
    {
        [LabelText("分配一次的时间")]
        public int BroadcastTime;
        [LabelText("位置数量")]
        public int SeatNum;
        [LabelText("数据数量")] 
        public int DataNum;
        [LabelText("存储上限")] 
        public int MaxCapacity;
    }
}
