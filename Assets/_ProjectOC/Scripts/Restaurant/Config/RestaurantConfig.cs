using Sirenix.OdinInspector;

namespace ProjectOC.RestaurantNS
{
    [LabelText("������������"), System.Serializable]
    public struct RestaurantConfig
    {
        [LabelText("����һ�ε�ʱ��")]
        public int BroadcastTime;
        [LabelText("λ������")]
        public int SeatNum;
        [LabelText("��������")] 
        public int DataNum;
        [LabelText("�洢����")] 
        public int MaxCapacity;
    }
}
