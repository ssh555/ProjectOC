using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
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

        }

        /// <summary>
        /// to-do : ���ڼ������ΪDisactieveʱ����
        /// </summary>
        public virtual void OnDisactive()
        {

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
                pos = this.ParentBPart.AttachedSocket.transform.position - (this.transform.position - this.ParentBPart.transform.position) + (this.ParentBPart.AttachedSocket.transform.rotation * this.PositionOffset);
                rot = ParentBPart.BaseRotation * this.RotationOffset * (this.ParentBPart.AttachedSocket.IsCanRotate ? ParentBPart.RotOffset : Quaternion.identity);
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
                bool b = this.ParentBPart.AttachedArea.GetMatchPointOnArea(TargetPos, AreaCheckRadius, out pos, out rot);
                if(b == false)
                {
                    pos = Vector3.negativeInfinity;
                    rot = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
                    return false;
                }
                rot *= ParentBPart.BaseRotation * (this.ParentBPart.AttachedArea.IsCanRotate ? ParentBPart.RotOffset : Quaternion.identity);
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
        [LabelText("Area ���뾶"), ShowInInspector, FoldoutGroup("Area"), PropertyTooltip("��λ m")]
        public static float AreaCheckRadius = 0.1f;

        #endregion

        #region TriggerCheck
        protected new Collider collider;
        private void Awake()
        {
            this.ParentBPart = this.GetComponentInParent<IBuildingPart>();
            this.collider = this.GetComponent<Collider>();
            this.collider.isTrigger = true;

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
            this.collider.enabled = true;
        }

        private void OnDisable()
        {
            this.collider.enabled = false;
        }

        private void OnDestroy()
        {
            BuildingManager.Instance.BuildingSocketList.Remove(this);
        }


        private void OnDrawGizmos()
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
                    Extension.GizmosExtension.DrawWireCollider(col, BuildingManager.Instance.DrawActiveSocket.color);
                }
            }
            else if (BuildingManager.Instance.DrawSocket.IsDraw)
            {
                var cols = this.GetComponentsInChildren<Collider>();
                foreach (Collider col in cols)
                {
                    Extension.GizmosExtension.DrawWireCollider(col, BuildingManager.Instance.DrawSocket.color);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
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
