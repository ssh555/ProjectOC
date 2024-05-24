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
        /// 自身的SocketType
        /// </summary>
        [LabelText("ID"), FoldoutGroup("SocketProperty"), PropertyTooltip("自身的SocketType,即ID")]
        public BuildingSocketType ID;
        [LabelText("是否可切换"), FoldoutGroup("SocketProperty"), PropertyTooltip("为true时，表示这个Socket能作为切换激活点使用")]
        public bool IsActiveSocket = true;


        ///// <summary>
        ///// 可以容纳的SocketType
        ///// </summary>
        //[ReadOnly, LabelText("MatchType"), FoldoutGroup("SocketProperty"), PropertyTooltip("可以容纳的SocketType,即与Active匹配用的Type")]
        //public BuildingSocketType InTakeType;

        /// <summary>
        /// to-do : 作为AcitveSocket时调用
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
        /// to-do : 不在激活，即置为Disactieve时调用
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
        /// 依附的BuildingPart
        /// </summary>
        protected IBuildingPart ParentBPart;

        /// <summary>
        /// 放置BPart时是否允许旋转
        /// </summary>
        [LabelText("启用BPart旋转偏移"), PropertyTooltip("放置BPart时是否允许有旋转偏移"), FoldoutGroup("Transform"), SerializeField]
        protected bool IsCanRotate = true;

        [LabelText("被吸附时吸附点旋转的变化值"), FoldoutGroup("Transform"), SerializeField]
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
        [LabelText("吸附时按键旋转偏移值"), ReadOnly, FoldoutGroup("Transform"), SerializeField]

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

        [LabelText("吸附Area时旋转"), FoldoutGroup("Transform"), SerializeField]
        protected bool IsRotateOnArea = true;

        [LabelText("BPart位置偏移量"), FoldoutGroup("Transform"), SerializeField, ReadOnly]
        protected Vector3 PositionOffset;

        [LabelText("BPart旋转偏移量"), FoldoutGroup("Transform"), SerializeField, ReadOnly]
        protected Quaternion RotationOffset = Quaternion.identity;


        /// <summary>
        /// 获取匹配点的坐标
        /// 返回负无穷大值为无效位置值
        /// 返回Nan为无效旋转值
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
        ///// 可放置区域Area类型
        ///// </summary>
        //[LabelText("可放置区域Area类型"), FoldoutGroup("Area")]
        //public BuildingSocketType CanPlaceAreaType;

        /// <summary>
        /// 用于area检测时的判断半径，即Socket与Area使用两套判定逻辑
        /// </summary>
        //[LabelText("Area 检测半径"), ShowInInspector, FoldoutGroup("Area"), PropertyTooltip("单位 m")]
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

            //// 设置箭头的颜色
            //Gizmos.color = arrowColor;

            //// 获取物体的位置、旋转和X轴
            //Vector3 position = transform.position;
            //Quaternion rotation = transform.rotation;
            //Vector3 xAxis = transform.right;

            //// 计算箭头末端的位置
            //Vector3 arrowEnd = position + xAxis * arrowLength;

            //// 绘制箭头线段
            //Gizmos.DrawLine(position, arrowEnd);

            ////// 计算箭头头部的位置
            ////Vector3 arrowHead1 = arrowEnd + (rotation * Quaternion.Euler(0, 160, 0) * xAxis) * 0.2f * arrowLength;
            ////Vector3 arrowHead2 = arrowEnd + (rotation * Quaternion.Euler(0, -160, 0) * xAxis) * 0.2f * arrowLength;

            ////// 绘制箭头头部的两个线段
            ////Gizmos.DrawLine(arrowEnd, arrowHead1);
            ////Gizmos.DrawLine(arrowEnd, arrowHead2);
            //// 绘制箭头头部的圆锥体

            //// 绘制起点处的圆圈
            //Gizmos.DrawWireSphere(position, circleRadius);

            Gizmos.DrawIcon(this.transform.position, "Socket");
        }

        #endregion

    }

}
