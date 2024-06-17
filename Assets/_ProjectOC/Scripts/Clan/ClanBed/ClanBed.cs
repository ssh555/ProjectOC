using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectOC.ClanNS
{
    [LabelText("床")]
    public class ClanBed : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, ML.Engine.InteractSystem.IInteraction
    {
        #region Config
        [LabelText("向上检测参数"), FoldoutGroup("配置"), ShowInInspector]
        public RaycastConfig ConfigUp;
        [LabelText("向下检测参数"), FoldoutGroup("配置"), ShowInInspector]
        public RaycastConfig ConfigDown;
        #endregion

        #region Data
        [LabelText("关联氏族"), ShowInInspector, ReadOnly]
        public Clan Clan = null;
        [LabelText("是否有关联氏族"), ShowInInspector, ReadOnly]
        public bool HaveClan => Clan != null && !string.IsNullOrEmpty(Clan.ID);
        [LabelText("是否能放置"), ShowInInspector, ReadOnly]
        public bool CanSetClan { get; private set; }
        public string InteractType { get; set; } = "Bed";
        public Vector3 PosOffset { get; set; } = Vector3.zero;
        #endregion

        public void OnDestroy()
        {
            if (ManagerNS.LocalGameManager.Instance?.IslandAreaManager != null)
            {
                ManagerNS.LocalGameManager.Instance.IslandAreaManager.UpdatedFieldTransformsAction -= HouseDetectionAction;
            }
        }

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                ManagerNS.LocalGameManager.Instance.IslandAreaManager.UpdatedFieldTransformsAction += HouseDetectionAction;
                ML.Engine.InventorySystem.ItemManager.Instance.AddItemIconObject("", transform, 
                    new Vector3(0, transform.GetComponent<BoxCollider>().size.y * 1.5f, 0), 
                    Quaternion.Euler(Vector3.zero), Vector3.one,
                    (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).currentCharacter.transform);
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }

        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Clan_UI/Prefab_Clan_UI_ClanBedPanel.prefab", ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false).Completed += (handle) =>
            {
                UI.UIClanBed uiPanel = (handle.Result).GetComponent<UI.UIClanBed>();
                uiPanel.Bed = this;
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
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
                ML.Engine.BuildingSystem.BuildingPart.BuildingPart bp = hit.collider.GetComponent<ML.Engine.BuildingSystem.BuildingPart.BuildingPart>();
                if (bp != null && (bp.Classification.Category2 == ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2.Roof || 
                    bp.Classification.Category2 == ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2.Floor))
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
                ML.Engine.BuildingSystem.BuildingPart.BuildingPart bp = hit.collider.GetComponent<ML.Engine.BuildingSystem.BuildingPart.BuildingPart>();
                if (bp != null && bp.Classification.Category2 == ML.Engine.BuildingSystem.BuildingPart.BuildingCategory2.Floor)
                {
                    flagDown = true;
                    break;
                }
            }
            CanSetClan = flagUp && flagDown;
            string icon = CanSetClan ? "Tex2D_Clan_UI_BedEnable" : "Tex2D_Clan_UI_BedDisable";
            GetComponentInChildren<ML.Engine.InventorySystem.ItemIcon>()?.SetSprite(ManagerNS.LocalGameManager.Instance.ClanManager.GetItemSprite(icon));
            return CanSetClan;
        }

        private void HouseDetectionAction()
        {
            if (ManagerNS.LocalGameManager.Instance.IslandAreaManager.updatedFieldTransforms.Contains(this.transform.parent))
            {
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
            if (HaveClan)
            {
                Clan.Bed = null;
            }
            if (clan != null && clan.HasBed)
            {
                clan.Bed.Clan = null;
            }
            Clan = clan;
            if (HaveClan)
            {
                clan.Bed = this;
            }
        }

        public void SetEmpty()
        {
            if (HaveClan)
            {
                Clan.Bed = null;
            }
            Clan = null;
        }

        private void OnDrawGizmosSelected()
        {
            // 向上检测
            Vector3 posUp = transform.TransformPoint(ConfigUp.Offset);
            Quaternion rotUp = transform.rotation * Quaternion.Euler(ConfigUp.Rotation);
            Vector3 scaleUp = Vector3.Scale(ConfigUp.Scale, transform.localScale);
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(posUp, rotUp, scaleUp);
            Gizmos.DrawWireCube(Vector3.zero, ConfigUp.Size);
            // 向下检测
            Vector3 posDown = transform.TransformPoint(ConfigDown.Offset);
            Quaternion rotDown = transform.rotation * Quaternion.Euler(ConfigDown.Rotation);
            Vector3 scaleDown = Vector3.Scale(ConfigDown.Scale, transform.localScale);
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(posDown, rotDown, scaleDown);
            Gizmos.DrawWireCube(Vector3.zero, ConfigDown.Size);
        }
    }
}