using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Player
{
    [Serializable]
    public class PlayerMouseLook
    {
        /// <summary>
        /// �Ƿ�����ת
        /// </summary>
        [LabelText("������ת")]
        public bool bCanRotate = true;
        /// <summary>
        /// X ������
        /// </summary>
        [LabelText("ˮƽ������")]
        public float XSensitivity = 2f;
        /// <summary>
        /// Y ������
        /// </summary>
        [LabelText("��ֱ������")]
        public float YSensitivity = 2f;
        /// <summary>
        /// �޶���ֱ��ת
        /// </summary>
        [LabelText("�޶���ֱ��ת����")]
        public bool clampVerticalRotation = true;
        /// <summary>
        /// ��ת��ֱ����ת
        /// </summary>
        [LabelText("��ת���Y��")]
        public bool InverseVerticalRotation = false;
        /// <summary>
        /// ��ֱ��ת ��Сֵ
        /// </summary>
        [LabelText("��ֱ��ת��Сֵ"), Range(-90, 90), ShowIf("clampVerticalRotation")]
        public float MinimumX = -90F;
        /// <summary>
        /// ��ֱ��ת ���ֵ
        /// </summary>
        [LabelText("��ֱ��ת���ֵ"), Range(-90, 90), ShowIf("clampVerticalRotation")]
        public float MaximumX = 90F;

        public void Rotate(Transform character, Transform camera, Vector2 inputV)
        {
            if (bCanRotate)
            {
                this.LookRotation(character, camera, inputV);
            }
            else
            {
                this.LookOveride(character, camera, inputV);
            }
        }


        /// <summary>
        /// �漰Input����ת
        /// ����Update or LateUpdate�е���
        /// </summary>
        /// <param name="character"></param>
        /// <param name="camera"></param>
        protected void LookRotation(Transform character, Transform camera, Vector2 inputV)
        {
            //// �������
            //Cursor.lockState = CursorLockMode.Locked;
            // ���� => ˮƽ��תֵ
            float yRot = inputV.x * XSensitivity;
            // ���� => ��ֱ��תֵ
            float xRot = inputV.y * YSensitivity;

            // ˮƽ��ת
            character.localRotation *= Quaternion.Euler(0f, yRot, 0f);
            // ��ֱ��ת
            camera.localRotation *= Quaternion.Euler(-xRot * (this.InverseVerticalRotation ? -1 : 1), 0f, 0f);
            // �޶���ֱ��ת
            if (clampVerticalRotation)
                camera.localRotation = ClampRotationAroundXAxis(camera.localRotation);
        }

        /// <summary>
        /// ������ת
        /// </summary>
        /// <param name="character"></param>
        /// <param name="camera"></param>
        protected void LookOveride(Transform character, Transform camera, Vector2 inputV)
        {
            // ��ɫֻ��ת Y ��
            character.localRotation = new Quaternion(0, character.localRotation.y, 0, character.localRotation.w);
            // �����ֻ��ת X ��
            camera.localRotation = new Quaternion(0, camera.localRotation.y, 0, camera.localRotation.w);
        }

        /// <summary>
        /// ��ֱ��ת�޶�����
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            // �ƴ�ֱ����ת (x,y,z) => (1,0,0)
            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x / q.w);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX) * q.w;
            return q;
        }

    }

}
