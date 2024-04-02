using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectOC.ClanNS;
using Sirenix.OdinInspector;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using ML.Engine.Manager;
using System;

namespace ProjectOC.Building
{
    public class Bed : BuildingPart, IInteraction
    {
        [LabelText("��������"), ShowInInspector, ReadOnly]
        public Clan Clan;
        [ShowInInspector, ReadOnly]
        public bool HasClan { get { return Clan != null && !string.IsNullOrEmpty(Clan.ID); } }
        #region ���ϼ��
        [LabelText("Offset"), FoldoutGroup("���ϼ��"), ShowInInspector]
        public Vector3 OffsetPositionUp;
        [LabelText("Length"), FoldoutGroup("���ϼ��"), ShowInInspector]
        public float LengthUp = 1f;
        [LabelText("Height"), FoldoutGroup("���ϼ��"), ShowInInspector]
        public float HeightUp = 1f;
        [LabelText("Width"), FoldoutGroup("���ϼ��"), ShowInInspector]
        public float WidthUp = 1f;
        [LabelText("Rotation"), FoldoutGroup("���ϼ��"), ShowInInspector]
        public Vector3 RotationUp;
        [LabelText("Scale"), FoldoutGroup("���ϼ��"), ShowInInspector]
        public Vector3 ScaleUp = Vector3.one;
        #endregion
        #region ���¼��
        [LabelText("Offset"), FoldoutGroup("���¼��"), ShowInInspector]
        public Vector3 OffsetPositionDown;
        [LabelText("Length"), FoldoutGroup("���¼��"), ShowInInspector]
        public float LengthDown = 1f;
        [LabelText("Width"), FoldoutGroup("���¼��"), ShowInInspector]
        public float WidthDown = 1f;
        [LabelText("Height"), FoldoutGroup("���¼��"), ShowInInspector]
        public float HeightDown = 1f;
        [LabelText("Rotation"), FoldoutGroup("���¼��"), ShowInInspector]
        public Vector3 RotationDown;
        [LabelText("Scale"), FoldoutGroup("���¼��"), ShowInInspector]
        public Vector3 ScaleDown = Vector3.one;
        #endregion
        public string InteractType { get; set; } = "Bed";
        public Vector3 PosOffset { get; set; } = Vector3.zero;

        private new void Start()
        {
            //ML.Engine.BuildingSystem.BuildingManager.Instance.Placer.OnBuildingModeExit += () =>
            //{
            //    // ������������ʱ�������������
            //    if (!HouseDetection())
            //    {
            //        SetClan(null);
            //    }
            //};
            this.enabled = false;
        }

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            // ���ǵ�һ���½�
            if (!isFirstBuild)
            {
                // ������������ʱ�������������
                if (!HouseDetection() && HasClan)
                {
                    SetEmpty();
                }
            }
            //isFirstBuild�ĸ��·��ڻ����ҪҪ�����ú���
            base.OnChangePlaceEvent(oldPos, newPos);
        }


        public bool HouseDetection()
        {
            // ���ϼ��
            Vector3 posUp = transform.TransformPoint(OffsetPositionUp);
            Quaternion rotUp = transform.rotation * Quaternion.Euler(RotationUp);
            Vector3 sizeUp = new Vector3(LengthUp, HeightUp, WidthUp) / 2;
            sizeUp = Vector3.Scale(sizeUp, ScaleUp);
            sizeUp = Vector3.Scale(sizeUp, transform.localScale);
            bool flagUp = false;
            foreach (RaycastHit hit in Physics.BoxCastAll(posUp, sizeUp, transform.up, rotUp))
            {
                BuildingPart bp = hit.collider.GetComponent<BuildingPart>();
                if (bp != null && bp.Classification.Category2 == BuildingCategory2.Roof)
                {
                    flagUp = true;
                    break;
                }
            }
            // ���¼��
            Vector3 posDown = transform.TransformPoint(OffsetPositionDown);
            Quaternion rotDown = transform.rotation * Quaternion.Euler(RotationDown);
            Vector3 sizeDown = new Vector3(LengthDown, HeightDown, WidthDown) / 2;
            sizeDown = Vector3.Scale(sizeDown, ScaleDown);
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
            if (flagUp && flagDown)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// ���ù�������
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
            // ���ϼ��
            Vector3 posUp = transform.TransformPoint(OffsetPositionUp);
            Quaternion rotUp = transform.rotation * Quaternion.Euler(RotationUp);
            Vector3 scaleUp = Vector3.Scale(ScaleUp, transform.localScale);
            Gizmos.color = UnityEngine.Color.green;
            Gizmos.matrix = Matrix4x4.TRS(posUp, rotUp, scaleUp);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(LengthUp, HeightUp, WidthUp));
            // ���¼��
            Vector3 posDown = transform.TransformPoint(OffsetPositionDown);
            Quaternion rotDown = transform.rotation * Quaternion.Euler(RotationDown);
            Vector3 scaleDown = Vector3.Scale(ScaleDown, transform.localScale);
            Gizmos.color = UnityEngine.Color.green;
            Gizmos.matrix = Matrix4x4.TRS(posDown, rotDown, scaleDown);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(LengthDown, HeightDown, WidthDown));
        }

        public void Interact(InteractComponent component)
        {
            GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIBedPanel.prefab", ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                ProjectOC.Building.UI.UIBed uiPanel = (handle.Result).GetComponent<ProjectOC.Building.UI.UIBed>();
                // ��ʼ���������
                uiPanel.Bed = this;
                uiPanel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                // Push
                GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }
    }
}