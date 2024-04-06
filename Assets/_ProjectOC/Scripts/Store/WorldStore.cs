using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    public class WorldStore : BuildingPart, IInteraction
    {
        [ShowInInspector, ReadOnly, SerializeField]
        public Store Store;

        public string InteractType { get; set; } = "WorldStore";
        public Vector3 PosOffset { get; set; } = Vector3.zero;
        
        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            // ��һ���½�
            if (isFirstBuild)
            {
                // �����߼�����
                string actorID = BuildingManager.Instance.GetActorID(this.Classification.ToString());
                if (!string.IsNullOrEmpty(actorID) && actorID.Split('_').Length == 3)
                {
                    string[] split = actorID.Split("_");
                    StoreType storeType = (StoreType)Enum.Parse(typeof(StoreType), split[1]);
                    int level = int.Parse(split[2]);
                    LocalGameManager.Instance.StoreManager.WorldStoreSetData(this, storeType, level - 1);
                }
            }
            if (oldPos != newPos)
            {
                Store.OnPositionChange();
            }
            //isFirstBuild�ĸ��·��ڻ����ҪҪ�����ú���
            base.OnChangePlaceEvent(oldPos,newPos);
        }

        public void Interact(InteractComponent component)
        {
            // TODO
            GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIStorePanel.prefab", ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                InventorySystem.UI.UIStore uiPanel = (handle.Result).GetComponent<InventorySystem.UI.UIStore>();
                uiPanel.Player = component.GetComponentInParent<Player.PlayerCharacter>();
                // ��ʼ���������
                uiPanel.Store = this.Store;
                uiPanel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                // Push
                GameManager.Instance.UIManager.PushPanel(uiPanel);
            };

        }
    }
}

