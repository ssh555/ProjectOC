using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    public class CreatureWorldStore : IWorldStore
    {
        [LabelText("ÕæÊµ²Ö¿â"), ShowInInspector, ReadOnly, SerializeField]
        public CreatureStore RealStore => Store as CreatureStore;

        public override void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Store_UI/Prefab_Store_UI_CreatureStorePanel.prefab",
                ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
                {
                    UI.UICreatureStore uiPanel = (handle.Result).GetComponent<UI.UICreatureStore>();
                    uiPanel.Store = RealStore;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
                };
        }
    }
}