using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.Manager;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingSocket
{
    public class BuildingSocket : MonoBehaviour
    {
        #region Socket
        /// <summary>
        /// �����SocketType
        /// </summary>
        [LabelText("ID"), FoldoutGroup("SocketProperty"), PropertyTooltip("�����SocketType,��ID")]
        public BuildingSocketType ID;
        [LabelText("�Ƿ���л�"), FoldoutGroup("SocketProperty"), PropertyTooltip("Ϊtrueʱ����ʾ���Socket����Ϊ�л������ʹ��")]
        public bool IsActiveSocket = true;


        ///// <summary>
        ///// �������ɵ�SocketType
        ///// </summary>
        //[ReadOnly, LabelText("MatchType"), FoldoutGroup("SocketProperty"), PropertyTooltip("�������ɵ�SocketType,����Activeƥ���õ�Type")]
        //public BuildingSocketType InTakeType;

        /// <summary>
        /// to-do : ��ΪAcitveSocketʱ����
        /// </summary>
        public virtual void OnActive()
        {
            //#if UNITY_EDITOR
            //Gizmos_Active = true;
            //#endif
            BuildingManager.Instance.VisualSocket.SetActive(true);
            BuildingManager.Instance.VisualSocket.transform.SetParent(this.transform);
            BuildingManager.Instance.VisualSocket.transform.localPosition = Vector3.zero;
        }

        /// <summary>
        /// to-do : ���ڼ������ΪDisactieveʱ����
        /// </summary>
        public virtual void OnDisactive()
        {
            //#if UNITY_EDITOR
            //            Gizmos_Active = false;
            //#endif
            BuildingManager.Instance.ResetVisualSocket();
        }

        public bool CheckMatch(BuildingSocket target)
        {
            return BuildingManager.Instance.Placer.Socket2SocketMatch.IsMatch(this.ID, target.ID, out PositionOffset, out RotationOffset);
        }
        #endregion

        #region Transform
        /// <summary>
        /// ������BuildingPart
        /// </summary>
        protected IBuildingPart ParentBPart;

        /// <summary>
        /// ����BPartʱ�Ƿ�������ת
        /// </summary>
        [LabelText("����BPart��תƫ��"), PropertyTooltip("����BPartʱ�Ƿ���������תƫ��"), FoldoutGroup("Transform"), SerializeField]
        protected bool IsCanRotate = true;

        [LabelText("������ʱ��������ת�ı仯ֵ"), FoldoutGroup("Transform"), SerializeField]
        protected Quaternion m_AsTargetRotDelta = Quaternion.identity;
        public Quaternion AsTargetRotDelta
        {
            get
            {
                if(m_AsTargetRotDelta.x == 0 && m_AsTargetRotDelta.y == 0 && m_AsTargetRotDelta.z == 0 && m_AsTargetRotDelta.w == 0)
                {
                    m_AsTargetRotDelta = Quaternion.identity;
                }
                return m_AsTargetRotDelta;
            }
        }
        [LabelText("����ʱ������תƫ��ֵ"), ReadOnly, FoldoutGroup("Transform"), SerializeField]

        protected Quaternion m_AsMatchRotOffset = Quaternion.identity;
        public Quaternion AsMatchRotOffset
        {
            get
            {
                if(m_AsMatchRotOffset.x == 0 && m_AsMatchRotOffset.y == 0 && m_AsMatchRotOffset.z == 0 && m_AsMatchRotOffset.w == 0)
                {
                    m_AsMatchRotOffset = Quaternion.identity;
                }
                return m_AsMatchRotOffset;
            }
            set
            {
                m_AsMatchRotOffset = value;
            }
        }

        public bool CanRotateMatchSocket => IsCanRotate;

        [LabelText("����Areaʱ��ת"), FoldoutGroup("Transform"), SerializeField]
        protected bool IsRotateOnArea = true;

        [LabelText("BPartλ��ƫ����"), FoldoutGroup("Transform"), SerializeField, ReadOnly]
        protected Vector3 PositionOffset;

        [LabelText("BPart��תƫ����"), FoldoutGroup("Transform"), SerializeField, ReadOnly]
        protected Quaternion RotationOffset = Quaternion.identity;


        /// <summary>
        /// ��ȡƥ��������
        /// ���ظ������ֵΪ��Чλ��ֵ
        /// ����NanΪ��Ч��תֵ
        /// </summary>
        /// <returns></returns>
        public bool GetMatchTransformOnSocket(out Vector3 pos, out Quaternion rot)
        {
            if (this.ParentBPart.AttachedSocket != null && CheckMatch(this.ParentBPart.AttachedSocket))
            {
                pos = this.ParentBPart.AttachedSocket.transform.position - (this.transform.position - this.ParentBPart.transform.position) + (this.ParentBPart.AttachedSocket.transform.rotation * this.RotationOffset * this.PositionOffset);
                rot = ParentBPart.BaseRotation * this.ParentBPart.AttachedSocket.transform.rotation * this.RotationOffset;// * (this.ParentBPart.AttachedSocket.IsCanRotate ? ParentBPart.RotOffset : Quaternion.identity);
                return true;
            }

            pos = Vector3.negativeInfinity;
            rot = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
            return false;
        }

        public bool GetMatchTransformOnArea(Vector3 TargetPos, out Vector3 pos, out Quaternion rot)
        {
            if (this.ParentBPart.AttachedArea != null)
            {
                bool b = this.ParentBPart.AttachedArea.GetMatchPointOnArea(TargetPos, out pos, out rot);
                if(b == false)
                {
                    pos = Vector3.negativeInfinity;
                    rot = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
                    return false;
                }
                if (IsRotateOnArea)
                {
                    rot *= ParentBPart.BaseRotation * (this.ParentBPart.AttachedArea.IsCanRotate ? ParentBPart.RotOffset : Quaternion.identity);
                }
                else
                {
                    rot = ParentBPart.BaseRotation * (this.ParentBPart.AttachedArea.IsCanRotate ? ParentBPart.RotOffset : Quaternion.identity);
                }
                return true;
            }

            pos = Vector3.negativeInfinity;
            rot = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
            return false;
        }

        #endregion

        #region Area
        ///// <summary>
        ///// �ɷ�������Area����
        ///// </summary>
        //[LabelText("�ɷ�������Area����"), FoldoutGroup("Area")]
        //public BuildingSocketType CanPlaceAreaType;

        /// <summary>
        /// ����area���ʱ���жϰ뾶����Socket��Areaʹ�������ж��߼�
        /// </summary>
        //[LabelText("Area ���뾶"), ShowInInspector, FoldoutGroup("Area"), PropertyTooltip("��λ m")]
        //public static float AreaCheckRadius = 0.1f;

        #endregion

        #region TriggerCheck
        protected Collider _collider;
        private void Awake()
        {
            this.ParentBPart = this.GetComponentInParent<IBuildingPart>();
            this._collider = this.GetComponent<Collider>();
            this._collider.isTrigger = true;

        }

        //private void OnTriggerStay(Collider other)
        //{
        //    if(this.ParentBPart.ActiveSocket != this)
        //    {
        //        return;
        //    }

        //    // ��⵽ BuildingArea
        //    BuildingArea.BuildingArea area = other.GetComponentInParent<BuildingArea.BuildingArea>();
        //    if(area != null)
        //    {
        //        if (area.Type.HasFlag(this.Type))
        //        {
        //            this.ParentBPart.AttachedArea = area;
        //        }
        //        return;
        //    }

        //    // ��⵽ BuildingSocket
        //    BuildingSocket socket = other.GetComponentInParent<BuildingSocket>();
        //    if(socket != null)
        //    {
        //        if(socket.InTakeType.HasFlag(this.Type))
        //        {
        //            this.ParentBPart.AttachedSocket = socket;
        //        }
        //        return;
        //    }
        //}
        #endregion

        #region Unity
        private void Start()
        {
            BuildingManager.Instance.BuildingSocketList.Add(this);
        }

        private void OnEnable()
        {
            this._collider.enabled = true;
        }

        private void OnDisable()
        {
            this._collider.enabled = false;
        }

        private void OnDestroy()
        {
            BuildingManager.Instance.BuildingSocketList.Remove(this);
        }

#if UNITY_EDITOR
        [ShowInInspector]
        private bool IsShowSocket = false;
#endif

        public void OnDrawGizmos()
        {
            if (BuildingManager.Instance == null
#if UNITY_EDITOR
                || !IsShowSocket
#endif
                )
            {
                return;
            }

            var cols = this.GetComponentsInChildren<Collider>();
            foreach (Collider col in cols)
            {
                Extension.GizmosExtension.DrawMeshCollider(col, BuildingManager.Instance.DrawAreaBaseGrid.color);
            }
        }

        private void _OnDrawGizmos()
        {
            if (BuildingManager.Instance == null)
            {
                return;
            }
            if(BuildingManager.Instance.DrawActiveSocket.IsDraw)
            {
                var cols = this.GetComponentsInChildren<Collider>();
                foreach(Collider col in cols)
                {
                    Extension.GizmosExtension.DrawMeshCollider(col, BuildingManager.Instance.DrawActiveSocket.color);
                }
            }
            else if (BuildingManager.Instance.DrawSocket.IsDraw)
            {
                var cols = this.GetComponentsInChildren<Collider>();
                foreach (Collider col in cols)
                {
                    Extension.GizmosExtension.DrawMeshCollider(col, BuildingManager.Instance.DrawSocket.color);
                }
            }

        }

        private void OnDrawGizmosSelected()
        {
            _OnDrawGizmos();
            //float arrowLength = 2f;
            //float circleRadius = 0.3f;
            //Color arrowColor = Color.red;

            //// ���ü�ͷ����ɫ
            //Gizmos.color = arrowColor;

            //// ��ȡ�����λ�á���ת��X��
            //Vector3 position = transform.position;
            //Quaternion rotation = transform.rotation;
            //Vector3 xAxis = transform.right;

            //// �����ͷĩ�˵�λ��
            //Vector3 arrowEnd = position + xAxis * arrowLength;

            //// ���Ƽ�ͷ�߶�
            //Gizmos.DrawLine(position, arrowEnd);

            ////// �����ͷͷ����λ��
            ////Vector3 arrowHead1 = arrowEnd + (rotation * Quaternion.Euler(0, 160, 0) * xAxis) * 0.2f * arrowLength;
            ////Vector3 arrowHead2 = arrowEnd + (rotation * Quaternion.Euler(0, -160, 0) * xAxis) * 0.2f * arrowLength;

            ////// ���Ƽ�ͷͷ���������߶�
            ////Gizmos.DrawLine(arrowEnd, arrowHead1);
            ////Gizmos.DrawLine(arrowEnd, arrowHead2);
            //// ���Ƽ�ͷͷ����Բ׶��

            //// ������㴦��ԲȦ
            //Gizmos.DrawWireSphere(position, circleRadius);

            Gizmos.DrawIcon(this.transform.position, "Socket");
        }

        #endregion

    }

}
