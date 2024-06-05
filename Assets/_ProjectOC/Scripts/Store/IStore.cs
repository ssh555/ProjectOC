using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    [LabelText("仓库"), Serializable]
    public abstract class IStore : DataNS.DataContainerOwner
    {
        private const string str = "";
        #region Data
        [LabelText("世界仓库"), ReadOnly, ShowInInspector, NonSerialized]
        public IWorldStore WorldStore;
        [ShowInInspector, ReadOnly]
        public string UID { get { return WorldStore?.InstanceID ?? str; } }
        [LabelText("仓库名字"), ReadOnly]
        public string Name = "";
        [LabelText("仓库类型"), ReadOnly]
        public ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 StoreType;
        [LabelText("玩家是否正在与此仓库交互"), ReadOnly]
        public bool IsInteracting;
        [LabelText("场景Icon对应的Item"), ReadOnly]
        public string WorldIconItemID;
        [LabelText("仓库等级"), ReadOnly]
        public int Level;
        #endregion

        public IStore(ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType, int level, bool initData = true)
        {
            StoreType = storeType;
            Name = storeType.ToString();
            Level = level;
            int levelMax = ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelMax;
            if (Level < 0) { Level = 0; }
            else if (Level > levelMax) { Level = levelMax; }
            if (initData)
            {
                InitData(ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelCapacity[Level],
                    ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelDataCapacity[Level]);
            }
        }
        public void Destroy()
        {
            ClearData();
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
        public void OnPositionChange()
        {
            (this as MissionNS.IMissionObj).OnPositionChangeTransport();
        }
        public override void PutIn(int index, DataNS.IDataObj data, int amount) { }
        public override Transform GetTransform() { return WorldStore != null ? WorldStore.transform : null; }
        public override string GetUID() { return UID; }
        public override MissionNS.MissionObjType GetMissionObjType() { return MissionNS.MissionObjType.Store; }
        public class Sort : IComparer<IStore>
        {
            public int Compare(IStore x, IStore y)
            {
                if (x == null || y == null)
                {
                    return (x == null).CompareTo((y == null));
                }
                int priorityX = (int)x.TransportPriority;
                int priorityY = (int)y.TransportPriority;
                if (priorityX != priorityY)
                {
                    return priorityX.CompareTo(priorityY);
                }
                return x.UID.CompareTo(y.UID);
            }
        }
    }
}