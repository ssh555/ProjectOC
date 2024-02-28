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
            // 不需要 Update
            this.enabled = false;
        }

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            // 第一次新建
            if (oldPos == newPos)
            {
                // 生成逻辑对象
                string actorID = BuildingManager.Instance.GetActorID(this.Classification.ToString().Replace('-', '_'));
                if (!string.IsNullOrEmpty(actorID))
                {
                    LocalGameManager.Instance.ProNodeManager.WorldNodeSetData(this, actorID);
                }
            }
        }

        public void Interact(InteractComponent component)
        {
            // 实例化UIPanel
            GameObject gameObject = GameManager.Instance.ABResourceManager.LoadLocalAB("ui/uipanel").LoadAsset<GameObject>("UIProNodePanel");
            InventorySystem.UI.UIProNode uiPanel = GameObject.Instantiate(gameObject, GameObject.Find("Canvas").transform, false).GetComponent<InventorySystem.UI.UIProNode>();
            uiPanel.Player = component.GetComponentInParent<Player.PlayerCharacter>();
            // 初始化相关数据
            uiPanel.ProNode = this.ProNode;
            // Push
            GameManager.Instance.UIManager.PushPanel(uiPanel);
        }
    }
}
