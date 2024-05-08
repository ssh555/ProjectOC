using Sirenix.OdinInspector;
using System.Collections.Generic;
using ML.Engine.BuildingSystem.BuildingPart;
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

        public bool CanUpgrade()
        {
            if ((this as ML.Engine.BuildingSystem.IBuildingUpgrade).HasUpgrade())
            {
                var formulas = ML.Engine.BuildingSystem.BuildingManager.Instance.GetUpgradeRaw(Classification.ToString());
                return (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).InventoryHaveItems(formulas);
            }
            return false;
        }

        public void OnUpgrade(ML.Engine.BuildingSystem.IBuildingUpgrade lastLevelBuild)
        {
            ManagerNS.LocalGameManager.Instance.Player.InventoryCostItems(ML.Engine.BuildingSystem.BuildingManager.Instance.GetRaw(Classification.ToString()), needJudgeNum: true, priority: -1);
            transform.SetParent(lastLevelBuild.transform.parent);
            InstanceID = lastLevelBuild.InstanceID;
            transform.position = lastLevelBuild.transform.position;
            transform.rotation = lastLevelBuild.transform.rotation;
            isFirstBuild = lastLevelBuild.isFirstBuild;
            if (lastLevelBuild is WorldStore worldStore)
            {
                ManagerNS.LocalGameManager.Instance.StoreManager.WorldStoreSetData(this, worldStore.Store);
            }
            Store.SetLevel(Classification.Category4 - 1);
            ML.Engine.BuildingSystem.BuildingManager.Instance.AddBuildingInstance(this);
            ML.Engine.BuildingSystem.BuildingManager.Instance.RemoveBuildingInstance(lastLevelBuild as BuildingPart);
            ML.Engine.Manager.GameManager.DestroyObj(lastLevelBuild.gameObject);
        }
    }
}
