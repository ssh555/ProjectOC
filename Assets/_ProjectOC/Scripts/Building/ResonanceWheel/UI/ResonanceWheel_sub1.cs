using ML.Engine.Input;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.WorkerEchoNS;
using ProjectOC.WorkerNS;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class ResonanceWheel_sub1 : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;

        private void Awake()
        {
            InitUITexture2D();

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

            //��Ҫ���ӿ���ʾ��������Ϣ

            BeastName = Info1.Find("Icon").Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
        }
        protected override void Start()
        {
            //KeyTips
            UIKeyTipComponents = this.transform.GetComponentsInChildren<UIKeyTipComponent>(true);
            foreach (var item in UIKeyTipComponents)
            {
                uiKeyTipDic.Add(item.InputActionName, item);
            }

            IsInit = true;
            Refresh();
            base.Start();
        }

        private List<AsyncOperationHandle> descriptionHandle = new List<AsyncOperationHandle>();
        private AsyncOperationHandle spriteatlasHandle;
        private void OnDestroy()
        {
            var abmgr = GameManager.Instance.ABResourceManager;
            foreach(var handle in descriptionHandle)
            {
                abmgr.ReleaseInstance(handle);
            }
            GameManager.DestroyObj(icon_genderfemaleSprite);
            GameManager.DestroyObj(icon_gendermaleSprite);
            abmgr.Release(spriteatlasHandle);
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




            //����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Expel.performed -= Expel_performed;

            //����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Receive.performed -= Receive_performed;

            // ����
            //ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            

        }

        private void RegisterInput()
        {
            


            //����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Expel.performed += Expel_performed;

            //����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Receive.performed += Receive_performed;

            // ����
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
        private Sprite icon_genderfemaleSprite, icon_gendermaleSprite;

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

        #region UI��������
        private UIKeyTipComponent[] UIKeyTipComponents;

        public ResonanceWheelUI parentUI;
        //BeastInfo
        private TMPro.TextMeshProUGUI Stamina;
        private TMPro.TextMeshProUGUI Speed;
        private TMPro.TextMeshProUGUI StaminaNum;
        private TMPro.TextMeshProUGUI SpeedNum;

        private Image GenderImage;


        private BeastSkill[] beastSkills;

         //��Ҫ���ӿ���ʾ��������Ϣ
        private TMPro.TextMeshProUGUI BeastName;

        #endregion

        public override void Refresh()
        {
            if (this.parentUI.ABJAProcessorJson_sub1 == null || !this.parentUI.ABJAProcessorJson_sub1.IsLoaded || !IsInit)
            {
                return;
            }

            if (UikeyTipIsInit == false)
            {
                KeyTip[] keyTips = inputManager.ExportKeyTipValues(this.parentUI.ABJAProcessorJson_sub1);
                foreach (var keyTip in keyTips)
                {
                    InputAction inputAction = inputManager.GetInputAction((keyTip.keymap.ActionMapName, keyTip.keymap.ActionName));
                    inputManager.GetInputActionBindText(inputAction);

                    UIKeyTipComponent uIKeyTipComponent = uiKeyTipDic[keyTip.keyname];
                    if (uIKeyTipComponent.keytip != null)
                    {
                        uIKeyTipComponent.keytip.text = inputManager.GetInputActionBindText(inputAction);
                    }
                    if (uIKeyTipComponent.description != null)
                    {
                        uIKeyTipComponent.description.text = keyTip.description.GetText();
                    }

                }
                UikeyTipIsInit = true;
            }

            //BeastInfo
            foreach (TextTip tp in this.parentUI.PanelTextContent_sub1.SkillType)
            {
                for (int i = 0; i < beastSkills.Length; i++) 
                {
                    if(tp.name == beastSkills[i].workType.ToString())
                    {
                        beastSkills[i].skillText.text = tp.GetDescription();
                    }
                }
            }

            //����������ϸ��Ϣ
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

                //�Ա�
                if (worker.Gender == Gender.Male)
                {
                    GenderImage.sprite = icon_gendermaleSprite;
                }
                else
                {
                    GenderImage.sprite = icon_genderfemaleSprite;
                }


                BeastName.text = worker.Name;

                var Info = this.transform.Find("HiddenBeastInfo2").Find("Info").Find("Scroll View").Find("Viewport").Find("Content");
                for (int i = 0; i < Info.childCount; i++)
                {
                    ML.Engine.Manager.GameManager.DestroyObj(Info.GetChild(i).gameObject);
                }

                foreach (var feature in worker.Features)
                {
                    GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UI/ResonanceWheel/Prefabs/Description.prefab", Info).Completed += (handle) =>
                        {
                            this.descriptionHandle.Add(handle);
                            var descriptionPrefab = handle.Result;
                            descriptionPrefab.transform.Find("Text1").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Name;
                            descriptionPrefab.transform.Find("Text2").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Description;
                            descriptionPrefab.transform.Find("Text3").GetComponent<TMPro.TextMeshProUGUI>().text =
                            "<color=#6FB502><b><sprite name=\"Triangle\" index=0 tint=1>" + feature.EffectsDescription + "</b></color>";

                        };
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

        #region Texture2D
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        private string ResonanceWheelSpriteAtlasPath = "OC/UI/ResonanceWheel/Texture/SA_ResonanceWheel_UI.spriteatlasv2";
        private void InitUITexture2D()
        {
            GM.ABResourceManager.LoadAssetAsync<SpriteAtlas>(ResonanceWheelSpriteAtlasPath).Completed += (handle) =>
            {
                spriteatlasHandle = handle;
                var atlas = handle.Result as SpriteAtlas;
                icon_genderfemaleSprite = atlas.GetSprite("icon_genderfemale");
                icon_gendermaleSprite = atlas.GetSprite("icon_gendermale");
            };
        }
        #endregion
    }

}
