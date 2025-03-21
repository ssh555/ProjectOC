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
        protected Collider _collider;

        /// <summary>
        /// 获取在Area上匹配点的坐标
        /// 返回负无穷大值为无效位置值
        /// 返回Nan为无效旋转值
        /// </summary>
        /// <returns></returns>
        public bool GetMatchPointOnArea(Vector3 point, out Vector3 pos, out Quaternion rot)
        {
            // 位于 Bound 内
            Bounds bs = new Bounds(this._collider.bounds.center, this._collider.bounds.size);
            if (bs.extents.y < 0.01f)
            {
                bs.extents = new Vector3(bs.extents.x, 0.01f, bs.extents.z);
                bs.size = new Vector3(bs.size.x, 0.02f, bs.size.z);
            }
            if(bs.Contains(point))
            {
                Vector3 scale = this.transform.localScale;
                Vector3 oldpos = this.transform.localPosition;
                var parent = this.transform.parent;
                this.transform.SetParent(null);
                this.transform.localScale = Vector3.one;

                // point 在 Area 坐标系下的表示
                Vector3 localPoint = this.transform.InverseTransformPoint(point);

                // 最近网格点局部坐标
                Vector3 localNearestGP = new Vector3(Mathf.RoundToInt(localPoint.x / BaseGridSideLength), localPoint.y, Mathf.RoundToInt(localPoint.z / BaseGridSideLength)) * BaseGridSideLength;
                localNearestGP.y = localPoint.y;

                // 不启用网格辅助 && 不在范围内 ? 返回原点 : 返回网格点
                //pos =(!BuildingManager.Instance.Placer.IsEnableGridSupport && Vector3.Distance(localPoint, localNearestGP) > radius) ? point : this.transform.TransformPoint(localNearestGP);
                // 不启用网格辅助 ? 返回原点 : 返回网格点
                pos = !BuildingManager.Instance.Placer.IsEnableGridSupport ? point : this.transform.TransformPoint(localNearestGP);
                // 使用Area.Rotation;
                rot = this.transform.rotation;

                this.transform.SetParent(parent);
                this.transform.localScale = scale;
                this.transform.localPosition = oldpos;
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
        public BuildingAreaType Type;

        /// <summary>
        /// 目标Part是否能放置于此区域
        /// </summary>
        /// <param name="BPart"></param>
        /// <returns></returns>
        public bool CheckAreaTypeMatch(IBuildingPart BPart)
        {
            return BuildingManager.Instance.Placer.AreaSocketMatch.IsMatch(BPart.ActiveSocket.ID, this.Type);
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
            this._collider = this.GetComponent<Collider>();
            this._collider.isTrigger = false;
        }

        private void Start()
        {
            BuildingManager.Instance.BuildingAreaList.Add(this);
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
            BuildingManager.Instance.BuildingAreaList.Remove(this);
        }

#if UNITY_EDITOR
        [ShowInInspector]
        private bool IsShowArea = false;
#endif
        private void OnDrawGizmos()
        {
            if (BuildingManager.Instance == null
#if UNITY_EDITOR
                || !IsShowArea
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

        private void OnDrawGizmosSelected()
        {
            if (BuildingManager.Instance == null)
            {
                return;
            }
            float smallSize = BaseGridSideLength; // 小格子的大小
            float largeSize = BoundGridSideLength; // 大格子的大小
            float height = this.GetComponent<Collider>().bounds.size.y;
            Vector3 pos = transform.position;

            if(BuildingManager.Instance.DrawAreaBaseGrid.IsDraw)
            {
                // 绘制小格子
                Gizmos.color = BuildingManager.Instance.DrawAreaBaseGrid.color;
                for (float y = pos.y; y < pos.y + height; y += smallSize)
                {
                    for (float x = pos.x; x < pos.x + largeSize; x += smallSize)
                    {
                        for (float z = pos.z; z < pos.z + largeSize; z += smallSize)
                        {
                            Gizmos.DrawCube(new Vector3(x, y, z), new Vector3(smallSize, smallSize, smallSize));
                        }
                    }
                }
            }

            if (BuildingManager.Instance.DrawAreaBoundGrid.IsDraw)
            {
                // 绘制大格子
                Gizmos.color = BuildingManager.Instance.DrawAreaBoundGrid.color;
                for (float y = pos.y; y < pos.y + height; y += largeSize)
                {
                    for (float x = pos.x; x < pos.x + largeSize; x += largeSize)
                    {
                        for (float z = pos.z; z < pos.z + largeSize; z += largeSize)
                        {
                            Gizmos.DrawCube(new Vector3(x, y, z), new Vector3(largeSize, largeSize, largeSize));
                        }
                    }
                }
            }



        }
    }
}

