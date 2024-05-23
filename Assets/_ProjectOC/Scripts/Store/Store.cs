using System;
using Sirenix.OdinInspector;

namespace ProjectOC.StoreNS
{
    [LabelText("�ֿ�"), Serializable]
    public class Store : IStore
    {
        #region Data
        [LabelText("�ֿ�ȼ�"), ReadOnly]
        public int Level;
        #endregion

        public Store(ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType) : base(storeType)
        {
            if (0 <= Level && Level <= ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelMax)
            {
                InitData(ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelCapacity[Level],
                ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelDataCapacity[Level]);
            }
        }
        public bool SetLevel(int newLevel)
        {
            if (WorldStore.transform != null && 0 <= newLevel && newLevel <= ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelMax)
            {
                ChangeCapacity(ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelCapacity[newLevel], 
                    ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelDataCapacity[newLevel]);
                Level = newLevel;
                return true;
            }
            return false;
        }
    }
}