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
using static ProjectOC.ResonanceWheelSystem.UI.UIBeastPanel;
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
using ProjectOC.Order;
namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class UICommunicationPanel : ML.Engine.UI.UIBasePanel<BeastPanelStruct>
    {
        #region Unity
        public bool IsInit = false;

        protected override void Awake()
        {
            base.Awake();

            
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
            base.Enter();
        }
        protected override void Exit()
        {
            base.Exit();
            ClearTemp();
        }

        public override void OnRecovery()
        {
            //Recovery ����ˢ�� ��ֹɾ����ť�첽����
            this.RegisterInput();
        }

        public override void OnPause()
        {
            this.UnregisterInput();
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Disable();

            //����
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Expel.performed -= Expel_performed;

            // ����
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

            // ����
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Expel.performed += Expel_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

            ML.Engine.Input.InputManager.Instance.Common.Common.RT.performed += RT_performed;

            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.started += TurnPage_started;

            ML.Engine.Input.InputManager.Instance.Common.Common.TurnPage.canceled += TurnPage_canceled;

            this.BeastList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
        }

        private void Expel_performed(InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, new UIManager.PopUpUIData("��������", "���޽�������ʧ��", null, () => {
                //TODO ��ֹ�ڵ�������worker����������
                try
                {
                    Workers[CurrentBeastIndex].StopHomeTimer();
                    LocalGameManager.Instance.WorkerManager.DeleteWorker(Workers[CurrentBeastIndex]);
                    this.BeastList.DeleteButton(CurrentBeastIndex, () => { this.Refresh(); });
                }
                catch { }
            }));
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

        #region UI��������
        //[ShowInInspector]
        private int CurrentBeastIndex { get { return this.BeastList.GetCurSelectedPos1(); } }

        [ShowInInspector]
        List<Worker> Workers = new List<Worker>();

        //BeastInfo
        private Image ProfileImage;
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

        private FeatureManager featManager => ManagerNS.LocalGameManager.Instance.FeatureManager;

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
            this.description = "BeastPanel���ݼ������";
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
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "DescriptionPool", 5, "Prefab_Worker_UI/Prefab_Worker_UI_FeatureTemplate.prefab");
            base.InitObjectPool();
        }
        [ShowInInspector]
        private UIBtnList BeastList;
        private Dictionary<Worker,SelectedButton> WorkerIndexDic = new Dictionary<Worker, SelectedButton>();

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
                var Name = tPrefab.transform.Find("Bio").Find("Name").GetComponent<TextMeshProUGUI>();
                tPrefab.transform.Find("Bio").Find("IconImage").GetComponent<Image>().sprite = LocalGameManager.Instance.WorkerManager.GetWorkerProfile(Workers[i].Category);

                Name.text = Workers[i].Name;

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
                BeastList.AddBtn(tPrefab, BtnSettingAction: (btn) => { WorkerIndexDic.Add(Workers[i], btn); });
                
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
                        worker.SetTimeStatus(time, (TimeStatus)TimeStatus.Work);
                    }
                    else if (worker.TimeArrangement.Status[time] == TimeStatus.Work)
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
