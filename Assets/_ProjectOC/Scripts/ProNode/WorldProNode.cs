using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.ProNodeNS
{
    public class WorldProNode : LandMassExpand.ElectAppliance, ML.Engine.InteractSystem.IInteraction, ML.Engine.BuildingSystem.IBuildingUpgrade
    {
        [LabelText("生产节点"), ShowInInspector, ReadOnly, SerializeField]
        public ProNode ProNode;
        public string InteractType { get; set; } = "WorldProNode";
        public Vector3 PosOffset { get; set; } = Vector3.zero;

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                string actorID = ML.Engine.BuildingSystem.BuildingManager.Instance.GetActorID(Classification.ToString());
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
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_ProNode_UI/Prefab_ProNode_UI_ProNodePanel.prefab", 
                ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                UI.UIProNode uiPanel = handle.Result.GetComponent<UI.UIProNode>();
                uiPanel.ProNode = ProNode;
                uiPanel.HasUpgrade = (this as ML.Engine.BuildingSystem.IBuildingUpgrade).HasUpgrade() || ProNode.Level > 0;
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }

        public void OnUpgradeSetData(ML.Engine.BuildingSystem.IBuildingUpgrade lastLevelBuild)
        {
            isFirstBuild = lastLevelBuild.isFirstBuild;
            if (lastLevelBuild is WorldProNode worldProNode)
            {
                ManagerNS.LocalGameManager.Instance.ProNodeManager.WorldNodeSetData(this, worldProNode.ProNode);
                ProNode.SetLevel(Classification.Category4 - 1);
                ManagerNS.LocalGameManager.Instance.BuildPowerIslandManager.electAppliances.Add(this);
                PowerCount = worldProNode.PowerCount;
            }
        }
    }
}