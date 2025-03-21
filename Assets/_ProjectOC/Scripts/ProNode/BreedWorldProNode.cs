using Sirenix.OdinInspector;

namespace ProjectOC.ProNodeNS
{
    public class BreedWoldProNode : IWorldProNode
    {
        [LabelText("真实生产节点"), ShowInInspector, ReadOnly]
        public BreedProNode RealProNode => ProNode as BreedProNode;
        private const string str = "Prefab_ProNode_UI/Prefab_ProNode_UI_BreedProNodePanel.prefab";
        public override void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(str,
                ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false).Completed += (handle) =>
                {
                    UI.UIBreedProNode uiPanel = handle.Result.GetComponent<UI.UIBreedProNode>();
                    uiPanel.ProNode = RealProNode;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
                };
        }
    }
}