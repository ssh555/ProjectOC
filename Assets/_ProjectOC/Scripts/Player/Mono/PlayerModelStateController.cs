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

            // ������Ծǰҡ => �ӹ���Ծǰҡ����
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
            // �ȸ�������
            // Maybe ����ֵֻ�仯ʱ����һ��
            this.modelAnimator.SetBool(this._inPreJump, moveAbility.IsInPreJump);

            base.Tick(deltatime);
        }

        public override void LateTick(float deltatime)
        {
            // �ȸ�������
            // Maybe ����ֵֻ�仯ʱ����һ��
            this.modelAnimator.SetInteger(this._moveStateHash, this.GetMoveState);
            this.modelAnimator.SetFloat(this._speedHash, this.moveAbility.moveSetting.Speed + this.moveAbility.moveSetting.ExtraSpeed);
            this.modelAnimator.SetFloat(this._speedYHash, this.moveAbility.moveSetting.Speed + this.moveAbility.moveSetting.ExtraVelocity.y);

            // �ٸ���״̬
            base.LateTick(deltatime);
        }
    }

}
