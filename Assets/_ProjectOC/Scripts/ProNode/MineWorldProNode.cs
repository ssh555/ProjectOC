using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    public class MineWorldProNode : IWorldProNode
    {
        [LabelText("真实生产节点"), ShowInInspector, ReadOnly]
        public MineProNode RealProNode => ProNode as MineProNode;
        public override void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_ProNode_UI/Prefab_ProNode_UI_MineProNodePanel.prefab",
                ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
                {
                    UI.UIMineProNode uiPanel = handle.Result.GetComponent<UI.UIMineProNode>();
                    uiPanel.ProNode = RealProNode;
                    uiPanel.HasUpgrade = (this as ML.Engine.BuildingSystem.IBuildingUpgrade).HasUpgrade() || ProNode.Level > 0;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
                };
        }
    }
}