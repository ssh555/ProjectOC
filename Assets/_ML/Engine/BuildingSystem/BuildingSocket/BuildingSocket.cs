using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingSocket
{
    public class BuildingSocket : MonoBehaviour
    {
        #region Socket
        /// <summary>
        /// �����SocketType
        /// </summary>
        [LabelText("SocketType"), FoldoutGroup("SocketProperty"), PropertyTooltip("�����SocketType,��ID")]
        public BuildingSocketType Type;

        /// <summary>
        /// �������ɵ�SocketType
        /// </summary>
        [LabelText("MatchType"), FoldoutGroup("SocketProperty"), PropertyTooltip("�������ɵ�SocketType,����Activeƥ���õ�Type")]
        public BuildingSocketType InTakeType;

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
        protected bool IsCanRotate = false;

        [LabelText("BPartλ��ƫ����"), FoldoutGroup("Transform"), SerializeField]
        protected Vector3 PositionOffset;

        /// <summary>
        /// ���õ�BPart��λ��
        /// </summary>
        public Vector3 BPartPosition
        {
            get => this.transform.position + this.PositionOffset;
        }

        [LabelText("BPart��תƫ����"), FoldoutGroup("Transform"), SerializeField]
        protected Quaternion RotationOffset;
        /// <summary>
        /// ���õ�BPart����ת
        /// </summary>
        public Quaternion BPartRotation
        {
            get => this.transform.rotation * this.RotationOffset;
        }

        /// <summary>
        /// ��ȡƥ��������
        /// ���ظ������ֵΪ��Чλ��ֵ
        /// ����NanΪ��Ч��תֵ
        /// </summary>
        /// <returns></returns>
        public bool GetMatchTransformOnSocket(out Vector3 pos, out Quaternion rot)
        {
            if (this.ParentBPart.AttachedSocket != null)
            {
                pos = this.ParentBPart.AttachedSocket.BPartPosition - (this.transform.position - this.ParentBPart.transform.position);
                rot = ParentBPart.BaseRotation * this.ParentBPart.AttachedSocket.BPartRotation * (this.IsCanRotate ? ParentBPart.RotOffset : Quaternion.identity);


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
                this.ParentBPart.AttachedArea.GetMatchPointOnArea(TargetPos, AreaCheckRadius, out pos, out rot);
                rot *= (this.ParentBPart.AttachedArea.IsCanRotate ? ParentBPart.RotOffset : Quaternion.identity);
                return true;
            }

            pos = Vector3.negativeInfinity;
            rot = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
            return false;
        }

        #endregion

        #region Area
        /// <summary>
        /// �ɷ�������Area����
        /// </summary>
        [LabelText("�ɷ�������Area����"), FoldoutGroup("Area")]
        public BuildingSystem.BuildingArea.BuildingAreaType CanPlaceAreaType;

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
    }

}
