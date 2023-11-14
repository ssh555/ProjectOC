using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Player
{
    /// <summary>
    /// 挂载于 PlayerModel 上用于耦合的动画帧事件接口
    /// </summary>
    public class PlayerModelAniamtionEvent : MonoBehaviour
    {
        PlayerModelStateController stateController => this.GetComponentInParent<PlayerCharacter>().playerModelStateController;

        PlayerNormalMove moveAbility => stateController.moveAbility;

        /// <summary>
        /// 起跳 => 估计用不上
        /// </summary>
        private void EnterInPreJump()
        {
            moveAbility.IsInPreJump = true;
        }

        /// <summary>
        /// 正式跳跃 => 需要在起跳动画(IsInPreJump)中标记动画帧事件，不然不会起跳
        /// </summary>
        private void ExitInPreJump()
        {
            moveAbility.IsInPreJump = false;
        }
    }
}

