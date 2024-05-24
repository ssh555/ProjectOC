using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    public class ManualWorldProNode : IWorldProNode
    {
        [LabelText("真实生产节点"), ShowInInspector, ReadOnly]
        public ManualProNode RealProNode => ProNode as ManualProNode;
        public override void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_ProNode_UI/Prefab_ProNode_UI_ManualProNodePanel.prefab",
                ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
                {
                    UI.UIManualProNode uiPanel = handle.Result.GetComponent<UI.UIManualProNode>();
                    uiPanel.ProNode = RealProNode;
                    uiPanel.HasUpgrade = (this as ML.Engine.BuildingSystem.IBuildingUpgrade).HasUpgrade() || ProNode.Level > 0;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
                };
        }
    }
}