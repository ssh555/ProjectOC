using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    public class WorldStore : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, ML.Engine.InteractSystem.IInteraction, ML.Engine.BuildingSystem.IBuildingUpgrade
    {
        [LabelText("²Ö¿â"), ShowInInspector, ReadOnly, SerializeField]
        public Store Store;
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

        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Store_UI/Prefab_Store_UI_StorePanel.prefab", 
                ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                UI.UIStore uiPanel = (handle.Result).GetComponent<UI.UIStore>();
                uiPanel.Store = Store;
                uiPanel.HasUpgrade = (this as ML.Engine.BuildingSystem.IBuildingUpgrade).HasUpgrade() || Store.Level > 0;
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
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
    }
}