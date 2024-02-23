using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheelUI;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class ResonanceWheel_sub2 : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;
        private void Start()
        {

            //parentUI = GameObject.Find("Canvas").GetComponentInChildren<ResonanceWheelUI>();

            //Ring
            var ringcontent = this.transform.Find("Ring").Find("Viewport").Find("Content");

            var ring = ringcontent.Find("Ring");
            Grid1 = ring.Find("Grid1");
            Grid2 = ring.Find("Grid2");
            Grid3 = ring.Find("Grid3");
            Grid4 = ring.Find("Grid4");
            Grid5 = ring.Find("Grid5");
            Grid6 = ring.Find("Grid6");
            Grid7 = ring.Find("Grid7");
            Grids.Add(new GridBeastType(Grid1.transform, ResonanceWheelUI.BeastType.WorkerEcho_Random));
            Grids.Add(new GridBeastType(Grid2.transform, ResonanceWheelUI.BeastType.WorkerEcho_Cat));
            Grids.Add(new GridBeastType(Grid3.transform, ResonanceWheelUI.BeastType.WorkerEcho_Deer));
            Grids.Add(new GridBeastType(Grid4.transform, ResonanceWheelUI.BeastType.WorkerEcho_Fox));
            Grids.Add(new GridBeastType(Grid5.transform, ResonanceWheelUI.BeastType.WorkerEcho_Rabbit));
            Grids.Add(new GridBeastType(Grid6.transform, ResonanceWheelUI.BeastType.WorkerEcho_Dog));
            Grids.Add(new GridBeastType(Grid7.transform, ResonanceWheelUI.BeastType.WorkerEcho_Seal));

            //BotKeyTips
            var kt = this.transform.Find("BotKeyTips").Find("KeyTips");
            KT_Back = new UIKeyTip();

            KT_Back.keytip = kt.Find("KT_Back").Find("Image").Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Back.description = kt.Find("KT_Back").Find("Image").Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            KT_Confirm = new UIKeyTip();
            KT_Confirm.keytip = kt.Find("KT_Confirm").Find("Image").Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Confirm.description = kt.Find("KT_Back").Find("Image").Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();



        IsInit = true;

          

            Refresh();
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            
            this.Enter();

            parentUI.MainToSub2();
            
        }

        public override void OnExit()
        {
            base.OnExit();
            
            this.Exit();
            
            ClearTemp();
            parentUI.Sub2ToMain();

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

        public class GridBeastType
        {
            public Transform transform;
            public ResonanceWheelUI.BeastType beastType;

            public GridBeastType(Transform transform, ResonanceWheelUI.BeastType beastType)
            {
                this.transform = transform;
                this.beastType = beastType;
            }

        }





        #region Internal

        private void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.Enable();
            this.Refresh();
        }

        private void Exit()
        {
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.Disable();
            this.UnregisterInput();
            
        }

        private void UnregisterInput()
        {
           

            //切换隐兽
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.NextGrid.performed -= NextGrid_performed;

            //确认
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.Confirm.performed -= Confirm_performed;




            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            

        }

        private void RegisterInput()
        {
            
            //切换隐兽
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.NextGrid.performed += NextGrid_performed;


            //确认
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.Confirm.performed += Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }





        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }







        private void NextGrid_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurrentGridIndex = (CurrentGridIndex + 1) % Grids.Count;
            this.Refresh();
        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            
            
            parentUI.Grids[parentUI.CurrentGridIndex].beastType = Grids[CurrentGridIndex].beastType;
            parentUI.currentBeastType = Grids[CurrentGridIndex].beastType;
            UIMgr.PopPanel();
            
        }
        #endregion

        #region UI
        #region Temp
        private List<Sprite> tempSprite = new List<Sprite>();
        private Dictionary<ML.Engine.InventorySystem.ItemType, GameObject> tempItemType = new Dictionary<ML.Engine.InventorySystem.ItemType, GameObject>();
        private List<GameObject> tempUIItems = new List<GameObject>();


        private void ClearTemp()
        {
            foreach (var s in tempSprite)
            {
                Destroy(s);
            }
            foreach (var s in tempItemType.Values)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItems)
            {
                Destroy(s);
            }
        }

        #endregion

        #region UI对象引用
        public ResonanceWheelUI parentUI;

        
        //ring
        [ShowInInspector]
        private List<GridBeastType> Grids = new List<GridBeastType>();
        private Transform Grid1, Grid2, Grid3, Grid4, Grid5, Grid6, Grid7;
        private int CurrentGridIndex = 0;//0到5

        //BotKeyTips
        private UIKeyTip KT_Confirm;
        private UIKeyTip KT_Back;

        #endregion

        public void Refresh()
        {
            if (ResonanceWheelUI.ABJAProcessorJson_sub2 == null || !ResonanceWheelUI.ABJAProcessorJson_sub2.IsLoaded || !IsInit)
            {
                Debug.Log("ABJAProcessorJson is null");
                return;
            }


            //ring

            for (int i = 0; i < Grids.Count; i++)
            {
                if (CurrentGridIndex == i)
                {
                    Grids[i].transform.Find("Selected").gameObject.SetActive(true);
                }
                else
                {
                    Grids[i].transform.Find("Selected").gameObject.SetActive(false);
                }
            }


            //BotKeyTips
            KT_Confirm.ReWrite(ResonanceWheelUI.PanelTextContent_sub2.confirm);
            KT_Back.ReWrite(ResonanceWheelUI.PanelTextContent_sub2.back);


    }
        #endregion



        #region TextContent
        [System.Serializable]
        public struct ResonanceWheel_sub2Struct
        {
            //BotKeyTips
            public KeyTip confirm;
            public KeyTip back;
        }
        #endregion

        
    }

}
