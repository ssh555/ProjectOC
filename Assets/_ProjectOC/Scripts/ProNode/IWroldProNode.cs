using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.ProNodeNS
{
    public abstract class IWorldProNode : LandMassExpand.ElectAppliance, ML.Engine.InteractSystem.IInteraction, ML.Engine.BuildingSystem.IBuildingUpgrade
    {
        [LabelText("生产节点"), ShowInInspector, ReadOnly, SerializeField]
        public IProNode ProNode;
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
                ProNode.ProNodeOnPositionChange(newPos - oldPos);
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }
        public override void PowerStateChange()
        {
            if (InPower) { ProNode.StartProduce(); }
            else { ProNode.StopProduce(); }
        }
        public void OnUpgradeSetData(ML.Engine.BuildingSystem.IBuildingUpgrade lastLevelBuild)
        {
            isFirstBuild = lastLevelBuild.isFirstBuild;
            if (lastLevelBuild is IWorldProNode worldProNode)
            {
                ManagerNS.LocalGameManager.Instance.ProNodeManager.WorldNodeSetData(this, worldProNode.ProNode);
                ProNode.SetLevel(Classification.Category4 - 1);
                ManagerNS.LocalGameManager.Instance.BuildPowerIslandManager.electAppliances.Add(this);
                PowerCount = worldProNode.PowerCount;
            }
        }
        public abstract void Interact(ML.Engine.InteractSystem.InteractComponent component);
    }
}