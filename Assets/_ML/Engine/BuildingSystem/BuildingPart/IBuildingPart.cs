using ML.Engine.InventorySystem.CompositeSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingPart
{
    /// <summary>
    /// 建筑物接口
    /// </summary>
    public interface IBuildingPart
    {
        #region ID
        // to-do : PlaceMode下新建建筑物时会赋予InstanceID, 仅此一次，并会存档记录，直至该建筑物销毁

        /// <summary>
        /// Prefab 层分类 => 分类标识
        /// </summary>
        public BuildingPartClassification Classification { get; }

        /// <summary>
        /// 建筑物实例ID => 建筑物实例ID组成 2023-09-01-13-59-59-999 or 当前秒数时间
        /// </summary>
        public string InstanceID { get; set; }

        /// <summary>
        /// 依附的 GameObject
        /// </summary>
        public GameObject gameObject { get;  }

        /// <summary>
        /// 依附的 Transfrom
        /// </summary>
        public Transform transform { get; }
        #endregion

        #region Mode
        /// <summary>
        /// Trigger 碰撞检测的结果 => 是否能放置
        /// </summary>
        public bool CanPlaceInPlaceMode { get; set; }

        public delegate bool CheckCanPlaceMode(IBuildingPart BPart);
        public event CheckCanPlaceMode CheckCanInPlaceMode;

        /// <summary>
        /// 当前所处的建造模式
        /// </summary>
        public BuildingMode Mode { get; set; }

        // to-do : Mode 更改时会一并更改材质表现, 实现接口时在具体类内部自行实现
        ///// <summary>
        ///// 存储原生的材质
        ///// </summary>
        //Dictionary<Renderer, Material> RowMat { get; set; }
        #endregion

        #region 旋转
        /// <summary>
        /// 自身初始旋转
        /// </summary>
        public Quaternion BaseRotation { get; }

        /// <summary>
        /// 旋转偏移量
        /// 自身旋转 = BaseRotation -> Socket|Area [ -> RotOffset]
        /// </summary>
        public Quaternion RotOffset { get; set; }
        #endregion

        #region BuildingArea
        /// <summary>
        /// 依附的Area,更新时更改位置和旋转，但是优先级低于AttachedSocket
        /// </summary>
        public BuildingArea.BuildingArea AttachedArea { get; set; }
        /// <summary>
        /// 用于BuildingArea匹配的ActiveSocket.AreaType
        /// </summary>
        public BuildingSocket.BuildingSocketType ActiveAreaType
        {
            get
            {
                return this.ActiveSocket == null ? BuildingSocket.BuildingSocketType.None : this.ActiveSocket.CanPlaceAreaType;
            }
        }
        #endregion

        #region BuildingSocket
        /// <summary>
        /// 依附的Socket => 更新时直接更新位置和旋转, 覆盖Area
        /// </summary>
        public BuildingSocket.BuildingSocket AttachedSocket { get; set; }

        /// <summary>
        /// 当前自己激活用于匹配Socket的Socket
        /// 只有处于Place状态时才有效
        /// 其他时候设置为null
        /// </summary>
        public BuildingSocket.BuildingSocket ActiveSocket { get; set; }

        /// <summary>
        /// 拥有的Socket列表，用于轮换
        /// </summary>
        public List<BuildingSocket.BuildingSocket> OwnedSocketList { get; }

        /// <summary>
        /// 注册BuildingSocket
        /// </summary>
        /// <param name="socket"></param>
        public void AddBuildingSocket(BuildingSocket.BuildingSocket socket)
        {
            if(!this.OwnedSocketList.Contains(socket))
            {
                this.OwnedSocketList.Add(socket);
            }
        }

        /// <summary>
        /// 注销BuildingSocket
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public bool RemoveBuildingSocket(BuildingSocket.BuildingSocket socket)
        {
            return this.OwnedSocketList.Remove(socket);
        }

        /// <summary>
        /// 轮换激活的插槽
        /// 更新 ActiveSocket
        /// 更新位置和旋转
        /// </summary>
        public abstract void AlternativeActiveSocket();
        #endregion

        #region Material
        /// <summary>
        /// 获得当前Mode对应的材质
        /// </summary>
        /// <returns></returns>
        public virtual Material GetCurModeMat()
        {
            return BuildingManager.Instance.GetBuldingMat(this.Mode);
        }

        /// <summary>
        /// 获得拷贝的材质
        /// </summary>
        /// <returns></returns>
        public abstract BuildingCopiedMaterial GetCopiedMaterial();

        /// <summary>
        /// 粘贴拷贝的材质
        /// </summary>
        /// <param name="copiedMaterial"></param>
        public abstract void SetCopiedMaterial(BuildingCopiedMaterial copiedMaterial);
        #endregion

        #region CheckTrigger
        /// <summary>
        /// 最近一次检测到的不能放置的帧数 FrameCount
        /// </summary>
        public int lastTriggerFrameCount { get; set; }
        public BuildingMode tmpTriggerMode { get; set; }
        public void CheckTriggerStay(Collider other)
        {
            if(this.Mode == BuildingMode.Place || this.Mode == BuildingMode.Destroy || this.Mode == BuildingMode.Edit)
            {
                if(this.Mode != BuildingMode.Destroy)
                {
                    this.tmpTriggerMode = this.Mode;
                }
                if (other.GetComponent<IBuildingPart>() != null)
                {
                    this.CanPlaceInPlaceMode = false;
                    this.Mode = BuildingMode.Destroy;
                    lastTriggerFrameCount = Time.frameCount;
                }
                // 同一帧检测到不能放置，则此帧就不能更改能否放置值，仅为不能放置
                else if(lastTriggerFrameCount != Time.frameCount)
                {
                    this.CanPlaceInPlaceMode = true;
                    this.Mode = this.tmpTriggerMode;
                }
                this.Mode = this.CanPlaceInPlaceMode ? this.tmpTriggerMode : BuildingMode.Destroy;
            }
        }
        #endregion

    }
}

