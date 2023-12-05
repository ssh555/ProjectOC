using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.TechTree.UI
{
    public class UITechPointPanel : UIBasePanel
    {
        // ����Grid Layout ÿ�е�����Ϊ�Ƽ��������Ĳ���


        public override void OnEnter()
        {
            base.OnEnter();
            ProjectOC.Input.InputManager.TechTreeInput.Enable();
            ProjectOC.Input.InputManager.PlayerInput.Disable();

        }

        public override void OnPause()
        {
            base.OnPause();
            ProjectOC.Input.InputManager.PlayerInput.Enable();
            ProjectOC.Input.InputManager.TechTreeInput.Disable();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            ProjectOC.Input.InputManager.PlayerInput.Disable();
            ProjectOC.Input.InputManager.TechTreeInput.Enable();
        }

        public override void OnExit()
        {
            base.OnExit();
            ProjectOC.Input.InputManager.PlayerInput.Enable();
            ProjectOC.Input.InputManager.TechTreeInput.Disable();
        }
    }

}
