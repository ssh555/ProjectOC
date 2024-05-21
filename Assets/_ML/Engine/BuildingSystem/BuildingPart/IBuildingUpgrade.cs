namespace ML.Engine.BuildingSystem
{
    public interface IBuildingUpgrade : BuildingPart.IBuildingPart
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
        public bool CanUpgrade()
        {
            if (HasUpgrade())
            {
                var formulas = BuildingManager.Instance.GetUpgradeRaw(Classification.ToString());
                return (Manager.GameManager.Instance.CharacterManager.GetLocalController() as ProjectOC.Player.OCPlayerController).InventoryHaveItems(formulas);
            }
            return false;
        }

        /// <summary>
        /// 升级时设置数据
        /// </summary>
        public void OnUpgradeSetData(IBuildingUpgrade lastLevelBuild);

        /// <summary>
        /// 升级时的回调，用于回调和更新数据
        /// </summary>
        public void OnUpgrade(IBuildingUpgrade lastLevelBuild)
        {
            (Manager.GameManager.Instance.CharacterManager.GetLocalController() as ProjectOC.Player.OCPlayerController).
                InventoryCostItems(BuildingManager.Instance.GetRaw(Classification.ToString()), needJudgeNum: true, priority: -1);
            transform.SetParent(lastLevelBuild.transform.parent);
            InstanceID = lastLevelBuild.InstanceID;
            transform.position = lastLevelBuild.transform.position;
            transform.rotation = lastLevelBuild.transform.rotation;
            OnUpgradeSetData(lastLevelBuild);
            BuildingManager.Instance.AddBuildingInstance(this as BuildingPart.BuildingPart);
            BuildingManager.Instance.RemoveBuildingInstance(lastLevelBuild as BuildingPart.BuildingPart);
            Manager.GameManager.DestroyObj(lastLevelBuild.gameObject);
        }
    }
}