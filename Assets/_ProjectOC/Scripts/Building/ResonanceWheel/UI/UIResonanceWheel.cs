using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using ML.Engine.Utility;
using ProjectOC.Player;
using Sirenix.OdinInspector;
using ProjectOC.WorkerNS;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static ProjectOC.ResonanceWheelSystem.UI.UIResonanceWheel;
using ProjectOC.ManagerNS;
using ML.Engine.UI;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class UIResonanceWheel : ML.Engine.UI.UIBasePanel<ResonanceWheelPanel>,ITickComponent
    {
        public IInventory inventory;
        private SpriteAtlas resonanceAtlas;
        #region Unity
        protected override void Awake()
        {
            base.Awake();
            workerEcho = ((GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).currentCharacter
                .interactComponent.CurrentInteraction as WorkerEchoBuilding).WorkerEcho;
            //exclusivePart
            exclusivePart = this.transform.Find("ExclusivePart");
            exclusivePart.gameObject.SetActive(true);

            // TopPart
            TopTitleText = this.transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            //FuctionType
            var content = this.transform.Find("FunctionType").Find("Content");
            HiddenBeastResonanceTemplate = content.Find("HiddenBeastResonanceContainer").Find("HiddenBeastResonanceTemplate");
            SongofSeaBeastsTemplate = content.Find("SongofSeaBeastsContainer").Find("SongofSeaBeastsTemplate");
            HiddenBeastResonanceText = HiddenBeastResonanceTemplate.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            SongofSeaBeastsText = SongofSeaBeastsTemplate.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            //Ring
            var ring = exclusivePart.Find("Ring").Find("Ring");
            Grids = new RingGrid[ring.childCount];

            for (int i = 0; i < ring.childCount; i++)
            {
                Grids[i] = new RingGrid();
                Grids[i].transform = ring.GetChild(i).transform;
                Grids[i].RingGridText = Grids[i].transform.Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
                Grids[i].isNull = true;
                Grids[i].isTiming = false;
                Grids[i].isResonating = false;
            }

            //ResonanceTarget
            var ResonanceTarget = exclusivePart.Find("ResonanceTarget");
            RTInfo = ResonanceTarget.Find("Info");
            ResonanceTargetTitle = RTInfo.Find("Name").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            RandomText = RTInfo.Find("CenterImage").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            CenterImage = RTInfo.Find("CenterImage").Find("Image").GetComponent<Image>();

            //ResonanceConsumpion
            currentBeastType = WorkerCategory.Random;//��ʼ��ΪRandom
            var ResonanceConsumpion = exclusivePart.Find("ResonanceConsumption");
            RCInfo = ResonanceConsumpion.Find("Info");
            TimerUI = RCInfo.Find("Timer");
            ResonanceConsumpionTitle = RCInfo.Find("Name").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            BotKeyTips = this.transform.Find("BotKeyTips");
        }
        protected override void Start()
        {
            Refresh();
            base.Start();
        }

        protected override void OnDestroy()
        {
            ClearTemp();
            (this as ITickComponent).DisposeTick();
        }
        #endregion

        #region Tick
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void Tick(float deltatime)
        {

            if (Grids == null) return;

            if (Grids[CurrentGridIndex].isNull)
            {

            }
            else if (Grids[CurrentGridIndex].isTiming)//ˢ�¼�ʱʱ��
            {
                var (min, sec) = Grids[CurrentGridIndex].worker.Timer.currentTimeInMSForm;
                TimerUI.GetComponentInChildren<TextMeshProUGUI>().text = min.ToString() + "min" + sec.ToString() + "s";
            }

        }
        #endregion

        #region Override
        public override void OnExit()
        {
            //TODO: �����ֽ�������ʱֻ���ز�����
            this.gameObject.SetActive(false);
            this.Exit();
            ResonanceWheel_sub1Instance = null;
            this.objectPool.OnDestroy();
            ClearTemp();
            isInitPool = false;
        }

        public override void OnPause()
        {
            //����base.OnPause();
            //base.OnPause();
            //this.Exit();
        }

        public override void OnRecovery()
        {
            this.Refresh();
            //����base.OnRecovery();
            //base.OnRecovery();
            //this.Enter();
        }

        protected override void Enter()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            isQuit = false;
            hasSub1Instance = false;
            Invoke("Refresh", 0.01f);
            base.Enter();
        }

        protected override void Exit()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
            base.Exit();
        }
        #endregion

        #region Internal

        public class RingGrid
        {
            public Sprite sprite;//������ʾ��ͼ
            public TMPro.TextMeshProUGUI RingGridText;
            public bool isNull;//�Ƿ�Ϊ��
            public bool isTiming;//�Ƿ����ڼ�ʱ
            public bool isResonating;//�Ƿ������

            public ExternWorker worker;//��Ӧ������
            public Transform transform;
            public string id;
            public WorkerCategory beastType;

            internal static void Reset(RingGrid ringGrid)
            {
                ringGrid.isNull = true;
                ringGrid.isTiming = false;
                ringGrid.isResonating = false;
            }
        }

        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.Disable();
            // �л���Ŀ
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.LastTerm.performed -= LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextTerm.performed -= NextTerm_performed;

            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed -= NextGrid_performed;

            //�л�����
            //ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;

            //��ʼ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StartResonance.performed -= StartResonance_performed;

            //ֹͣ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StopResonance.performed -= StopResonance_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.Enable();
            // �л���Ŀ
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.LastTerm.performed += LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextTerm.performed += NextTerm_performed;

            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed += NextGrid_performed;

            //�л�����
            //ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;

            //��ʼ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StartResonance.performed += StartResonance_performed;

            //ֹͣ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StopResonance.performed += StopResonance_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(hasSub1Instance)
            {
                isQuit = true;
                UIMgr.PopPanel();
            }
            UIMgr.PopPanel();
        }

        private void LastTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurrentFuctionTypeIndex = (CurrentFuctionTypeIndex + 2 - 1) % 2;
            this.Refresh();
        }

        private void NextTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurrentFuctionTypeIndex = (CurrentFuctionTypeIndex + 1) % 2;
            this.Refresh();
        }

        private void NextGrid_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Vector2 vector2 = obj.ReadValue<Vector2>();
            float angle = Mathf.Atan2(vector2.x, vector2.y);
            angle = angle * 180 / Mathf.PI;
            if (angle<0)
            {
                angle = angle + 360;
            }
            if (angle < 36 || angle > 324) CurrentGridIndex = 0;
            else if (angle > 36 && angle < 108) CurrentGridIndex = 4;
            else if (angle > 108 && angle < 180) CurrentGridIndex = 3;
            else if (angle > 180 && angle < 252) CurrentGridIndex = 2;
            else if (angle > 252 && angle < 324) CurrentGridIndex = 1;
            this.Refresh();
        }

        private void SwitchTarget_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_ResonanceWheel_UIPanel/Prefab_ResonanceWheel_UI_ResonanceWheelUI_sub2.prefab").Completed += (handle) =>
            {
                var panel = handle.Result.GetComponent<UIResonanceWheel_sub2>();
                panel.transform.SetParent(this.transform.parent, false);
                panel.parentUI = this;
                GameManager.Instance.UIManager.PushPanel(panel);
            };
        }

        private void StartResonance_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (Grids[CurrentGridIndex].isNull == false) return;
            ExternWorker ExternWorker = null;
            if (LocalGameManager.Instance.WorkerEchoManager.Level == 1) //GameManager.Instance.Level == 1
            {
                //�ܷ�ɹ��ϳ� �п�
                ExternWorker = workerEcho.SummonWorker(currentBeastType, CurrentGridIndex,inventory);
            }
            else
            {
                if(currentBeastType!= WorkerCategory.None)
                {
                    ExternWorker = workerEcho.SummonWorker(currentBeastType, CurrentGridIndex, inventory);
                }
            }

            if (ExternWorker != null)
            {
                Grids[CurrentGridIndex].worker = ExternWorker;
                Grids[CurrentGridIndex].isNull = false;
                Grids[CurrentGridIndex].isResonating = true;
                Grids[CurrentGridIndex].beastType = currentBeastType;
            }
            
            //�����Ӽ�ʱ���ӻص���ˢ��
            foreach (var grid in Grids)  
            {
                if (grid.isNull)
                {
                    //�ո���
                }
                else
                {
                    if(!grid.worker.Timer.IsTimeUp)//��ʱδ����
                    {
                        //ˢ�¼�ʱ���ز�
                        grid.transform.Find("Image").GetComponent<Image>().sprite = sprite2;
                        grid.isTiming = true;
                    }

                    grid.worker.Timer.OnEndEvent += () =>
                    {
                        //ˢ���ز�
                        grid.transform.Find("Image").GetComponent<Image>().sprite = LocalGameManager.Instance.WorkerManager.GetWorkerProfile(grid.worker.workerCategory);
                        grid.isNull = false;
                        grid.isTiming = false;
                        this.Refresh();
                    };
                }
            }
            this.Refresh();
        }

        private void StopResonance_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (Grids[CurrentGridIndex].isNull)
            {
                //��ǰ�޹���
                return;
            }
            if(!Grids[CurrentGridIndex].isTiming)
            {
                //��ǰ�޹���
                return;
            }
            workerEcho.StopEcho(Grids[CurrentGridIndex].beastType.ToString(), CurrentGridIndex, inventory);
            Grids[CurrentGridIndex].isNull = true;
            Grids[CurrentGridIndex].isTiming = false;
            Grids[CurrentGridIndex].worker.Timer.End();
            this.Refresh();
        }
        #endregion

        #region external
        public bool ResonanceWheelIsBusy()
        {
            for (int i = 0; i < Grids.Length; i++)
            {
                if (!Grids[i].isNull) return true;
            }
            return false;
        }

        #endregion
        #region ChangeToSubUI

        public void MainToSub1()
        {
            exclusivePart = this.transform.Find("ExclusivePart");
            var ReasonanceTarget = exclusivePart.Find("ResonanceTarget");
            var ResonanceConsumption = exclusivePart.Find("ResonanceConsumption");

            ReasonanceTarget.gameObject.SetActive(false);
            ResonanceConsumption.gameObject.SetActive(false);
            BotKeyTips.gameObject.SetActive(false);
            // �л���Ŀ

            //�л�����

            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;

            //��ʼ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StartResonance.performed -= StartResonance_performed;

            //ֹͣ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StopResonance.performed -= StopResonance_performed;

            // ����
            
        }


        public void Sub1ToMain()
        {
            exclusivePart = this.transform.Find("ExclusivePart");
            var ReasonanceTarget = exclusivePart.Find("ResonanceTarget");
            var ResonanceConsumption = exclusivePart.Find("ResonanceConsumption");
            
            ReasonanceTarget.gameObject.SetActive(true);
            ResonanceConsumption.gameObject.SetActive(true);
            BotKeyTips.gameObject.SetActive(true);
            // �л���Ŀ

            //�л�����

            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;

            //��ʼ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StartResonance.performed += StartResonance_performed;

            //ֹͣ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StopResonance.performed += StopResonance_performed;

            // ����

            hasSub1Instance = false;
        }

        public void MainToSub2()
        {
            exclusivePart = this.transform.Find("ExclusivePart");
            
            //setfalse ��ui�Ķ��в���
            exclusivePart?.gameObject.SetActive(false);
            BotKeyTips.gameObject.SetActive(false);

            // �л���Ŀ


            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed -= NextGrid_performed;

            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;

            //��ʼ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StartResonance.performed -= StartResonance_performed;

            //ֹͣ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StopResonance.performed -= StopResonance_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        public void Sub2ToMain()
        {
            exclusivePart = this.transform.Find("ExclusivePart");
            //setfalse ��ui�Ķ��в���
            if (exclusivePart != null)
                exclusivePart.gameObject.SetActive(true);
            BotKeyTips.gameObject.SetActive(true);
            // �л���Ŀ

            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed += NextGrid_performed;


            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;

            //��ʼ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StartResonance.performed += StartResonance_performed;

            //ֹͣ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StopResonance.performed += StopResonance_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }

        #endregion

        #region UI
        #region Temp

        private void ClearTemp()
        {
            beastTypeDic.Clear();


        }

        #endregion

        #region UI��������
        public WorkerEcho workerEcho;
        public Transform exclusivePart;//��ui���в���
        private UIResonanceWheel_sub1 Sub1nstance = null;
        private bool hasSub1Instance = false;//�Ƿ���sub1�ĵ�һ����ʵ��
        public bool isQuit = false;

        //topPart
        private TMPro.TextMeshProUGUI TopTitleText;
        private Transform HiddenBeastResonanceTemplate;
        private Transform SongofSeaBeastsTemplate;
        private int CurrentFuctionTypeIndex = 0;//0ΪHBR 1ΪSSB
        private TMPro.TextMeshProUGUI HiddenBeastResonanceText;
        private TMPro.TextMeshProUGUI SongofSeaBeastsText;

        //ring
        [ShowInInspector]
        public RingGrid[] Grids;
        public int CurrentGridIndex = 0;

        //ResonanceTarget
        private Transform RTInfo;
        private TMPro.TextMeshProUGUI ResonanceTargetTitle;
        private TMPro.TextMeshProUGUI RandomText;
        private Image CenterImage;

        //ResonanceConsumpion
        public WorkerCategory currentBeastType;
        private Transform RCInfo;
        private Transform TimerUI;
        private TMPro.TextMeshProUGUI ResonanceConsumpionTitle;

        private Transform BotKeyTips;
        #endregion

        #region temp
        public Sprite sprite1,sprite2,sprite3;
        [ShowInInspector]
        public Dictionary<WorkerCategory, Sprite> beastTypeDic = new Dictionary<WorkerCategory, Sprite>();
        #endregion

        public override void Refresh()
        {
            if (!this.gameObject.activeInHierarchy) return;

            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !isInitPool)
            {
                return;
            }
            #region TopPart
            TopTitleText.text = PanelTextContent.toptitle;

            #region FunctionType
            GameObject HBR = HiddenBeastResonanceTemplate.Find("Selected").gameObject;
            GameObject SSB = SongofSeaBeastsTemplate.Find("Selected").gameObject;
            HBR.SetActive(CurrentFuctionTypeIndex == 0);
            SSB.SetActive(CurrentFuctionTypeIndex == 1);

            HiddenBeastResonanceText.text = PanelTextContent.HiddenBeastResonanceText;
            SongofSeaBeastsText.text= PanelTextContent.SongofSeaBeastsText;
            #endregion

            #endregion

            #region Ring
            if(Grids.Length==0) return;

            if (Grids[CurrentGridIndex].isNull)//�ո���
            {
                if (hasSub1Instance)
                {
                    UIMgr.PopPanel();
                    hasSub1Instance=false;
                }
                //����Ϊ��������
                var Name = RCInfo.Find("Name");
                var Consumables = RCInfo.Find("Consumables");
                var StartBtn = RCInfo.Find("StartResonance");
                var StopBtn = RCInfo.Find("StopResonance");
                Name.GetComponentInChildren<TextMeshProUGUI>().text = "��������";
                Consumables.gameObject.SetActive(true);
                StartBtn.gameObject.SetActive(true);
                StopBtn.gameObject.SetActive(false);
                Grids[CurrentGridIndex].transform.Find("Image").GetComponent<Image>().sprite = sprite3;
                //ȡ��ֹͣ��������
                var SwitchTarget = exclusivePart.Find("ResonanceTarget").Find("Info").Find("SwitchTarget");
                SwitchTarget.gameObject.SetActive(true);
                if (LocalGameManager.Instance.WorkerEchoManager.Level == 1)
                {
                    SwitchTarget.gameObject.SetActive(false);
                    //�л�����
                    ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;
                }
                else
                {
                    SwitchTarget.gameObject.SetActive(true);
                    //�л�����
                    ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;
                }
                if (beastTypeDic.ContainsKey(currentBeastType))
                {
                    CenterImage.sprite = beastTypeDic[currentBeastType];
                }

                //���¼�ʱ��Ϊ��Ҫ������ʱ��
                var (min, sec) = workerEcho.GetRealTimeCostInMSForm(currentBeastType);
                TimerUI.GetComponentInChildren<TextMeshProUGUI>().text = min.ToString() + "min" + sec.ToString() + "s";
            }
            else if (Grids[CurrentGridIndex].isTiming)//��ʱ����
            {
                if (hasSub1Instance)
                {
                    UIMgr.PopPanel();
                    hasSub1Instance = false;
                }
                //����Ϊ������
                var Consumables = RCInfo.Find("Consumables");
                var StartBtn = RCInfo.Find("StartResonance");
                var StopBtn = RCInfo.Find("StopResonance");
                Consumables.gameObject.SetActive(false);
                StartBtn.gameObject.SetActive(false);
                StopBtn.gameObject.SetActive(true);

                //����CenterImage
                this.CenterImage.sprite = this.beastTypeDic[Grids[CurrentGridIndex].beastType];

                //ȡ��ѡ�������
                var SwitchTarget = exclusivePart.Find("ResonanceTarget").Find("Info").Find("SwitchTarget");
                SwitchTarget.gameObject.SetActive(false);

                //�л�����
                ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;
            }
            else//��ʱ��ɸ���
            {
                if(!isQuit)
                {
                    if (!hasSub1Instance)
                    {
                        var panel = GameObject.Instantiate(ResonanceWheel_sub1Instance);
                        panel.transform.SetParent(this.transform.parent, false);
                        panel.parentUI = this;
                        Sub1nstance = panel;
                        GameManager.Instance.UIManager.PushPanel(panel);
                        hasSub1Instance = true;
                    }
                    else
                    {
                        Sub1nstance.Refresh();
                    }
                } 
            }
            //selected
            for (int i = 0; i < Grids.Length; i++) 
            {
                if (CurrentGridIndex == i)
                {
                    Grids[i].transform.Find("Selected").gameObject.SetActive(true);
                }
                else
                {
                    Grids[i].transform.Find("Selected").gameObject.SetActive(false);
                }

                Grids[i].RingGridText.text = PanelTextContent.GridText;
            }
            #endregion

            #region ResonanceTarget
            ResonanceTargetTitle.text = PanelTextContent.ResonanceTargetTitle;
            RandomText.text=PanelTextContent.RandomText;

            #endregion

            #region ResonanceConsumpion
            this.objectPool.ResetPool("SlotPrefabPool");
            if (Grids[CurrentGridIndex].isNull)
            {
                ResonanceConsumpionTitle.text = PanelTextContent.ResonanceConsumpionTitle.description[0];//0 ����������
                //��ʾ������Ʒ��ϸ
                var Consumables = RCInfo.Find("Consumables");
                if (currentBeastType != WorkerCategory.None) 
                {
                    string cb = "WorkerEcho_"+currentBeastType.ToString();
                    foreach (var item in GameManager.Instance.GetLocalManager<WorkerEchoManager>().GetRaw(cb))
                    {
                        var tPrefab = this.objectPool.GetNextObject("SlotPrefabPool", Consumables);
                        int needNum = item.num;
                        int haveNum = this.inventory.GetItemAllNum(item.id);
                        tPrefab.transform.Find("ItemNumber").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = needNum.ToString() + "/" + haveNum.ToString();
                        if (needNum > haveNum)
                        {
                            tPrefab.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.red;
                        }
                        tPrefab.transform.Find("ItemName").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(item.id);
                        tPrefab.transform.Find("ItemIcon").GetComponent<Image>().sprite = ItemManager.Instance.GetItemSprite(item.id);
                    }
                }
            }
            else if (Grids[CurrentGridIndex].isTiming)
            {
                ResonanceConsumpionTitle.text = PanelTextContent.ResonanceConsumpionTitle.description[1];//1 ��������
            }
            #endregion
        }
        #endregion

        #region Resource

        private UIResonanceWheel_sub1 ResonanceWheel_sub1Instance;
        private bool isInitPool = false;
        #region TextContent
        [System.Serializable]
        public struct ResonanceWheelPanel
        {
            //topPart
            public TextContent toptitle;
            public KeyTip Lastterm;
            public KeyTip Nextterm;
            public TextContent HiddenBeastResonanceText;
            public TextContent SongofSeaBeastsText;

            //ring
            public KeyTip Nextgrid;
            public TextContent GridText;

            //ResonanceTarget
            public TextContent ResonanceTargetTitle;
            public TextContent RandomText;
            public KeyTip SwitchTarget;

            //ResonanceConsumpion
            public MultiTextContent ResonanceConsumpionTitle;
            public KeyTip StartResonance;
            public KeyTip StopResonance;

            //BotKeyTips
            public KeyTip Back;

        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/ResonanceWheel";
            this.abname = "ResonanceWheelPanel";
            this.description = "ResonanceWheelPanel���ݼ������";
        }
        #endregion

        protected override void InitObjectPool()
        {
            Synchronizer synchronizer = new Synchronizer(7, () => { isInitPool = true; Refresh(); });
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Texture2D, "Texture2DPool", 1,
            "SA_ResonanceWheel_UI", (handle) =>
            {
                var resonanceAtlas = handle.Result as SpriteAtlas;
                string Pre = "Tex2D_Resonance_UI_";

                sprite1 = resonanceAtlas.GetSprite(Pre + "icon_beast");
                sprite2 = resonanceAtlas.GetSprite(Pre + "icon_timing");
                sprite3 = resonanceAtlas.GetSprite(Pre + "gray_background");

                beastTypeDic.Add(WorkerCategory.CookWorker, resonanceAtlas.GetSprite(Pre + WorkerCategory.CookWorker));
                beastTypeDic.Add(WorkerCategory.HandCraftWorker, resonanceAtlas.GetSprite(Pre + WorkerCategory.HandCraftWorker));
                beastTypeDic.Add(WorkerCategory.IndustryWorker, resonanceAtlas.GetSprite(Pre + WorkerCategory.IndustryWorker));
                beastTypeDic.Add(WorkerCategory.MagicWorker, resonanceAtlas.GetSprite(Pre + WorkerCategory.MagicWorker));
                beastTypeDic.Add(WorkerCategory.TransportWorker, resonanceAtlas.GetSprite(Pre + WorkerCategory.TransportWorker));
                beastTypeDic.Add(WorkerCategory.CollectWorker, resonanceAtlas.GetSprite(Pre + WorkerCategory.CollectWorker));
                beastTypeDic.Add(WorkerCategory.Random, resonanceAtlas.GetSprite(Pre + WorkerCategory.Random));
                synchronizer.Check();
            }
            );
            
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "SlotPrefabPool", 5, "Prefab_ResonanceWheel_UIPrefab/Prefab_ResonanceWheel_UI_Slot.prefab", (handle) =>
            {
                synchronizer.Check();
            });

            this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "ResonanceWheelUI_sub1", 1, "Prefab_ResonanceWheel_UIPanel/Prefab_ResonanceWheel_UI_ResonanceWheelUI_sub1.prefab", (handle) =>
            {
                ResonanceWheel_sub1Instance = (handle.Result as GameObject).GetComponent<UIResonanceWheel_sub1>();
                synchronizer.Check();
            });
            base.InitObjectPool();
        }
        #endregion
    }
}
