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
        /// 是否能旋转
        /// </summary>
        [LabelText("启用旋转")]
        public bool bCanRotate = true;
        /// <summary>
        /// X 灵敏度
        /// </summary>
        [LabelText("水平灵敏度")]
        public float XSensitivity = 2f;
        /// <summary>
        /// Y 灵敏度
        /// </summary>
        [LabelText("垂直灵敏度")]
        public float YSensitivity = 2f;
        /// <summary>
        /// 限定垂直旋转
        /// </summary>
        [LabelText("限定垂直旋转度数")]
        public bool clampVerticalRotation = true;
        /// <summary>
        /// 翻转垂直轴旋转
        /// </summary>
        [LabelText("翻转鼠标Y轴")]
        public bool InverseVerticalRotation = false;
        /// <summary>
        /// 垂直旋转 最小值
        /// </summary>
        [LabelText("垂直旋转最小值"), Range(-90, 90), ShowIf("clampVerticalRotation")]
        public float MinimumX = -90F;
        /// <summary>
        /// 垂直旋转 最大值
        /// </summary>
        [LabelText("垂直旋转最大值"), Range(-90, 90), ShowIf("clampVerticalRotation")]
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
        /// 涉及Input的旋转
        /// 需在Update or LateUpdate中调用
        /// </summary>
        /// <param name="character"></param>
        /// <param name="camera"></param>
        protected void LookRotation(Transform character, Transform camera, Vector2 inputV)
        {
            //// 锁定鼠标
            //Cursor.lockState = CursorLockMode.Locked;
            // 输入 => 水平旋转值
            float yRot = inputV.x * XSensitivity;
            // 输入 => 垂直旋转值
            float xRot = inputV.y * YSensitivity;

            // 水平旋转
            character.localRotation *= Quaternion.Euler(0f, yRot, 0f);
            // 垂直旋转
            camera.localRotation *= Quaternion.Euler(-xRot * (this.InverseVerticalRotation ? -1 : 1), 0f, 0f);
            // 限定垂直旋转
            if (clampVerticalRotation)
                camera.localRotation = ClampRotationAroundXAxis(camera.localRotation);
        }

        /// <summary>
        /// 锁死旋转
        /// </summary>
        /// <param name="character"></param>
        /// <param name="camera"></param>
        protected void LookOveride(Transform character, Transform camera, Vector2 inputV)
        {
            // 角色只旋转 Y 轴
            character.localRotation = new Quaternion(0, character.localRotation.y, 0, character.localRotation.w);
            // 摄像机只旋转 X 轴
            camera.localRotation = new Quaternion(0, camera.localRotation.y, 0, camera.localRotation.w);
        }

        /// <summary>
        /// 垂直旋转限定函数
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            // 绕垂直轴旋转 (x,y,z) => (1,0,0)
            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x / q.w);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX) * q.w;
            return q;
        }

    }

}
