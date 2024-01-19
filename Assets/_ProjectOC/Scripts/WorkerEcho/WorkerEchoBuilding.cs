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

        public WorkerEcho workerEcho;
        private void Awake()
        {
            this.workerEcho = new WorkerEcho(this);
            this.InteractType = "WorkerEcho";
            this.PosOffset = Vector3.zero;
            this.CheckCanDestory += CheckCanDestroyOrEdit();
        }

        public void Interact(InteractComponent component)
        {
            //µ¯¹²ÃùÂÖUI
            Debug.Log("µ¯UI");
            ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(null);
        }
        
        private bool CanEditOrDestroy(IBuildingPart buildingPart)
        {
            return (workerEcho.GetStatus() == EchoStatusType.Echoing || workerEcho.GetStatus() == EchoStatusType.Waiting);
        }
        private IBuildingPart.CheckMode CheckCanDestroyOrEdit()
        {
            return new IBuildingPart.CheckMode(CanEditOrDestroy);
        }

    }
}