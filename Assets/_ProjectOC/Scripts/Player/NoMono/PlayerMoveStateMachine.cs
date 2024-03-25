using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.FSM;


namespace ProjectOC.Player
{
    [LabelText("Player �ƶ�״̬��")]
    public class PlayerMoveStateMachine : StateMachine
    {
        protected PlayerNormalMove.PlayerMoveSetting moveData;

        #region ״̬
        /// <summary>
        /// ����״̬ -> 0
        /// </summary>
        protected State idleState;
        /// <summary>
        /// ��״̬ -> 1
        /// </summary>
        protected State walkState;
        /// <summary>
        /// ��״̬ -> 2
        /// </summary>
        protected State runState;
        /// <summary>
        /// ��״̬ -> 3
        /// </summary>
        protected State crouchState;
        ///// <summary>
        ///// ����״̬ -> 4
        ///// </summary>
        //protected State slideState;
        /// <summary>
        /// ���ڿ���״̬ -> 5
        /// </summary>
        protected State inAirState;
        #endregion

        #region �ƶ�״̬��������
        [LabelText("�ƶ�״̬����"), System.Serializable]
        public class MoveStateParams
        {
            public void RuntimeInit(UnityEngine.InputSystem.InputAction acc, UnityEngine.InputSystem.InputAction crouch)
            {
                this.AccelerationInputAction = acc;
                this.CrouchInputAction = crouch;
            }

            #region ��λ
            [LabelText("���ټ�"), ShowInInspector, ReadOnly, FoldoutGroup("��λ")]
            public UnityEngine.InputSystem.InputAction AccelerationInputAction;

            [LabelText("�׷���"), ShowInInspector, ReadOnly, FoldoutGroup("��λ")]
            public UnityEngine.InputSystem.InputAction CrouchInputAction;
            #endregion

            #region ���߱���
            [LabelText("��С�����ٶ�"), ShowInInspector, FoldoutGroup("���߱���")]
            public float lowestWalkSpeed;

            [LabelText("��С�����ٶ�|��������ٶ�"), ShowInInspector, FoldoutGroup("���߱���")]
            public float lowestRunSpeed;

            [LabelText("������ٶ�"), ShowInInspector, FoldoutGroup("���߱���")]
            public float maxRunSpeed;

            [LabelText("���߼��ٶ�"), ShowInInspector, FoldoutGroup("���߱���")]
            public float walkAccSpeed;

            [LabelText("���ܼ��ٶ�"), ShowInInspector, FoldoutGroup("���߱���")]
            public float runAccSpeed;

            [LabelText("�Ƿ����"), ShowInInspector, FoldoutGroup("���߱���"), ReadOnly]
            public bool IsAcc;
            #endregion

            #region �׷�
            [LabelText("�׷���������ٶ�"), ShowInInspector, FoldoutGroup("�׷�")]
            public float crouchLimitSpeed;

            [LabelText("�׷����ٶ�"), ShowInInspector, FoldoutGroup("�׷�")]
            public float crouchAccSpeed;

            [LabelText("�Ƿ�׷�"), ShowInInspector, FoldoutGroup("�׷�"), ReadOnly]
            public bool IsCrouch;
            #endregion

            #region ���ڿ���
            [LabelText("������������ٶ�"), ShowInInspector, FoldoutGroup("����")]
            public float inAirLimitSpeed;
            [LabelText("���ռ��ٶ�"), ShowInInspector, Range(0,1),FoldoutGroup("����")]
            public float airControl;
            public float inAirAccSpeed => airControl * walkAccSpeed;
            //�����ٶȴ���AirControlBoostVelocityThreshold�˿�������
            [LabelText("����������������"), ShowInInspector, FoldoutGroup("����"),Range(0,1)]
            public float airControlBoostMultiplier;
            [LabelText("�������������ٶ���ֵ"), ShowInInspector, FoldoutGroup("����"),Range(0,1),PropertyTooltip("���ڿ�������ٶȰٷֱ���ֵ����Ը�ϵ��")]
            public float airControlBoostVelocityThreshold;
            #endregion
            //[LabelText("�������ٶ�"), ShowInInspector, FoldoutGroup("����")]
            //public float slideInitSpeed;
        }
        protected MoveStateParams stateParams;
        #endregion

        /// <summary>
        /// �ƶ�Ч��״̬��
        /// </summary>
        private Animator moveAnimator;

        private bool _isGrounded => this.moveData.IsGrounded;

        /// <summary>
        /// ��ʼ����ɫ���ƶ�״̬��
        /// </summary>
        public PlayerMoveStateMachine(PlayerNormalMove.PlayerMoveSetting MoveSetting, MoveStateParams stateParams)
        {
            this.moveData = MoveSetting;
            this.stateParams = stateParams;
            // ����Ĭ���ƶ����ٶ�Ϊ���߼��ٶ�
            this.moveData.AddAcceleration = this.stateParams.walkAccSpeed;
            // ����Ĭ���ƶ�����ٶ�Ϊ��������ٶ�
            this.moveData.MaxSpeed = this.stateParams.lowestRunSpeed;

            #region ���ý�ɫ���ƶ�״̬��|���õ�
            // Idle ����
            this.idleState = new State("Idle");
            // Idle ����ʱ
            this.idleState.BindEnterAction((stateMachine, preState, curState) =>
            {
                // ���Ŵ�������
                if (this.moveAnimator != null)
                {
                    this.moveAnimator.SetInteger("MoveState", 0);
                }
                this.ChangeVelocityParams();
            });

            // Walk ����
            this.walkState = new State("Walk");
            // Walk ����ʱ
            this.walkState.BindEnterAction((stateMachine, preState, curState) =>
            {
                // �������߶���
                if (this.moveAnimator != null)
                {
                    this.moveAnimator.SetInteger("MoveState", 1);
                }
                this.ChangeVelocityParams();
            });

            // Run ����
            this.runState = new State("Run");
            // Run ����ʱ
            this.runState.BindEnterAction((stateMachine, preState, curState) =>
            {
                // ���ű��ܶ���
                if (this.moveAnimator != null)
                {
                    this.moveAnimator.SetInteger("MoveState", 2);
                }
                this.ChangeVelocityParams();
            });

            // Crouch �׷�
            this.crouchState = new State("Crouch");
            // Crouch ����ʱ
            this.crouchState.BindEnterAction((stateMachine, preState, curState) =>
            {
                // ���Ŷ׷�����
                if (this.moveAnimator != null)
                {
                    this.moveAnimator.SetInteger("MoveState", 3);
                }
                this.stateParams.IsCrouch = true;

                this.ChangeVelocityParams();
            });

            #region ����ע��
            // Slide ����
            //this.slideState = new State("Slide");
            //// Slide ����ʱ
            //this.slideState.BindEnterAction((stateMachine, preState, curState) =>
            //{
            //    // ���Ż�������
            //    if (this.moveAnimator != null)
            //    {
            //        this.moveAnimator.SetInteger("MoveState", 4);
            //    }

            //    // �����ƶ�
            //    this.moveData.bCanMove = false;

            //    // �����ӿ���ת
            //    if (this.mouseLook != null)
            //    {
            //        this.mouseLook.bCanRotate = false;
            //    }

            //    // ���û�������
            //    this.moveData.MaxSpeed = this.stateParams.slideInitSpeed;
            //    this.moveData.Speed = this.stateParams.slideInitSpeed;
            //});
            //// Slide �뿪ʱ
            //this.slideState.BindExitAction((stateMachine, exitState, nextState) =>
            //{
            //    this.stateParams.IsCrouch = true;
            //    // ���ö׷�����
            //    this.moveData.AddAcceleration = this.stateParams.crouchAccSpeed;
            //    this.moveData.MaxSpeed = this.stateParams.crouchLimitSpeed;

            //    // �����ƶ�
            //    this.moveData.bCanMove = true;
            //    // �����ӿ���ת
            //    if (this.mouseLook != null)
            //    {
            //        this.mouseLook.bCanRotate = true;
            //    }

            //});
            #endregion

            // InAir ���ڿ���
            this.inAirState = new State("InAir");
            // InAir ����ʱ
            this.inAirState.BindEnterAction((stateMachine, preState, curState) =>
            {
                // ���Ŵ��ڿ��ж���
                if (this.moveAnimator != null)
                {
                    this.moveAnimator.SetInteger("MoveState", 5);
                }
                this.ChangeVelocityParams();
            });
            this.inAirState.BindExitAction((stateMachine, preState, curState) =>
            {
                this.ChangeVelocityParams();
            });
            this.inAirState.BindUpdateAction((stateMachine, curState) =>
            {
                if (moveData.Speed >= stateParams.airControlBoostVelocityThreshold * stateParams.inAirLimitSpeed)
                {
                    this.moveData.AddAcceleration = stateParams.inAirAccSpeed * stateParams.airControlBoostMultiplier;
                }
                else
                {
                    this.moveData.AddAcceleration = stateParams.inAirAccSpeed;
                }
            });
            #endregion

            #region ���ñ�
            // �ӵ�
            this.AddState(this.idleState);
            this.AddState(this.walkState);
            this.AddState(this.runState);
            this.AddState(this.crouchState);
            //this.AddState(this.slideState);
            this.AddState(this.inAirState);

            // �ӱ�
            // Idle -> Walk : �ٶȸ���lowestWalkSpeed
            this.ConnectState(this.idleState.Name, "Walk", (stateController, curState) =>
            {
                return this.moveData.Speed >= this.stateParams.lowestWalkSpeed && !this.stateParams.IsCrouch;
            });
            // Idle -> Crouch : ���ڶ׷�
            this.ConnectState("Idle", "Crouch", (stateController, curState) =>
            {
                return this.stateParams.IsCrouch;
            });
            // Idle -> InAir : �����ڵ���
            this.ConnectState("Idle", "InAir", (stateController, curState) =>
            {
                return !_isGrounded;
            });

            // Walk -> Run : �ٶȸ���lowestRunSpeed
            this.ConnectState("Walk", "Run", (stateController, curState) =>
            {
                return this.moveData.Speed >= this.stateParams.lowestRunSpeed && this.stateParams.IsAcc;
            });
            // Walk -> Idle : �ٶȵ���lowestWalkSpeed
            this.ConnectState("Walk", "Idle", (stateController, curState) =>
            {
                return this.moveData.Speed < this.stateParams.lowestWalkSpeed;
            });
            // Walk -> Crouch : ���ڶ׷�̬
            this.ConnectState("Walk", "Crouch", (stateController, curState) =>
            {
                return this.stateParams.IsCrouch;
            });
            // Walk -> InAir : �����ڵ���
            this.ConnectState("Walk", "InAir", (stateController, curState) =>
            {
                return !_isGrounded;
            });

            // Crouch -> Idle : �����ڶ׷�̬ && �ٶȵ���lowestWalkSpeed
            this.ConnectState("Crouch", "Idle", (stateController, curState) =>
            {
                return !this.stateParams.IsCrouch && this.moveData.Speed < this.stateParams.lowestWalkSpeed;
            });
            // Crouch -> Walk : �����ڶ׷�̬ && �ٶȸ���lowestWalkSpeed
            // Crouch -> Walk|Run : ���¼��ټ�
            this.ConnectState("Crouch", "Walk", (stateController, curState) =>
            {
                return (!this.stateParams.IsCrouch && this.moveData.Speed >= this.stateParams.lowestWalkSpeed) || (this.stateParams.AccelerationInputAction.WasPressedThisFrame() && this.moveData.Speed < this.stateParams.lowestRunSpeed);
            });
            this.ConnectState("Crouch", "Run", (stateController, curState) =>
            {
                return !this.stateParams.IsCrouch && this.moveData.Speed >= this.stateParams.lowestRunSpeed;
            });
            // Crouch -> InAir : �����ڵ���
            this.ConnectState("Crouch", "InAir", (stateController, curState) =>
            {
                return !_isGrounded;

            });

            // Run -> Walk : �ٶȵ���lowestRunSpeed
            this.ConnectState("Run", "Walk", (stateController, curState) =>
            {
                return this.moveData.Speed < this.stateParams.lowestRunSpeed;
            });
            //// Run -> Slide : ���¶׷���
            //this.ConnectState("Run", "Slide", (stateController, curState) =>
            //{
            //    return Input.GetKeyDown(this.stateParams.CrouchKey);
            //});
            // Run -> Crouch : ���ڶ׷�̬
            this.ConnectState("Run", "Crouch", (stateController, curState) =>
            {
                return this.stateParams.IsCrouch;
            });
            // Run -> InAir : �����ڵ���
            this.ConnectState("Run", "InAir", (stateController, curState) =>
            {
                return !_isGrounded;

            });

            #region ����
            //// Slide -> Crouch : �ٶȵ���crouchLimitSpeed
            //this.ConnectState("Slide", "Crouch", (stateController, curState) =>
            //{
            //    return this.moveData.Speed < this.stateParams.crouchLimitSpeed;
            //});
            //// Slide -> InAir : �����ڵ���
            //this.ConnectState("Slide", "InAir", (stateController, curState) =>
            //{
            //    return !this.moveData.IsGrounded;
            //});
            #endregion

            // InAir -> Idle
            this.ConnectState("InAir", "Idle", (stateController, curState) =>
            {
                return _isGrounded;
            });
            #endregion

            // ���ó�ʼ״̬
            this.SetInitState("Idle");

            this.stateParams.IsCrouch = false;
            this.stateParams.IsAcc = false;

            this.ChangeVelocityParams();
        }

        public override void Update(float deltaTime)
        {
            this.UpdateKey();
        }

        protected void UpdateKey()
        {
            if (this.stateParams.AccelerationInputAction.WasPressedThisFrame())
            {
                this.EnterAcc();
            }
            if (!this.stateParams.AccelerationInputAction.IsInProgress())
            {
                this.ExitAcc();
            }


            if (this.stateParams.CrouchInputAction.WasPressedThisFrame())
            {
                this.SetCrouch(!this.stateParams.IsCrouch);

            }

        }

        public void SetMoveAnimator(Animator animator)
        {
            this.moveAnimator = animator;
        }

        /// <summary>
        /// �������̬
        /// </summary>
        public void EnterAcc()
        {
            this.stateParams.IsAcc = true;
            this.stateParams.IsCrouch = false;

            this.ChangeVelocityParams();
        }

        /// <summary>
        /// �뿪����̬
        /// </summary>
        public void ExitAcc()
        {
            this.stateParams.IsAcc = false;

            this.ChangeVelocityParams();
        }

        public void SetCrouch(bool isCrouch)
        {
            this.stateParams.IsCrouch = isCrouch;
            if (this.stateParams.IsCrouch)
            {
                this.stateParams.IsAcc = false;
            }

            this.ChangeVelocityParams();
        }

        /// <summary>
        /// ���ݵ�ǰ״̬�����ƶ�����
        /// </summary>
        protected void ChangeVelocityParams()
        {
            if (!this.moveData.IsGrounded)
            {
                this.moveData.MaxSpeed = this.stateParams.inAirLimitSpeed;
                this.moveData.AddAcceleration = this.stateParams.inAirAccSpeed;
            }
            else if (this.stateParams.IsAcc)
            {
                this.moveData.MaxSpeed = this.stateParams.maxRunSpeed;
                this.moveData.AddAcceleration = this.stateParams.runAccSpeed;
            }
            else if (this.stateParams.IsCrouch)
            {
                this.moveData.MaxSpeed = this.stateParams.crouchLimitSpeed;
                this.moveData.AddAcceleration = this.stateParams.crouchAccSpeed;

                // �׷���������
                this.moveData.Speed = Mathf.Clamp(this.moveData.Speed, 0, this.moveData.MaxSpeed);
            }
            else
            {
                this.moveData.MaxSpeed = this.stateParams.lowestRunSpeed;
                this.moveData.AddAcceleration = this.stateParams.walkAccSpeed;
            }
        }


        public override void ResetState()
        {
            base.ResetState();

            this.stateParams.IsAcc = this.stateParams.IsCrouch = false;
        }
    }
}
