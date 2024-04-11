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
        [ShowInInspector, ReadOnly, SerializeField]
        public Store Store;
        public ItemIcon ItemIcon { get => GetComponentInChildren<ItemIcon>(); }
        public string InteractType { get; set; } = "WorldStore";
        public Vector3 PosOffset { get; set; } = Vector3.zero;
        
        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            // 第一次新建
            if (isFirstBuild)
            {
                // 生成逻辑对象
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
            //isFirstBuild的更新放在基类里，要要到引用后面
            base.OnChangePlaceEvent(oldPos,newPos);
        }

        public void Interact(InteractComponent component)
        {
            GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIStorePanel.prefab", ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                InventorySystem.UI.UIStore uiPanel = (handle.Result).GetComponent<InventorySystem.UI.UIStore>();
                uiPanel.Player = component.GetComponentInParent<Player.PlayerCharacter>();
                uiPanel.Store = this.Store;
                uiPanel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }

        public bool CanUpgrade(IInventory inventory)
        {
            string CID = Classification.ToString();
            if (BuildingManager.Instance.GetUpgradeID(CID) != null && BuildingManager.Instance.IsValidBPartID(CID))
            {
                List<Formula> formulas = BuildingManager.Instance.GetUpgradeRaw(CID);
                foreach (Formula formula in formulas)
                {
                    if (inventory.GetItemAllNum(formula.id) < formula.num)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public void OnUpgrade(IBuildingUpgrade lastLevelBuild, IInventory inventory)
        {
            string lastLevelID = BuildingManager.Instance.GetID(lastLevelBuild.Classification.ToString());
            string upgradeID = BuildingManager.Instance.GetID(Classification.ToString());
            CompositeManager.Instance.OnlyCostResource(inventory, upgradeID);
            CompositeManager.Instance.OnlyReturnResource(inventory, lastLevelID);
            transform.SetParent(lastLevelBuild.transform.parent);
            InstanceID = lastLevelBuild.InstanceID;
            transform.position = lastLevelBuild.transform.position;
            transform.rotation = lastLevelBuild.transform.rotation;
            if (lastLevelBuild is WorldStore worldStore)
            {
                LocalGameManager.Instance.StoreManager.WorldStoreSetData(this, worldStore.Store);
            }
            Store.SetLevel(Classification.Category4 - 1);
            ML.Engine.Manager.GameManager.DestroyObj(lastLevelBuild.gameObject);
        }
    }
}

