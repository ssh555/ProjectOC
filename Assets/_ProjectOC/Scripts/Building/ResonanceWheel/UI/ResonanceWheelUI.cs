using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using ML.Engine.UI;
using Newtonsoft.Json;
using ProjectOC.Player;
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
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheel_sub1;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheel_sub2;
//using static System.Net.Mime.MediaTypeNames;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class ResonanceWheelUI : ML.Engine.UI.UIBasePanel,ITickComponent
    {

        public IInventory inventory;

        
        



        #region Unity
        public bool IsInit = false;
        private void Start()
        {

            workerEcho = (GameObject.Find("PlayerCharacter").GetComponent<PlayerCharacter>().interactComponent.CurrentInteraction as WorkerEchoBuilding).workerEcho;


            this.GetComponent<ResonanceWheelUI>().enabled = true;
            StartCoroutine(InitUIPrefabs());
            StartCoroutine(InitUITexture2D());
            InitUITextContents();
            
            
            //exclusivePart
            exclusivePart = this.transform.Find("ExclusivePart");
            exclusivePart.gameObject.SetActive(true);

            

            // TopPart
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

            HiddenBeastResonanceText = HiddenBeastResonanceTemplate.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            SongofSeaBeastsText = SongofSeaBeastsTemplate.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            //Ring
            var ring = exclusivePart.Find("Ring").Find("Ring");
            Grids = new RingGrid[ring.childCount];

            for(int i = 0;i < ring.childCount; i++)
            {
                Grids[i] = new RingGrid();
                Grids[i].transform = ring.GetChild(i).transform;
                Grids[i].RingGridText = Grids[i].transform.Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
                Grids[i].isNull = true;
                Grids[i].isTiming = false;
                Grids[i].isResonating = false;

            }


            KT_NextGrid = new UIKeyTip();
            KT_NextGrid.keytip= exclusivePart.Find("Ring").Find("SelectKeyTip").GetComponent<TMPro.TextMeshProUGUI>();
            

            //ResonanceTarget
            var ResonanceTarget = exclusivePart.Find("ResonanceTarget");
            RTInfo = ResonanceTarget.Find("Info");
            ResonanceTargetTitle = RTInfo.Find("Name").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            RandomText = RTInfo.Find("Random").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            SwitchTargetText = new UIKeyTip();
            SwitchTargetText.keytip = RTInfo.Find("SwitchTarget").Find("Image").Find("KeyTip").Find("Image").Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            SwitchTargetText.description = RTInfo.Find("SwitchTarget").Find("Image").Find("KeyTip").Find("Image").Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();
            //ResonanceConsumpion
            currentBeastType = BeastType.WorkerEcho_Random;//初始化为Random
            var ResonanceConsumpion = exclusivePart.Find("ResonanceConsumption");
            RCInfo = ResonanceConsumpion.Find("Info");
            TimerUI = RCInfo.Find("Timer");
            ResonanceConsumpionTitle = RCInfo.Find("Name").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();


            StartResonanceText = new UIKeyTip();
            StartResonanceText.keytip = RCInfo.Find("StartResonance").Find("Image").Find("KeyTip").Find("Image").Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            StartResonanceText.description = RCInfo.Find("StartResonance").Find("Image").Find("KeyTip").Find("Image").Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            StopResonanceText = new UIKeyTip();
            StopResonanceText.keytip = RCInfo.Find("StopResonance").Find("Image").Find("KeyTip").Find("Image").Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            StopResonanceText.description = RCInfo.Find("StopResonance").Find("Image").Find("KeyTip").Find("Image").Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            
            



            //BotKeyTips
            var kt = this.transform.Find("BotKeyTips").Find("KeyTips");
            KT_Back = new UIKeyTip();
            KT_Back.img = kt.Find("KT_Back").Find("Image").GetComponent<Image>();
            KT_Back.keytip = KT_Back.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Back.description = KT_Back.img.transform.Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();


            //sub1





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

            if (Grids == null) return;

            if (Grids[CurrentGridIndex].isNull)
            {

            }
            else if (Grids[CurrentGridIndex].isTiming)//刷新计时时间
            {
                TimerUI.GetComponentInChildren<TextMeshProUGUI>().text = Grids[CurrentGridIndex].worker.timer.ConvertToMinAndSec();
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

            //TODO: 共鸣轮建筑存在时只隐藏不销毁
            this.gameObject.SetActive(false);
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


            this.Refresh();

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
            public Sprite sprite;//隐兽显示贴图
            public TMPro.TextMeshProUGUI RingGridText;
            public bool isNull;//是否为空
            public bool isTiming;//是否正在计时
            public bool isResonating;//是否共鸣完成

            public ExternWorker worker;//对应的隐兽
            public Transform transform;
            public string id;
            public BeastType beastType;

            internal static void Reset(RingGrid ringGrid)
            {
                ringGrid.isNull = true;
                ringGrid.isTiming = false;
                ringGrid.isResonating = false;
            }
        }

        private void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.Enable();
            isQuit = false;
            hasSub1nstance = false;

            Invoke("Refresh", 0.01f);
            //this.Refresh();
        }

        private void Exit()
        {
            this.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.Disable();
        }

        private void UnregisterInput()
        {
            // 切换类目
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.LastTerm.performed -= LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextTerm.performed -= NextTerm_performed;

            //切换隐兽
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed -= NextGrid_performed;

            //切换对象
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;

            //开始共鸣
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StartResonance.performed -= StartResonance_performed;

            //停止共鸣
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StopResonance.performed -= StopResonance_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            

        }

        private void RegisterInput()
        {
            // 切换类目
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.LastTerm.performed += LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextTerm.performed += NextTerm_performed;

            //切换隐兽
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed += NextGrid_performed;


            //切换对象
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;

            //开始共鸣
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StartResonance.performed += StartResonance_performed;

            //停止共鸣
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StopResonance.performed += StopResonance_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }



        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            if(hasSub1nstance)
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
            var panel = GameObject.Instantiate(resonanceWheel_Sub2);
            panel.transform.SetParent(this.transform.parent, false);
            panel.parentUI = this;
            
            ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);

        }

        private void StartResonance_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {


            if (Grids[CurrentGridIndex].isNull == false) return;


            //检查背包

            string cb = currentBeastType.ToString();

            
            ExternWorker worker = null;
            if (workerEcho.Level == 1) //GameManager.Instance.Level == 1
            {
                //能否成功合成 判空
                worker = workerEcho.SummonWorker(cb,CurrentGridIndex,inventory);

            }
            else
            {
                if(currentBeastType!=BeastType.WorkerEcho_Null)
                {
                    worker = workerEcho.SummonWorker(cb, CurrentGridIndex, inventory);
                }
                   
            }

            if (worker != null)
            {
                Grids[CurrentGridIndex].worker = worker;
                Grids[CurrentGridIndex].isNull = false;
                Grids[CurrentGridIndex].isResonating = true;

                Grids[CurrentGridIndex].beastType = currentBeastType;

            }
            else
            {
            }


            //给格子计时器加回调并刷新
            foreach (var grid in Grids)  
            {
                if (grid.isNull)
                {
                    //空格子
                }
                else
                {
                    if(!grid.worker.timer.IsTimeUp)//计时未结束
                    {
                        //刷新计时中素材
                        grid.transform.Find("Image").GetComponent<Image>().sprite = sprite2;
                        grid.isTiming = true;
                    }

                    grid.worker.timer.OnEndEvent += () =>
                    {
                        //刷新素材
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
                //当前无共鸣

                return;
            }
            if(!Grids[CurrentGridIndex].isTiming)
            {
                //当前无共鸣

                return;
            }


            workerEcho.StopEcho(Grids[CurrentGridIndex].beastType.ToString(), CurrentGridIndex, inventory);


            Grids[CurrentGridIndex].isNull = true;
            Grids[CurrentGridIndex].isTiming = false;
            Grids[CurrentGridIndex].worker.timer.End();
            this.Refresh();

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
            

            // 切换类目

            //切换隐兽

            //切换对象
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;

            //开始共鸣
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StartResonance.performed -= StartResonance_performed;

            //停止共鸣
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StopResonance.performed -= StopResonance_performed;

            // 返回
            
        }


        public void Sub1ToMain()
        {
            exclusivePart = this.transform.Find("ExclusivePart");
            var ReasonanceTarget = exclusivePart.Find("ResonanceTarget");
            var ResonanceConsumption = exclusivePart.Find("ResonanceConsumption");
            

            ReasonanceTarget.gameObject.SetActive(true);
            ResonanceConsumption.gameObject.SetActive(true);
            

            // 切换类目
            
            //切换隐兽
            
            //切换对象
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;

            //开始共鸣
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StartResonance.performed += StartResonance_performed;

            //停止共鸣
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StopResonance.performed += StopResonance_performed;

            // 返回
            

            hasSub1nstance = false;
        }

        public void MainToSub2()
        {
            exclusivePart = this.transform.Find("ExclusivePart");
            
            //setfalse 主ui的独有部分
            exclusivePart?.gameObject.SetActive(false);


            // 切换类目
            

            //切换隐兽
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed -= NextGrid_performed;

            //切换对象
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;

            //开始共鸣
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StartResonance.performed -= StartResonance_performed;

            //停止共鸣
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StopResonance.performed -= StopResonance_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        public void Sub2ToMain()
        {
            exclusivePart = this.transform.Find("ExclusivePart");
            //setfalse 主ui的独有部分
            if (exclusivePart != null)
                exclusivePart.gameObject.SetActive(true);

            // 切换类目
            

            //切换隐兽
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed += NextGrid_performed;


            //切换对象
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;

            //开始共鸣
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StartResonance.performed += StartResonance_performed;

            //停止共鸣
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.StopResonance.performed += StopResonance_performed;

            // 返回
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

        #region UI对象引用
        public WorkerEcho workerEcho;
        public ResonanceWheel_sub1 resonanceWheel_Sub1;//详细隐兽界面 prefab
        public ResonanceWheel_sub2 resonanceWheel_Sub2;//随机选择隐兽界面 prefab
        

        public Transform exclusivePart;//主ui独有部分

        private ResonanceWheel_sub1 Sub1nstance = null;
        private bool hasSub1nstance = false;//是否有sub1的单一生成实例
        public bool isQuit = false;

        //topPart
        private TMPro.TextMeshProUGUI TopTitleText;
        private UIKeyTip KT_LastTerm;
        private Transform HiddenBeastResonanceTemplate;
        private Transform SongofSeaBeastsTemplate;
        private int CurrentFuctionTypeIndex = 0;//0为HBR 1为SSB
        private UIKeyTip KT_NextTerm;
        private TMPro.TextMeshProUGUI HiddenBeastResonanceText;
        private TMPro.TextMeshProUGUI SongofSeaBeastsText;

        //ring
        [ShowInInspector]
        public RingGrid[] Grids;
        public int CurrentGridIndex = 0;
        private UIKeyTip KT_NextGrid;

        //ResonanceTarget
        private Transform RTInfo;
        private TMPro.TextMeshProUGUI ResonanceTargetTitle;
        private TMPro.TextMeshProUGUI RandomText;
        private UIKeyTip SwitchTargetText;

        //ResonanceConsumpion

        public enum BeastType
        {
            WorkerEcho_Null=0,
            WorkerEcho_Random,
            WorkerEcho_Cat,
            WorkerEcho_Deer,
            WorkerEcho_Fox,
            WorkerEcho_Rabbit,
            WorkerEcho_Dog,
            WorkerEcho_Seal
        }


        public BeastType currentBeastType;

        private Transform RCInfo;
        private Transform TimerUI;
        private TMPro.TextMeshProUGUI ResonanceConsumpionTitle;
        private UIKeyTip StartResonanceText;
        private UIKeyTip StopResonanceText;

        private GameObject SlotPrefab;


        //BotKeyTips
        private UIKeyTip KT_Back;


        #endregion

        #region temp
        public Sprite sprite1,sprite2,sprite3;
        [ShowInInspector]
        public Dictionary<BeastType,Sprite> beastTypeDic = new Dictionary<BeastType, Sprite>();
        #endregion

        public override void Refresh()
        {
            if (!this.gameObject.activeInHierarchy) return;

            if (ABJAProcessorJson_Main == null || !ABJAProcessorJson_Main.IsLoaded || !IsInit)
            {
                Debug.Log("ABJAProcessorJson is null");
                return;
            }
            
            #region TopPart
            TopTitleText.text = PanelTextContent_Main.toptitle;



            #region FunctionType
            this.KT_LastTerm.ReWrite(PanelTextContent_Main.lastterm);
            GameObject HBR = HiddenBeastResonanceTemplate.Find("Selected").gameObject;
            GameObject SSB = SongofSeaBeastsTemplate.Find("Selected").gameObject;

            if (CurrentFuctionTypeIndex == 0)
            {
                HBR.SetActive(false);
                SSB.SetActive(true);
            }
            else if (CurrentFuctionTypeIndex == 1)
            {
                HBR.SetActive(true);
                SSB.SetActive(false);
            }

            this.KT_NextTerm.ReWrite(PanelTextContent_Main.nextterm);

            HiddenBeastResonanceText.text = PanelTextContent_Main.HiddenBeastResonanceText;
            SongofSeaBeastsText.text= PanelTextContent_Main.SongofSeaBeastsText;
            #endregion


            #endregion

            #region Ring
            this.KT_NextGrid.ReWrite(PanelTextContent_Main.nextgrid);
            if(Grids.Length==0) return;

            if (Grids[CurrentGridIndex].isNull)//空格子
            {
                if (hasSub1nstance)
                {
                    UIMgr.PopPanel();
                    hasSub1nstance=false;
                }


                //更新为共鸣消耗
                var Name = RCInfo.Find("Name");
                var Consumables = RCInfo.Find("Consumables");
                var StartBtn = RCInfo.Find("StartResonance");
                var StopBtn = RCInfo.Find("StopResonance");
                Name.GetComponentInChildren<TextMeshProUGUI>().text = "共鸣消耗";
                Consumables.gameObject.SetActive(true);
                StartBtn.gameObject.SetActive(true);
                StopBtn.gameObject.SetActive(false);


                StartResonanceText.ReWrite(PanelTextContent_Main.StartResonanceText);



                Grids[CurrentGridIndex].transform.Find("Image").GetComponent<Image>().sprite = sprite3;

                //取消停止共鸣功能
                var SwitchTarget = exclusivePart.Find("ResonanceTarget").Find("Info").Find("SwitchTarget");
                SwitchTarget.gameObject.SetActive(true);

                if (workerEcho.Level == 1)
                {
                    SwitchTarget.gameObject.SetActive(false);
                    //切换对象
                    ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;
                }
                else
                {
                    SwitchTarget.gameObject.SetActive(true);
                    //切换对象
                    ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;

                    Image RandomImage = RTInfo.Find("Random").Find("Image").GetComponent<Image>();

                    if(beastTypeDic.ContainsKey(currentBeastType))
                    {
                        RandomImage.sprite = beastTypeDic[currentBeastType];
                    }
                   
                }

                //切换对象
                ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;
            }
            else if (Grids[CurrentGridIndex].isTiming)//计时格子
            {

                if (hasSub1nstance)
                {
                    UIMgr.PopPanel();
                    hasSub1nstance = false;
                }

                //更新为共鸣中
                //var Name = RCInfo.Find("Name");
                var Consumables = RCInfo.Find("Consumables");
                var StartBtn = RCInfo.Find("StartResonance");
                var StopBtn = RCInfo.Find("StopResonance");
                //Name.GetComponentInChildren<TextMeshProUGUI>().text = "共鸣中";
                Consumables.gameObject.SetActive(false);
                StartBtn.gameObject.SetActive(false);
                StopBtn.gameObject.SetActive(true);

                StopResonanceText.ReWrite(PanelTextContent_Main.StopResonanceText);
                //取消选择对象功能
                var SwitchTarget = exclusivePart.Find("ResonanceTarget").Find("Info").Find("SwitchTarget");
                SwitchTarget.gameObject.SetActive(false);

                //切换对象
                ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;


            }
            else//计时完成格子
            {
                if(!isQuit)
                {
                    if (!hasSub1nstance)
                    {
                        var panel = GameObject.Instantiate(resonanceWheel_Sub1);
                        panel.transform.SetParent(this.transform.parent, false);
                        panel.parentUI = this;
                        Sub1nstance = panel;
                        ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                        hasSub1nstance = true;
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

                Grids[i].RingGridText.text = PanelTextContent_Main.GridText;
            }


            #endregion


            #region ResonanceTarget
            ResonanceTargetTitle.text = PanelTextContent_Main.ResonanceTargetTitle;
            RandomText.text=PanelTextContent_Main.RandomText;
            SwitchTargetText.ReWrite(PanelTextContent_Main.SwitchTargetText);


            

            #endregion

            #region ResonanceConsumpion
            if (Grids[CurrentGridIndex].isNull)
            {
                ResonanceConsumpionTitle.text = PanelTextContent_Main.ResonanceConsumpionTitle.description[0];//0 代表共鸣消耗
                //显示消耗物品详细

                var Consumables = RCInfo.Find("Consumables");
                if (Consumables.childCount > 0)
                {
                    for (int i = 0; i < Consumables.childCount; i++)
                    {
                        Destroy(Consumables.GetChild(i).gameObject);
                    }
                }




                if (currentBeastType != BeastType.WorkerEcho_Null) 
                {
                    string cb = currentBeastType.ToString();

                    foreach (var item in GameManager.Instance.GetLocalManager<WorkerEchoManager>().GetRaw(cb))
                    {
                        var descriptionPrefab = Instantiate(SlotPrefab, Consumables);
                        int needNum = item.num;
                        int haveNum = GameObject.Find("PlayerCharacter").GetComponent<PlayerCharacter>().Inventory.GetItemAllNum(item.id);
                        descriptionPrefab.transform.Find("ItemNumber").Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = needNum.ToString() + "/" + haveNum.ToString();
                        if (needNum > haveNum)
                        {
                            descriptionPrefab.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.red;
                        }

                        descriptionPrefab.transform.Find("ItemName").GetComponent<TMPro.TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(item.id);
                    }
                }

                


            }
            else if (Grids[CurrentGridIndex].isTiming)
            {
                ResonanceConsumpionTitle.text = PanelTextContent_Main.ResonanceConsumpionTitle.description[1];//1 代表共鸣中
            }


            StartResonanceText.ReWrite(PanelTextContent_Main.StartResonanceText);
            #endregion

            #region BotKeyTips
            KT_Back.ReWrite(PanelTextContent_Main.back);
            #endregion

        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct ResonanceWheelPanel
        {
            //topPart
            public TextContent toptitle;
            public KeyTip lastterm;
            public KeyTip nextterm;
            public TextContent HiddenBeastResonanceText;
            public TextContent SongofSeaBeastsText;

            //ring
            public KeyTip nextgrid;
            public TextContent GridText;

            //ResonanceTarget
            public TextContent ResonanceTargetTitle;
            public TextContent RandomText;
            public KeyTip SwitchTargetText;

            //ResonanceConsumpion
            public MultiTextContent ResonanceConsumpionTitle;
            public KeyTip StartResonanceText;
            public KeyTip StopResonanceText;

            //BotKeyTips
            public KeyTip back;
        }

        public static ResonanceWheelPanel PanelTextContent_Main => ABJAProcessorJson_Main.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheelPanel> ABJAProcessorJson_Main;

        public static ResonanceWheel_sub1Struct PanelTextContent_sub1 => ABJAProcessorJson_sub1.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheel_sub1Struct> ABJAProcessorJson_sub1;

        public static ResonanceWheel_sub2Struct PanelTextContent_sub2 => ABJAProcessorJson_sub2.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheel_sub2Struct> ABJAProcessorJson_sub2;
        private void InitUITextContents()
        {
            if (ABJAProcessorJson_Main == null)
            {
                ABJAProcessorJson_Main = new ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheelPanel>("Json/TextContent/ResonanceWheel", "ResonanceWheelPanel", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UI共鸣轮Panel_Main数据");
                ABJAProcessorJson_Main.StartLoadJsonAssetData();
            }

            if (ABJAProcessorJson_sub1 == null)
            {
                ABJAProcessorJson_sub1 = new ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheel_sub1Struct>("Json/TextContent/ResonanceWheel", "ResonanceWheel_sub1", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UI共鸣轮Panel_sub1数据");
                ABJAProcessorJson_sub1.StartLoadJsonAssetData();
            }

            if (ABJAProcessorJson_sub2 == null)
            {
                ABJAProcessorJson_sub2 = new ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheel_sub2Struct>("Json/TextContent/ResonanceWheel", "ResonanceWheel_sub2", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UI共鸣轮Panel_sub2数据");
                ABJAProcessorJson_sub2.StartLoadJsonAssetData();
            }
        }
        #endregion
        #region Texture2D
        public static AssetBundle Texture2DAB;
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        private string ResonanceWheelTexture2DPath = "ui/resonancewheel/texture2d";

        private IEnumerator InitUITexture2D()
        {

            var crequest = GM.ABResourceManager.LoadLocalABAsync(ResonanceWheelTexture2DPath, null, out var Texture2DAB);
            yield return crequest;
            if (crequest != null)
            {
                Texture2DAB = crequest.assetBundle;
            }
            Texture2D texture2D;
            
            texture2D  = Texture2DAB.LoadAsset<Texture2D>("icon_beast");
            sprite1 =  Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            texture2D = Texture2DAB.LoadAsset<Texture2D>("icon_timing");
            sprite2 = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            texture2D = Texture2DAB.LoadAsset<Texture2D>("gray_background");
            sprite3 = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));

            texture2D = Texture2DAB.LoadAsset<Texture2D>("Cat");
            beastTypeDic.Add(BeastType.WorkerEcho_Cat, Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f)));

            texture2D = Texture2DAB.LoadAsset<Texture2D>("Deer");
            beastTypeDic.Add(BeastType.WorkerEcho_Deer, Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f)));

            texture2D = Texture2DAB.LoadAsset<Texture2D>("Dog");
            beastTypeDic.Add(BeastType.WorkerEcho_Dog, Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f)));

            texture2D = Texture2DAB.LoadAsset<Texture2D>("Fox");
            beastTypeDic.Add(BeastType.WorkerEcho_Fox, Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f)));

            texture2D = Texture2DAB.LoadAsset<Texture2D>("Rabbit");
            beastTypeDic.Add(BeastType.WorkerEcho_Rabbit, Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f)));

            texture2D = Texture2DAB.LoadAsset<Texture2D>("Seal");
            beastTypeDic.Add(BeastType.WorkerEcho_Seal, Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f)));

            texture2D = Texture2DAB.LoadAsset<Texture2D>("Random");
            beastTypeDic.Add(BeastType.WorkerEcho_Random, Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f)));
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
            SlotPrefab = this.PrefabsAB.LoadAsset<GameObject>("Slot");
            this.Refresh();
        }
        #endregion



    }

}
