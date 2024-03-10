using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using ML.Engine.Manager;
using ML.Engine.UI;
using ProjectOC.LandMassExpand;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.ProNodeNS
{
    public class WorldProNode : ElectAppliance, IInteraction
    {
        [ShowInInspector, ReadOnly, SerializeField]
        public ProNode ProNode;

        public string InteractType { get; set; } = "WorldProNode";
        public Vector3 PosOffset { get; set; } = Vector3.zero;


        protected override void Start()
        {
            base.Start();
            // ����Ҫ Update
            this.enabled = false;
        }

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            base.OnChangePlaceEvent(oldPos, newPos);
            // ��һ���½�
            if (oldPos == newPos)
            {
                // �����߼�����
                string actorID = BuildingManager.Instance.GetActorID(this.Classification.ToString());
                if (!string.IsNullOrEmpty(actorID))
                {
                    LocalGameManager.Instance.ProNodeManager.WorldNodeSetData(this, actorID);
                }
            }
        }

        public void Interact(InteractComponent component)
        {
            // ʵ����UIPanel
            GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIProNodePanel.prefab", GameManager.Instance.UIManager.GetCanvas.transform, true).Completed += (handle) =>
            {
                InventorySystem.UI.UIProNode uiPanel = handle.Result.GetComponent<InventorySystem.UI.UIProNode>();
                uiPanel.Player = component.GetComponentInParent<Player.PlayerCharacter>();
                // ��ʼ���������
                uiPanel.ProNode = this.ProNode;
                // Push
                GameManager.Instance.UIManager.PushPanel(uiPanel);
            };

        }
    }
}
