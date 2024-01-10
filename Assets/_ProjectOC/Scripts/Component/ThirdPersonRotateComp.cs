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

        /// <summary>
        /// 摄像机transform
        /// </summary>
        [LabelText("摄像机")]
        public CinemachineVirtualCamera VCamera;

        #region Property|Field
        [LabelText("摄像机 X 轴旋转")]
        public Transform VCamRotX;
        [LabelText("摄像机 Y 轴旋转")]
        public Transform VCamRotY;
        [LabelText("摄像机 Z 轴旋转")]
        public Transform VCamRotZ;

        /// <summary>
        /// 摄像机视点中心
        /// </summary>
        [LabelText("摄像机视点中心")]
        public Transform VCamViewer;

        /// <summary>
        /// X 方向旋转速度
        /// </summary>
        [LabelText("X 方向旋转速度"), ShowInInspector]
        public float rotSpeedX;

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
        public bool bEnableDeltaTime = false;

        private UnityEngine.InputSystem.InputAction MouseXInput;
        private UnityEngine.InputSystem.InputAction MouseYInput;
        public void RuntimeSetMouseInput(UnityEngine.InputSystem.InputAction X, UnityEngine.InputSystem.InputAction Y)
        {
            this.MouseXInput = X;
            this.MouseYInput = Y;
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
            if (!this.bRotEnable)
            {
                return;
            }

            // 锁定鼠标
            //Cursor.lockState = CursorLockMode.Locked;

            // 获得鼠标当前位置的X和Y
            // 旋转Y轴
            float mouseX = this.MouseXInput.ReadValue<float>() * rotSpeedX * (bEnableDeltaTime ? Time.deltaTime : 1);
            this.VCamRotY.localRotation *= Quaternion.Euler(0f, mouseX, 0f);

            // 旋转X轴
            float mouseY = this.MouseYInput.ReadValue<float>() * rotSpeedY * (bEnableDeltaTime ? Time.deltaTime : 1);


            this.VCamRotX.localRotation *= Quaternion.Euler(mouseY * (bInVerseRotX ? -1 : 1), 0f, 0f);
            this.VCamRotX.localRotation = this.ClampRotationAroundXAxis(this.VCamRotX.localRotation);
            //this.ConstrainCamera();
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


        //private void ConstrainCamera()
        //{
        //    Vector3 point = this.VCamViewer.InverseTransformPoint(this.VCamera.transform.position);
        //    point = point.normalized * this.radius;
        //    this.VCamera.transform.position = this.VCamViewer.TransformPoint(point);
        //}

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
