using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;


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
        public bool CanUpgrade(IInventory inventory);
        /// <summary>
        /// ����ʱ�Ļص������ڻص��͸�������
        /// </summary>
        public void OnUpgrade(IBuildingUpgrade lastLevelBuild, IInventory inventory);
    }
}