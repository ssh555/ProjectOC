using Cinemachine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Player
{
    /// <summary>
    /// 不使用时记得调用 DisposeTick
    /// </summary>
    [System.Serializable]
    public class ThirdPersonRotateComp : ML.Engine.Timer.ITickComponent
    {
        /// <summary>
        /// 是否启用旋转
        /// </summary>
        [LabelText("是否启用旋转"), ShowInInspector]
        public bool bRotEnable;

        private bool bSpringScaleEnable = true;
        /// <summary>
        /// 摄像机transform
        /// </summary>
        [LabelText("摄像机")]
        public CinemachineVirtualCamera VCamera;
        [LabelText("LookAt目标")]
        public Transform TargetTransf;
        public Cinemachine3rdPersonFollow Cinemachine3Rd => VCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        #region Property|Field
        /// <summary>
        /// 摄像机视点中心
        /// </summary>

        /// <summary>
        /// X 方向旋转速度
        /// </summary>
        [LabelText("X 方向旋转速度"), ShowInInspector]
        public float rotSpeedX;

        private float cinemachineTargetYaw;
        private float cinemachineTargetPitch;
        
        /// <summary>
        /// Y 方向旋转速度
        /// </summary>
        [LabelText("Y 方向旋转速度"), ShowInInspector]
        public float rotSpeedY;

        [LabelText("X 轴旋转最小值")]
        public float rotXMin;
        [LabelText("X 轴旋转最大值")]
        public float rotXMax;
        [LabelText("翻转垂直旋转")]
        public bool bInVerseRotX;

        [LabelText("启用deltaTime")]
        public bool bEnableDeltaTime = true;

        #region 相机跟随
        [LabelText("当前长度")]
        public float SpringCurLength = 0.1f;
        [LabelText("弹簧臂伸缩速度")]
        public float scrollSpeed = 0.001f;
        [LabelText("弹簧臂长最大"),Range(0,10)] 
        public float SpringMaxLength = 4f;
        [LabelText("弹簧臂长最小"),Range(0,10)] 
        public float SpringMinLength = 2f;
        #endregion
        
        private UnityEngine.InputSystem.InputAction MouseXInput;
        private UnityEngine.InputSystem.InputAction MouseYInput;
        private UnityEngine.InputSystem.InputAction MouseScroll;
        public void RuntimeSetMouseInput(UnityEngine.InputSystem.InputAction X, UnityEngine.InputSystem.InputAction Y,UnityEngine.InputSystem.InputAction Scroll)
        {
            this.MouseXInput = X;
            this.MouseYInput = Y;
            this.MouseScroll = Scroll;
        }
        #endregion

        #region ITickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        #endregion

        #region Tick
        public void LateTick(float deltatime)
        {
            if (this.bRotEnable)
            {
                //相机轴旋转
                if (bSpringScaleEnable)
                {
                    float mouseScroll = this.MouseScroll.ReadValue<float>() * scrollSpeed;
                    SpringCurLength =  Mathf.Clamp(SpringCurLength + mouseScroll,SpringMinLength,SpringMaxLength);
                    Cinemachine3Rd.CameraDistance = SpringCurLength;
                    // Vector3 camPos =  VCamera.transform.localPosition.normalized* SpringCurLength;
                    // VCamera.transform.localPosition = camPos;
                }
                
                // 旋转Y轴、X轴
                float mouseX = this.MouseXInput.ReadValue<float>() * rotSpeedX * (bEnableDeltaTime ? Time.deltaTime : 1);
                float mouseY = this.MouseYInput.ReadValue<float>() * rotSpeedY * (bEnableDeltaTime ? Time.deltaTime : 1) * (bInVerseRotX ? -1 : 1);
                cinemachineTargetYaw += mouseX;
                cinemachineTargetPitch += mouseY;
            }
            cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
            cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, this.rotXMin, this.rotXMax);
            TargetTransf.rotation = Quaternion.Euler(cinemachineTargetPitch,cinemachineTargetYaw, 0.0f);
        }
        #endregion


        /// <summary>
        /// 垂直旋转限定函数
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            // 绕垂直轴旋转 (x,y,z) => (1,0,0)
            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x / q.w);

            angleX = Mathf.Clamp(angleX, this.rotXMin, this.rotXMax);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX) * q.w;
            return q;
        }
        private float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
        
        public void Init()
        {
            VCamera.transform.SetParent(null);
        }

        public void RegisterTick(int priority)
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterLateTick(priority, this);
        }

        public void UnregisterTick()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterLateTick(this);
        }
    }

}
