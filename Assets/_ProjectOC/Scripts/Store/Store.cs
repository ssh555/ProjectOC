using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    [LabelText("�ֿ�"), Serializable]
    public class Store : DataNS.ItemContainerOwner
    {
        #region Data
        [LabelText("����ֿ�"), ReadOnly]
        public WorldStore WorldStore;
        [ShowInInspector, ReadOnly]
        public string UID { get { return WorldStore?.InstanceID ?? ""; } }
        [LabelText("�ֿ�����"), ReadOnly]
        public string Name = "";
        [LabelText("�ֿ�����"), ReadOnly]
        public ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 StoreType;
        [LabelText("�ֿ�ȼ�"), ReadOnly]
        public int Level;
        [LabelText("����Ƿ�������˲ֿ⽻��"), ReadOnly]
        public bool IsInteracting;
        [LabelText("����Icon��Ӧ��Item"), ReadOnly]
        public string WorldIconItemID;
        #endregion

        public Store(ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType)
        {
            StoreType = storeType;
            Name = storeType.ToString();
            if (0 <= Level && Level <= ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelMax)
            {
                InitData(ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelCapacity[Level],
                ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelDataCapacity[Level]);
            }
        }

        public void Destroy()
        {
            ClearData();
        }

        public void OnPositionChange()
        {
            (this as MissionNS.IMissionObj).OnPositionChangeTransport();
        }

        public bool SetLevel(int newLevel)
        {
            if (WorldStore.transform != null && 0 <= newLevel && newLevel <= ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelMax)
            {
                ChangeCapacity(ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelCapacity[newLevel], 
                    ManagerNS.LocalGameManager.Instance.StoreManager.Config.LevelDataCapacity[newLevel]);
                Level = newLevel;
                (this as MissionNS.IMissionObj).UpdateTransport();
                return true;
            }
            return false;
        }

        public override Transform GetTransform() { return WorldStore?.transform; }
        public override string GetUID() { return UID; }

        public override MissionNS.MissionObjType GetMissionObjType() { return MissionNS.MissionObjType.Store; }

        public class Sort : IComparer<Store>
        {
            public int Compare(Store x, Store y)
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