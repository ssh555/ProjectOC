using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using Newtonsoft.Json;
using ProjectOC.WorkerEchoNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class ResonanceWheel_sub1 : ML.Engine.UI.UIBasePanel
    {

        public IInventory inventory;

        #region Input
        /// <summary>
        /// 用于Drop和Destroy按键响应Cancel
        /// 长按响应了Destroy就置为true
        /// Cancel就不响应Drop 并 重置
        /// </summary>

        #endregion

        #region Unity
        public bool IsInit = false;
        private void Start()
        {
            InitUITextContents();

            //BeastInfo
            var Info1 = this.transform.Find("HiddenBeastInfo1").Find("Info");
            Stamina = Info1.Find("PhysicalStrength").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Speed = Info1.Find("MovingSpeed").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            var GInfo = Info1.Find("SkillGraph").Find("Viewport").Find("Content").Find("Ring");
            Cook = GInfo.Find("Skill1").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            HandCraft = GInfo.Find("Skill6").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Industry = GInfo.Find("Skill5").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Magic = GInfo.Find("Skill4").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Transport = GInfo.Find("Skill3").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Collect = GInfo.Find("Skill2").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();

            var Info2 = this.transform.Find("HiddenBeastInfo2").Find("Info");
            var btn1 = this.transform.Find("HiddenBeastInfo2").Find("btn1");
            expel = new UIKeyTip();
            expel.keytip = btn1.Find("KeyTip").Find("Image").Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            expel.description = btn1.Find("KeyTip").Find("Image").Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            var btn2 = this.transform.Find("HiddenBeastInfo2").Find("btn2");
            receive = new UIKeyTip();
            receive.keytip = btn2.Find("KeyTip").Find("Image").Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            receive.description = btn2.Find("KeyTip").Find("Image").Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            

            //BotKeyTips
            var kt = this.transform.Find("BotKeyTips").Find("KeyTips");
            KT_Back = new UIKeyTip();
            KT_Back.img = kt.Find("KT_Back").Find("Image").GetComponent<Image>();
            KT_Back.keytip = KT_Back.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Back.description = KT_Back.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            IsInit = true;
            Refresh();
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.Enter();
            GameObject.Find("Canvas").GetComponentInChildren<ResonanceWheelUI>().MainToSub1();
        }

        public override void OnExit()
        {
            base.OnExit();
            this.Exit();
            ClearTemp();
            GameObject.Find("Canvas").GetComponentInChildren<ResonanceWheelUI>().Sub1ToMain();
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
 

   
        private int CurrentFuctionTypeIndex = 0;//0为HBR 1为SSB
        private int CurrentGridIndex = 0;//0到4


        private void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Enable();
            
            this.Refresh();
        }

        private void Exit()
        {
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Disable();
            
            this.UnregisterInput();
        }

        private void UnregisterInput()
        {




            //驱逐
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Expel.performed -= Expel_performed;

            //收留
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Receive.performed -= Receive_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            

        }

        private void RegisterInput()
        {
            


            //驱逐
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Expel.performed += Expel_performed;

            //收留
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Receive.performed += Receive_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }





        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }



        private void Expel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            parentUI = GameObject.Find("Canvas").GetComponentInChildren<ResonanceWheelUI>();
            GameManager.Instance.GetLocalManager<WorkerEcho>().ExpelWorker(parentUI.CurrentGridIndex);

            //ui

            Debug.Log("Expel_performed!");
        }

        private void Receive_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            parentUI = GameObject.Find("Canvas").GetComponentInChildren<ResonanceWheelUI>();
            GameManager.Instance.GetLocalManager<WorkerEcho>().SpawnWorker(parentUI.CurrentGridIndex);
            Debug.Log("Receive_performed!");
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
        //BeastInfo
        private TMPro.TextMeshProUGUI Stamina;
        private TMPro.TextMeshProUGUI Speed;

        private TMPro.TextMeshProUGUI Cook;
        private TMPro.TextMeshProUGUI HandCraft;
        private TMPro.TextMeshProUGUI Industry;
        private TMPro.TextMeshProUGUI Magic;
        private TMPro.TextMeshProUGUI Transport;
        private TMPro.TextMeshProUGUI Collect;

        private UIKeyTip expel;
        private UIKeyTip receive;


        //BotKeyTips
        private UIKeyTip KT_Back;


        #endregion

        public void Refresh()
        {

            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                Debug.Log("ABJAProcessorJson is null");
                return;
            }


            //BeastInfo
            Stamina.text = PanelTextContent.Stamina;
            Speed.text = PanelTextContent.Speed;

            Cook.text = PanelTextContent.Cook;
            HandCraft.text = PanelTextContent.HandCraft;
            Industry.text = PanelTextContent.Industry;
            Magic.text = PanelTextContent.Magic;
            Transport.text = PanelTextContent.Transport;
            Collect.text = PanelTextContent.Collect;

            expel.ReWrite(PanelTextContent.expel);
            receive.ReWrite(PanelTextContent.receive);


            //BotKeyTips
            KT_Back.ReWrite(PanelTextContent.back);
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct ResonanceWheel_sub1Struct
        {
            //BeastInfo
            public TextContent Stamina;
            public TextContent Speed;

            public TextContent Cook;
            public TextContent HandCraft;
            public TextContent Industry;
            public TextContent Magic;
            public TextContent Transport;
            public TextContent Collect;

            public KeyTip expel;
            public KeyTip receive;

            //BotKeyTips
            public KeyTip back;
        }

        public static ResonanceWheel_sub1Struct PanelTextContent => ABJAProcessorJson.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheel_sub1Struct> ABJAProcessorJson;

        private void InitUITextContents()
        {
            if (ABJAProcessorJson == null)
            {
                ABJAProcessorJson = new ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheel_sub1Struct>("Binary/TextContent/ResonanceWheel_sub1", "ResonanceWheel_sub1", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UI共鸣轮Panel_sub1数据");
                ABJAProcessorJson.StartLoadJsonAssetData();
            }

        }
        #endregion


    }

}
