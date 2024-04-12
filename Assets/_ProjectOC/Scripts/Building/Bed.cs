using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using ProjectOC.ClanNS;
using Sirenix.OdinInspector;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using ML.Engine.InventorySystem;


namespace ProjectOC.Building
{
    public class Bed : BuildingPart, IInteraction
    {
        #region 参数
        [LabelText("关联氏族"), ShowInInspector, ReadOnly]
        public Clan Clan;
        [LabelText("是否有关联氏族"), ShowInInspector, ReadOnly]
        public bool HasClan { get { return Clan != null && !string.IsNullOrEmpty(Clan.ID); } }
        [LabelText("是否能放置"), ShowInInspector, ReadOnly]
        public bool CanSetClan { get; private set; }

        [LabelText("向上检测参数"), ShowInInspector]
        public RaycastConfig ConfigUp;
        [LabelText("向下检测参数"), ShowInInspector]
        public RaycastConfig ConfigDown;
        public string InteractType { get; set; } = "Bed";
        public Vector3 PosOffset { get; set; } = Vector3.zero;

        public ItemIcon ItemIcon { get => GetComponentInChildren<ItemIcon>(); }
        #endregion

        protected override void Start()
        {
            this.enabled = false;
        }

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            // 第一次新建
            if (isFirstBuild)
            {
                LocalGameManager.Instance.IslandAreaManager.UpdatedFieldTransformsAction += HouseDetectionAction;
                ItemManager.Instance.AddItemIconObject("", transform, new Vector3(0, transform.GetComponent<BoxCollider>().size.y * 1.5f, 0), Quaternion.Euler(Vector3.zero), Vector3.one);
            }
            //isFirstBuild的更新放在基类里，要要到引用后面
            base.OnChangePlaceEvent(oldPos, newPos);
        }

        public bool HouseDetection()
        {
            // 向上检测
            Vector3 posUp = transform.TransformPoint(ConfigUp.Offset);
            Quaternion rotUp = transform.rotation * Quaternion.Euler(ConfigUp.Rotation);
            Vector3 sizeUp = ConfigUp.Size / 2;
            sizeUp = Vector3.Scale(sizeUp, ConfigUp.Scale);
            sizeUp = Vector3.Scale(sizeUp, transform.localScale);
            bool flagUp = false;
            foreach (RaycastHit hit in Physics.BoxCastAll(posUp, sizeUp, transform.up, rotUp))
            {
                BuildingPart bp = hit.collider.GetComponent<BuildingPart>();
                if (bp != null && (bp.Classification.Category2 == BuildingCategory2.Roof || bp.Classification.Category2 == BuildingCategory2.Floor))
                {
                    flagUp = true;
                    break;
                }
            }
            // 向下检测
            Vector3 posDown = transform.TransformPoint(ConfigDown.Offset);
            Quaternion rotDown = transform.rotation * Quaternion.Euler(ConfigDown.Rotation);
            Vector3 sizeDown = ConfigDown.Size / 2;
            sizeDown = Vector3.Scale(sizeDown, ConfigDown.Scale);
            sizeDown = Vector3.Scale(sizeDown, transform.localScale);
            bool flagDown = false;
            foreach (RaycastHit hit in Physics.BoxCastAll(posDown, sizeDown, -transform.up, rotDown))
            {
                BuildingPart bp = hit.collider.GetComponent<BuildingPart>();
                if (bp != null && bp.Classification.Category2 == BuildingCategory2.Floor)
                {
                    flagDown = true;
                    break;
                }
            }
            this.CanSetClan = flagUp && flagDown;
            string icon = this.CanSetClan ? "UI_Bed_Icon_Enable" : "UI_Bed_Icon_Disable";
            ItemIcon?.SetSprite(ItemManager.Instance.GetItemSprite(icon));
            return this.CanSetClan;
        }

        private void HouseDetectionAction()
        {
            if (LocalGameManager.Instance.IslandAreaManager.updatedFieldTransforms.Contains(this.transform.parent))
            {
                // 床不在屋子里时，清空氏族数据
                if (!HouseDetection())
                {
                    SetEmpty();
                }
            }
        }

        /// <summary>
        /// 设置关联氏族
        /// </summary>
        public void SetClan(Clan clan)
        {
            if (this.HasClan)
            {
                this.Clan.Bed = null;
            }
            if (clan != null && clan.HasBed)
            {
                clan.Bed.Clan = null;
            }
            this.Clan = clan;
            if (this.HasClan)
            {
                clan.Bed = this;
            }
        }

        public void SetEmpty()
        {
            if (this.HasClan)
            {
                this.Clan.Bed = null;
            }
            this.Clan = null;
        }

        private void OnDrawGizmosSelected()
        {
            // 向上检测
            Vector3 posUp = transform.TransformPoint(ConfigUp.Offset);
            Quaternion rotUp = transform.rotation * Quaternion.Euler(ConfigUp.Rotation);
            Vector3 scaleUp = Vector3.Scale(ConfigUp.Scale, transform.localScale);
            Gizmos.color = UnityEngine.Color.green;
            Gizmos.matrix = Matrix4x4.TRS(posUp, rotUp, scaleUp);
            Gizmos.DrawWireCube(Vector3.zero, ConfigUp.Size);
            // 向下检测
            Vector3 posDown = transform.TransformPoint(ConfigDown.Offset);
            Quaternion rotDown = transform.rotation * Quaternion.Euler(ConfigDown.Rotation);
            Vector3 scaleDown = Vector3.Scale(ConfigDown.Scale, transform.localScale);
            Gizmos.color = UnityEngine.Color.green;
            Gizmos.matrix = Matrix4x4.TRS(posDown, rotDown, scaleDown);
            Gizmos.DrawWireCube(Vector3.zero, ConfigDown.Size);
        }

        public void Interact(InteractComponent component)
        {
            GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIBedPanel.prefab", ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                ProjectOC.Building.UI.UIBed uiPanel = (handle.Result).GetComponent<ProjectOC.Building.UI.UIBed>();
                // 初始化相关数据
                uiPanel.Bed = this;
                uiPanel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                // Push
                GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }
        public void OnDestroy()
        {
            if (LocalGameManager.Instance?.IslandAreaManager != null)
            {
                LocalGameManager.Instance.IslandAreaManager.UpdatedFieldTransformsAction -= HouseDetectionAction;
            }
        }
    }
}