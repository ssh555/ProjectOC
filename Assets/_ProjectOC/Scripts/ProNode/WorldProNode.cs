using ML.Engine.BuildingSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.ProNodeNS
{
    public class WorldProNode : LandMassExpand.ElectAppliance, ML.Engine.InteractSystem.IInteraction, IBuildingUpgrade
    {
        [LabelText("生产节点"), ShowInInspector, ReadOnly, SerializeField]
        public ProNode ProNode;
        public string InteractType { get; set; } = "WorldProNode";
        public Vector3 PosOffset { get; set; } = Vector3.zero;
        

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                string actorID = BuildingManager.Instance.GetActorID(this.Classification.ToString());
                if (!string.IsNullOrEmpty(actorID))
                {
                    ManagerNS.LocalGameManager.Instance.ProNodeManager.WorldNodeSetData(this, actorID);
                }
            }
            if (oldPos != newPos)
            {
                ProNode.OnPositionChange(newPos - oldPos);
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }

        public override void PowerStateChange()
        {
            if (InPower)
            {
                ProNode.StartProduce();
            }
            else
            {
                ProNode.StopProduce();
            }
        }

        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_ProNode_UI/Prefab_ProNode_UI_ProNodePanel.prefab", ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                UI.UIProNode uiPanel = handle.Result.GetComponent<UI.UIProNode>();
                uiPanel.ProNode = ProNode;
                uiPanel.HasUpgrade = (this as IBuildingUpgrade).HasUpgrade() || this.ProNode.Level > 0;
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }

        public bool CanUpgrade()
        {
            if ((this as IBuildingUpgrade).HasUpgrade())
            {
                var formulas = BuildingManager.Instance.GetUpgradeRaw(Classification.ToString());
                return (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).InventoryHasItems(formulas);
            }
            return false;
        }

        public void OnUpgrade(IBuildingUpgrade lastLevelBuild)
        {
            string lastLevelID = BuildingManager.Instance.GetID(lastLevelBuild.Classification.ToString());
            string upgradeID = BuildingManager.Instance.GetID(Classification.ToString());
            var inventorys = (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).GetInventorys(true, -1);
            ML.Engine.InventorySystem.CompositeSystem.CompositeManager.Instance.OnlyCostResource(inventorys, upgradeID);
            var inventory = (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory;
            ML.Engine.InventorySystem.CompositeSystem.CompositeManager.Instance.OnlyReturnResource(inventory, lastLevelID);
            transform.SetParent(lastLevelBuild.transform.parent);
            InstanceID = lastLevelBuild.InstanceID;
            transform.position = lastLevelBuild.transform.position;
            transform.rotation = lastLevelBuild.transform.rotation;
            isFirstBuild = lastLevelBuild.isFirstBuild;
            if (lastLevelBuild is WorldProNode worldProNode)
            {
                ManagerNS.LocalGameManager.Instance.ProNodeManager.WorldNodeSetData(this, worldProNode.ProNode);
                ProNode.SetLevel(Classification.Category4 - 1);
                PowerCount = worldProNode.PowerCount;
            }
            ML.Engine.Manager.GameManager.DestroyObj(lastLevelBuild.gameObject);
        }
    }
}
