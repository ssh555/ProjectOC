using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using ML.Engine.TextContent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerEchoNS
{
    public class WorkerEchoBuilding : BuildingPart, IInteraction
    {
        public string InteractType { get; set; }
        public Vector3 PosOffset { get; set; }

        public ResonanceWheelSystem.UI.ResonanceWheelUI uIResonanceWheel;

        public WorkerEcho workerEcho;
        protected override void Awake()
        {
            this.workerEcho = new WorkerEcho(this);
            this.InteractType = "WorkerEcho";
            this.PosOffset = Vector3.zero;
            this.CheckCanDestory += CheckCanDestroyOrEdit();
        }

        public void Interact(InteractComponent component)
        {
            //µ¯¹²ÃùÂÖUI
            //Debug.Log("µ¯UI");
            ML.Engine.Input.InputManager.Instance.Common.Disable();
            var panel = GameObject.Instantiate(uIResonanceWheel);
            panel.transform.SetParent(GameObject.Find("Canvas").transform, false);
            ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
        }

        private bool CanEditOrDestroy(IBuildingPart buildingPart)
        {
            return (workerEcho.GetStatus() == EchoStatusType.None);
        }
        private IBuildingPart.CheckMode CheckCanDestroyOrEdit()
        {
            return new IBuildingPart.CheckMode(CanEditOrDestroy);
        }

    }
}