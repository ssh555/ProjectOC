using Cinemachine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Player
{
    /// <summary>
    /// ��ʹ��ʱ�ǵõ��� DisposeTick
    /// </summary>
    [System.Serializable]
    public class ThirdPersonRotateComp : ML.Engine.Timer.ITickComponent
    {
        /// <summary>
        /// �Ƿ�������ת
        /// </summary>
        [LabelText("�Ƿ�������ת"), ShowInInspector]
        public bool bRotEnable;

        private bool bSpringScaleEnable = true;
        /// <summary>
        /// �����transform
        /// </summary>
        [LabelText("�����")]
        public CinemachineVirtualCamera VCamera;
        [LabelText("LookAtĿ��")]
        public Transform TargetTransf;
        public Cinemachine3rdPersonFollow Cinemachine3Rd => VCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        #region Property|Field
        /// <summary>
        /// ������ӵ�����
        /// </summary>

        /// <summary>
        /// X ������ת�ٶ�
        /// </summary>
        [LabelText("X ������ת�ٶ�"), ShowInInspector]
        public float rotSpeedX;

        private float cinemachineTargetYaw;
        private float cinemachineTargetPitch;
        
        /// <summary>
        /// Y ������ת�ٶ�
        /// </summary>
        [LabelText("Y ������ת�ٶ�"), ShowInInspector]
        public float rotSpeedY;

        [LabelText("X ����ת��Сֵ")]
        public float rotXMin;
        [LabelText("X ����ת���ֵ")]
        public float rotXMax;
        [LabelText("��ת��ֱ��ת")]
        public bool bInVerseRotX;

        [LabelText("����deltaTime")]
        public bool bEnableDeltaTime = true;

        #region �������
        [LabelText("��ǰ����")]
        public float SpringCurLength = 0.1f;
        [LabelText("���ɱ������ٶ�")]
        public float scrollSpeed = 0.001f;
        [LabelText("���ɱ۳����"),Range(0,10)] 
        public float SpringMaxLength = 4f;
        [LabelText("���ɱ۳���С"),Range(0,10)] 
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
                //�������ת
                if (bSpringScaleEnable)
                {
                    float mouseScroll = this.MouseScroll.ReadValue<float>() * scrollSpeed;
                    SpringCurLength =  Mathf.Clamp(SpringCurLength + mouseScroll,SpringMinLength,SpringMaxLength);
                    Cinemachine3Rd.CameraDistance = SpringCurLength;
                    // Vector3 camPos =  VCamera.transform.localPosition.normalized* SpringCurLength;
                    // VCamera.transform.localPosition = camPos;
                }
                
                // ��תY�ᡢX��
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
        /// ��ֱ��ת�޶�����
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            // �ƴ�ֱ����ת (x,y,z) => (1,0,0)
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
