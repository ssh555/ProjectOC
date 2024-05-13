using ML.Engine.Input;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;
using System;
using static ProjectOC.ResonanceWheelSystem.UI.BeastPanel;
using ML.Engine.Utility;
using static ML.Engine.UI.UIBtnListContainer;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using System.Drawing;
using Unity.Burst.CompilerServices;
using UnityEngine.Purchasing;
using Sirenix.Utilities;
using TMPro;
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class BeastPanel : ML.Engine.UI.UIBasePanel<BeastPanelStruct>, ITickComponent
    {
        #region Unity
        public bool IsInit = false;

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
            Cook = GInfo.Find("Cook").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            HandCraft = GInfo.Find("HandCraft").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Industry = GInfo.Find("Industry").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Magic = GInfo.Find("Magic").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Transport = GInfo.Find("Transport").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Collect = GInfo.Find("Collect").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();

            var GContent = SwitchInfo.Find("Content");
            this.CookEfficiency = GContent.Find("CookEfficiency").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            this.CookEfficiencyNumText = GContent.Find("CookEfficiency").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            this.CollectEfficiency = GContent.Find("CollectEfficiency").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            this.CollectEfficiencyNumText = GContent.Find("CollectEfficiency").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            this.MagicEfficiency = GContent.Find("MagicEfficiency").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            this.MagicEfficiencyNumText = GContent.Find("MagicEfficiency").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            this.IndustryEfficiency = GContent.Find("IndustryEfficiency").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            this.IndustryEfficiencyNumText = GContent.Find("IndustryEfficiency").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            this.WeightMax = GContent.Find("WeightMax").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            this.WeightMaxNumText = GContent.Find("WeightMax").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();

            this.Info2 = this.transform.Find("HiddenBeastInfo2").Find("Content").Find("Info2");
            DescriptionScrollView = Info2.Find("Scroll View").GetComponent<ScrollRect>();
            this.TimerUI = Info2.Find("Timer").Find("CountNumber").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            var KetTips = this.transform.Find("BotKeyTips").Find("KeyTips");
            this.KT_ViewMoreInformation = KetTips.Find("KT_ViewMoreInformation");
            this.KT_CancelMoreInformation = KetTips.Find("KT_CancelMoreInformation");
        }
        protected override void Start()
        {
            IsInit = true;
            Refresh();
            base.Start();
        }

        #endregion

        #region Override
        protected override void Enter()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            base.Enter();
        }
        protected override void Exit()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
            base.Exit();
            ClearTemp();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.BeastList.EnableBtnList();
            this.ScheduleList.EnableBtnList();
        }

        public override void OnExit()
        {
            base.OnExit();
            this.BeastList.DisableBtnList();
            this.ScheduleList.DisableBtnList();
        }

        
        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Disable();

            //驱逐
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Expel.performed -= Expel_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            ML.Engine.Input.InputManager.Instance.Common.Common.RT.performed -= RT_performed;

            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.started -= TurnPage_started;

            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.canceled -= TurnPage_canceled;

            this.BeastList.RemoveAllListener();
            this.BeastList.DeBindInputAction();
        }
        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Enable();

            // 驱逐
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Expel.performed += Expel_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

            ML.Engine.Input.InputManager.Instance.Common.Common.RT.performed += RT_performed;

            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.started += TurnPage_started;

            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.canceled += TurnPage_canceled;

            this.BeastList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
        }

        private void Expel_performed(InputAction.CallbackContext obj)
        {
            LocalGameManager.Instance.WorkerManager.DeleteWorker(Workers[CurrentBeastIndex]);
            this.BeastList.DeleteButton(CurrentBeastIndex, () => { this.Refresh(); });
            
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
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
            #region temp
        private Sprite icon_genderfemaleSprite, icon_gendermaleSprite;

        private void ClearTemp()
        {
            GameManager.DestroyObj(icon_genderfemaleSprite);
            GameManager.DestroyObj(icon_gendermaleSprite);
        }

        #endregion

        #region UI对象引用
        [ShowInInspector]
        private int CurrentBeastIndex { get { return this.BeastList.GetCurSelectedPos1(); } }

        [ShowInInspector]
        List<Worker> Workers = new List<Worker>();

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
        private TMPro.TextMeshProUGUI IndustryEfficiency;
        private TMPro.TextMeshProUGUI IndustryEfficiencyNumText;
        private TMPro.TextMeshProUGUI WeightMax;
        private TMPro.TextMeshProUGUI WeightMaxNumText;

        private Transform Info1;
        private Transform Info2;
        private Transform SwitchInfo;
        private TMPro.TextMeshProUGUI TimerUI;

        private TMPro.TextMeshProUGUI Cook;
        private TMPro.TextMeshProUGUI HandCraft;
        private TMPro.TextMeshProUGUI Industry;
        private TMPro.TextMeshProUGUI Magic;
        private TMPro.TextMeshProUGUI Transport;
        private TMPro.TextMeshProUGUI Collect;

        private ScrollRect DescriptionScrollView;


        private Transform KT_ViewMoreInformation;
        private Transform KT_CancelMoreInformation;

        private int SwitchInfoIndex = 0;

        #endregion

        #region Tick
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void Tick(float deltatime)
        {
            Workers = LocalGameManager.Instance.WorkerManager.GetWorkers();
            if(CurrentBeastIndex >= 0 && CurrentBeastIndex < Workers.Count)
            {
                Worker worker = Workers[CurrentBeastIndex];
                TimerUI.text = worker.MinSec.Item1.ToString() + "Min" + worker.MinSec.Item2.ToString() + "s";
            }
        }

        protected override void OnDestroy()
        {
            (this as ITickComponent).DisposeTick();
        }

        #endregion

        public override void Refresh()
        {
            if (this.ABJAProcessorJson == null || !this.ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }

            Workers = LocalGameManager.Instance.WorkerManager.GetWorkers();
            #region BeastInfo
            if (CurrentBeastIndex != -1)
            {

                this.transform.Find("HiddenBeastInfo2").Find("Content").gameObject.SetActive(true);

                WalkSpeed.text = this.PanelTextContent.Speed;

                Cook.text = this.PanelTextContent.Cook;
                HandCraft.text = this.PanelTextContent.HandCraft;
                Industry.text = this.PanelTextContent.Industry;
                Magic.text = this.PanelTextContent.Magic;
                Transport.text = this.PanelTextContent.Transport;
                Collect.text = this.PanelTextContent.Collect;

                Worker worker = Workers[CurrentBeastIndex];
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
                else if(SwitchInfoIndex == 1)
                {
                    this.CookEfficiencyNumText.text = skillDic[SkillType.Cook].LevelCurrent.ToString();
                    this.CollectEfficiencyNumText.text = skillDic[SkillType.Collect].LevelCurrent.ToString();
                    this.MagicEfficiencyNumText.text = skillDic[SkillType.Magic].LevelCurrent.ToString();
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

                this.objectPool.ResetPool("SimpleDescriptionPool");
                this.objectPool.ResetPool("FullDescriptionPool");
                foreach (var feature in worker.GetSortFeature())
                {
                    if(SwitchInfoIndex == 0)
                    {
                        var tPrefab = this.objectPool.GetNextObject("SimpleDescriptionPool");
                        tPrefab.transform.Find("Text1").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Name;
                        DescriptionList.AddBtn(tPrefab);
                    }
                    else if(SwitchInfoIndex == 1)
                    {
                        var tPrefab = this.objectPool.GetNextObject("FullDescriptionPool");
                        tPrefab.transform.Find("Content").Find("Text1").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Name;
                        tPrefab.transform.Find("Content").Find("Text2").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Description;
                        Debug.Log(feature.Name + " "+ feature.Description+" " + feature.EffectsDescription);
                        tPrefab.transform.Find("Content").Find("Text3").GetComponent<TMPro.TextMeshProUGUI>().text =
                    "<color=#6FB502><b><sprite name=\"Tex2D_TMP_Triangle\" tint=1>" + feature.EffectsDescription + "</b></color>";
                        DescriptionList.AddBtn(tPrefab);
                    }
                }

                //倒计时
                Info2.Find("Timer").gameObject.SetActive(!worker.HaveHome);

                //更新日程表
                var schedule = this.transform.Find("BeastDuty").Find("Part1").Find("Info").Find("Schedule").Find("Time");
                TimeStatus[] workerTimeStatus = worker.TimeArrangement.Status;
                
                for (int i = 0; i < workerTimeStatus.Length; i++)
                {
                    //Debug.Log("34 " + i);
                    Image img = schedule.Find("T" + i.ToString()).GetComponent<Image>();
                    switch (workerTimeStatus[i])
                    {
                        case TimeStatus.Relax:
                            //Debug.Log("workerTimeStatus Relax");
                            img.color = UnityEngine.Color.green;
                            break;
                        case TimeStatus.Work_Transport:
                            //Debug.Log("workerTimeStatus Work_Transport");
                            img.color = UnityEngine.Color.blue;
                            break;
                        case TimeStatus.Work_OnDuty:
                            //Debug.Log("workerTimeStatus Work_OnDuty");
                            img.color = UnityEngine.Color.yellow;
                            break;
                    }
                }
            }
            else
            {
                this.transform.Find("HiddenBeastInfo2").Find("Content").gameObject.SetActive(false);
            }
            #endregion

            #region KeyTip
            KT_ViewMoreInformation.gameObject.SetActive(SwitchInfoIndex == 0);
            KT_CancelMoreInformation.gameObject.SetActive(SwitchInfoIndex == 1);
            #endregion

        }
        #endregion

        #region Resource

        #region TextContent
        [System.Serializable]
        public struct BeastPanelStruct
        {
            //BeastInfo
            public TextContent Speed;

            public TextContent Cook;
            public TextContent HandCraft;
            public TextContent Industry;
            public TextContent Magic;
            public TextContent Transport;
            public TextContent Collect;

            public KeyTip Expel;
            public KeyTip Back;
        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/ResonanceWheel";
            this.abname = "BeastPanel";
            this.description = "BeastPanel数据加载完成";
        }
        #endregion

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
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "BeastBioPool", LocalGameManager.Instance.WorkerManager.GetWorkers().Count, "Prefab_ResonanceWheel_UIPrefab/Prefab_ResonanceWheel_UI_BeastBio.prefab");
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "SimpleDescriptionPool", 5, "Prefab_ResonanceWheel_UIPrefab/Prefab_ResonanceWheel_UI_SimpleDescription.prefab");
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "FullDescriptionPool", 5, "Prefab_ResonanceWheel_UIPrefab/Prefab_ResonanceWheel_UI_FullDescription.prefab");
            base.InitObjectPool();
        }
        [ShowInInspector]
        private UIBtnList BeastList;
        [ShowInInspector]
        private UIBtnList DescriptionList;
        [ShowInInspector]
        private UIBtnList ScheduleList;
        protected override void InitBtnInfo()
        {

            BeastList = new UIBtnList(this.transform.Find("HiddenBeastInfo1").Find("Info").GetComponentInChildren<UIBtnListInitor>());
            DescriptionList = new UIBtnList(this.Info2.GetComponentInChildren<UIBtnListInitor>());
            ScheduleList = new UIBtnList(this.transform.Find("BeastDuty").Find("Part1").Find("Info").Find("Schedule").Find("Time").GetComponentInChildren<UIBtnListInitor>());
        }
        protected override void InitBtnInfoAfterInitObjectPool()
        {
            Workers = LocalGameManager.Instance.WorkerManager.GetWorkers();

            this.objectPool.ResetAllObject();

            for (int i = 0; i < Workers.Count; i++)
            {
                var tPrefab = this.objectPool.GetNextObject("BeastBioPool");
                int APStatus = Workers[i].GetAPStatu();
                var AP = tPrefab.transform.Find("Bio").Find("AP");
                for (int j = 0; j < AP.childCount - 1; j++)
                {
                    AP.GetChild(j).gameObject.SetActive(APStatus == j);
                }
                int MoodStatus = Workers[i].GetMoodStatu();
                var Mood = tPrefab.transform.Find("Bio").Find("Mood");
                for (int j = 0; j < AP.childCount - 1; j++)
                {
                    Mood.GetChild(j).gameObject.SetActive(MoodStatus == j);
                }

                AP.Find("Number").Find("Text").GetComponent<TextMeshProUGUI>().text = Workers[i].APCurrent.ToString();
                Mood.Find("Number").Find("Text").GetComponent <TextMeshProUGUI>().text = Workers[i].EMCurrent.ToString();
                tPrefab.transform.Find("Bio").Find("IconImage").Find("Image").gameObject.SetActive(!Workers[i].HaveHome);
                BeastList.AddBtn(tPrefab);
            }
            this.BeastList.OnSelectButtonChanged += () => { this.Refresh(); };
            this.ScheduleList.SetAllBtnAction(() => {

                if (CurrentBeastIndex == -1) return;
                Worker worker = Workers[CurrentBeastIndex];
                int time = ScheduleList.GetCurSelectedPos1();
                if (worker != null && time != -1) 
                {
                    if (worker.TimeArrangement.Status[time] == TimeStatus.Relax)
                    {
                        worker.SetTimeStatus(time, TimeStatus.Work_Transport);
                    }
                    else if (worker.TimeArrangement.Status[time] == TimeStatus.Work_Transport)
                    {
                        worker.SetTimeStatus(time, TimeStatus.Relax);
                    }
                    this.Refresh();
                }
                
            });
            
        }
        #endregion
    }


}
