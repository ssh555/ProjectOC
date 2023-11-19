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
        /// 自身的SocketType
        /// </summary>
        [LabelText("SocketType"), FoldoutGroup("SocketProperty"), PropertyTooltip("自身的SocketType,即ID")]
        public BuildingSocketType Type;

        /// <summary>
        /// 可以容纳的SocketType
        /// </summary>
        [LabelText("MatchType"), FoldoutGroup("SocketProperty"), PropertyTooltip("可以容纳的SocketType,即与Active匹配用的Type")]
        public BuildingSocketType InTakeType;

        /// <summary>
        /// to-do : 作为AcitveSocket时调用
        /// </summary>
        public virtual void OnActive()
        {

        }

        /// <summary>
        /// to-do : 不在激活，即置为Disactieve时调用
        /// </summary>
        public virtual void OnDisactive()
        {

        }


        #endregion

        #region Transform
        /// <summary>
        /// 依附的BuildingPart
        /// </summary>
        protected IBuildingPart ParentBPart;

        /// <summary>
        /// 放置BPart时是否允许旋转
        /// </summary>
        [LabelText("启用BPart旋转偏移"), PropertyTooltip("放置BPart时是否允许有旋转偏移"), FoldoutGroup("Transform"), SerializeField]
        protected bool IsCanRotate = false;

        [LabelText("BPart位置偏移量"), FoldoutGroup("Transform"), SerializeField]
        protected Vector3 PositionOffset;

        /// <summary>
        /// 放置的BPart的位置
        /// </summary>
        public Vector3 BPartPosition
        {
            get => this.transform.position + this.PositionOffset;
        }

        [LabelText("BPart旋转偏移量"), FoldoutGroup("Transform"), SerializeField]
        protected Quaternion RotationOffset;
        /// <summary>
        /// 放置的BPart的旋转
        /// </summary>
        public Quaternion BPartRotation
        {
            get => this.transform.rotation * this.RotationOffset;
        }

        /// <summary>
        /// 获取匹配点的坐标
        /// 返回负无穷大值为无效位置值
        /// 返回Nan为无效旋转值
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
        /// 可放置区域Area类型
        /// </summary>
        [LabelText("可放置区域Area类型"), FoldoutGroup("Area")]
        public BuildingSystem.BuildingArea.BuildingAreaType CanPlaceAreaType;

        /// <summary>
        /// 用于area检测时的判断半径，即Socket与Area使用两套判定逻辑
        /// </summary>
        [LabelText("Area 检测半径"), ShowInInspector, FoldoutGroup("Area"), PropertyTooltip("单位 m")]
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

        //    // 检测到 BuildingArea
        //    BuildingArea.BuildingArea area = other.GetComponentInParent<BuildingArea.BuildingArea>();
        //    if(area != null)
        //    {
        //        if (area.Type.HasFlag(this.Type))
        //        {
        //            this.ParentBPart.AttachedArea = area;
        //        }
        //        return;
        //    }

        //    // 检测到 BuildingSocket
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
