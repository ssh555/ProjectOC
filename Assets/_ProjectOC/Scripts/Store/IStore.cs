using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    public abstract class IStore : DataNS.DataContainerOwner
    {
        #region Data
        [LabelText("����ֿ�"), ReadOnly, HideInInspector]
        public IWorldStore WorldStore;
        [ShowInInspector, ReadOnly]
        public string UID { get { return WorldStore?.InstanceID ?? ""; } }
        [LabelText("�ֿ�����"), ReadOnly]
        public string Name = "";
        [LabelText("�ֿ�����"), ReadOnly]
        public ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 StoreType;
        [LabelText("����Ƿ�������˲ֿ⽻��"), ReadOnly]
        public bool IsInteracting;
        [LabelText("����Icon��Ӧ��Item"), ReadOnly]
        public string WorldIconItemID;
        #endregion

        public IStore(ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2 storeType)
        {
            StoreType = storeType;
            Name = storeType.ToString();
        }

        public void Destroy()
        {
            ClearData();
        }

        public void OnPositionChange()
        {
            (this as MissionNS.IMissionObj).OnPositionChangeTransport();
        }

        public override Transform GetTransform() { return WorldStore?.transform; }
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