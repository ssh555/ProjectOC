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
                string actorID = BuildingManager.Instance.GetActorID(this.Classification.ToString().Replace('-', '_'));
                if (!string.IsNullOrEmpty(actorID) && actorID.Split('_').Length == 3)
                {
                    //Debug.Log($"{actorID}");
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
            GameObject gameObject = GameManager.Instance.ABResourceManager.LoadLocalAB("ui/uipanel").LoadAsset<GameObject>("UIStorePanel");
            InventorySystem.UI.UIStore uiPanel = GameObject.Instantiate(gameObject, GameObject.Find("Canvas").transform, false).GetComponent<InventorySystem.UI.UIStore>();
            // 初始化相关数据
            uiPanel.Store = this.Store;
            // Push
            GameManager.Instance.UIManager.PushPanel(uiPanel);
        }
    }
}

