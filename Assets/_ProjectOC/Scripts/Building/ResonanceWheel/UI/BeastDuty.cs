using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using Newtonsoft.Json;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using ProjectOC.WorkerEchoNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheel_sub1;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheel_sub2;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheelUI;


namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class BeastDuty : ML.Engine.UI.UIBasePanel
    {





        #region Unity
        public bool IsInit = false;
        private void Start()
        {
            







        IsInit = true;

          

            Refresh();
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            
            this.Enter();


            
        }

        public override void OnExit()
        {
            base.OnExit();
            
            this.Exit();
            
            ClearTemp();

        }

        public override void OnPause()
        {
            base.OnPause();
            this.Exit();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            this.Enter();
        }

        #endregion

        #region Internal

        private void Enter()
        {
            this.RegisterInput();
            
            this.Refresh();
        }

        private void Exit()
        {

            this.UnregisterInput();
            
        }

        private void UnregisterInput()
        {





            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            

        }



        private void RegisterInput()
        {


            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }




        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }
        #endregion

        #region UI


        #region temp
        private void ClearTemp()
        {

        }

        #endregion

        #region UI对象引用





        #endregion

        public void Refresh()
        {




        }   
        #endregion






       




       

    }


}
