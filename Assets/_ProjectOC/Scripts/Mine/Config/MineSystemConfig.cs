using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Numerics;

namespace ProjectOC.ProNodeNS
{
    [LabelText("�ɿ�ϵͳ��������"), System.Serializable]
    public struct MineSystemConfig
    {
        [LabelText("�������ƶ��ٶ�")]
        public float MainIslandSpeed;
        [LabelText("�����ĳ�ʼλ�� ���ͼ�е�����")]
        public UnityEngine.Vector2 MainIslandInitPos;

        [LabelText("�ɿ�Ȧ�뾶")]
        public float MiningCircleRadius;

        [LabelText("����̨���������")]
        public float IslandRudderSensitivity;
        [LabelText("����̨��ͼ��ʼ������ ��0-1��")]
        public float IslandRudderInitZoomRate;
        [LabelText("����̨��ͼ�����ٶ�")]
        public float IslandRudderZoomSpeed;
        [LabelText("����̨��ͼ���Ŵ��� ")]
        public float IslandRudderZoomInLimit;
        [LabelText("����̨��ͼ��С��С����")]
        public float IslandRudderZoomOutLimit;

        [LabelText("Ǳ�������ͼ���������")]
        public float SelectMineralSourcesSensitivity;
        [LabelText("Ǳ�������ͼ�����ʼ������ ��0-1��")]
        public float SelectMineralSourcesInitZoomRate;
        [LabelText("Ǳ�������ͼ�����ٶ�")]
        public float SelectMineralSourcesZoomSpeed;
        [LabelText("Ǳ�������ͼ���Ŵ��� ")]
        public float SelectMineralSourcesZoomInLimit;
        [LabelText("Ǳ�������ͼ��С��С����")]
        public float SelectMineralSourcesZoomOutLimit;

        [LabelText("Ǳ����С��ͼ���������")]
        public float SmallMapSensitivity;
        [LabelText("Ǳ����С��ͼ�����ʼ������ ��0-1��")]
        public float SmallMapInitZoomRate;
        [LabelText("Ǳ����С��ͼ�����ٶ�")]
        public float SmallMapZoomSpeed;
        [LabelText("Ǳ����С��ͼ���Ŵ��� ")]
        public float SmallMapZoomInLimit;
        [LabelText("Ǳ����С��ͼ��С��С����")]
        public float SmallMapZoomOutLimit;

        public MineSystemConfig(MineSystemConfig config)
        {
            MainIslandSpeed = config.MainIslandSpeed;
            MainIslandInitPos = config.MainIslandInitPos;
            MiningCircleRadius = config.MiningCircleRadius;
            IslandRudderSensitivity = config.IslandRudderSensitivity;

            IslandRudderInitZoomRate = config.IslandRudderInitZoomRate;
            IslandRudderZoomSpeed = config.IslandRudderZoomSpeed;
            IslandRudderZoomInLimit = config.IslandRudderZoomInLimit;
            IslandRudderZoomOutLimit = config.IslandRudderZoomOutLimit;

            SelectMineralSourcesSensitivity = config.SelectMineralSourcesSensitivity;
            SelectMineralSourcesInitZoomRate = config.SelectMineralSourcesInitZoomRate;
            SelectMineralSourcesZoomSpeed = config.SelectMineralSourcesZoomSpeed;
            SelectMineralSourcesZoomInLimit = config.SelectMineralSourcesZoomInLimit;
            SelectMineralSourcesZoomOutLimit = config .SelectMineralSourcesZoomOutLimit;

            SmallMapSensitivity = config.SmallMapSensitivity;
            SmallMapInitZoomRate = config.SmallMapInitZoomRate;
            SmallMapZoomSpeed = config.SmallMapZoomSpeed;
            SmallMapZoomInLimit = config.SmallMapZoomInLimit;
            SmallMapZoomOutLimit = config.SmallMapZoomOutLimit;
        }
    }

}
