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

namespace ProjectOC.StoreNS
{
    public class WorldStore : BuildingPart, IInteraction
    {
        [ShowInInspector, ReadOnly, SerializeField]
        public Store Store;

        public string InteractType { get; set; } = "WorldStore";
        public Vector3 PosOffset { get; set; } = Vector3.zero;

        private void Start()
        {
            // 不需要 Update
            this.enabled = false;
        }
        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            // 第一次新建
            if(oldPos == newPos)
            {
                // 生成逻辑对象
                string actorID = BuildingManager.Instance.GetActorID(this.Classification.ToString());
                if (!string.IsNullOrEmpty(actorID) && actorID.Split('_').Length == 3)
                {
                    string[] split = actorID.Split("_");
                    StoreType storeType = (StoreType)Enum.Parse(typeof(StoreType), split[1]);
                    int level = int.Parse(split[2]);
                    LocalGameManager.Instance.StoreManager.WorldStoreSetData(this, storeType, level);
                }
            }
        }

        public void Interact(InteractComponent component)
        {
            // 实例化UIPanel
            InventorySystem.UI.UIStore uiStorePanel = GameManager.Instance.ABResourceManager.LoadLocalAB("ui/uipanel").LoadAsset<InventorySystem.UI.UIStore>("UIStorePanel");
            // 初始化相关数据
            uiStorePanel.Store = this.Store;
            // Push
            ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiStorePanel);
        }
    }
}

