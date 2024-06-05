using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.RestaurantNS
{
    public class WorldRestaurant : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, ML.Engine.InteractSystem.IInteraction
    {
        [LabelText("²ÍÌü"), ShowInInspector, ReadOnly]
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
                Restaurant.OnPositionChange(newPos - oldPos);
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }
        private const string str = "Prefab_Restaurant_UI/Prefab_Restaurant_UI_RestaurantPanel.prefab";
        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(str, 
                ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false).Completed += (handle) =>
            {
                UI.UIRestaurant uiPanel = handle.Result.GetComponent<UI.UIRestaurant>();
                uiPanel.Restaurant = Restaurant;
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }
    }
}