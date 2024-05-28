using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    public abstract class IWorldStore : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, ML.Engine.InteractSystem.IInteraction, ML.Engine.BuildingSystem.IBuildingUpgrade
    {
        [LabelText("²Ö¿â"), ShowInInspector, ReadOnly]
        public IStore Store;
        public string InteractType { get; set; } = "WorldStore";
        public Vector3 PosOffset { get; set; } = Vector3.zero;

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                ManagerNS.LocalGameManager.Instance.StoreManager.WorldStoreSetData(this, Classification.Category2, Classification.Category4 - 1);
            }
            if (oldPos != newPos)
            {
                Store.OnPositionChange();
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }
        public void OnUpgradeSetData(ML.Engine.BuildingSystem.IBuildingUpgrade lastLevelBuild)
        {
            isFirstBuild = lastLevelBuild.isFirstBuild;
            if (lastLevelBuild is WorldStore worldStore)
            {
                ManagerNS.LocalGameManager.Instance.StoreManager.WorldStoreSetData(this, worldStore.Store);
                Store.SetLevel(Classification.Category4 - 1);
            }
        }
        public abstract void Interact(ML.Engine.InteractSystem.InteractComponent component);
    }
}