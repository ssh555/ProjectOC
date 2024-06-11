using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    public class AutoWorldProNode : IWorldProNode
    {
        [LabelText("真实生产节点"), ShowInInspector, ReadOnly]
        public AutoProNode RealProNode => ProNode as AutoProNode;
        private const string strPrefab_ProNode_UI_AutoProNodePanel = "Prefab_ProNode_UI/Prefab_ProNode_UI_AutoProNodePanel.prefab";
        public override void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(strPrefab_ProNode_UI_AutoProNodePanel,
                ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false).Completed += (handle) =>
                {
                    UI.UIAutoProNode uiPanel = handle.Result.GetComponent<UI.UIAutoProNode>();
                    uiPanel.ProNode = RealProNode;
                    uiPanel.HasUpgrade = (this as ML.Engine.BuildingSystem.IBuildingUpgrade).HasUpgrade() || ProNode.Level > 0;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
                };
        }
    }
}