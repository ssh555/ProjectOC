using ML.Engine.FSM;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Player
{
    public sealed class PlayerModelStateController : StateController
    {
        public PlayerModelStateController(int priority, Animator model, Animator move, PlayerCharacter player) : base(priority)
        {
            this.modelAnimator = model;
            this.playerCharacter = player;
            this.moveAnimator = move;

            // 启用跳跃前摇 => 接管跳跃前摇结束
            moveAbility.EnablePreJump = true;
        }

        private Animator modelAnimator;
        private Animator moveAnimator;
        private PlayerCharacter playerCharacter;

        public PlayerNormalMove moveAbility => this.moveAnimator.GetComponent<PlayerCharacter>().moveAbility;

        private readonly int _inPreJump = Animator.StringToHash("PreJump");

        private readonly int _moveStateHash = Animator.StringToHash("MoveState");
        private readonly int _speedHash = Animator.StringToHash("Speed");
        private readonly int _speedYHash = Animator.StringToHash("SpeedY");

        /// <summary>
        /// 0 -> Idle
        /// 1 -> Walk
        /// 2 -> Run
        /// 3 -> Crouch
        /// 5 -> InAir
        /// </summary>
        private int GetMoveState
        {
            get
            {
                return moveAnimator.GetInteger(_moveStateHash);
            }
        }

        public override void Tick(float deltatime)
        {
            // 先更新条件
            // Maybe 可以只值变化时更新一次
            this.modelAnimator.SetBool(this._inPreJump, moveAbility.IsInPreJump);

            base.Tick(deltatime);
        }

        public override void LateTick(float deltatime)
        {
            // 先更新条件
            // Maybe 可以只值变化时更新一次
            this.modelAnimator.SetInteger(this._moveStateHash, this.GetMoveState);
            this.modelAnimator.SetFloat(this._speedHash, this.moveAbility.moveSetting.Speed + this.moveAbility.moveSetting.ExtraSpeed);
            this.modelAnimator.SetFloat(this._speedYHash, this.moveAbility.moveSetting.Speed + this.moveAbility.moveSetting.ExtraVelocity.y);

            // 再更新状态
            base.LateTick(deltatime);
        }
    }

}
