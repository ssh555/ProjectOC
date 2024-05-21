using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    public interface ITransport 
    {
        #region Property
        [LabelText("负责该搬运的刁民"), ReadOnly, ShowInInspector]
        public WorkerNS.Worker Worker { get; set; }
        [LabelText("需要搬运的数量"), ReadOnly, ShowInInspector]
        public int MissionNum { get; set; }
        [LabelText("当前拿到的数量"), ReadOnly, ShowInInspector]
        public int CurNum { get; set; }
        [LabelText("完成的数量"), ReadOnly, ShowInInspector]
        public int FinishNum { get; set; }
        [LabelText("取货地预留的数量"), ReadOnly, ShowInInspector]
        public int SoureceReserveNum { get; set; }
        [LabelText("送货地预留的数量"), ReadOnly, ShowInInspector]
        public int TargetReserveNum { get; set; }
        [LabelText("是否到达取货地"), ReadOnly, ShowInInspector]
        public bool ArriveSource { get; set; }
        [LabelText("是否到达目的地"), ReadOnly, ShowInInspector]
        public bool ArriveTarget { get; set; }
        [LabelText("是否是有效的"), ReadOnly, ShowInInspector]
        public bool IsValid { get; }
        #endregion

        #region Method
        public Transform GetSourceTransform();
        public Transform GetTargetTransform();
        public void UpdateDestination();
        /// <summary>
        /// 从取货地拿出
        /// </summary>
        public void PutOutSource();
        /// <summary>
        /// 放入送货地
        /// </summary>
        public void PutInTarget();
        /// <summary>
        /// 结束搬运
        /// </summary>
        public void End();
        #endregion
    }
}