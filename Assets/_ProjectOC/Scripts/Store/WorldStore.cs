using Sirenix.OdinInspector;

namespace ProjectOC.StoreNS
{
    public class WorldStore : IWorldStore
    {
        [LabelText("ÕæÊµ²Ö¿â"), ShowInInspector, ReadOnly]
        public Store RealStore => Store as Store;
        private const string str = "Prefab_Store_UI/Prefab_Store_UI_StorePanel.prefab";
        public override void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(str,
                ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false).Completed += (handle) =>
                {
                    UI.UIStore uiPanel = (handle.Result).GetComponent<UI.UIStore>();
                    uiPanel.Store = RealStore;
                    uiPanel.HasUpgrade = (this as ML.Engine.BuildingSystem.IBuildingUpgrade).HasUpgrade() || RealStore.Level > 0;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
                };
        }
    }
}