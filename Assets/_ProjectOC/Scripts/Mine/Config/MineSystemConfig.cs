using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.ProNodeNS
{
    [LabelText("采矿系统配置数据"), System.Serializable]
    public struct MineSystemConfig
    {
        [LabelText("主岛的移动速度")]
        public float MainIslandSpeed;
        [LabelText("采矿圈半径")]
        public float MiningCircleRadius;

        [LabelText("岛舵台光标灵敏度")]
        public float IslandRudderSensitivity;
        [LabelText("岛舵台地图初始缩放率 （0-1）")]
        public float InitZoomRate;
        [LabelText("岛舵台地图缩放速度")]
        public float ZoomSpeed;
        [LabelText("岛舵台地图最大放大倍数 ")]
        public float ZoomInLimit;
        [LabelText("岛舵台地图最小缩小倍数")]
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
