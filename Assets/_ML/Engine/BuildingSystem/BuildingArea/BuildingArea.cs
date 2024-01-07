using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingArea
{
    // Require Collider
    // 碰撞检测交由BuildingSocket自己检测
    [RequireComponent(typeof(Collider))]
    public class BuildingArea : MonoBehaviour
    {
        #region Static Grid
        /// <summary>
        /// 用于依附的基础单位格子的边长，单位为m
        /// </summary>
        [LabelText("基础单位格子边长"), PropertyTooltip("单位 m"), Range(0, 100), FoldoutGroup("网格"), ShowInInspector]
        public static float BaseGridSideLength = 0.50f;

        /// <summary>
        /// 用于包裹基础单位格子的边长，单位为m
        /// </summary>
        [LabelText("包裹单位格子边长"), PropertyTooltip("单位 m"), Range(0, 1000), FoldoutGroup("网格"), ShowInInspector]
        public static float BoundGridSideLength = 10.00f;
        #endregion

        /// <summary>
        /// 放置BPart时是否允许旋转
        /// </summary>
        [LabelText("启用BPart旋转偏移"), PropertyTooltip("放置BPart时是否允许有旋转偏移"), FoldoutGroup("自身属性"), SerializeField]
        public bool IsCanRotate = false;

        /// <summary>
        /// 区域碰撞检测Collider
        /// </summary>
        protected new Collider collider;

        /// <summary>
        /// 获取在Area上匹配点的坐标
        /// 返回负无穷大值为无效位置值
        /// 返回Nan为无效旋转值
        /// </summary>
        /// <returns></returns>
        public bool GetMatchPointOnArea(Vector3 point, float radius, out Vector3 pos, out Quaternion rot)
        {
            // 位于 Bound 内
            if(this.collider.bounds.Contains(point))
            {
                Vector3 scale = this.transform.localScale;
                this.transform.localScale = Vector3.one;

                // point 在 Area 坐标系下的表示
                Vector3 localPoint = this.transform.InverseTransformPoint(point);

                // 最近网格点局部坐标
                Vector3 localNearestGP = new Vector3(Mathf.RoundToInt(localPoint.x / BaseGridSideLength), localPoint.y, Mathf.RoundToInt(localPoint.z / BaseGridSideLength)) * BaseGridSideLength;

                // 不启用网格辅助 && 不在范围内 ? 返回原点 : 返回网格点
                pos =(!BuildingManager.Instance.Placer.IsEnableGridSupport && Vector3.Distance(localPoint, localNearestGP) > radius) ? point : this.transform.TransformPoint(localNearestGP);
                // 使用Area.Rotation
                rot = this.transform.rotation;
                this.transform.localScale = scale;
                return true;
            }

            // 不处于 Area 内
            pos = Vector3.negativeInfinity;
            rot = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
            return false;
        }

        /// <summary>
        /// 用于匹配的AreaType
        /// </summary>
        [LabelText("匹配类型")]
        public BuildingSocket.BuildingSocketType Type;

        /// <summary>
        /// 目标Part是否能放置于此区域
        /// </summary>
        /// <param name="BPart"></param>
        /// <returns></returns>
        public bool CheckAreaTypeMatch(IBuildingPart BPart)
        {
            return (BPart.ActiveAreaType & this.Type) != 0;
        }
        
        /// <summary>
        /// 启用网格辅助时在Area表面绘制网格
        /// </summary>
        public virtual void DrawGrid()
        {
            // to-do
        }

        private void Awake()
        {
            this.collider = this.GetComponent<Collider>();
            this.collider.isTrigger = true;
        }

        private void Start()
        {
            StartCoroutine(__DelayInit__());
        }

        private IEnumerator __DelayInit__()
        {
            while(BuildingManager.Instance == null || BuildingManager.Instance.BuildingAreaList == null)
            {
                yield return null;
            }
            BuildingManager.Instance.BuildingAreaList.Add(this);
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
            BuildingManager.Instance.BuildingAreaList.Remove(this);
        }
    }
}

