using UnityEngine;

namespace ProjectOC.WorkerNS
{
    public class WorkerEchoBuilding : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, ML.Engine.InteractSystem.IInteraction
    {
        public string InteractType { get; set; } = "WorkerEcho";
        public Vector3 PosOffset { get; set; } = Vector3.zero;

        public WorkerEcho WorkerEcho;

        protected override void Awake()
        {
            base.Awake();
            WorkerEcho = new WorkerEcho(this);
            CheckCanDestory += CheckCanDestroyOrEdit();
            CheckCanEdit += CheckCanDestroyOrEdit();
        }
        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_ResonanceWheel_UIPanel/Prefab_ResonanceWheel_UI_ResonanceWheelUI.prefab", 
                ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                var uiPanel = handle.Result.GetComponent<ResonanceWheelSystem.UI.ResonanceWheelUI>();
                uiPanel.inventory = ManagerNS.LocalGameManager.Instance.Player.GetInventory();
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }
        private bool CanEditOrDestroy(ML.Engine.BuildingSystem.BuildingPart.IBuildingPart buildingPart)
        {
            return (WorkerEcho.GetStatus() == EchoStatusType.None);
        }
        private ML.Engine.BuildingSystem.BuildingPart.IBuildingPart.CheckMode CheckCanDestroyOrEdit()
        {
            return new ML.Engine.BuildingSystem.BuildingPart.IBuildingPart.CheckMode(CanEditOrDestroy);
        }
        public override void OnBPartDestroy()
        {
            base.OnBPartDestroy();
            CheckCanDestory -= CheckCanDestroyOrEdit();
            CheckCanEdit -= CheckCanDestroyOrEdit();
        }
    }
}