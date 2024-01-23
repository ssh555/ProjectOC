using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using Newtonsoft.Json;
using ProjectOC.WorkerEchoNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting.Dependencies.Sqlite;
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
            //BeastInfo
            var Info1 = this.transform.Find("HiddenBeastInfo1").Find("Info");
            Stamina = Info1.Find("PhysicalStrength").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Speed = Info1.Find("MovingSpeed").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            StaminaNum = Info1.Find("PhysicalStrength").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            SpeedNum = Info1.Find("MovingSpeed").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            var GInfo = Info1.Find("SkillGraph").Find("Viewport").Find("Content").Find("Ring");
            Cook = GInfo.Find("Skill1").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            HandCraft = GInfo.Find("Skill6").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Industry = GInfo.Find("Skill5").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Magic = GInfo.Find("Skill4").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Transport = GInfo.Find("Skill3").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Collect = GInfo.Find("Skill2").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();

            
            var btn1 = this.transform.Find("HiddenBeastInfo2").Find("btn1");
            expel = new UIKeyTip();
            expel.keytip = btn1.Find("KeyTip").Find("Image").Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            expel.description = btn1.Find("KeyTip").Find("Image").Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            var btn2 = this.transform.Find("HiddenBeastInfo2").Find("btn2");
            receive = new UIKeyTip();
            receive.keytip = btn2.Find("KeyTip").Find("Image").Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            receive.description = btn2.Find("KeyTip").Find("Image").Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            //需要调接口显示的隐兽信息

            BeastName= Info1.Find("Icon").Find("Name").GetComponent<TMPro.TextMeshProUGUI>();




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
            parentUI.workerEcho.ExpelWorker(parentUI.CurrentGridIndex);

            //ui

            Debug.Log("Expel_performed!");
        }

        private void Receive_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            parentUI = GameObject.Find("Canvas").GetComponentInChildren<ResonanceWheelUI>();
            parentUI.workerEcho.SpawnWorker(parentUI.CurrentGridIndex);
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
        private TMPro.TextMeshProUGUI StaminaNum;
        private TMPro.TextMeshProUGUI SpeedNum;


        private TMPro.TextMeshProUGUI Cook;
        private TMPro.TextMeshProUGUI HandCraft;
        private TMPro.TextMeshProUGUI Industry;
        private TMPro.TextMeshProUGUI Magic;
        private TMPro.TextMeshProUGUI Transport;
        private TMPro.TextMeshProUGUI Collect;





        private UIKeyTip expel;
        private UIKeyTip receive;

         //需要调接口显示的隐兽信息
        private TMPro.TextMeshProUGUI BeastName;



        //BotKeyTips
        private UIKeyTip KT_Back;


        #endregion

        public void Refresh()
        {

            if (ResonanceWheelUI.ABJAProcessorJson_sub1 == null || !ResonanceWheelUI.ABJAProcessorJson_sub1.IsLoaded || !IsInit)
            {
                Debug.Log("ABJAProcessorJson is null");
                return;
            }


            //BeastInfo
            Stamina.text = ResonanceWheelUI.PanelTextContent_sub1.Stamina;
            Speed.text = ResonanceWheelUI.PanelTextContent_sub1.Speed;

            Cook.text = ResonanceWheelUI.PanelTextContent_sub1.Cook;
            HandCraft.text = ResonanceWheelUI.PanelTextContent_sub1.HandCraft;
            Industry.text = ResonanceWheelUI.PanelTextContent_sub1.Industry;
            Magic.text = ResonanceWheelUI.PanelTextContent_sub1.Magic;
            Transport.text = ResonanceWheelUI.PanelTextContent_sub1.Transport;
            Collect.text = ResonanceWheelUI.PanelTextContent_sub1.Collect;

            expel.ReWrite(ResonanceWheelUI.PanelTextContent_sub1.expel);
            receive.ReWrite(ResonanceWheelUI.PanelTextContent_sub1.receive);



            //更新隐兽详细信息
            Debug.Log("更新隐兽详细信息 " + parentUI.Grids[parentUI.CurrentGridIndex].worker.worker.Name);
            Worker worker = parentUI.Grids[parentUI.CurrentGridIndex].worker.worker;
            StaminaNum.text = worker.APMax.ToString();
            SpeedNum.text = worker.WalkSpeed.ToString();

            

            List<float> datas = new List<float>();
            // 烹饪
            datas.Add(worker.Skill[WorkType.Cook].Level / 10f);
            // 轻工
            datas.Add(worker.Skill[WorkType.HandCraft].Level / 10f);
            // 精工
            datas.Add(worker.Skill[WorkType.Industry].Level / 10f);
            // 术法
            datas.Add(worker.Skill[WorkType.Magic].Level / 10f);
            // 搬运
            datas.Add(worker.Skill[WorkType.Transport].Level / 10f);
            // 采集
            datas.Add(worker.Skill[WorkType.Collect].Level / 10f);

            var radar = this.transform.Find("HiddenBeastInfo1").Find("Info").Find("SkillGraph").Find("Viewport").Find("Content").Find("Radar").GetComponent<UIPolygon>();
            radar.DrawPolygon(datas);

            
            BeastName.text = worker.Name;


            foreach (var feature in worker.Features)
            {
                var Info = this.transform.Find("HiddenBeastInfo2").Find("Info");
                var DescriptionPrefab = Instantiate(parentUI.PrefabsAB.LoadAsset<GameObject>("Description"), Info);
                DescriptionPrefab.transform.Find("Text1").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Name;
                DescriptionPrefab.transform.Find("Text2").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Description;
                DescriptionPrefab.transform.Find("Text3").GetComponent<TMPro.TextMeshProUGUI>().text = feature.EffectsDescription;
            }
            //BotKeyTips
            KT_Back.ReWrite(ResonanceWheelUI.PanelTextContent_sub1.back);
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

        #endregion
        



    }

}
