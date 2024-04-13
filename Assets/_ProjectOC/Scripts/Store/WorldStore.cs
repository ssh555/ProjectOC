using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
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
        public ItemIcon ItemIcon { get => GetComponentInChildren<ItemIcon>(); }
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
                uiPanel.Player = component.GetComponentInParent<Player.PlayerCharacter>();
                uiPanel.Store = this.Store;
                uiPanel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);
                GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }

        public bool CanUpgrade()
        {
            string CID = Classification.ToString();
            if ((this as IBuildingUpgrade).CanUpgrade())
            {
                List<Formula> formulas = BuildingManager.Instance.GetUpgradeRaw(CID);
                Player.PlayerCharacter player = GameObject.Find("PlayerCharacter(Clone)")?.GetComponent<Player.PlayerCharacter>();
                return player.InventoryHasItems(formulas);
            }
            return false;
        }

        public void OnUpgrade(IBuildingUpgrade lastLevelBuild)
        {
            string lastLevelID = BuildingManager.Instance.GetID(lastLevelBuild.Classification.ToString());
            string upgradeID = BuildingManager.Instance.GetID(Classification.ToString());
            CompositeManager.Instance.OnlyCostResource(upgradeID);
            Player.PlayerCharacter player = GameObject.Find("PlayerCharacter(Clone)")?.GetComponent<Player.PlayerCharacter>();
            CompositeManager.Instance.OnlyReturnResource(player.Inventory, lastLevelID);
            transform.SetParent(lastLevelBuild.transform.parent);
            InstanceID = lastLevelBuild.InstanceID;
            transform.position = lastLevelBuild.transform.position;
            transform.rotation = lastLevelBuild.transform.rotation;
            if (lastLevelBuild is WorldStore worldStore)
            {
                LocalGameManager.Instance.StoreManager.WorldStoreSetData(this, worldStore.Store);
            }
            Store.SetLevel(Classification.Category4 - 1);
            GameManager.DestroyObj(lastLevelBuild.gameObject);
        }

        public void OnUpgrade(IBuildingUpgrade lastLevelBuild, IBuildingUpgradeParam param)
        {
            OnUpgrade(lastLevelBuild);
        }
    }
}

