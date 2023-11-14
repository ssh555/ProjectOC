using ProjectOC.Terrain;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Player
{
    [Serializable]
    public class PlayerNormalMove
    {
        #region 内部类|结构体
        [LabelText("玩家移动数据设置"), Serializable]
        public class PlayerMoveSetting  
        {
            #region 启用|禁用移动
            [LabelText("能否基本移动"), FoldoutGroup("启用|禁用移动")]
            public bool bCanMove;

            [LabelText("在空中是否能移动"), FoldoutGroup("启用|禁用移动")]
            public bool bCanMoveInAir;
            #endregion

            #region 移动参数
            [LabelText("水平输入最大速度"), Range(0, float.PositiveInfinity), FoldoutGroup("移动参数")]
            public float MaxSpeed;

            [LabelText("最小移动速度"), Range(0, float.PositiveInfinity), FoldoutGroup("移动参数")]
            public float MinSpeed;

            [LabelText("移动加速度"), FoldoutGroup("移动参数"), Space(10)]
            public float AddAcceleration;

            [LabelText("启用阻尼"), FoldoutGroup("移动参数")]
            public bool UseDrag;
            [LabelText("固定空气阻尼"), Range(0, float.MaxValue), FoldoutGroup("移动参数"), PropertyTooltip("V -= kV")]
            public float airDrag;
            [LabelText("可变地面阻尼"), ShowInInspector, ReadOnly, FoldoutGroup("移动参数"), PropertyTooltip("V -= kV")]
            protected float terrainDrag
            {
                get
                {
                    return this.TerrainDrag == null ? 0 : this.TerrainDrag.TerrainDrag;
                }
            }

            [HideInInspector]
            public MonoTerrainMoveDrag TerrainDrag;

            [LabelText("最终阻尼"), ShowInInspector, ReadOnly, FoldoutGroup("移动参数"), PropertyTooltip("V -= kV * Max(-ln(V * 2), 1)")]
            public float FinalDrag
            {
                get
                {
                    return this.UseDrag ? this.airDrag + this.terrainDrag : 0;
                }
            }

            [LabelText("启用重力"), FoldoutGroup("移动参数")]
            public bool UseGravity;

            [LabelText("启用重力分量"), FoldoutGroup("移动参数")]
            public bool UseGravityWeight;

            [LabelText("重力加速度"), FoldoutGroup("移动参数")]
            public float GravityAcceleration;

            [LabelText("向前移动速度系数"), Range(0, 1), Space(10), FoldoutGroup("移动参数")]
            public float ForwardSpeedRate;
            [LabelText("向后移动速度系数"), Range(0, 1), FoldoutGroup("移动参数")]
            public float BackwardSpeedRate;
            [LabelText("左右移动速度系数"), Range(0, 1), FoldoutGroup("移动参数")]
            public float StrafeSpeedRate;

            [LabelText("当前输入速度大小"), ReadOnly, FoldoutGroup("移动参数")]
            private float speed;
            [LabelText("当前输入速度大小"), ShowInInspector, ReadOnly, FoldoutGroup("移动参数")]
            public float Speed
            {
                get
                {
                    return speed;
                }
                set
                {
                    speed = value;
                    velocity = velocity.normalized * speed;
                }
            }

            [LabelText("当前输入速度"), ReadOnly, FoldoutGroup("移动参数")]
            private Vector3 velocity;
            [LabelText("当前输入速度"), ShowInInspector, ReadOnly, FoldoutGroup("移动参数")]
            public Vector3 Velocity
            {
                get
                {
                    return velocity;
                }
                set
                {
                    velocity = value;
                    speed = velocity.magnitude;
                }
            }

            [LabelText("当前额外速度"), ReadOnly, FoldoutGroup("移动参数"), PropertyTooltip("重力、移动和其他不是用户输入导致的速度更改")]
            private Vector3 extraVelocity;
            [LabelText("当前额外速度"), ShowInInspector, ReadOnly, FoldoutGroup("移动参数"), PropertyTooltip("重力、移动和其他不是用户输入导致的速度更改")]
            public Vector3 ExtraVelocity
            {
                get
                {
                    return extraVelocity;
                }
                set
                {
                    extraVelocity = value;
                    extraSpeed = extraVelocity.magnitude;
                }
            }

            [LabelText("当前额外速度大小"), ReadOnly, FoldoutGroup("移动参数"), PropertyTooltip("重力、移动和其他不是用户输入导致的速度更改")]
            private float extraSpeed;
            [LabelText("当前额外速度大小"), ShowInInspector, ReadOnly, FoldoutGroup("移动参数"), PropertyTooltip("重力、移动和其他不是用户输入导致的速度更改")]
            public float ExtraSpeed
            {
                get
                {
                    return extraSpeed;
                }
                set
                {
                    extraSpeed = value;
                    extraVelocity = extraVelocity.normalized * extraSpeed;
                }
            }

            [LabelText("模型是否旋转"), ShowInInspector, ReadOnly, FoldoutGroup("移动参数")]
            public bool CanModelRotate;
            #endregion

            #region 按键
            [LabelText("按下|抬起跳跃"), FoldoutGroup("按键"), PropertyTooltip("true : 按下跳跃, false : 抬起跳跃")]
            public bool JumpDownOrUp;
            #endregion

            #region 检测设置
            [LabelText("检测层"), FoldoutGroup("检测设置")]
            public LayerMask DetectLayer;
            #endregion

            #region 跳跃参数
            [LabelText("启用跳跃"), FoldoutGroup("跳跃参数")]
            public bool bEnableJump;
            [LabelText("最大跳跃次数"), FoldoutGroup("跳跃参数")]
            public int MaxJumpCount;
            [LabelText("当前剩余跳跃次数"), FoldoutGroup("跳跃参数")]
            public int RemainJumpCount;
            [LabelText("能否跳跃"), FoldoutGroup("跳跃参数")]
            public bool bCanJump;
            [LabelText("是否使用世界Up轴"), FoldoutGroup("跳跃参数"), PropertyTooltip("true : 使用WorldUp跳跃，false : 使用SelfUp跳跃")]
            public bool bIsWorldUpAxis;
            [LabelText("跳跃叠加速度"), FoldoutGroup("跳跃参数"), PropertyTooltip("true : 当前速度叠加，false : 直接更改垂直速度")]
            public bool IsJumpVelocityStack;
            [LabelText("跳跃初速度"), Range(0, float.PositiveInfinity), FoldoutGroup("跳跃参数"), ShowIf("@IsJumpVelocityStack == true")]
            public float JumpInitialVelocity;
            [LabelText("跳跃时间"), FoldoutGroup("跳跃参数"), PropertyTooltip("跳跃持续时间"), ShowIf("@IsJumpVelocityStack == false"), ShowInInspector]
            public double jumpTime => this.jumpSpeedCurve == null ? -1 : (this.jumpSpeedCurve.keys.Length > 0 ? this.jumpSpeedCurve.keys[this.jumpSpeedCurve.length - 1].time : -1);
            [LabelText("跳跃实际速度参数"), FoldoutGroup("跳跃参数"), PropertyTooltip("横轴:百分比(0-1)，纵轴:速度"), ShowIf("@IsJumpVelocityStack == false")]
            public AnimationCurve jumpSpeedCurve;
            #endregion

            [HideInInspector]
            public bool IsGrounded;

            [HideInInspector, NonSerialized]
            public PlayerNormalMove OwnMove;
        }
        #endregion

        #region Field
        /// <summary>
        /// 玩家移动设置
        /// </summary>
        public PlayerMoveSetting moveSetting;

        /// <summary>
        /// 移动控制器
        /// </summary>
        [LabelText("移动控制器"), FoldoutGroup("移动组件引用")]
        public CharacterController controller;

        [LabelText("移动方向视点"), FoldoutGroup("移动组件引用")]
        public Transform Viewer;

        [LabelText("移动模型"), FoldoutGroup("移动组件引用")]
        public Transform Model;
        [LabelText("移动旋转率"), Range(0,1), FoldoutGroup("移动组件引用"), ShowIf("@Model!=null")]
        public float rotRate = 0.5f;
        [LabelText("最小旋转角"), Range(0, 10), FoldoutGroup("移动组件引用"), ShowIf("@Model!=null")]
        public float minRotAngle = 1;
        #endregion

        #region 地面检测
        [LabelText("自检测处于地面"), ShowInInspector, ReadOnly, FoldoutGroup("地面检测")]
        private bool checkGround = false;

        [LabelText("角色控制器处于地面"), ShowInInspector, ReadOnly, FoldoutGroup("地面检测")]
        private bool characterGround
        {
            get
            {
                return this.controller == null ? false : this.controller.isGrounded;
            }
        }

        /// <summary>
        /// 是否处于地面
        /// </summary>
        [LabelText("实际处于地面"), ShowInInspector, ReadOnly, FoldoutGroup("地面检测")]
        public bool IsGrounded
        {
            get
            {
                if (this.controller != null)
                {
                    return this.controller.isGrounded || checkGround;
                }
                return checkGround;
            }
        }
        #endregion

        #region 实际速度参数
        [LabelText("当前实际速度大小"), ShowInInspector, ReadOnly, FoldoutGroup("实际速度参数")]
        protected float CurrentSpeed => this.CurrentVelocity.magnitude;

        [LabelText("当前实际速度"), ShowInInspector, ReadOnly, FoldoutGroup("实际速度参数")]
        protected Vector3 CurrentVelocity
        {
            get
            {
                if (this.controller != null)
                {
                    return this.controller.velocity;
                }
                return Vector3.zero;
            }
        }

        ML.Engine.Timer.CounterDownTimer _jumpTimer;
        ML.Engine.Timer.CounterDownTimer jumpTimer
        {
            get
            {
                if (this._jumpTimer == null)
                {
                    this._jumpTimer = new ML.Engine.Timer.CounterDownTimer(0.1f, false, false);
                    this._jumpTimer.OnUpdateEvent += JumpTimer;
                }
                return this._jumpTimer;
            }
        }

        #endregion

        public PlayerNormalMove()
        {
            this.moveSetting = new PlayerMoveSetting();
            this.moveSetting.OwnMove = this;
            this.moveSetting.CanModelRotate = true;

        }

        #region 移动Move
        /// <summary>
        /// 获得当前帧的移动方向
        /// </summary>
        /// <returns></returns>
        public Vector3 GetInputMoveDir(Vector2 inputV)
        {
            Vector3 moveDir;
            moveDir = inputV;
            moveDir.z = moveDir.y;
            moveDir.y = 0;

            //if (MathF.Abs(moveDir.x) > float.Epsilon)
            //{
            //    _speedRate = this.moveSetting.StrafeSpeedRate;
            //}
            //if (moveDir.z < -float.Epsilon)
            //{
            //    _speedRate = this.moveSetting.BackwardSpeedRate;
            //}
            //if (moveDir.z > float.Epsilon)
            //{
            //    _speedRate = this.moveSetting.ForwardSpeedRate;
            //}

            moveDir = this.Viewer.TransformDirection(moveDir);
            moveDir.y = 0;
            return moveDir;
        }


        /// <summary>
        /// 自带输入检测的移动
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdatePosition(float deltaTime, Vector2 inputV)
        {
            // 移动方向
            Vector3 moveDir = Vector3.zero;

            // 当前移动速度系数
            float _speedRate = 0;

            // 移动输入
            if (this.moveSetting.bCanMove)
            {
                // 处于地面 || 允许空中移动
                if (IsGrounded || this.moveSetting.bCanMoveInAir)
                {
                    moveDir = this.GetInputMoveDir(inputV);
                    _speedRate = 1;
                }
            }
            // 移动
            this.UpdatePosition(deltaTime, moveDir, _speedRate);

            CheckGround(deltaTime);
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="moveDir"></param>
        public void UpdatePosition(float deltaTime, Vector3 moveDir, float _speedRate = 1)
        {
            moveDir.Normalize();

            // 输入速度更新
            if(!this.IsGrounded)
            // 能移动 && 移动速度率为正 && 有移动方向 && 没有达到最大速度则增加速度
            if (this.moveSetting.bCanMove && _speedRate > float.Epsilon && moveDir.magnitude > float.Epsilon && this.moveSetting.Speed <= this.moveSetting.MaxSpeed * _speedRate)
            {
                // 增加速度
                this.moveSetting.Speed = Math.Clamp(this.moveSetting.Speed + this.moveSetting.AddAcceleration * deltaTime, 0, this.moveSetting.MaxSpeed * _speedRate);
                // 更新输入方向
                this.moveSetting.Velocity = moveDir * this.moveSetting.Speed;
            }
            // 输入速度衰减
            else if(this.moveSetting.Speed > this.moveSetting.MinSpeed)
            {
                // V -= KV * Max(ln(V*2), 1) => V < 0.74 开始加速衰减
                this.moveSetting.Speed -= this.moveSetting.Speed * DragEquation(this.moveSetting.FinalDrag) * deltaTime * Mathf.Max(-Mathf.Log(this.moveSetting.Speed / this.moveSetting.MaxSpeed), 1);
                if (this.moveSetting.Speed < this.moveSetting.MinSpeed)
                {
                    this.moveSetting.Speed = 0;
                }
            }
            if (this.moveSetting.Speed * deltaTime <= this.controller.minMoveDistance)
            {
                this.moveSetting.Velocity = Vector3.zero;
            }

            // 输入速度方向变换 => 应用于上下坡
            if (this.moveSetting.Velocity.sqrMagnitude > float.Epsilon)
            {
                // controller 的身高，向下检测的距离
                float distance = (Mathf.Max(this.controller.height, this.controller.radius * 2) + this.controller.skinWidth * 2);

                // center 检测点 => 平移向下移动到controller底部了，
                Vector3 Pbot = this.controller.transform.position + this.controller.center;
                //Pbot.y -= Mathf.Max(this.controller.height * 0.5f, this.controller.radius);
                // 移动方向相对于center有偏移的检测点
                Vector3 Ptop = Pbot;
                float offset = (this.controller.radius + this.controller.skinWidth + Math.Clamp(this.moveSetting.Speed * deltaTime, this.controller.minMoveDistance, this.controller.stepOffset + this.controller.minMoveDistance));
                RaycastHit hitInfo;
                // 向前方检测一下，更改偏移量，避免Ptop的偏移进入前方碰撞体内部
                Ptop.y = Ptop.y - this.controller.skinWidth + this.controller.stepOffset;
                if (Physics.Raycast(Ptop, this.moveSetting.Velocity, out hitInfo, offset, this.moveSetting.DetectLayer))
                {
                    offset = (hitInfo.point - Ptop).magnitude;
                }
                Ptop = Pbot;
                Ptop += this.moveSetting.Velocity.normalized * offset;
                // 两点射线都检测到了才更改速度方向
                RaycastHit hitInfo1, hitInfo2;
                // 向下检测
                if (Physics.Raycast(Pbot, -this.controller.transform.up, out hitInfo1, distance, this.moveSetting.DetectLayer) && Physics.Raycast(Ptop, -this.controller.transform.up, out hitInfo2, distance, this.moveSetting.DetectLayer))
                {
                    // 方向
                    Vector3 direction = (hitInfo2.point - hitInfo1.point).normalized;
                    // 向上移动时是否满足移动的坡度限制
                    if (Vector3.Angle(Vector3.up, direction) <= 90)
                    {
                        float angle = Vector3.Angle(Vector3.ProjectOnPlane(direction, Vector3.up), direction);
                        if (angle > 1 && angle <= this.controller.slopeLimit)
                        {
                            // 更新速度方向
                            this.moveSetting.Velocity = this.moveSetting.Speed * direction;
                        }
                    }
                    else
                    {
                        // 更新速度方向
                        this.moveSetting.Velocity = this.moveSetting.Speed * direction;
                    }
                }
            }
            
            Vector3 motion = (this.moveSetting.Velocity + this.moveSetting.ExtraVelocity) * deltaTime;
            this.controller.Move(motion);

            // 向上碰撞到掩体
            if (Physics.SphereCast(this.controller.transform.position + this.controller.center + Mathf.Max((this.controller.height * 0.5f - this.controller.radius), 0) * this.controller.transform.up, this.controller.radius, this.controller.transform.up, out RaycastHit _hitInfo, this.controller.skinWidth, this.moveSetting.DetectLayer))
            {
                this.moveSetting.ExtraVelocity = new Vector3(this.moveSetting.ExtraVelocity.x, -this.moveSetting.ExtraVelocity.y, this.moveSetting.ExtraVelocity.z);
                this.moveSetting.Velocity = new Vector3(this.moveSetting.Velocity.x, -this.moveSetting.Velocity.y, this.moveSetting.Velocity.z);
                motion = (this.moveSetting.Velocity + this.moveSetting.ExtraVelocity) * deltaTime;
                this.controller.Move(motion);
            }

            this.moveSetting.IsGrounded = this.IsGrounded;


            // 额外速度衰减
            if (this.moveSetting.ExtraSpeed > this.moveSetting.MinSpeed)
            {
                this.moveSetting.ExtraSpeed -= this.moveSetting.ExtraSpeed * DragEquation(this.moveSetting.FinalDrag) * deltaTime;
                if (this.moveSetting.ExtraSpeed < this.moveSetting.MinSpeed)
                {
                    this.moveSetting.ExtraSpeed = 0;
                }
            }
            if (this.moveSetting.ExtraSpeed > 0.00001f && this.moveSetting.ExtraSpeed * deltaTime <= this.controller.minMoveDistance)
            {
                this.moveSetting.ExtraVelocity = Vector3.zero;
            }


            // 根据实际运动方向更新 模型旋转
            if (this.moveSetting.CanModelRotate == true && this.Model && this.controller.velocity.magnitude > this.moveSetting.MinSpeed)
            {
                moveDir = this.controller.velocity;
                moveDir.y = 0;
                moveDir.Normalize();

                var rot = (this.Model.rotation * Quaternion.FromToRotation(this.Model.forward, moveDir).normalized).normalized;

                if (Quaternion.Angle(rot, this.Model.rotation) < this.minRotAngle)
                {
                    this.Model.rotation = rot;
                }
                else
                {
                    this.Model.rotation = Quaternion.Euler(0, Quaternion.Slerp(this.Model.rotation, rot, this.rotRate).eulerAngles.y, 0).normalized;
                }

            }
        }

        protected void CheckGround(float deltaTime)
        {
            Vector3 p = this.controller.transform.position + this.controller.center - this.controller.transform.up * (Mathf.Max(0, this.controller.height * 0.5f - this.controller.radius) + this.controller.radius * 0.9f + this.controller.skinWidth);
            float radius = this.controller.radius;
            if (Physics.CheckSphere(p, radius, this.moveSetting.DetectLayer))
            {
                this.checkGround = true;
            }
            else
            {
                this.checkGround = false;
            }
            
            if (this.moveSetting.UseGravity)
            {
                if (this.moveSetting.UseGravityWeight)
                {
                    // origin
                    RaycastHit[] hitResults = Physics.SphereCastAll(this.controller.transform.position + this.controller.center - Mathf.Max(this.controller.height * 0.5f - this.controller.radius, 0) * this.controller.transform.up,
                        // radius
                        this.controller.radius,
                        // direction
                        Vector3.down,
                        // distance
                        (this.controller.radius + this.controller.skinWidth) * 0.5f + this.controller.skinWidth,
                        this.moveSetting.DetectLayer);
                    Vector3 gravity = this.moveSetting.GravityAcceleration * Vector3.down;
                    Vector3 sum = Vector3.zero;
                    foreach (var hit in hitResults)
                    {
                        Vector3 normal = hit.normal;
                        normal = Vector3.Project(gravity, normal);
                        sum += normal;
                        normal = Vector3.Project(sum, gravity);
                        if (normal.sqrMagnitude >= gravity.sqrMagnitude)
                        {
                            gravity = Vector3.zero;
                            break;
                        }
                    }
                    if (gravity.sqrMagnitude > 0.001f)
                    {
                        gravity -= sum;
                        this.moveSetting.ExtraVelocity += gravity * deltaTime;
                    }
                }
                else if (!this.controller.isGrounded && !Physics.CheckSphere(p, radius * 0.11f, this.moveSetting.DetectLayer))
                {
                    this.moveSetting.ExtraVelocity += this.moveSetting.GravityAcceleration * Vector3.down * deltaTime;
                }
            }


            if (this.IsGrounded)
            {
                if ((this.moveSetting.ExtraVelocity.y + this.moveSetting.Velocity.y) < float.Epsilon)
                {
                    this.moveSetting.RemainJumpCount = this.moveSetting.MaxJumpCount;
                }
            }
            // 落地 -> 垂直速度清0
            if (this.controller.isGrounded)
            {
                if ((this.moveSetting.ExtraVelocity.y + this.moveSetting.Velocity.y) < float.Epsilon)
                {
                    this.moveSetting.Velocity.Set(this.moveSetting.Velocity.x, 0, this.moveSetting.Velocity.z);
                    this.moveSetting.ExtraVelocity.Set(this.moveSetting.Velocity.x, 0, this.moveSetting.Velocity.z);
                    //this.moveSetting.Velocity = new Vector3(this.moveSetting.Velocity.x, 0, this.moveSetting.Velocity.z);
                    //this.moveSetting.ExtraVelocity = new Vector3(this.moveSetting.ExtraVelocity.x, 0, this.moveSetting.ExtraVelocity.z);
                }
            }

        }
        
        /// <summary>
        /// 施加速度 => 外部因素的改变引起，如冲击力
        /// </summary>
        /// <param name="vel"></param>
        public void AddVelocity(Vector3 vel)
        {
            this.moveSetting.ExtraVelocity += vel;
        }
        
        /// <summary>
        /// 归0输入速度
        /// </summary>
        public void ClearInputSpeed()
        {
            this.moveSetting.Speed = 0;
        }
        /// <summary>
        /// 归0额外速度
        /// </summary>
        public void ClearExtraSpeed()
        {
            this.moveSetting.ExtraSpeed = 0;
        }
        /// <summary>
        /// 归0所有速度
        /// </summary>
        public void ClearBothSpeed()
        {
            this.ClearInputSpeed();
            this.ClearExtraSpeed();
        }
        #endregion

        #region 跳跃Jump
        /// <summary>
        /// 启用起跳前摇
        /// </summary>
        [LabelText("启用跳跃前摇")]
        public bool EnablePreJump = false;
        private bool _lastIsInPreJump = false;
        private bool _isInPreJump = false;
        /// <summary>
        /// 是否处于起跳前摇
        /// </summary>
        [ShowInInspector, ReadOnly, LabelText("处于跳跃前摇"), ShowIf("@EnablePreJump == true")]
        public bool IsInPreJump
        {
            get => this._isInPreJump;
            set
            {
                this._lastIsInPreJump = this._isInPreJump;
                this._isInPreJump = value;
                if (this._isInPreJump == false && this._lastIsInPreJump == true)
                {
                    --this.moveSetting.RemainJumpCount;
                    this.ForceJump();
                    this._lastIsInPreJump = false;
                }
            }
        }

        /// <summary>
        /// 自带输入检测的跳跃
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public void UpdateJump(float deltaTime, UnityEngine.InputSystem.InputAction jumpAction)
        {
            if (CanJump() && ((this.moveSetting.JumpDownOrUp && jumpAction.WasPressedThisFrame() || (!this.moveSetting.JumpDownOrUp && jumpAction.WasReleasedThisFrame()))))
            {
                if (this.EnablePreJump)
                {
                    if (!this._isInPreJump)
                    {
                        this.IsInPreJump = true;
                    }
                }
                else
                {
                    Jump();
                }
            }
        }

        /// <summary>
        /// 响应Jump
        /// </summary>
        public void ResponseJump()
        {
            if (this.EnablePreJump)
            {
                if (!this._isInPreJump)
                {
                    this.IsInPreJump = true;
                }
            }
            else
            {
                Jump();
            }
        }

        protected bool CanJump()
        {
            return (this.moveSetting.bEnableJump && this.moveSetting.bCanJump && this.moveSetting.RemainJumpCount > 0);
        }

        /// <summary>
        /// 跳跃
        /// </summary>
        public bool Jump()
        {
            if (!CanJump())
            {
                return false;
            }
            --this.moveSetting.RemainJumpCount;
            this.ForceJump();
            return true;
        }

        /// <summary>
        /// 强制跳跃，忽略是否启用跳跃、是否能跳跃、跳跃次数的限制，且不更改这些变量
        /// </summary>
        public void ForceJump()
        {
            // 叠加速度
            if (this.moveSetting.IsJumpVelocityStack)
            {
                Vector3 jVel = this.moveSetting.bIsWorldUpAxis ? Vector3.up : this.controller.transform.TransformDirection(Vector3.up);
                this.moveSetting.ExtraVelocity += jVel * this.moveSetting.JumpInitialVelocity;
            }
            // 直接应用速度
            else
            {
                this.jumpTimer.Reset(this.moveSetting.jumpTime);
            }
        }

        protected void JumpTimer(double timer)
        {
            // WorldUpAxis
            if (this.moveSetting.bIsWorldUpAxis)
            {
                this.moveSetting.ExtraVelocity = new Vector3(this.moveSetting.ExtraVelocity.x, this.moveSetting.jumpSpeedCurve.Evaluate((float)this.jumpTimer.Time), this.moveSetting.ExtraVelocity.z);
            }
            // SelfUpAxis
            else
            {
                Vector3 jVel = this.controller.transform.InverseTransformVector(this.moveSetting.ExtraVelocity);
                jVel.y = this.moveSetting.jumpSpeedCurve.Evaluate((float)this.jumpTimer.Time);
                this.moveSetting.ExtraVelocity = this.controller.transform.TransformVector(jVel);
            }
            // 落地
            if (this.controller.isGrounded && this.moveSetting.ExtraVelocity.y < 0)
            {
                this.jumpTimer.End();
            }

        }
#endregion

        /// <summary>
        /// 阻尼乘数因子计算公式
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        protected float DragEquation(float x)
        {
            return 100 * x / (x + 500);
        }
    }

}

