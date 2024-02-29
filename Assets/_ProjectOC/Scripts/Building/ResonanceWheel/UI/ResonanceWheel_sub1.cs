using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using Newtonsoft.Json;
using ProjectOC.Player;
using ProjectOC.ProNodeNS;
using ProjectOC.WorkerEchoNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class ResonanceWheel_sub1 : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;
        private void Start()
        {
            //parentUI = GameObject.Find("Canvas").GetComponentInChildren<ResonanceWheelUI>();
            StartCoroutine(InitUIPrefabs());
            StartCoroutine(InitUITexture2D());


            //BeastInfo
            var Info1 = this.transform.Find("HiddenBeastInfo1").Find("Info");
            Stamina = Info1.Find("PhysicalStrength").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Speed = Info1.Find("MovingSpeed").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            StaminaNum = Info1.Find("PhysicalStrength").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            SpeedNum = Info1.Find("MovingSpeed").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();

            GenderImage = Info1.Find("Icon").Find("GenderImage").GetComponent<Image>();


            var GInfo = Info1.Find("SkillGraph").Find("Viewport").Find("Content").Find("Ring");
            beastSkills = new BeastSkill[GInfo.childCount];


            for (int i = 0; i < GInfo.childCount; i++)
            {
                beastSkills[i].skillText = GInfo.GetChild(i).Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
                beastSkills[i].workType = (WorkType)Enum.Parse(typeof(WorkType), GInfo.GetChild(i).name);
            }


            
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
            parentUI.MainToSub1();
        }

        public override void OnExit()
        {
            base.OnExit();
            this.Exit();
            ClearTemp();
            parentUI.Sub1ToMain();
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

         private struct BeastSkill
        {
            public WorkType workType;
            public TMPro.TextMeshProUGUI skillText;
        
        }





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
            //ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            

        }

        private void RegisterInput()
        {
            


            //驱逐
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Expel.performed += Expel_performed;

            //收留
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Receive.performed += Receive_performed;

            // 返回
            //ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }





        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            
            UIMgr.PopPanel();
        }



        private void Expel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //parentUI = GameObject.Find("Canvas").GetComponentInChildren<ResonanceWheelUI>();
            parentUI.workerEcho.ExpelWorker(parentUI.CurrentGridIndex);

            //ui
            ResonanceWheelUI.RingGrid.Reset(parentUI.Grids[parentUI.CurrentGridIndex]);
            
            UIMgr.PopPanel();
        }

        private void Receive_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //parentUI = GameObject.Find("Canvas").GetComponentInChildren<ResonanceWheelUI>();
            parentUI.workerEcho.SpawnWorker(parentUI.CurrentGridIndex, Vector3.zero);
            
            ResonanceWheelUI.RingGrid.Reset(parentUI.Grids[parentUI.CurrentGridIndex]);
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
        //BeastInfo
        private TMPro.TextMeshProUGUI Stamina;
        private TMPro.TextMeshProUGUI Speed;
        private TMPro.TextMeshProUGUI StaminaNum;
        private TMPro.TextMeshProUGUI SpeedNum;

        private Image GenderImage;


        private BeastSkill[] beastSkills;



        private GameObject DescriptionPrefab;//预制体


        private UIKeyTip expel;
        private UIKeyTip receive;

         //需要调接口显示的隐兽信息
        private TMPro.TextMeshProUGUI BeastName;



        //BotKeyTips
        private UIKeyTip KT_Back;


        #endregion

        public override void Refresh()
        {

            if (ResonanceWheelUI.ABJAProcessorJson_sub1 == null || !ResonanceWheelUI.ABJAProcessorJson_sub1.IsLoaded || !IsInit)
            {
                Debug.Log("ABJAProcessorJson is null");
                return;
            }


            //BeastInfo

            foreach (TextTip tp in ResonanceWheelUI.PanelTextContent_sub1.SkillType)
            {


                for (int i = 0; i < beastSkills.Length; i++) 
                {
                    if(tp.name == beastSkills[i].workType.ToString())
                    {
                        beastSkills[i].skillText.text = tp.GetDescription();
                    }
                }


            }

            


            
            expel.ReWrite(ResonanceWheelUI.PanelTextContent_sub1.expel);
            receive.ReWrite(ResonanceWheelUI.PanelTextContent_sub1.receive);



            //更新隐兽详细信息
            WorkerEcho workerEcho = (GameObject.Find("PlayerCharacter").GetComponent<PlayerCharacter>().interactComponent.CurrentInteraction as WorkerEchoBuilding).workerEcho;



            Worker worker = workerEcho.GetExternWorkers()[parentUI.CurrentGridIndex]?.worker;
            if (worker != null)
            {
                StaminaNum.text = worker.APMax.ToString();
                SpeedNum.text = worker.WalkSpeed.ToString();



                List<float> datas = new List<float>
                {
                                // 烹饪
                    worker.Skill[WorkType.Cook].Level / 10f,
                    // 轻工
                    worker.Skill[WorkType.HandCraft].Level / 10f,
                    // 精工
                    worker.Skill[WorkType.Industry].Level / 10f,
                    // 术法
                    worker.Skill[WorkType.Magic].Level / 10f,
                    // 搬运
                    worker.Skill[WorkType.Transport].Level / 10f,
                    // 采集
                    worker.Skill[WorkType.Collect].Level / 10f
                    //0.2f,0.3f,0.5f,0.7f,0.8f,0.1f
                };

                var radar = this.transform.Find("HiddenBeastInfo1").Find("Info").Find("SkillGraph").Find("Viewport").Find("Content").Find("Radar").GetComponent<UIPolygon>();
                radar.DrawPolygon(datas);

                //性别
                if (worker.Gender == Gender.Male)
                {
                    GenderImage.sprite = icon_gendermaleSprite;
                }
                else
                {
                    GenderImage.sprite = icon_genderfemaleSprite;
                }


                BeastName.text = worker.Name;


                if (this.PrefabsAB == null) return;

                var Info = this.transform.Find("HiddenBeastInfo2").Find("Info").Find("Scroll View").Find("Viewport").Find("Content");
                for (int i = 0; i < Info.childCount; i++)
                {
                    Destroy(Info.GetChild(i).gameObject);
                }

                foreach (var feature in worker.Features)
                {

                    var descriptionPrefab = Instantiate(DescriptionPrefab, Info);
                    descriptionPrefab.transform.Find("Text1").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Name;
                    descriptionPrefab.transform.Find("Text2").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Description;
                    descriptionPrefab.transform.Find("Text3").GetComponent<TMPro.TextMeshProUGUI>().text =
                    "<color=#6FB502><b><sprite name=\"Triangle\" index=0 tint=1>" + feature.EffectsDescription + "</b></color>";
                }
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
            public TextTip[] SkillType;


            public KeyTip expel;
            public KeyTip receive;

            //BotKeyTips
            public KeyTip back;
        }

        #endregion

        #region Prefab
        public AssetBundle PrefabsAB;
        public IEnumerator InitUIPrefabs()
        {
            this.PrefabsAB = null;
            var abmgr = GameManager.Instance.ABResourceManager;
            // 载入 keyTipPrefab
            var crequest = abmgr.LoadLocalABAsync("ui/resonancewheel/resonancewheelprefabs", null, out var PrefabsAB);
            
            if (crequest != null)
            {
                yield return crequest;
                PrefabsAB = crequest.assetBundle;
            }

            this.PrefabsAB = PrefabsAB;
            DescriptionPrefab = this.PrefabsAB.LoadAsset<GameObject>("Description");
            this.Refresh();
        }
        #endregion
        #region temp
        public Sprite icon_genderfemaleSprite, icon_gendermaleSprite;
        #endregion

        #region Texture2D
        public static AssetBundle Texture2DAB;
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        private string ResonanceWheelTexture2DPath = "ui/resonancewheel/texture2d";
        private IEnumerator InitUITexture2D()
        {

            var crequest = GM.ABResourceManager.LoadLocalABAsync(ResonanceWheelTexture2DPath, null, out var Texture2DAB);
            
            if (crequest != null)
            {
                yield return crequest;
                Texture2DAB = crequest.assetBundle;
            }
            Texture2D texture2D;

            texture2D = Texture2DAB.LoadAsset<Texture2D>("icon_genderfemale");
            icon_genderfemaleSprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            texture2D = Texture2DAB.LoadAsset<Texture2D>("icon_gendermale");
            icon_gendermaleSprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));



        }

        #endregion



    }

}
