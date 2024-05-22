using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    public class WorldStore : IWorldStore, ML.Engine.BuildingSystem.IBuildingUpgrade
    {
        [LabelText("ÕæÊµ²Ö¿â"), ShowInInspector, ReadOnly, SerializeField]
        public Store RealStore => Store as Store;

        public void OnUpgradeSetData(ML.Engine.BuildingSystem.IBuildingUpgrade lastLevelBuild)
        {
            isFirstBuild = lastLevelBuild.isFirstBuild;
            if (lastLevelBuild is WorldStore worldStore)
            {
                ManagerNS.LocalGameManager.Instance.StoreManager.WorldStoreSetData(this, worldStore.Store);
                RealStore.SetLevel(Classification.Category4 - 1);
            }
        }
        public override void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Store_UI/Prefab_Store_UI_StorePanel.prefab",
                ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
                {
                    UI.UIStore uiPanel = (handle.Result).GetComponent<UI.UIStore>();
                    uiPanel.Store = RealStore;
                    uiPanel.HasUpgrade = (this as ML.Engine.BuildingSystem.IBuildingUpgrade).HasUpgrade() || RealStore.Level > 0;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
                };
        }
    }
}