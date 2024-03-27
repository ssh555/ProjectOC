using ProjectOC.Terrain;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectOC.Player
{
    [Serializable]
    public class PlayerNormalMove
    {
        #region �ڲ���|�ṹ��
        [LabelText("����ƶ���������"), Serializable]
        public class PlayerMoveSetting  
        {
            #region ����|�����ƶ�
            [LabelText("�ܷ�����ƶ�"), FoldoutGroup("����|�����ƶ�")]
            public bool bCanMove;

            [LabelText("��������"), FoldoutGroup("����|�����ƶ�")]
            public bool UseGravity = true;

            [LabelText("������������"), FoldoutGroup("����|�����ƶ�")]
            public bool UseGravityWeight;

            [LabelText("��������"), FoldoutGroup("����|�����ƶ�")]
            public bool UseDrag = true;
            
            [LabelText("������Ծ"), FoldoutGroup("����|�����ƶ�")]
            public bool bEnableJump;
            
            #endregion

            #region ͨ��

            [LabelText("����"), FoldoutGroup("ͨ��")] 
            public float Mass;

            [LabelText("�������ٶ�"), FoldoutGroup("ͨ��")]
            public float Gravity;

            [LabelText("����Ħ��ϵ��"), ShowInInspector, ReadOnly, FoldoutGroup("ͨ��"), PropertyTooltip("V -= kV")]
            public float terrainDrag
            {
                get
                {
                    return this.TerrainDrag == null ? 0 : this.TerrainDrag.TerrainDrag;
                }
            }

            [HideInInspector]
            public MonoTerrainMoveDrag TerrainDrag;
            
            [LabelText("����Ħ��ϵ��"), Range(0, 20), FoldoutGroup("ͨ��"), PropertyTooltip("Ь/���ṩ��Ħ����ϵ��")] 
            public float BottomDrag;
            [LabelText("׹��ˮƽ���ٶ�"), Range(0, 20), FoldoutGroup("ͨ��"), PropertyTooltip("ˮƽĦ��ϵ��")]
            public float FallDeceleration;
            [LabelText("��������ϵ��"), Range(0, 20), FoldoutGroup("ͨ��"), PropertyTooltip("����Ħ��")]
            public float airDrag;
            private float airDragAcc => airDrag * Speed;
            
            [LabelText("��������"), ShowInInspector, ReadOnly, FoldoutGroup("ͨ��"), PropertyTooltip("����ͨͨ�ļ��ٶ�")]
            public float FinalDrag
            {
                get
                {
                    float horizontalDragAcc = IsGrounded ? this.terrainDrag + this.BottomDrag : FallDeceleration;
                    return this.UseDrag ? airDragAcc + horizontalDragAcc: 0;
                }
            }


            [LabelText("�׷����"), FoldoutGroup("ͨ��")] 
            public int CrouchedHalfHeight;
            #endregion

            
            #region �ƶ�����

            [LabelText("��󲽸�"), ShowInInspector,Range(0, 1.5f), FoldoutGroup("�ƶ�����")]
            public float MaxStepHeight;

            [LabelText("�����ߵ���Ƕ�"), ShowInInspector,Range(0, 360), FoldoutGroup("�ƶ�����")]
            public float WalkerbleFloorAngle;

            [LabelText("��С�ƶ��ٶ�"), Range(0, 10), FoldoutGroup("�ƶ�����"), PropertyTooltip("С�ڸ�ֵ��ֱ��ֹͣ")]
            public float MinSpeed;

            [LabelText("��ǰˮƽ��������ٶ�"), ReadOnly, Range(0, 50), FoldoutGroup("�ƶ�����")]
            public float MaxSpeed;

            [LabelText("��ǰ�ƶ����ٶ�"), ReadOnly, FoldoutGroup("�ƶ�����"), Space(10)]
            public float AddAcceleration;
            
            [LabelText("��ǰ�ٶȴ�С"), ReadOnly, FoldoutGroup("�ƶ�����")]
            private float speed;
            [LabelText("��ǰ�ٶȴ�С"), ShowInInspector, ReadOnly, FoldoutGroup("�ƶ�����")]
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

            [LabelText("��ǰ�ٶ�"), ReadOnly, FoldoutGroup("�ƶ�����")]
            private Vector3 velocity;
            [LabelText("��ǰ�ٶ�"), ShowInInspector, ReadOnly, FoldoutGroup("�ƶ�����")]
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

            [LabelText("��ǰ�����ٶ�"), ReadOnly, FoldoutGroup("�ƶ�����"), PropertyTooltip("�������ƶ������������û����뵼�µ��ٶȸ���")]
            private Vector3 extraVelocity;
            [LabelText("��ǰ�����ٶ�"), ShowInInspector, ReadOnly, FoldoutGroup("�ƶ�����"), PropertyTooltip("�������ƶ������������û����뵼�µ��ٶȸ���")]
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

            [LabelText("��ǰ�����ٶȴ�С"), ReadOnly, FoldoutGroup("�ƶ�����"), PropertyTooltip("�������ƶ������������û����뵼�µ��ٶȸ���")]
            private float extraSpeed;
            [LabelText("��ǰ�����ٶȴ�С"), ShowInInspector, ReadOnly, FoldoutGroup("�ƶ�����"), PropertyTooltip("�������ƶ������������û����뵼�µ��ٶȸ���")]
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

            [LabelText("��ɫģ���Ƿ���ת"), ShowInInspector, ReadOnly, FoldoutGroup("�ƶ�����")]
            public bool CanModelRotate;
            [LabelText("�ܷ����ת��"), FoldoutGroup("�ƶ�����")]
            public bool AirRotationControl = true;
            #endregion

            #region ����
            [LabelText("����|̧����Ծ"), FoldoutGroup("����"), PropertyTooltip("true : ������Ծ, false : ̧����Ծ"),HideInInspector]
            public bool JumpDownOrUp;
            #endregion

            #region �������
            [LabelText("����"), FoldoutGroup("�������")]
            public LayerMask DetectLayer;
            #endregion

            #region ��Ծ����
            [LabelText("�����Ծ����"), FoldoutGroup("��Ծ����")]
            public int MaxJumpCount;
            [LabelText("��ǰʣ����Ծ����"), ReadOnly, FoldoutGroup("��Ծ����")]
            public int RemainJumpCount = 0;
            [LabelText("�ܷ���Ծ"), FoldoutGroup("��Ծ����")]
            public bool bCanJump;
            [LabelText("�Ƿ�ʹ������Up��"), FoldoutGroup("��Ծ����"), PropertyTooltip("true : ʹ��WorldUp��Ծ��false : ʹ��SelfUp��Ծ")]
            public bool bIsWorldUpAxis;
            [LabelText("��Ծ�����ٶ�"), FoldoutGroup("��Ծ����"), PropertyTooltip("true : ��ǰ�ٶȵ��ӣ�false : ֱ�Ӹ��Ĵ�ֱ�ٶ�")]
            public bool IsJumpVelocityStack;
            [LabelText("��Ծ�̳л����ٶ�X"), FoldoutGroup("��Ծ����")]
            public bool ImpactBaseVelocityX;
            [LabelText("��Ծ�̳л����ٶ�Z"), FoldoutGroup("��Ծ����")]
            public bool ImpactBaseVelocityZ;
            [LabelText("��Ծ�̳л����ٶ�Y"), FoldoutGroup("��Ծ����")]
            public bool ImpactBaseVelocityY;
 

            [LabelText("��Ծʱ��"), FoldoutGroup("��Ծ����"), PropertyTooltip("��Ծ����ʱ��"), ShowIf("@IsJumpVelocityStack == false"), ShowInInspector]
            public double jumpTime => this.jumpSpeedCurve == null ? -1 : (this.jumpSpeedCurve.keys.Length > 0 ? this.jumpSpeedCurve.keys[this.jumpSpeedCurve.length - 1].time : -1);
            [LabelText("��Ծʵ���ٶȲ���"), FoldoutGroup("��Ծ����"), PropertyTooltip("����:�ٷֱ�(0-1)������:�ٶ�"), ShowIf("@IsJumpVelocityStack == false")]
            public AnimationCurve jumpSpeedCurve;
            #endregion

            [HideInInspector]
            public bool IsGrounded;

            [HideInInspector, NonSerialized]
            public PlayerNormalMove OwnMove;
            
            public PlayerMoveSetting(PlayerNormalMove _playerNormalMove)
            {
                OwnMove = _playerNormalMove;
                CanModelRotate = true;

                //�о��ò߻���CharacterController�����ֱ��һ��
                // OwnMove.controller.GetComponent<Rigidbody>().mass = Mass;
                // OwnMove.controller.stepOffset = MaxStepHeight;
                // OwnMove.controller.slopeLimit = WalkerbleFloorAngle;
            }
        }
        #endregion

        #region Field
        /// <summary>
        /// ����ƶ�����
        /// </summary>
        public PlayerMoveSetting moveSetting;

        /// <summary>
        /// �ƶ�������
        /// </summary>
        [LabelText("�ƶ�������"), FoldoutGroup("�ƶ��������")]
        public CharacterController controller;

        [LabelText("�ƶ������ӵ�"), FoldoutGroup("�ƶ��������")]
        public Transform Viewer;

        [LabelText("�ƶ�ģ��"), FoldoutGroup("�ƶ��������")]
        public Transform Model;
        [LabelText("�ƶ���ת��"), Range(0,1), FoldoutGroup("�ƶ��������"), ShowIf("@Model!=null"), PropertyTooltip("�ƶ�ʱ��ת�ٶ�")]
        public float rotRate = 0.5f;
        [LabelText("��С��ת��"), Range(0, 10), FoldoutGroup("�ƶ��������"), ShowIf("@Model!=null")]
        public float minRotAngle = 1;
        #endregion

        #region ������
        [LabelText("�Լ�⴦�ڵ���"), ShowInInspector, ReadOnly, FoldoutGroup("������")]
        private bool checkGround = false;
        /// <summary>
        /// �Ƿ��ڵ���
        /// </summary>
        [LabelText("ʵ�ʴ��ڵ���"), ShowInInspector, ReadOnly, FoldoutGroup("������")]
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

        #region ʵ���ٶȲ���
        [LabelText("��ǰʵ���ٶȴ�С"), ShowInInspector, ReadOnly, FoldoutGroup("ʵ���ٶȲ���")]
        protected float CurrentSpeed => this.CurrentVelocity.magnitude;

        [LabelText("��ǰʵ���ٶ�"), ShowInInspector, ReadOnly, FoldoutGroup("ʵ���ٶȲ���")]
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
            this.moveSetting = new PlayerMoveSetting(this);
        }

        #region �ƶ�Move
        /// <summary>
        /// ��õ�ǰ֡���ƶ�����
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
        /// �Դ���������ƶ�
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdatePosition(float deltaTime, Vector2 inputV)
        {
            // �ƶ�����
            Vector3 moveDir = Vector3.zero;

            // ��ǰ�ƶ��ٶ�ϵ��
            float _speedRate = 0;

            // �ƶ�����
            if (this.moveSetting.bCanMove)
            {
                moveDir = this.GetInputMoveDir(inputV);
                _speedRate = 1;
            }
            // �ƶ�
            this.UpdatePosition(deltaTime, moveDir, _speedRate);

            CheckGround(deltaTime);
        }

        /// <summary>
        /// �ƶ�
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="moveDir"></param>
        public void UpdatePosition(float deltaTime, Vector3 moveDir, float _speedRate = 1)
        {
            moveDir.Normalize();

            // �����ٶȸ��£����ּ��ٺ͸�������������ͨ�� �Ӽ��ٶ�
            // ���ƶ� && �ƶ��ٶ���Ϊ�� && ���ƶ����� && û�дﵽ����ٶ��������ٶ�
            if (this.moveSetting.bCanMove && _speedRate > float.Epsilon && moveDir.magnitude > float.Epsilon && this.moveSetting.Speed <= this.moveSetting.MaxSpeed * _speedRate)
            {
                // �����ٶ�
                this.moveSetting.Speed = Math.Clamp(this.moveSetting.Speed + Mathf.Max(0,this.moveSetting.AddAcceleration - this.moveSetting.FinalDrag) * deltaTime, 0, this.moveSetting.MaxSpeed * _speedRate);
                // �������뷽��
                this.moveSetting.Velocity = moveDir * this.moveSetting.Speed;
            }
            // �������ٶȣ���ʼ���� �����ٶ�˥��
            else if(this.moveSetting.Speed > this.moveSetting.MinSpeed)
            {
                // V -= KV * Max(ln(V*2), 1) => V < 0.74 ��ʼ����˥��
                this.moveSetting.Speed -= this.moveSetting.FinalDrag * deltaTime;
                if (this.moveSetting.Speed < this.moveSetting.MinSpeed)
                {
                    this.moveSetting.Speed = 0;
                }
            }
            if (this.moveSetting.Speed * deltaTime <= this.controller.minMoveDistance)
            {
                this.moveSetting.Velocity = Vector3.zero;
            }

            // �����ٶȷ���任 => Ӧ����������
            if (this.moveSetting.Velocity.sqrMagnitude > float.Epsilon)
            {
                // controller ����ߣ����¼��ľ���
                float distance = (Mathf.Max(this.controller.height, this.controller.radius * 2) + this.controller.skinWidth * 2);

                // center ���� => ƽ�������ƶ���controller�ײ��ˣ�
                Vector3 Pbot = this.controller.transform.position + this.controller.center;
                //Pbot.y -= Mathf.Max(this.controller.height * 0.5f, this.controller.radius);
                // �ƶ����������center��ƫ�Ƶļ���
                Vector3 Ptop = Pbot;
                float offset = (this.controller.radius + this.controller.skinWidth + Math.Clamp(this.moveSetting.Speed * deltaTime, this.controller.minMoveDistance, this.controller.stepOffset + this.controller.minMoveDistance));
                RaycastHit hitInfo;
                // ��ǰ�����һ�£�����ƫ����������Ptop��ƫ�ƽ���ǰ����ײ���ڲ�
                Ptop.y = Ptop.y - this.controller.skinWidth + this.controller.stepOffset;
                if (Physics.Raycast(Ptop, this.moveSetting.Velocity, out hitInfo, offset, this.moveSetting.DetectLayer))
                {
                    offset = (hitInfo.point - Ptop).magnitude;
                }
                Ptop = Pbot;
                Ptop += this.moveSetting.Velocity.normalized * offset;
                // �������߶���⵽�˲Ÿ����ٶȷ���
                RaycastHit hitInfo1, hitInfo2;
                // ���¼��
                if (Physics.Raycast(Pbot, -this.controller.transform.up, out hitInfo1, distance, this.moveSetting.DetectLayer) && Physics.Raycast(Ptop, -this.controller.transform.up, out hitInfo2, distance, this.moveSetting.DetectLayer))
                {
                    // ����
                    Vector3 direction = (hitInfo2.point - hitInfo1.point).normalized;
                    // �����ƶ�ʱ�Ƿ������ƶ����¶�����
                    if (Vector3.Angle(Vector3.up, direction) <= 90)
                    {
                        float angle = Vector3.Angle(Vector3.ProjectOnPlane(direction, Vector3.up), direction);
                        if (angle > 1 && angle <= this.controller.slopeLimit)
                        {
                            // �����ٶȷ���
                            this.moveSetting.Velocity = this.moveSetting.Speed * direction;
                        }
                    }
                    else
                    {
                        // �����ٶȷ���
                        this.moveSetting.Velocity = this.moveSetting.Speed * direction;
                    }
                }
            }
            
            Vector3 motion = (this.moveSetting.Velocity + this.moveSetting.ExtraVelocity) * deltaTime;
            this.controller.Move(motion);

            // ������ײ������
            if (Physics.SphereCast(this.controller.transform.position + this.controller.center + Mathf.Max((this.controller.height * 0.5f - this.controller.radius), 0) * this.controller.transform.up, this.controller.radius, this.controller.transform.up, out RaycastHit _hitInfo, this.controller.skinWidth, this.moveSetting.DetectLayer))
            {
                this.moveSetting.ExtraVelocity = new Vector3(this.moveSetting.ExtraVelocity.x, -this.moveSetting.ExtraVelocity.y, this.moveSetting.ExtraVelocity.z);
                this.moveSetting.Velocity = new Vector3(this.moveSetting.Velocity.x, -this.moveSetting.Velocity.y, this.moveSetting.Velocity.z);
                motion = (this.moveSetting.Velocity + this.moveSetting.ExtraVelocity) * deltaTime;
                this.controller.Move(motion);
            }

            this.moveSetting.IsGrounded = this.IsGrounded;


            // �����ٶ�˥��
            if (this.moveSetting.ExtraSpeed > this.moveSetting.MinSpeed)
            {
                this.moveSetting.ExtraSpeed -= this.moveSetting.FinalDrag * deltaTime;
                if (this.moveSetting.ExtraSpeed < this.moveSetting.MinSpeed)
                {
                    this.moveSetting.ExtraSpeed = 0;
                }
            }
            if (this.moveSetting.ExtraSpeed > 0.00001f && this.moveSetting.ExtraSpeed * deltaTime <= this.controller.minMoveDistance)
            {
                this.moveSetting.ExtraVelocity = Vector3.zero;
            }


            // ����ʵ���˶�������� ģ����ת
            if (this.moveSetting.CanModelRotate && (moveSetting.AirRotationControl || IsGrounded) && this.Model && this.controller.velocity.magnitude > this.moveSetting.MinSpeed)
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
                    Vector3 gravity = this.moveSetting.Gravity * Vector3.down;
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
                    this.moveSetting.ExtraVelocity += this.moveSetting.Gravity * Vector3.down * deltaTime;
                }
            }


            if (this.IsGrounded)
            {
                if ((this.moveSetting.ExtraVelocity.y + this.moveSetting.Velocity.y) < float.Epsilon)
                {
                    this.moveSetting.RemainJumpCount = this.moveSetting.MaxJumpCount;
                }
            }
            else
            {
                this.moveSetting.RemainJumpCount = Mathf.Min(this.moveSetting.RemainJumpCount,this.moveSetting.MaxJumpCount-1) ;
            }
            // ��� -> ��ֱ�ٶ���0
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
        /// ʩ���ٶ� => �ⲿ���صĸı�����������
        /// </summary>
        /// <param name="vel"></param>
        public void AddVelocity(Vector3 vel)
        {
            this.moveSetting.ExtraVelocity += vel;
        }
        
        /// <summary>
        /// ��0�����ٶ�
        /// </summary>
        public void ClearInputSpeed()
        {
            this.moveSetting.Speed = 0;
        }
        /// <summary>
        /// ��0�����ٶ�
        /// </summary>
        public void ClearExtraSpeed()
        {
            this.moveSetting.ExtraSpeed = 0;
        }
        /// <summary>
        /// ��0�����ٶ�
        /// </summary>
        public void ClearBothSpeed()
        {
            this.ClearInputSpeed();
            this.ClearExtraSpeed();
        }
        #endregion

        #region ��ԾJump
        /// <summary>
        /// ��������ǰҡ
        /// </summary>
        [LabelText("������Ծǰҡ")]
        public bool EnablePreJump = false;
        private bool _lastIsInPreJump = false;
        private bool _isInPreJump = false;
        /// <summary>
        /// �Ƿ�������ǰҡ
        /// </summary>
        [ShowInInspector, ReadOnly, LabelText("������Ծǰҡ"), ShowIf("@EnablePreJump == true")]
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
        /// �Դ����������Ծ
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
        /// ��ӦJump
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
        /// ��Ծ
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
        /// ǿ����Ծ�������Ƿ�������Ծ���Ƿ�����Ծ����Ծ���������ƣ��Ҳ�������Щ����
        /// </summary>
        public void ForceJump()
        {
            if (this.moveSetting.bIsWorldUpAxis)
            {
                (moveSetting.Velocity,moveSetting.ExtraVelocity) = ImpactJumpSpeed(moveSetting.Velocity,moveSetting.ExtraVelocity);
            }
            else
            {
                Vector3 tmpVelocity = this.controller.transform.InverseTransformVector(this.moveSetting.Velocity);
                Vector3 tmpExtraVelocity = this.controller.transform.InverseTransformVector(this.moveSetting.ExtraVelocity);
                
                (tmpVelocity,tmpExtraVelocity)  = ImpactJumpSpeed(tmpVelocity,tmpExtraVelocity);
                this.moveSetting.Velocity = this.controller.transform.TransformVector(tmpVelocity);
                this.moveSetting.ExtraVelocity = this.controller.transform.TransformVector(tmpExtraVelocity);
            }

            this.jumpTimer.Reset(this.moveSetting.jumpTime);
        }

        protected void JumpTimer(double timer)
        {
            // WorldUpAxis
            if (this.moveSetting.bIsWorldUpAxis)
            {
                Vector3 jVel = this.moveSetting.ExtraVelocity;
                jVel += Vector3.up * this.moveSetting.jumpSpeedCurve.Evaluate((float)this.jumpTimer.Time);
                this.moveSetting.ExtraVelocity = jVel;
            }
            // SelfUpAxis,��ǽ����
            else
            {
                //��Ϊ�ֲ��ٶȣ��̳оֲ�����
                Vector3 jVel = this.controller.transform.InverseTransformVector(this.moveSetting.ExtraVelocity);
                jVel.y += this.moveSetting.jumpSpeedCurve.Evaluate((float)this.jumpTimer.Time);
                this.moveSetting.ExtraVelocity = this.controller.transform.TransformVector(jVel);
            }
            // ���
            if (this.controller.isGrounded && this.moveSetting.ExtraVelocity.y < 0)
            {
                this.jumpTimer.End();
            }
        }

        private (Vector3,Vector3) ImpactJumpSpeed(Vector3 _inSpeedXZ,Vector3 _inSpeedY)
        {
            if (!moveSetting.ImpactBaseVelocityX)
                _inSpeedXZ.x = 0;
            if (!moveSetting.ImpactBaseVelocityZ)
                _inSpeedXZ.z = 0;
            if (!moveSetting.ImpactBaseVelocityY)
                _inSpeedY.y = 0;

            return (_inSpeedXZ,_inSpeedY);
        }
        
#endregion
    }

}

