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

        /// <summary>
        /// �����transform
        /// </summary>
        [LabelText("�����")]
        public CinemachineVirtualCamera VCamera;

        #region Property|Field
        [LabelText("����� X ����ת")]
        public Transform VCamRotX;
        [LabelText("����� Y ����ת")]
        public Transform VCamRotY;
        [LabelText("����� Z ����ת")]
        public Transform VCamRotZ;

        /// <summary>
        /// ������ӵ�����
        /// </summary>
        [LabelText("������ӵ�����")]
        public Transform VCamViewer;

        /// <summary>
        /// X ������ת�ٶ�
        /// </summary>
        [LabelText("X ������ת�ٶ�"), ShowInInspector]
        public float rotSpeedX;

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

            // �������
            //Cursor.lockState = CursorLockMode.Locked;

            // �����굱ǰλ�õ�X��Y
            // ��תY��
            float mouseX = this.MouseXInput.ReadValue<float>() * rotSpeedX * (bEnableDeltaTime ? Time.deltaTime : 1);
            this.VCamRotY.localRotation *= Quaternion.Euler(0f, mouseX, 0f);

            // ��תX��
            float mouseY = this.MouseYInput.ReadValue<float>() * rotSpeedY * (bEnableDeltaTime ? Time.deltaTime : 1);


            this.VCamRotX.localRotation *= Quaternion.Euler(mouseY * (bInVerseRotX ? -1 : 1), 0f, 0f);
            this.VCamRotX.localRotation = this.ClampRotationAroundXAxis(this.VCamRotX.localRotation);
            //this.ConstrainCamera();
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
