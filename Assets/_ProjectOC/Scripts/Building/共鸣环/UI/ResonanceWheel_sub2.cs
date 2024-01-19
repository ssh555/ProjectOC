using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
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

        public IInventory inventory;

        #region Input
        /// <summary>
        /// 用于Drop和Destroy按键响应Cancel
        /// 长按响应了Destroy就置为true
        /// Cancel就不响应Drop 并 重置
        /// </summary>
        private bool ItemIsDestroyed = false;
        #endregion

        #region Unity
        public bool IsInit = false;
        private void Start()
        {
            InitUITextContents();

            parentUI = GameObject.Find("Canvas").GetComponentInChildren<ResonanceWheelUI>();

            //Ring
            var ringcontent = this.transform.Find("Ring").Find("Viewport").Find("Content");

            var ring = ringcontent.Find("Ring");
            Grid1 = ring.Find("Grid1");
            Grid2 = ring.Find("Grid2");
            Grid3 = ring.Find("Grid3");
            Grid4 = ring.Find("Grid4");
            Grid5 = ring.Find("Grid5");
            Grid6 = ring.Find("Grid6");
            Grids.Add(new GridBeastType(Grid1.transform, "123"));
            Grids.Add(new GridBeastType(Grid2.transform, "124"));
            Grids.Add(new GridBeastType(Grid3.transform, "125"));
            Grids.Add(new GridBeastType(Grid4.transform, "126"));
            Grids.Add(new GridBeastType(Grid5.transform, "127"));
            Grids.Add(new GridBeastType(Grid6.transform, "128"));

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

            GameObject.Find("Canvas").GetComponentInChildren<ResonanceWheelUI>().MainToSub2();
            
        }

        public override void OnExit()
        {
            base.OnExit();
            
            this.Exit();
            
            ClearTemp();
            GameObject.Find("Canvas").GetComponentInChildren<ResonanceWheelUI>().Sub2ToMain();

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
            public string beastID;

            public GridBeastType(Transform transform, string beastID)
            {
                this.transform = transform;
                this.beastID = beastID;
            }

        }





        #region Internal

        private void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.Enable();
            //ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(0);
            this.Refresh();
        }

        private void Exit()
        {
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.Disable();
            //ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(1);
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
            Debug.Log("sub2 Back_performed");
            UIMgr.PopPanel();
            

        }







        private void NextGrid_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurrentGridIndex = (CurrentGridIndex + 1) % Grids.Count;
            this.Refresh();
        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            
            
            parentUI.Grids[parentUI.CurrentGridIndex].id = Grids[CurrentGridIndex].beastID;
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
        private Transform Grid1, Grid2, Grid3, Grid4, Grid5, Grid6;
        private int CurrentGridIndex = 0;//0到5

        //BotKeyTips
        private UIKeyTip KT_Confirm;
        private UIKeyTip KT_Back;

        #endregion

        public void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
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
            KT_Confirm.ReWrite(PanelTextContent.confirm);
            KT_Back.ReWrite(PanelTextContent.back);


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

        public static ResonanceWheel_sub2Struct PanelTextContent => ABJAProcessorJson.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheel_sub2Struct> ABJAProcessorJson;

        private void InitUITextContents()
        {
            if (ABJAProcessorJson == null)
            {
                ABJAProcessorJson = new ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheel_sub2Struct>("Binary/TextContent/ResonanceWheel_sub2", "ResonanceWheel_sub2", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UI共鸣轮Panel_sub2数据");
                ABJAProcessorJson.StartLoadJsonAssetData();
            }

        }
        #endregion

    }

}
