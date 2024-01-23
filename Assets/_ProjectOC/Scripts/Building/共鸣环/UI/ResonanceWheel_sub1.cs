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
        /// ����Drop��Destroy������ӦCancel
        /// ������Ӧ��Destroy����Ϊtrue
        /// Cancel�Ͳ���ӦDrop �� ����
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

            //��Ҫ���ӿ���ʾ��������Ϣ

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
 

   
        private int CurrentFuctionTypeIndex = 0;//0ΪHBR 1ΪSSB
        private int CurrentGridIndex = 0;//0��4


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




            //����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Expel.performed -= Expel_performed;

            //����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Receive.performed -= Receive_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            

        }

        private void RegisterInput()
        {
            


            //����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Expel.performed += Expel_performed;

            //����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Receive.performed += Receive_performed;

            // ����
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

        #region UI��������
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

         //��Ҫ���ӿ���ʾ��������Ϣ
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



            //����������ϸ��Ϣ
            Debug.Log("����������ϸ��Ϣ " + parentUI.Grids[parentUI.CurrentGridIndex].worker.worker.Name);
            Worker worker = parentUI.Grids[parentUI.CurrentGridIndex].worker.worker;
            StaminaNum.text = worker.APMax.ToString();
            SpeedNum.text = worker.WalkSpeed.ToString();

            

            List<float> datas = new List<float>();
            // ���
            datas.Add(worker.Skill[WorkType.Cook].Level / 10f);
            // �Ṥ
            datas.Add(worker.Skill[WorkType.HandCraft].Level / 10f);
            // ����
            datas.Add(worker.Skill[WorkType.Industry].Level / 10f);
            // ����
            datas.Add(worker.Skill[WorkType.Magic].Level / 10f);
            // ����
            datas.Add(worker.Skill[WorkType.Transport].Level / 10f);
            // �ɼ�
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
