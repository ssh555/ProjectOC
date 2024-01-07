using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InteractSystem
{
    public interface IInteraction
    {
        /// <summary>
        /// 可交互物类别 -> 用于UI
        /// 对应json文件 : InteractKeyTip.json
        /// </summary>
        public string InteractType { get; set; }

        /// <summary>
        /// 依附的GameObject
        /// </summary>
        public GameObject gameObject { get; }

        /// <summary>
        /// 显示的世界位置偏移
        /// </summary>
        public Vector3 PosOffset { get; set; }

        /// <summary>
        /// 作为选中的可交互物时调用
        /// </summary>
        /// <param name="component"></param>
        public void OnSelectedEnter(InteractComponent component)
        {

        }

        /// <summary>
        /// 退出作为选中的可交互物时调用
        /// </summary>
        /// <param name="component"></param>
        public void OnSelectedExit(InteractComponent component)
        {

        }

        /// <summary>
        /// 交互确认时调用
        /// </summary>
        /// <param name="component"></param>
        public void Interact(InteractComponent component)
        {

        }
    }
}
