using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using ML.Engine.TextContent;
using ProjectOC.Player;
using ProjectOC.ResonanceWheelSystem.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace ProjectOC.WorkerEchoNS
{
    public class WorkerEchoBuilding : BuildingPart, IInteraction
    {
        public string InteractType { get; set; }
        public Vector3 PosOffset { get; set; }

        public ResonanceWheelSystem.UI.ResonanceWheelUI uIResonanceWheel;

        public ResonanceWheelSystem.UI.ResonanceWheelUI uIResonanceWheelInstance;

        public WorkerEcho workerEcho;
        protected override void Awake()
        {
            this.workerEcho = new WorkerEcho(this);
            this.InteractType = "WorkerEcho";
            this.PosOffset = Vector3.zero;
            this.CheckCanDestory += CheckCanDestroyOrEdit();
            this.uIResonanceWheelInstance = null;
        }

        public void Interact(InteractComponent component)
        {
            if (this.uIResonanceWheelInstance == null)
            {
                //µ¯¹²ÃùÂÖUI

                Debug.Log("µ¯UI");
                //ML.Engine.Input.InputManager.Instance.Common.Disable();
                uIResonanceWheelInstance = GameObject.Instantiate(uIResonanceWheel);
                uIResonanceWheelInstance.GetComponentInParent<ResonanceWheelUI>().inventory = component.gameObject.GetComponentInParent<PlayerCharacter>().Inventory;
                uIResonanceWheelInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);

                
            }
            else
            {
                uIResonanceWheelInstance.gameObject.SetActive(true);
            }
            ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uIResonanceWheelInstance);

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