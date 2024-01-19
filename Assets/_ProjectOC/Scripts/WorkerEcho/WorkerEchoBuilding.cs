using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerEchoNS
{
    public class WorkerEchoBuilding : BuildingPart, IInteraction
    {
        public string InteractType { get; set; }
        public Vector3 PosOffset { get; set; }
        public WorkerEcho WorkerEcho = new WorkerEcho(); 
        private void Awake()
        {
            this.InteractType = "BuilidngPart";
            this.PosOffset = this.transform.position;
        }
        public void Interact(InteractComponent component)
        {

        }

    }
}