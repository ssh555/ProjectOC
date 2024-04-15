using ML.Engine.BuildingSystem.BuildingPart;
using System.Collections.Generic;

namespace ML.Engine.BuildingSystem
{
    public interface IBuildingUpgrade : IBuildingPart
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
        public bool CanUpgrade();
        /// <summary>
        /// ����ʱ�Ļص������ڻص��͸�������
        /// </summary>
        public void OnUpgrade(IBuildingUpgrade lastLevelBuild);
        /// <summary>
        /// ����ʱ�Ļص������ڻص��͸�������
        /// </summary>
        public void OnUpgrade(IBuildingUpgrade lastLevelBuild, IBuildingUpgradeParam param);
    }
}