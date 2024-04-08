using ML.Engine.BuildingSystem;
using ML.Engine.InteractSystem;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
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
        

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            // 第一次新建
            if (isFirstBuild)
            {
                // 生成逻辑对象
                string actorID = BuildingManager.Instance.GetActorID(this.Classification.ToString());
                if (!string.IsNullOrEmpty(actorID))
                {
                    LocalGameManager.Instance.ProNodeManager.WorldNodeSetData(this, actorID);
                }
            }
            if (oldPos != newPos)
            {
                ProNode.OnPositionChange();
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }

        public void Interact(InteractComponent component)
        {
            // 实例化UIPanel
            GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIProNodePanel.prefab", GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                InventorySystem.UI.UIProNode uiPanel = handle.Result.GetComponent<InventorySystem.UI.UIProNode>();
                uiPanel.Player = component.GetComponentInParent<Player.PlayerCharacter>();
                // 初始化相关数据
                uiPanel.ProNode = this.ProNode;
                // Push
                GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }
    }
}
