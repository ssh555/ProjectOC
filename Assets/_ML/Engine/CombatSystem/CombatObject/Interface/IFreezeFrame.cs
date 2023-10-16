using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.CombatSystem.CombatObject
{
    /// <summary>
    /// 卡肉|顿帧 参数
    /// </summary>
    [System.Serializable]
    public struct FreezeFrameParams
    {
        /// <summary>
        /// 卡肉|顿帧等级
        /// </summary>
        [LabelText("卡肉|顿帧等级"), SerializeField]
        public byte freezeLevel;

        /// <summary>
        /// 持续时间
        /// </summary>
        [LabelText("持续时间"), SerializeField]
        public float duration;

        /// <summary>
        /// 时间流速
        /// </summary>
        [LabelText("时间流速"), SerializeField]
        public float timeScale;
    }

    /// <summary>
    /// 卡肉|顿帧 接口
    /// </summary>
    public interface IFreezeFrame
    {
        /// <summary>
        /// 应用卡肉
        /// </summary>
        public abstract void ApplyFreezeFrame(FreezeFrameParams freezeFrameParams);

        /// <summary>
        /// 结束卡肉
        /// </summary>
        public abstract void EndFreezeFrame();
    }

}
