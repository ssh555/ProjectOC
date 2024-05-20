using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using ML.Engine.UI;
using ML.Engine.Utility;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
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
        protected override void Awake()
        {
            base.Awake();
            //BeastInfo
            this.Info1 = this.transform.Find("HiddenBeastInfo2").Find("Content").Find("Info1");
            var Content = Info1.Find("Content");
            APMax = Content.Find("APMax").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            APMaxNumText = Content.Find("APMax").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            MoodMax = Content.Find("MoodMax").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            MoodMaxNumText = Content.Find("MoodMax").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            WalkSpeed = Content.Find("WalkSpeed").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            WalkSpeedNumText = Content.Find("WalkSpeed").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();


            GenderImage = Info1.Find("Icon").Find("GenderImage").GetComponent<Image>();
            BeastName = Info1.Find("Icon").Find("Name").GetComponent<TMPro.TextMeshProUGUI>();

            this.SwitchInfo = this.Info1.Find("SwitchInfo");
            var GInfo = SwitchInfo.Find("SkillGraph").Find("Viewport").Find("Content").Find("Ring");
            beastSkills = new BeastSkill[GInfo.childCount];
            for (int i = 0; i < GInfo.childCount; i++)
            {
                beastSkills[i].skillText = GInfo.GetChild(i).Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
                beastSkills[i].workType = (SkillType)Enum.Parse(typeof(SkillType), GInfo.GetChild(i).name);
            }

            var GContent = SwitchInfo.Find("Content");
            this.CookEfficiency = GContent.Find("CookEfficiency").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            this.CookEfficiencyNumText = GContent.Find("CookEfficiency").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            this.CollectEfficiency = GContent.Find("CollectEfficiency").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            this.CollectEfficiencyNumText = GContent.Find("CollectEfficiency").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            this.MagicEfficiency = GContent.Find("MagicEfficiency").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            this.MagicEfficiencyNumText = GContent.Find("MagicEfficiency").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            this.HandCraftEfficiency = GContent.Find("HandCraftEfficiency").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            this.HandCraftEfficiencyNumText = GContent.Find("HandCraftEfficiency").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            this.IndustryEfficiency = GContent.Find("IndustryEfficiency").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            this.IndustryEfficiencyNumText = GContent.Find("IndustryEfficiency").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            this.WeightMax = GContent.Find("WeightMax").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            this.WeightMaxNumText = GContent.Find("WeightMax").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();

            this.Info2 = this.transform.Find("HiddenBeastInfo2").Find("Content").Find("Info2");
            DescriptionScrollView = Info2.Find("Scroll View").GetComponent<ScrollRect>();

            var KetTips = this.transform.Find("BotKeyTips").Find("KeyTips");
            this.KT_ViewMoreInformation = KetTips.Find("KT_ViewMoreInformation");
            this.KT_CancelMoreInformation = KetTips.Find("KT_CancelMoreInformation");
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
            public SkillType workType;
            public TMPro.TextMeshProUGUI skillText;
        }

        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Disable();
            //驱逐
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Expel.performed -= Expel_performed;

            //收留
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Receive.performed -= Receive_performed;

            ML.Engine.Input.InputManager.Instance.Common.Common.RT.performed -= RT_performed;

            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.started -= TurnPage_started;

            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.canceled -= TurnPage_canceled;

        }

        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Enable();
            //驱逐
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Expel.performed += Expel_performed;

            //收留
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub1.Receive.performed += Receive_performed;

            ML.Engine.Input.InputManager.Instance.Common.Common.RT.performed += RT_performed;

            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.started += TurnPage_started;

            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.canceled += TurnPage_canceled;
        }


        private void Expel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            parentUI.workerEcho.ExpelWorker(parentUI.CurrentGridIndex);

            //ui
            ResonanceWheelUI.RingGrid.Reset(parentUI.Grids[parentUI.CurrentGridIndex]);

            this.objectPool.ResetPool("SimpleDescriptionPool");
            this.objectPool.ResetPool("FullDescriptionPool");

            UIMgr.PopPanel();
        }

        private void Receive_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            parentUI.workerEcho.SpawnWorker(parentUI.CurrentGridIndex);
            
            ResonanceWheelUI.RingGrid.Reset(parentUI.Grids[parentUI.CurrentGridIndex]);
            UIMgr.PopPanel();
        }

        private void RT_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            SwitchInfoIndex = (SwitchInfoIndex + 1) % SwitchInfo.childCount;
            this.Refresh();
        }

        private float TimeInterval = 0.2f;
        private CounterDownTimer timer = null;
        private void TurnPage_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (timer == null)
            {
                timer = new CounterDownTimer(TimeInterval, true, true, 1, 2);
                timer.OnEndEvent += () =>
                {
                    var vector2 = obj.ReadValue<UnityEngine.Vector2>();
                    this.DescriptionScrollView.verticalScrollbar.value += vector2.y;
                };
            }
        }
        private void TurnPage_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(timer);
            timer = null;
        }
        #endregion

        #region UI
        #region Temp
        private Sprite icon_genderfemaleSprite, icon_gendermaleSprite;
        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();
        private void ClearTemp()
        {
            GameManager.DestroyObj(icon_genderfemaleSprite);
            GameManager.DestroyObj(icon_gendermaleSprite);
            // sprite
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s.Value);
            }
        }

        #endregion

        #region UI对象引用
        public ResonanceWheelUI parentUI;
        //BeastInfo
        private Image GenderImage;
        private TMPro.TextMeshProUGUI BeastName;

        private TMPro.TextMeshProUGUI APMax;
        private TMPro.TextMeshProUGUI APMaxNumText;
        private TMPro.TextMeshProUGUI MoodMax;
        private TMPro.TextMeshProUGUI MoodMaxNumText;
        private TMPro.TextMeshProUGUI WalkSpeed;
        private TMPro.TextMeshProUGUI WalkSpeedNumText;

        private TMPro.TextMeshProUGUI CookEfficiency;
        private TMPro.TextMeshProUGUI CookEfficiencyNumText;
        private TMPro.TextMeshProUGUI CollectEfficiency;
        private TMPro.TextMeshProUGUI CollectEfficiencyNumText;
        private TMPro.TextMeshProUGUI MagicEfficiency;
        private TMPro.TextMeshProUGUI MagicEfficiencyNumText;
        private TMPro.TextMeshProUGUI HandCraftEfficiency;
        private TMPro.TextMeshProUGUI HandCraftEfficiencyNumText;
        private TMPro.TextMeshProUGUI IndustryEfficiency;
        private TMPro.TextMeshProUGUI IndustryEfficiencyNumText;
        private TMPro.TextMeshProUGUI WeightMax;
        private TMPro.TextMeshProUGUI WeightMaxNumText;

        private Transform Info1;
        private Transform Info2;
        private Transform SwitchInfo;

        private Transform KT_ViewMoreInformation;
        private Transform KT_CancelMoreInformation;
        private ScrollRect DescriptionScrollView;


        private BeastSkill[] beastSkills;
        private int SwitchInfoIndex = 0;

        #endregion
        private FeatureManager featManager => ManagerNS.LocalGameManager.Instance.FeatureManager;
        public override void Refresh()
        {
            Debug.Log("Refresh "+ (ABJAProcessorJson == null)+" "+ ABJAProcessorJson.IsLoaded+" "+ isInitObjectPool);
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !isInitObjectPool)
            {
                return;
            }
            Debug.Log("asda Refresh");
            foreach (TextTip tp in PanelTextContent.SkillType)
            {
                for (int i = 0; i < beastSkills.Length; i++)
                {
                    if (tp.name == beastSkills[i].workType.ToString())
                    {
                        beastSkills[i].skillText.text = tp.GetDescription();
                    }
                }
            }

            WorkerEcho workerEcho = parentUI.workerEcho;
            Worker worker = workerEcho.GetExternWorkers()[parentUI.CurrentGridIndex]?.Worker;

            if(worker != null)
            {
                for (int i = 0; i < SwitchInfo.childCount; i++)
                {
                    SwitchInfo.GetChild(i).gameObject.SetActive(i == SwitchInfoIndex);
                }
                Dictionary<SkillType, Skill> skillDic = worker.Skill;
                if (SwitchInfoIndex == 0)
                {
                    List<float> datas = new List<float>();

                    datas.Add(skillDic[SkillType.Collect].LevelCurrent / 10f);
                    datas.Add(skillDic[SkillType.Transport].LevelCurrent / 10f);
                    datas.Add(skillDic[SkillType.Magic].LevelCurrent / 10f);
                    datas.Add(skillDic[SkillType.Industry].LevelCurrent / 10f);
                    datas.Add(skillDic[SkillType.HandCraft].LevelCurrent / 10f);
                    datas.Add(skillDic[SkillType.Cook].LevelCurrent / 10f);
                    var radar = SwitchInfo.Find("SkillGraph").Find("Viewport").Find("Content").Find("Radar").GetComponent<UIPolygon>();
                    radar.DrawPolygon(datas);
                }
                else if (SwitchInfoIndex == 1)
                {
                    this.CookEfficiencyNumText.text = skillDic[SkillType.Cook].LevelCurrent.ToString();
                    this.CollectEfficiencyNumText.text = skillDic[SkillType.Collect].LevelCurrent.ToString();
                    this.MagicEfficiencyNumText.text = skillDic[SkillType.Magic].LevelCurrent.ToString();
                    this.HandCraftEfficiencyNumText.text = worker.GetEff(SkillType.HandCraft).ToString();
                    this.IndustryEfficiencyNumText.text = skillDic[SkillType.Industry].LevelCurrent.ToString();
                    this.WeightMaxNumText.text = worker.RealBURMax.ToString();
                }


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

                APMaxNumText.text = worker.APMax.ToString();
                MoodMax.text = worker.EMMax.ToString();
                WalkSpeedNumText.text = worker.RealWalkSpeed.ToString();

                this.objectPool.ResetPool("DescriptionPool");
                Debug.Log("asda " + worker.GetFeatures());
                foreach (var feature in worker.GetFeatures())
                {
                    var tPrefab = this.objectPool.GetNextObject("DescriptionPool");
                    Debug.Log(tPrefab);
                    var Normal = tPrefab.transform.Find("Normal");
                    var Specific = tPrefab.transform.Find("Specific");
                    string featID = feature.ID;
                    Normal.gameObject.SetActive(SwitchInfoIndex == 0);
                    Specific.gameObject.SetActive(SwitchInfoIndex == 1);
                    Transform feat = SwitchInfoIndex == 1 ? tPrefab.transform.Find("Specific") : tPrefab.transform.Find("Normal");
                    if (!string.IsNullOrEmpty(featID))
                    {
                        string iconName = featManager.GetIcon(featID);
                        feat.Find("Icon").gameObject.SetActive(!string.IsNullOrEmpty(iconName));
                        if (!string.IsNullOrEmpty(iconName))
                        {
                            if (!tempSprite.ContainsKey(iconName))
                            {
                                tempSprite.Add(iconName, ManagerNS.LocalGameManager.Instance.WorkerManager.GetSprite(iconName));
                            }
                            feat.Find("Icon").GetComponent<Image>().sprite = tempSprite[iconName];
                        }
                        feat.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = featManager.GetName(featID);
                        if (SwitchInfoIndex == 1)
                        {
                            feat.Find("Desc").GetComponent<TMPro.TextMeshProUGUI>().text = featManager.GetItemDescription(featID);
                            feat.Find("DescIcon").gameObject.SetActive(true);
                            feat.Find("DescIcon").GetComponent<Image>().color = featManager.GetColorForUI(featID);
                            feat.Find("EffectDesc").GetComponent<TMPro.TextMeshProUGUI>().text =
                                $"<color={featManager.GetColorStrForUI(featID)}>" + featManager.GetEffectsDescription(featID) + "</color>";
                        }
                        DescriptionList.AddBtn(tPrefab);
                    }
                }
                #region KeyTip
                KT_ViewMoreInformation.gameObject.SetActive(SwitchInfoIndex == 0);
                KT_CancelMoreInformation.gameObject.SetActive(SwitchInfoIndex == 1);
                #endregion
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
            this.abpath = "OCTextContent/ResonanceWheel";
            this.abname = "ResonanceWheel_sub1";
            this.description = "ResonanceWheel_sub1数据加载完成";
        }

        #endregion
        private bool isInitObjectPool = false;
        protected override void InitObjectPool()
        {
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Texture2D, "Texture2DPool", 1,
            "SA_ResonanceWheel_UI", (handle) =>
            {
                string Pre = "Tex2D_Resonance_UI_";
                SpriteAtlas resonanceWheelAtlas = handle.Result as SpriteAtlas;
                icon_genderfemaleSprite = resonanceWheelAtlas.GetSprite(Pre + "icon_genderfemale");
                icon_gendermaleSprite = resonanceWheelAtlas.GetSprite(Pre + "icon_gendermale");
            }
            );

            UIBtnList.Synchronizer synchronizer = new UIBtnList.Synchronizer(1, () => { isInitObjectPool = true; });
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "DescriptionPool", 5, "Prefab_Worker_UI/Prefab_Worker_UI_FeatureTemplate.prefab", OnCompleted: (hanlde) => { synchronizer.Check(); });
            base.InitObjectPool();
        }
        [ShowInInspector]
        private UIBtnList DescriptionList;
        protected override void InitBtnInfo()
        {
            DescriptionList = new UIBtnList(this.Info2.GetComponentInChildren<UIBtnListInitor>());
        }
        #endregion
    }

}
