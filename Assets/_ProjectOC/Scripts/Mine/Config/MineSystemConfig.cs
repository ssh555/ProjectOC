using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ProNodeNS
{
    [LabelText("�ɿ�ϵͳ��������"), System.Serializable]
    public struct MineSystemConfig
    {
        [LabelText("�������ƶ��ٶ�")]
        public float MainIslandSpeed;
        [LabelText("�ɿ�Ȧ�뾶")]
        public float MiningCircleRadius;

        [LabelText("����̨���������")]
        public float IslandRudderSensitivity;
        [LabelText("����̨��ͼ��ʼ������ ��0-1��")]
        public float InitZoomRate;
        [LabelText("����̨��ͼ�����ٶ�")]
        public float ZoomSpeed;
        [LabelText("����̨��ͼ���Ŵ��� ")]
        public float ZoomInLimit;
        [LabelText("����̨��ͼ��С��С����")]
        public float ZoomOutLimit;

        public MineSystemConfig(MineSystemConfig config)
        {
            MainIslandSpeed = config.MainIslandSpeed;
            MiningCircleRadius = config.MiningCircleRadius;
            IslandRudderSensitivity = config.IslandRudderSensitivity;

            InitZoomRate = config.InitZoomRate;
            ZoomSpeed = config.ZoomSpeed;
            ZoomInLimit = config.ZoomInLimit;
            ZoomOutLimit = config.ZoomOutLimit;
        }
    }

}
