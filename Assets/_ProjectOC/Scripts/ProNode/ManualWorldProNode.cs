using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    public class ManualWorldProNode : IWorldProNode
    {
        [LabelText("��ʵ�����ڵ�"), ShowInInspector, ReadOnly]
        public ManualProNode RealProNode => ProNode as ManualProNode;
        private const string str = "Prefab_ProNode_UI/Prefab_ProNode_UI_ManualProNodePanel.prefab";
        public override void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(str,
                ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false).Completed += (handle) =>
                {
                    UI.UIManualProNode uiPanel = handle.Result.GetComponent<UI.UIManualProNode>();
                    uiPanel.ProNode = RealProNode;
                    uiPanel.HasUpgrade = (this as ML.Engine.BuildingSystem.IBuildingUpgrade).HasUpgrade() || ProNode.Level > 0;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
                };
        }
    }
}