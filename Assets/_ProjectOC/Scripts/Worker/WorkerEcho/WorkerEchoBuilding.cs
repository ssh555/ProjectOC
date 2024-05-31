using ML.Engine.Manager;
using ProjectOC.ResonanceWheelSystem.UI;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    public class WorkerEchoBuilding : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, ML.Engine.InteractSystem.IInteraction
    {
        public string InteractType { get; set; } = "WorkerEcho";
        public Vector3 PosOffset { get; set; } = Vector3.zero;

        public WorkerEcho WorkerEcho;

        private string uIResonanceWheelPrefab = "Prefab_ResonanceWheel_UIPanel/Prefab_ResonanceWheel_UI_ResonanceWheelUI.prefab";
        public ResonanceWheelSystem.UI.ResonanceWheelUI uIResonanceWheelInstance;


        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                WorkerEcho.SetFeature();
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }

        protected override void Awake()
        {
            WorkerEcho = new WorkerEcho(this);
            base.Awake();
            CheckCanDestory += CheckCanDestroyOrEdit();
            CheckCanEdit += CheckCanDestroyOrEdit();
        }
        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            if (this.uIResonanceWheelInstance == null)
            {
                GameManager.Instance.ABResourceManager.InstantiateAsync(uIResonanceWheelPrefab).Completed += (handle) =>
                {
                    uIResonanceWheelInstance = handle.Result.GetComponent<ResonanceWheelUI>();
                    uIResonanceWheelInstance.GetComponentInParent<ResonanceWheelUI>().inventory = (GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory;
                    uIResonanceWheelInstance.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false);
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uIResonanceWheelInstance);
                };
            }
            else
            {
                uIResonanceWheelInstance.gameObject.SetActive(true);
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uIResonanceWheelInstance);
            }

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