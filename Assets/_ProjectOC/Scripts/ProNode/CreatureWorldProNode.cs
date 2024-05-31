using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    public class CreatureWorldProNode : IWorldProNode
    {
        [LabelText("真实生产节点"), ShowInInspector, ReadOnly]
        public CreatureProNode RealProNode => ProNode as CreatureProNode;
        public override void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_ProNode_UI/Prefab_ProNode_UI_CreatureProNodePanel.prefab",
                ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false).Completed += (handle) =>
                {
                    UI.UICreatureProNode uiPanel = handle.Result.GetComponent<UI.UICreatureProNode>();
                    uiPanel.ProNode = RealProNode;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
                };
        }
    }
}