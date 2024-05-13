namespace ML.Engine.BuildingSystem
{
    public interface IBuildingUpgrade : BuildingPart.IBuildingPart
    {
        /// <summary>
        /// �Ƿ��ܹ�����
        /// </summary>
        public bool HasUpgrade()
        {
            string upgradeCID = BuildingManager.Instance.GetUpgradeCID(Classification.ToString());
            return upgradeCID != null && BuildingManager.Instance.IsValidBPartID(upgradeCID);
        }

        /// <summary>
        /// �Ƿ���������������
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
        /// ����ʱ��������
        /// </summary>
        public void OnUpgradeSetData(IBuildingUpgrade lastLevelBuild);

        /// <summary>
        /// ����ʱ�Ļص������ڻص��͸�������
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