using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.Input;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using Newtonsoft.Json;
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
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheelUI;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class ResonanceWheel_sub2 : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;
        private void Start()
        {

            //KeyTips
            UIKeyTipComponents = this.transform.GetComponentsInChildren<UIKeyTipComponent>(true);
            foreach (var item in UIKeyTipComponents)
            {
                item.InitData();
                uiKeyTipDic.Add(item.InputActionName, item);
            }

            //Ring
            var ringcontent = this.transform.Find("Ring").Find("Viewport").Find("Content");

            var ring = ringcontent.Find("Ring").Find("ring");

            Grids = new GridBeastType[ring.childCount];
            for (int i = 0; i < ring.childCount; i++)
            {
                Grids[i].transform  = ring.GetChild(i);
                Grids[i].beastType = (BeastType)Enum.Parse(typeof(BeastType), ring.GetChild(i).name);
                Grids[i].image = Grids[i].transform.Find("Image").GetComponent<Image>();
                Grids[i].image.sprite = parentUI.beastTypeDic[Grids[i].beastType];
            }

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

        private struct GridBeastType
        {
            public Transform transform;
            public ResonanceWheelUI.BeastType beastType;
            public Image image;

            public GridBeastType(Transform transform, ResonanceWheelUI.BeastType beastType,Image image)
            {
                this.transform = transform;
                this.beastType = beastType;
                this.image = image;
            }

        }





        #region Internal

        private void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.Enable();
            UikeyTipIsInit = false;
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

            Vector2 vector2 = obj.ReadValue<Vector2>();

            float angle = Mathf.Atan2(vector2.x, vector2.y);

            angle = angle * 180 / Mathf.PI;
            if (angle < 0)
            {
                angle = angle + 360;
            }

            if (angle < 25.7 || angle > 334.3) CurrentGridIndex = 0;
            else if (angle > 25.7 && angle < 77.1) CurrentGridIndex = 6;
            else if (angle > 77.1 && angle < 128.5) CurrentGridIndex = 5;
            else if (angle > 128.5 && angle < 179.9) CurrentGridIndex = 4;
            else if (angle > 179.9 && angle < 231.3) CurrentGridIndex = 3;
            else if (angle > 231.3 && angle < 282.7) CurrentGridIndex = 2;
            else if (angle > 282.7 && angle < 334.3) CurrentGridIndex = 1;

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

        private Dictionary<string, UIKeyTipComponent> uiKeyTipDic = new Dictionary<string, UIKeyTipComponent>();
        private bool UikeyTipIsInit;
        private InputManager inputManager => GameManager.Instance.InputManager;
        private void ClearTemp()
        {
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempItemType.Values)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempUIItems)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }

            uiKeyTipDic = null;
        }

        #endregion

        #region UI对象引用
        private UIKeyTipComponent[] UIKeyTipComponents;

        public ResonanceWheelUI parentUI;

        
        //ring
        [ShowInInspector]
        private GridBeastType[] Grids;
        private Transform Grid1, Grid2, Grid3, Grid4, Grid5, Grid6, Grid7;
        private int CurrentGridIndex = 0;//0到5

        //BotKeyTips
        private UIKeyTip KT_Confirm;
        private UIKeyTip KT_Back;

        #endregion

        public override void Refresh()
        {
            if (this.parentUI.ABJAProcessorJson_sub2 == null || !this.parentUI.ABJAProcessorJson_sub2.IsLoaded || !IsInit)
            {
                return;
            }

            if (UikeyTipIsInit == false)
            {
                KeyTip[] keyTips = inputManager.ExportKeyTipValues(this.parentUI.PanelTextContent_sub2);
                foreach (var keyTip in keyTips)
                {
                    
                    InputAction inputAction = inputManager.GetInputAction((keyTip.keymap.ActionMapName, keyTip.keymap.ActionName));
                    inputManager.GetInputActionBindText(inputAction);

                    UIKeyTipComponent uIKeyTipComponent = uiKeyTipDic[keyTip.keyname];
                    if (uIKeyTipComponent.uiKeyTip.keytip != null)
                    {
                        uIKeyTipComponent.uiKeyTip.keytip.text = inputManager.GetInputActionBindText(inputAction);
                    }
                    if (uIKeyTipComponent.uiKeyTip.description != null)
                    {
                        uIKeyTipComponent.uiKeyTip.description.text = keyTip.description.GetText();
                    }

                }
                UikeyTipIsInit = true;
            }

            //ring

            for (int i = 0; i < Grids.Length; i++)
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

    }
        #endregion



        #region TextContent
        [System.Serializable]
        public struct ResonanceWheel_sub2Struct
        {
            //BotKeyTips
            public KeyTip Confirm;
            public KeyTip Back;
        }
        #endregion



    }

}
