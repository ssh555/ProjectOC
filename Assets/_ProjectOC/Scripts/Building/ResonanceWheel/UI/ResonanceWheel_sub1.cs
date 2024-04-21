using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ML.Engine.Utility;
using ProjectOC.ManagerNS;
using ProjectOC.WorkerEchoNS;
using ProjectOC.WorkerNS;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheel_sub1;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class ResonanceWheel_sub1 : ML.Engine.UI.UIBasePanel<ResonanceWheel_sub1Struct>
    {
        #region Unity
        public bool IsInit = false;

        protected override void Awake()
        {
            base.Awake();

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

            BeastName = Info1.Find("Icon").Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
        }
        protected override void Start()
        {
            IsInit = true;
            Refresh();
            base.Start();
        }

        private List<AsyncOperationHandle> descriptionHandle = new List<AsyncOperationHandle>();
        private AsyncOperationHandle spriteatlasHandle;
        protected override void OnDestroy()
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
            parentUI.MainToSub1();
        }

        public override void OnExit()
        {
            base.OnExit();
            ClearTemp();
            parentUI.Sub1ToMain();
        }
        #endregion

        #region Internal
        private struct BeastSkill
        {
            public WorkType workType;
            public TMPro.TextMeshProUGUI skillText;
        }

        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Disable();
            //驱逐
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Expel.performed -= Expel_performed;

            //收留
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Receive.performed -= Receive_performed;

        }

        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Enable();
            //驱逐
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Expel.performed += Expel_performed;

            //收留
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Receive.performed += Receive_performed;
        }

        private void Expel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            parentUI.workerEcho.ExpelWorker(parentUI.CurrentGridIndex);

            //ui
            ResonanceWheelUI.RingGrid.Reset(parentUI.Grids[parentUI.CurrentGridIndex]);
            
            UIMgr.PopPanel();
        }

        private void Receive_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            parentUI.workerEcho.SpawnWorker(parentUI.CurrentGridIndex);
            
            ResonanceWheelUI.RingGrid.Reset(parentUI.Grids[parentUI.CurrentGridIndex]);
            UIMgr.PopPanel();
        }
        #endregion

        #region UI
        #region Temp
        private Sprite icon_genderfemaleSprite, icon_gendermaleSprite;

        private void ClearTemp()
        {
            GameManager.DestroyObj(icon_genderfemaleSprite);
            GameManager.DestroyObj(icon_gendermaleSprite);
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

         //需要调接口显示的隐兽信息
        private TMPro.TextMeshProUGUI BeastName;

        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }

            //BeastInfo
            foreach (TextTip tp in PanelTextContent.SkillType)
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

        #region Resource
        #region TextContent
        [System.Serializable]
        public struct ResonanceWheel_sub1Struct
        {
            //BeastInfo
            public TextTip[] SkillType;

            public KeyTip Expel;
            public KeyTip Receive;

        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/ResonanceWheel";
            this.abname = "ResonanceWheel_sub1";
            this.description = "ResonanceWheel_sub1数据加载完成";
        }

        #endregion
        protected override void InitObjectPool()
        {
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Texture2D, "Texture2DPool", 1,
            "OC/UI/ResonanceWheel/Texture/SA_ResonanceWheel_UI.spriteatlasv2", (handle) =>
            {
                SpriteAtlas resonanceWheelAtlas = handle.Result as SpriteAtlas;
                icon_genderfemaleSprite = resonanceWheelAtlas.GetSprite("icon_genderfemale");
                icon_genderfemaleSprite = resonanceWheelAtlas.GetSprite("icon_gendermale");
            }
            );
            base.InitObjectPool();
        }
        #endregion
    }

}
