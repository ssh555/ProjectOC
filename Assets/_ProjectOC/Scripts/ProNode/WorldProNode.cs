using ML.Engine.BuildingSystem;
using ML.Engine.InteractSystem;
using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.Manager;
using ProjectOC.LandMassExpand;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace ProjectOC.ProNodeNS
{
    public class WorldProNode : ElectAppliance, IInteraction, IBuildingUpgrade
    {
        [LabelText("生产节点"), ShowInInspector, ReadOnly, SerializeField]
        public ProNode ProNode;
        public ItemIcon ItemIcon { get => GetComponentInChildren<ItemIcon>(); }
        public string InteractType { get; set; } = "WorldProNode";
        public Vector3 PosOffset { get; set; } = Vector3.zero;
        

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                string actorID = BuildingManager.Instance.GetActorID(this.Classification.ToString());
                if (!string.IsNullOrEmpty(actorID))
                {
                    LocalGameManager.Instance.ProNodeManager.WorldNodeSetData(this, actorID);
                }
            }
            if (oldPos != newPos)
            {
                ProNode.OnPositionChange();
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }

        public void Interact(InteractComponent component)
        {
            GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIProNodePanel.prefab", GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                InventorySystem.UI.UIProNode uiPanel = handle.Result.GetComponent<InventorySystem.UI.UIProNode>();
                uiPanel.Player = component.GetComponentInParent<Player.PlayerCharacter>();
                uiPanel.ProNode = ProNode;
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
            if (lastLevelBuild is WorldProNode worldProNode)
            {
                LocalGameManager.Instance.ProNodeManager.WorldNodeSetData(this, worldProNode.ProNode);
            }
            ProNode.SetLevel(Classification.Category4 - 1);
            GameManager.DestroyObj(lastLevelBuild.gameObject);
        }

        public void OnUpgrade(IBuildingUpgrade lastLevelBuild, IBuildingUpgradeParam param)
        {
            OnUpgrade(lastLevelBuild);
        }
    }
}
