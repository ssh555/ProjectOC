using ML.Engine.BuildingSystem.BuildingPart;
using System.Collections.Generic;

namespace ML.Engine.BuildingSystem
{
    public interface IBuildingUpgrade : IBuildingPart
    {
        /// <summary>
        /// 是否能够升级
        /// </summary>
        public bool HasUpgrade()
        {
            string upgradeCID = BuildingManager.Instance.GetUpgradeCID(Classification.ToString());
            return upgradeCID != null && BuildingManager.Instance.IsValidBPartID(upgradeCID);
        }
        /// <summary>
        /// 是否满足升级的条件
        /// </summary>
        public bool CanUpgrade();
        /// <summary>
        /// 升级时的回调，用于回调和更新数据
        /// </summary>
        public void OnUpgrade(IBuildingUpgrade lastLevelBuild);
        /// <summary>
        /// 升级时的回调，用于回调和更新数据
        /// </summary>
        public void OnUpgrade(IBuildingUpgrade lastLevelBuild, IBuildingUpgradeParam param);
    }
}