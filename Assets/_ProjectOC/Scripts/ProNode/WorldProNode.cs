using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.ProNodeNS
{
    public class WorldProNode : BuildingPart, IInteraction
    {
        [ShowInInspector, ReadOnly, SerializeField]
        public ProNode ProNode;

        public string InteractType { get; set; } = "WorldProNode";
        public Vector3 PosOffset { get; set; } = Vector3.zero;


        private void Start()
        {
            // ����Ҫ Update
            this.enabled = false;
        }

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            // ��һ���½�
            if (oldPos == newPos)
            {
                // �����߼�����
                string actorID = BuildingManager.Instance.GetActorID(this.Classification.ToString().Replace('-', '_'));
                if (!string.IsNullOrEmpty(actorID))
                {
                    LocalGameManager.Instance.ProNodeManager.WorldNodeSetData(this, actorID);
                }
            }
        }

        public void Interact(InteractComponent component)
        {
            // ʵ����UIPanel
            GameObject gameObject = GameManager.Instance.ABResourceManager.LoadLocalAB("ui/uipanel").LoadAsset<GameObject>("UIProNodePanel");
            InventorySystem.UI.UIProNode uiPanel = GameObject.Instantiate(gameObject, GameObject.Find("Canvas").transform, false).GetComponent<InventorySystem.UI.UIProNode>();
            uiPanel.Player = component.GetComponentInParent<Player.PlayerCharacter>();
            // ��ʼ���������
            uiPanel.ProNode = this.ProNode;
            // Push
            GameManager.Instance.UIManager.PushPanel(uiPanel);
        }
    }
}
