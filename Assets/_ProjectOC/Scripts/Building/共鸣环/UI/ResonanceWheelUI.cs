using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using Newtonsoft.Json;
using ProjectOC.TechTree.UI;
using ProjectOC.WorkerEchoNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class ResonanceWheelUI : ML.Engine.UI.UIBasePanel,ITickComponent
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
            InitUITextContents();

            //exclusivePart
            exclusivePart = this.transform.Find("ExclusivePart");
            exclusivePart.gameObject.SetActive(true);

            // TopTitle
            TopTitleText = this.transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            //FuctionType

            var content = this.transform.Find("FunctionType").Find("Content");
            KT_LastTerm = new UIKeyTip();
            KT_LastTerm.img = content.Find("KT_LastTerm").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_LastTerm.keytip = KT_LastTerm.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();

            HiddenBeastResonanceTemplate = content.Find("HiddenBeastResonanceContainer").Find("HiddenBeastResonanceTemplate");
            SongofSeaBeastsTemplate = content.Find("SongofSeaBeastsContainer").Find("SongofSeaBeastsTemplate");

            KT_NextTerm = new UIKeyTip();
            KT_NextTerm.img = content.Find("KT_NextTerm").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_NextTerm.keytip = KT_NextTerm.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();

            //Ring
            var ringcontent = exclusivePart.Find("Ring").Find("Viewport").Find("Content");
            KT_NextGrid = new UIKeyTip();
            KT_NextGrid.keytip= ringcontent.Find("SelectKey").GetComponent<TMPro.TextMeshProUGUI>();
            var ring = ringcontent.Find("Ring");
            Grid1 = ring.Find("Grid1");
            Grid2 = ring.Find("Grid2");
            Grid3 = ring.Find("Grid3");
            Grid4 = ring.Find("Grid4");
            Grid5 = ring.Find("Grid5");
            Grids.Add(new RingGrid(Grid1.transform));
            Grids.Add(new RingGrid(Grid2.transform));
            Grids.Add(new RingGrid(Grid3.transform));
            Grids.Add(new RingGrid(Grid4.transform));
            Grids.Add(new RingGrid(Grid5.transform));


            //ResonanceConsumpion
            var ResonanceConsumpion = exclusivePart.Find("ResonanceConsumption");
            RCInfo = ResonanceConsumpion.Find("Info");
            TimerUI = RCInfo.Find("Timer");
            IsInit = true;
            Refresh();
        }

        #endregion

        #region Tick
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void Tick(float deltatime)
        {
            if (Grids[CurrentGridIndex].isNull)
            {

            }
            else if (Grids[CurrentGridIndex].isTiming)//ˢ�¼�ʱʱ��
            {
                TimerUI.GetComponentInChildren<TextMeshProUGUI>().text = Grids[CurrentGridIndex].worker.timer.CurrentTime.ToString();
            }



        }

        #endregion




        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.Enter();
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        }

        public override void OnExit()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);

            base.OnExit();
            this.Exit();
            ClearTemp();
        }

        public override void OnPause()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
            

            //base.OnPause();
            //this.Exit();
        }

        public override void OnRecovery()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);

            


            //base.OnRecovery();
            //this.Enter();
        }


        private void OnDestroy()
        {
            ClearTemp();
            (this as ITickComponent).DisposeTick();
        }

        #endregion

        #region Internal

        public class RingGrid
        {
            public Sprite sprite;//������ʾ��ͼ
            public bool isNull;//�Ƿ�Ϊ��
            public bool isTiming;//�Ƿ����ڼ�ʱ
            public bool isResonating;//�Ƿ������

            public ExternWorker worker;//��Ӧ������
            public Transform transform;
            public string id;



            public RingGrid(Transform transform)
            {
                this.sprite = null;
                this.isNull = true;
                this.isTiming = false;
                this.isResonating = false;

                this.worker = null;
                this.transform = transform;
                this.id = null;
            }
            public static void Reset(RingGrid grid,Transform transform)
            {
                grid.sprite = null;
                grid.isNull = true;
                grid.isTiming = false;
                grid.isResonating = false;
                grid.worker = null;
                grid.transform = transform;
                grid.id = null;
            }
        }




        private void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.Enable();

            



            this.Refresh();
        }

        private void Exit()
        {
            this.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.Disable();
            
        }

        private void UnregisterInput()
        {
            // �л���Ŀ
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.LastTerm.performed -= LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextTerm.performed -= NextTerm_performed;

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

        private void RegisterInput()
        {
            // �л���Ŀ
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.LastTerm.performed += LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextTerm.performed += NextTerm_performed;

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



        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Debug.Log("main Back_performed");
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
            Debug.Log("NextGrid_performed!");
            CurrentGridIndex = (CurrentGridIndex + 1) % Grids.Count;
            this.Refresh();
        }

        private void SwitchTarget_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var panel = GameObject.Instantiate(resonanceWheel_Sub2);
            panel.transform.SetParent(this.transform.parent, false);

            
            ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
            //ActiveSubUI();
            Debug.Log("SwitchTarget_performed!");
            
        }

        private void StartResonance_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //��鱳��
            Debug.Log("StartResonance_perfozqrmed!");
            Debug.Log(GameManager.Instance.GetLocalManager<WorkerManager>());
            ExternWorker worker;
            if (GameManager.Instance.GetLocalManager<WorkerEcho>().Level == 1) 
            {
                //�ܷ�ɹ��ϳ� �п�
                worker = GameManager.Instance.GetLocalManager<WorkerEcho>().SummonWorker1(CurrentGridIndex);
                
                  
            }
            else
            {
                worker = GameManager.Instance.GetLocalManager<WorkerEcho>().SummonWorker2(CurrentGridIndex, Grids[CurrentGridIndex].id);
            }

            if (worker != null)
            {
                Grids[CurrentGridIndex].worker = worker;
                Grids[CurrentGridIndex].isNull = false;
                Grids[CurrentGridIndex].isResonating = true;
            }
            else
            {
                Debug.Log("���ϲ��㣡");
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
                    if(!grid.worker.timer.IsTimeUp)//��ʱδ����
                    {
                        //ˢ�¼�ʱ���ز�
                        grid.transform.Find("Image").GetComponent<Image>().sprite = sprite2;
                        grid.isTiming = true;
                    }
                    
                    grid.worker.timer.OnEndEvent += () =>
                    {
                        //ˢ���ز�
                        grid.transform.Find("Image").GetComponent<Image>().sprite = sprite1;
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

            Grids[CurrentGridIndex].isNull = true;
            Grids[CurrentGridIndex].isTiming = false;
            Grids[CurrentGridIndex].worker.timer.End();
            this.Refresh();

        }
        #endregion

        #region ChangeToSubUI

        public void MainToSub1()
        {
            Debug.Log("MainToSub1");
            exclusivePart = this.transform.Find("ExclusivePart");
            var ReasonanceTarget = exclusivePart.Find("ResonanceTarget");
            var ResonanceConsumption = exclusivePart.Find("ResonanceConsumption");
            var BotKeyTips = exclusivePart.Find("BotKeyTips");

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
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }


        public void Sub1ToMain()
        {
            Debug.Log("Sub1ToMain");
            exclusivePart = this.transform.Find("ExclusivePart");
            var ReasonanceTarget = exclusivePart.Find("ResonanceTarget");
            var ResonanceConsumption = exclusivePart.Find("ResonanceConsumption");
            var BotKeyTips = exclusivePart.Find("BotKeyTips");

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
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }

        public void MainToSub2()
        {
            exclusivePart = this.transform.Find("ExclusivePart");
            Debug.Log("MainToSub2");
            
            //setfalse ��ui�Ķ��в���
            exclusivePart?.gameObject.SetActive(false);


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
        public ResonanceWheel_sub1 resonanceWheel_Sub1;//��ϸ���޽��� 
        public ResonanceWheel_sub2 resonanceWheel_Sub2;//���ѡ�����޽��� 

        public Transform exclusivePart;//��ui���в���


        private bool hasSub2Instance = false;//�Ƿ���sub2�ĵ�һ����ʵ��


        private TMPro.TextMeshProUGUI TopTitleText;

        private UIKeyTip KT_LastTerm;
        private Transform HiddenBeastResonanceTemplate;
        private Transform SongofSeaBeastsTemplate;
        private int CurrentFuctionTypeIndex = 0;//0ΪHBR 1ΪSSB
        private UIKeyTip KT_NextTerm;

        //ring
        [ShowInInspector]
        public List<RingGrid> Grids = new List<RingGrid>();
        private Transform Grid1, Grid2, Grid3, Grid4, Grid5;
        
        public int CurrentGridIndex = 0;//0��4

        private UIKeyTip KT_NextGrid;

        //ResonanceConsumpion
        private Transform RCInfo;
        private Transform TimerUI;


        #endregion

        #region temp
        public Sprite sprite1,sprite2,sprite3;
        #endregion

        public void Refresh()
        {
            /*            if (ABJAProcessor == null || !ABJAProcessor.IsLoaded || !IsInit)
                        {
                            Debug.Log("ABJAProcessor is null");
                            return;
                        }


                        TopTitleText.text = PanelTextContent.toptitle;

                        #region FunctionType
                        this.KT_LastTerm.ReWrite(PanelTextContent.lastterm);
                        GameObject HBR = HiddenBeastResonanceTemplate.Find("Selected").gameObject;
                        GameObject SSB = SongofSeaBeastsTemplate.Find("Selected").gameObject;

                        if (CurrentFuctionTypeIndex == 0) 
                        {
                            HBR.SetActive(false);
                            SSB.SetActive(true);
                        }
                        else if(CurrentFuctionTypeIndex == 1)
                        {
                            HBR.SetActive(true);
                            SSB.SetActive(false);
                        }

                        this.KT_NextTerm.ReWrite(PanelTextContent.nextterm);
                        #endregion*/

            #region Ring
            //this.KT_NextGrid.ReWrite(PanelTextContent.nextgrid);
            Debug.Log("Refresh");
            if(Grids.Count==0) return;

            if (Grids[CurrentGridIndex].isNull)//�ո���
            {
                if(hasSub2Instance)
                {
                    UIMgr.PopPanel();
                    hasSub2Instance=false;
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

                //RingGrid.Reset(Grids[CurrentGridIndex], Grids[CurrentGridIndex].transform);


                Grids[CurrentGridIndex].transform.Find("Image").GetComponent<Image>().sprite = sprite3;

                //ȡ��ֹͣ��������
                var SwitchTarget = exclusivePart.Find("ResonanceTarget").Find("Info").Find("SwitchTarget");
                SwitchTarget.gameObject.SetActive(true);

                //�л�����
                ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;
            }
            else if (Grids[CurrentGridIndex].isTiming)//��ʱ����
            {

                if (hasSub2Instance)
                {
                    UIMgr.PopPanel();
                    hasSub2Instance = false;
                }

                //����Ϊ������
                var Name = RCInfo.Find("Name");
                var Consumables = RCInfo.Find("Consumables");
                var StartBtn = RCInfo.Find("StartResonance");
                var StopBtn = RCInfo.Find("StopResonance");
                Name.GetComponentInChildren<TextMeshProUGUI>().text = "������";
                Consumables.gameObject.SetActive(false);
                StartBtn.gameObject.SetActive(false);
                StopBtn.gameObject.SetActive(true);

                //ȡ��ѡ�������
                var SwitchTarget = exclusivePart.Find("ResonanceTarget").Find("Info").Find("SwitchTarget");
                SwitchTarget.gameObject.SetActive(false);

                //�л�����
                ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;


            }
            else//��ʱ��ɸ���
            {
                if(!hasSub2Instance)
                {
                    var panel = GameObject.Instantiate(resonanceWheel_Sub1);
                    panel.transform.SetParent(this.transform.parent, false);
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                    hasSub2Instance=true;
                }
                
                //����������ϸ��Ϣ

            }

            for (int i = 0; i < Grids.Count; i++) 
            {

                




                if (CurrentGridIndex == i)
                {
                    Grids[i].transform.Find("Selected").gameObject.SetActive(true);
                }
                else
                {
                    Grids[i].transform.Find("Selected").gameObject.SetActive(false);
                }

                

            }

            


            #endregion


        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct ResonanceWheelPanel
        {
            public TextContent toptitle;
            public TextTip[] itemtype;
            public KeyTip lastterm;
            public KeyTip nextterm;
            public KeyTip nextgrid;
        }

        public static ResonanceWheelPanel PanelTextContent => ABJAProcessor.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheelPanel> ABJAProcessor;

        private void InitUITextContents()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheelPanel>("JSON/TextContent/ResonanceWheel", "ResonanceWheelPanel", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UI������Panel����");
                ABJAProcessor.StartLoadJsonAssetData();
            }
            Debug.Log("1 "+ABJAProcessor.Datas.toptitle);
        }
        #endregion


        
    }

}
