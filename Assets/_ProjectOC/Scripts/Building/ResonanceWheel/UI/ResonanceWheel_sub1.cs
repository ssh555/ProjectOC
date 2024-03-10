using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.Input;
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
using UnityEngine.InputSystem;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
using UnityEngine.U2D;
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

            //KeyTips
            UIKeyTipComponents = this.transform.GetComponentsInChildren<UIKeyTipComponent>(true);
            foreach (var item in UIKeyTipComponents)
            {
                item.InitData();
                uiKeyTipDic.Add(item.InputActionName, item);
            }

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

            //需要调接口显示的隐兽信息

            BeastName= Info1.Find("Icon").Find("Name").GetComponent<TMPro.TextMeshProUGUI>();

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
            UikeyTipIsInit = false;
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
        private Dictionary<string, UIKeyTipComponent> uiKeyTipDic = new Dictionary<string, UIKeyTipComponent>();
        private bool UikeyTipIsInit;
        private InputManager inputManager => GameManager.Instance.InputManager;

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
            uiKeyTipDic = null;
        }

        #endregion

        #region UI对象引用
        private UIKeyTipComponent[] UIKeyTipComponents;

        public ResonanceWheelUI parentUI;
        //BeastInfo
        private TMPro.TextMeshProUGUI Stamina;
        private TMPro.TextMeshProUGUI Speed;
        private TMPro.TextMeshProUGUI StaminaNum;
        private TMPro.TextMeshProUGUI SpeedNum;

        private Image GenderImage;


        private BeastSkill[] beastSkills;

        private GameObject DescriptionPrefab;//预制体

         //需要调接口显示的隐兽信息
        private TMPro.TextMeshProUGUI BeastName;

        #endregion

        public override void Refresh()
        {

            if (ResonanceWheelUI.ABJAProcessorJson_sub1 == null || !ResonanceWheelUI.ABJAProcessorJson_sub1.IsLoaded || !IsInit)
            {
                Debug.Log("ABJAProcessorJson is null");
                return;
            }

            if (UikeyTipIsInit == false)
            {
                KeyTip[] keyTips = inputManager.ExportKeyTipValues(ResonanceWheelUI.PanelTextContent_sub1);
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

            //更新隐兽详细信息
            WorkerEcho workerEcho = parentUI.workerEcho;


/*            Stamina.text = (ResonanceWheelUI.PanelTextContent_sub1.SkillType.Where(tag => tag.name == "Stamina") as TextTip).GetDescription();
            Speed.text = (ResonanceWheelUI.PanelTextContent_sub1.SkillType.Where(tag => tag.name == "Speed") as TextTip).GetDescription();*/

            Worker worker = workerEcho.GetExternWorkers()[parentUI.CurrentGridIndex]?.worker;
            if (worker != null)
            {
                StaminaNum.text = worker.APMax.ToString();
                SpeedNum.text = worker.WalkSpeed.ToString();

                List<float> datas = new List<float>();

                Dictionary<WorkType, Skill> skillDic = worker.Skill;
                foreach (var skill in skillDic)
                {
                    datas.Add(skillDic[skill.Key].Level / 10f);
                }

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
        }
        #endregion



        #region TextContent
        [System.Serializable]
        public struct ResonanceWheel_sub1Struct
        {
            //BeastInfo
            public TextTip[] SkillType;


            public KeyTip Expel;
            public KeyTip Receive;

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

            SpriteAtlas atlas = Texture2DAB.LoadAsset<SpriteAtlas>("SA_ResonanceWheel_UI");
            icon_genderfemaleSprite = atlas.GetSprite("icon_genderfemale");
            icon_gendermaleSprite = atlas.GetSprite("icon_gendermale");


        }

        #endregion



    }

}
