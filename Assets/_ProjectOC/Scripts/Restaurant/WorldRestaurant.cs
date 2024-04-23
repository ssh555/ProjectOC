using ML.Engine.InteractSystem;
using Sirenix.OdinInspector;
using UnityEngine;


namespace ProjectOC.RestaurantNS
{
    public class WorldRestaurant : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, IInteraction
    {
        [LabelText("²ÍÌü"), ShowInInspector, ReadOnly, SerializeField]
        public Restaurant Restaurant;
        public string InteractType { get; set; } = "WorldRestaurant";
        public Vector3 PosOffset { get; set; } = Vector3.zero;

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                ManagerNS.LocalGameManager.Instance.RestaurantManager.WorldRestaurantSetData(this);
            }
            if (oldPos != newPos)
            {
                Restaurant.OnPositionChange();
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }

        public void Interact(InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIRestaurantPanel.prefab", ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                InventorySystem.UI.UIRestaurant uiPanel = handle.Result.GetComponent<InventorySystem.UI.UIRestaurant>();
                uiPanel.Restaurant = Restaurant;
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }
    }
}
