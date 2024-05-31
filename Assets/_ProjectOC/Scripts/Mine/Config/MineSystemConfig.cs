using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Numerics;

namespace ProjectOC.ProNodeNS
{
    [LabelText("采矿系统配置数据"), System.Serializable]
    public struct MineSystemConfig
    {
        [LabelText("主岛的移动速度")]
        public float MainIslandSpeed;
        [LabelText("主岛的初始位置 大地图中的坐标")]
        public UnityEngine.Vector2 MainIslandInitPos;

        [LabelText("采矿圈半径")]
        public float MiningCircleRadius;

        [LabelText("岛舵台光标灵敏度")]
        public float IslandRudderSensitivity;
        [LabelText("岛舵台地图初始缩放率 （0-1）")]
        public float IslandRudderInitZoomRate;
        [LabelText("岛舵台地图缩放速度")]
        public float IslandRudderZoomSpeed;
        [LabelText("岛舵台地图最大放大倍数 ")]
        public float IslandRudderZoomInLimit;
        [LabelText("岛舵台地图最小缩小倍数")]
        public float IslandRudderZoomOutLimit;

        [LabelText("潜航器大地图光标灵敏度")]
        public float SelectMineralSourcesSensitivity;
        [LabelText("潜航器大地图界面初始缩放率 （0-1）")]
        public float SelectMineralSourcesInitZoomRate;
        [LabelText("潜航器大地图缩放速度")]
        public float SelectMineralSourcesZoomSpeed;
        [LabelText("潜航器大地图最大放大倍数 ")]
        public float SelectMineralSourcesZoomInLimit;
        [LabelText("潜航器大地图最小缩小倍数")]
        public float SelectMineralSourcesZoomOutLimit;

        [LabelText("潜航器小地图光标灵敏度")]
        public float SmallMapSensitivity;
        [LabelText("潜航器小地图界面初始缩放率 （0-1）")]
        public float SmallMapInitZoomRate;
        [LabelText("潜航器小地图缩放速度")]
        public float SmallMapZoomSpeed;
        [LabelText("潜航器小地图最大放大倍数 ")]
        public float SmallMapZoomInLimit;
        [LabelText("潜航器小地图最小缩小倍数")]
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
