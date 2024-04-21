using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.StoreNS
{
    public class WorldStore : BuildingPart, IInteraction, IBuildingUpgrade
    {
        [LabelText("²Ö¿â"), ShowInInspector, ReadOnly, SerializeField]
        public Store Store;
        public string InteractType { get; set; } = "WorldStore";
        public Vector3 PosOffset { get; set; } = Vector3.zero;

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                string actorID = BuildingManager.Instance.GetActorID(this.Classification.ToString());
                if (!string.IsNullOrEmpty(actorID) && actorID.Split('_').Length == 3)
                {
                    string[] split = actorID.Split("_");
                    StoreType storeType = (StoreType)Enum.Parse(typeof(StoreType), split[1]);
                    int level = int.Parse(split[2]);
                    LocalGameManager.Instance.StoreManager.WorldStoreSetData(this, storeType, level - 1);
                }
            }
            if (oldPos != newPos)
            {
                Store.OnPositionChange();
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }

        public void Interact(InteractComponent component)
        {
            GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIStorePanel.prefab", GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                InventorySystem.UI.UIStore uiPanel = (handle.Result).GetComponent<InventorySystem.UI.UIStore>();
                uiPanel.Store = this.Store;
                uiPanel.HasUpgrade = (this as IBuildingUpgrade).HasUpgrade() || this.Store.Level > 0;
                GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }

        public bool CanUpgrade()
        {
            string CID = Classification.ToString();
            if ((this as IBuildingUpgrade).HasUpgrade())
            {
                List<Formula> formulas = BuildingManager.Instance.GetUpgradeRaw(CID);
                return (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).InventoryHasItems(formulas);
            }
            return false;
        }

        public void OnUpgrade(IBuildingUpgrade lastLevelBuild)
        {
            string lastLevelID = BuildingManager.Instance.GetID(lastLevelBuild.Classification.ToString());
            string upgradeID = BuildingManager.Instance.GetID(Classification.ToString());
            CompositeManager.Instance.OnlyCostResource(upgradeID);
            CompositeManager.Instance.OnlyReturnResource(lastLevelID);
            transform.SetParent(lastLevelBuild.transform.parent);
            InstanceID = lastLevelBuild.InstanceID;
            transform.position = lastLevelBuild.transform.position;
            transform.rotation = lastLevelBuild.transform.rotation;
            isFirstBuild = lastLevelBuild.isFirstBuild;
            if (lastLevelBuild is WorldStore worldStore)
            {
                LocalGameManager.Instance.StoreManager.WorldStoreSetData(this, worldStore.Store);
            }
            Store.SetLevel(Classification.Category4 - 1);
            GameManager.DestroyObj(lastLevelBuild.gameObject);
        }
    }
}

