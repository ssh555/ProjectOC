using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.FSM;


namespace ProjectOC.Player
{
    [LabelText("Player 移动状态机")]
    public class PlayerMoveStateMachine : StateMachine
    {
        protected PlayerNormalMove.PlayerMoveSetting moveData;

        #region 状态
        /// <summary>
        /// 待机状态 -> 0
        /// </summary>
        protected State idleState;
        /// <summary>
        /// 走状态 -> 1
        /// </summary>
        protected State walkState;
        /// <summary>
        /// 跑状态 -> 2
        /// </summary>
        protected State runState;
        /// <summary>
        /// 蹲状态 -> 3
        /// </summary>
        protected State crouchState;
        ///// <summary>
        ///// 滑铲状态 -> 4
        ///// </summary>
        //protected State slideState;
        /// <summary>
        /// 处于空中状态 -> 5
        /// </summary>
        protected State inAirState;
        #endregion

        #region 移动状态参数设置
        [LabelText("移动状态参数"), System.Serializable]
        public class MoveStateParams
        {
            public void RuntimeInit(UnityEngine.InputSystem.InputAction acc, UnityEngine.InputSystem.InputAction crouch)
            {
                this.AccelerationInputAction = acc;
                this.CrouchInputAction = crouch;
            }

            #region 键位
            [LabelText("加速键"), ShowInInspector, ReadOnly, FoldoutGroup("键位")]
            public UnityEngine.InputSystem.InputAction AccelerationInputAction;

            [LabelText("蹲伏键"), ShowInInspector, ReadOnly, FoldoutGroup("键位")]
            public UnityEngine.InputSystem.InputAction CrouchInputAction;
            #endregion

            #region 行走奔跑
            [LabelText("最小行走速度"), ShowInInspector, FoldoutGroup("行走奔跑")]
            public float lowestWalkSpeed;

            [LabelText("最小奔跑速度|最大行走速度"), ShowInInspector, FoldoutGroup("行走奔跑")]
            public float lowestRunSpeed;

            [LabelText("最大奔跑速度"), ShowInInspector, FoldoutGroup("行走奔跑")]
            public float maxRunSpeed;

            [LabelText("行走加速度"), ShowInInspector, FoldoutGroup("行走奔跑")]
            public float walkAccSpeed;

            [LabelText("奔跑加速度"), ShowInInspector, FoldoutGroup("行走奔跑")]
            public float runAccSpeed;

            [LabelText("是否加速"), ShowInInspector, FoldoutGroup("行走奔跑"), ReadOnly]
            public bool IsAcc;
            #endregion

            #region 蹲伏
            [LabelText("蹲伏最大行走速度"), ShowInInspector, FoldoutGroup("蹲伏")]
            public float crouchLimitSpeed;

            [LabelText("蹲伏加速度"), ShowInInspector, FoldoutGroup("蹲伏")]
            public float crouchAccSpeed;

            [LabelText("是否蹲伏"), ShowInInspector, FoldoutGroup("蹲伏"), ReadOnly]
            public bool IsCrouch;
            #endregion

            #region 处于空中
            [LabelText("浮空最大行走速度"), ShowInInspector, FoldoutGroup("浮空")]
            public float inAirLimitSpeed;
            [LabelText("浮空加速度"), ShowInInspector, Range(0,1),FoldoutGroup("浮空")]
            public float airControl;
            public float inAirAccSpeed => airControl * walkAccSpeed;
            //横向速度大于AirControlBoostVelocityThreshold乘空气控制
            [LabelText("空气控制提升乘数"), ShowInInspector, FoldoutGroup("浮空"),Range(0,1)]
            public float airControlBoostMultiplier;
            [LabelText("空气控制提升速度阈值"), ShowInInspector, FoldoutGroup("浮空"),Range(0,1),PropertyTooltip("高于空中最大速度百分比阈值后乘以该系数")]
            public float airControlBoostVelocityThreshold;
            #endregion
            //[LabelText("滑铲初速度"), ShowInInspector, FoldoutGroup("滑铲")]
            //public float slideInitSpeed;
        }
        protected MoveStateParams stateParams;
        #endregion

        /// <summary>
        /// 移动效果状态机
        /// </summary>
        private Animator moveAnimator;

        private bool _isGrounded => this.moveData.IsGrounded;

        /// <summary>
        /// 初始化角色的移动状态机
        /// </summary>
        public PlayerMoveStateMachine(PlayerNormalMove.PlayerMoveSetting MoveSetting, MoveStateParams stateParams)
        {
            this.moveData = MoveSetting;
            this.stateParams = stateParams;
            // 设置默认移动加速度为行走加速度
            this.moveData.AddAcceleration = this.stateParams.walkAccSpeed;
            // 设置默认移动最大速度为最大行走速度
            this.moveData.MaxSpeed = this.stateParams.lowestRunSpeed;

            #region 配置角色的移动状态机|配置点
            // Idle 待机
            this.idleState = new State("Idle");
            // Idle 进入时
            this.idleState.BindEnterAction((stateMachine, preState, curState) =>
            {
                // 播放待机动画
                if (this.moveAnimator != null)
                {
                    this.moveAnimator.SetInteger("MoveState", 0);
                }
                this.ChangeVelocityParams();
            });

            // Walk 行走
            this.walkState = new State("Walk");
            // Walk 进入时
            this.walkState.BindEnterAction((stateMachine, preState, curState) =>
            {
                // 播放行走动画
                if (this.moveAnimator != null)
                {
                    this.moveAnimator.SetInteger("MoveState", 1);
                }
                this.ChangeVelocityParams();
            });

            // Run 奔跑
            this.runState = new State("Run");
            // Run 进入时
            this.runState.BindEnterAction((stateMachine, preState, curState) =>
            {
                // 播放奔跑动画
                if (this.moveAnimator != null)
                {
                    this.moveAnimator.SetInteger("MoveState", 2);
                }
                this.ChangeVelocityParams();
            });

            // Crouch 蹲伏
            this.crouchState = new State("Crouch");
            // Crouch 进入时
            this.crouchState.BindEnterAction((stateMachine, preState, curState) =>
            {
                // 播放蹲伏动画
                if (this.moveAnimator != null)
                {
                    this.moveAnimator.SetInteger("MoveState", 3);
                }
                this.stateParams.IsCrouch = true;

                this.ChangeVelocityParams();
            });

            #region 滑铲注释
            // Slide 滑铲
            //this.slideState = new State("Slide");
            //// Slide 进入时
            //this.slideState.BindEnterAction((stateMachine, preState, curState) =>
            //{
            //    // 播放滑铲动画
            //    if (this.moveAnimator != null)
            //    {
            //        this.moveAnimator.SetInteger("MoveState", 4);
            //    }

            //    // 禁用移动
            //    this.moveData.bCanMove = false;

            //    // 禁用视口旋转
            //    if (this.mouseLook != null)
            //    {
            //        this.mouseLook.bCanRotate = false;
            //    }

            //    // 设置滑铲参数
            //    this.moveData.MaxSpeed = this.stateParams.slideInitSpeed;
            //    this.moveData.Speed = this.stateParams.slideInitSpeed;
            //});
            //// Slide 离开时
            //this.slideState.BindExitAction((stateMachine, exitState, nextState) =>
            //{
            //    this.stateParams.IsCrouch = true;
            //    // 设置蹲伏参数
            //    this.moveData.AddAcceleration = this.stateParams.crouchAccSpeed;
            //    this.moveData.MaxSpeed = this.stateParams.crouchLimitSpeed;

            //    // 启用移动
            //    this.moveData.bCanMove = true;
            //    // 启用视口旋转
            //    if (this.mouseLook != null)
            //    {
            //        this.mouseLook.bCanRotate = true;
            //    }

            //});
            #endregion

            // InAir 处于空中
            this.inAirState = new State("InAir");
            // InAir 进入时
            this.inAirState.BindEnterAction((stateMachine, preState, curState) =>
            {
                // 播放处于空中动画
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

            #region 配置边
            // 加点
            this.AddState(this.idleState);
            this.AddState(this.walkState);
            this.AddState(this.runState);
            this.AddState(this.crouchState);
            //this.AddState(this.slideState);
            this.AddState(this.inAirState);

            // 加边
            // Idle -> Walk : 速度高于lowestWalkSpeed
            this.ConnectState(this.idleState.Name, "Walk", (stateController, curState) =>
            {
                return this.moveData.Speed >= this.stateParams.lowestWalkSpeed && !this.stateParams.IsCrouch;
            });
            // Idle -> Crouch : 处于蹲伏
            this.ConnectState("Idle", "Crouch", (stateController, curState) =>
            {
                return this.stateParams.IsCrouch;
            });
            // Idle -> InAir : 不处于地面
            this.ConnectState("Idle", "InAir", (stateController, curState) =>
            {
                return !_isGrounded;
            });

            // Walk -> Run : 速度高于lowestRunSpeed
            this.ConnectState("Walk", "Run", (stateController, curState) =>
            {
                return this.moveData.Speed >= this.stateParams.lowestRunSpeed && this.stateParams.IsAcc;
            });
            // Walk -> Idle : 速度低于lowestWalkSpeed
            this.ConnectState("Walk", "Idle", (stateController, curState) =>
            {
                return this.moveData.Speed < this.stateParams.lowestWalkSpeed;
            });
            // Walk -> Crouch : 处于蹲伏态
            this.ConnectState("Walk", "Crouch", (stateController, curState) =>
            {
                return this.stateParams.IsCrouch;
            });
            // Walk -> InAir : 不处于地面
            this.ConnectState("Walk", "InAir", (stateController, curState) =>
            {
                return !_isGrounded;
            });

            // Crouch -> Idle : 不处于蹲伏态 && 速度低于lowestWalkSpeed
            this.ConnectState("Crouch", "Idle", (stateController, curState) =>
            {
                return !this.stateParams.IsCrouch && this.moveData.Speed < this.stateParams.lowestWalkSpeed;
            });
            // Crouch -> Walk : 不处于蹲伏态 && 速度高于lowestWalkSpeed
            // Crouch -> Walk|Run : 按下加速键
            this.ConnectState("Crouch", "Walk", (stateController, curState) =>
            {
                return (!this.stateParams.IsCrouch && this.moveData.Speed >= this.stateParams.lowestWalkSpeed) || (this.stateParams.AccelerationInputAction.WasPressedThisFrame() && this.moveData.Speed < this.stateParams.lowestRunSpeed);
            });
            this.ConnectState("Crouch", "Run", (stateController, curState) =>
            {
                return !this.stateParams.IsCrouch && this.moveData.Speed >= this.stateParams.lowestRunSpeed;
            });
            // Crouch -> InAir : 不处于地面
            this.ConnectState("Crouch", "InAir", (stateController, curState) =>
            {
                return !_isGrounded;

            });

            // Run -> Walk : 速度低于lowestRunSpeed
            this.ConnectState("Run", "Walk", (stateController, curState) =>
            {
                return this.moveData.Speed < this.stateParams.lowestRunSpeed;
            });
            //// Run -> Slide : 按下蹲伏键
            //this.ConnectState("Run", "Slide", (stateController, curState) =>
            //{
            //    return Input.GetKeyDown(this.stateParams.CrouchKey);
            //});
            // Run -> Crouch : 处于蹲伏态
            this.ConnectState("Run", "Crouch", (stateController, curState) =>
            {
                return this.stateParams.IsCrouch;
            });
            // Run -> InAir : 不处于地面
            this.ConnectState("Run", "InAir", (stateController, curState) =>
            {
                return !_isGrounded;

            });

            #region 滑铲
            //// Slide -> Crouch : 速度低于crouchLimitSpeed
            //this.ConnectState("Slide", "Crouch", (stateController, curState) =>
            //{
            //    return this.moveData.Speed < this.stateParams.crouchLimitSpeed;
            //});
            //// Slide -> InAir : 不处于地面
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

            // 设置初始状态
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
        /// 进入加速态
        /// </summary>
        public void EnterAcc()
        {
            this.stateParams.IsAcc = true;
            this.stateParams.IsCrouch = false;

            this.ChangeVelocityParams();
        }

        /// <summary>
        /// 离开加速态
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
        /// 根据当前状态更新移动参数
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

                // 蹲伏立即减速
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
