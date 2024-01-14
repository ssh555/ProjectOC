using ML.Engine.BuildingSystem;
using ML.Engine.TextContent;
using ML.Engine.UI;
using Newtonsoft.Json;
using ProjectOC.TechTree.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ML.Engine.BuildingSystem.MonoBuildingManager;

namespace ProjectOC.Player.UI
{
    public class PlayerUIPanel : UIBasePanel, ML.Engine.Timer.ITickComponent
    {
        public PlayerCharacter player;

        public UITechPointPanel uITechPointPanel;

        public InventorySystem.UI.UIInfiniteInventory uIInfiniteInventory;

        public ResonanceWheelSystem.UI.ResonanceWheelUI uIResonanceWheel;

        private IUISelected CurSelected;

        private SelectedButton EnterBuildBtn;
        private SelectedButton EnterTechTreeBtn;
        private SelectedButton EnterInventoryBtn;
        private SelectedButton EnterResonanceWheelBtn;
        private BuildingManager BM => BuildingManager.Instance;

        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public bool IsInit = false;
        private void Start()
        {
            InitUITextContents();

            var btnList = this.transform.Find("ButtonList");
            this.EnterBuildBtn = btnList.Find("EnterBuild").GetComponent<SelectedButton>();
            this.EnterBuildBtn.OnInteract += () =>
            {
                if (ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.WasPressedThisFrame() && BM.Mode == BuildingMode.None)
                {
                    if (BM.GetRegisterBPartCount() > 0)
                    {
                        BM.Mode = BuildingMode.Interact;
                    }
                    else
                    {
                        Debug.LogWarning("当前建筑物数量为0，无法进入建造模式!");
                    }
                }
            };



            this.EnterTechTreeBtn = btnList.Find("EnterTechTree").GetComponent<SelectedButton>();
            this.EnterTechTreeBtn.OnInteract += () =>
            {
                var panel = GameObject.Instantiate(uITechPointPanel);
                panel.transform.SetParent(this.transform.parent, false);
                panel.inventory = this.player.Inventory;
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
            };

            this.EnterInventoryBtn = btnList.Find("EnterInventory").GetComponent<SelectedButton>();
            this.EnterInventoryBtn.OnInteract += () =>
            {
                // 实例化
                var panel = GameObject.Instantiate(uIInfiniteInventory, this.transform.parent, false);

                // 初始化
                panel.inventory = this.player.Inventory as ML.Engine.InventorySystem.InfiniteInventory;

                // Push
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                
            };


            this.EnterResonanceWheelBtn = btnList.Find("EnterResonanceWheel").GetComponent<SelectedButton>();
            this.EnterResonanceWheelBtn.OnInteract += () =>
            {
                Debug.Log("werwerw");
                var panel = GameObject.Instantiate(uIResonanceWheel);
                panel.transform.SetParent(this.transform.parent, false);
                panel.inventory = this.player.Inventory;




                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                Debug.Log("2 " + ML.Engine.Manager.GameManager.Instance.UIManager.GetTopUIPanel());

            };

            var btn = btnList.GetComponentsInChildren<SelectedButton>();
            
            for(int i = 0; i < btn.Length; ++i)
            {
                int last = (i - 1 + btn.Length) % btn.Length;
                int next = (i + 1 + btn.Length) % btn.Length;

                btn[i].UpUI = btn[last];
                btn[i].DownUI = btn[next];
            }

            this.CurSelected = EnterBuildBtn;
            this.CurSelected.OnSelectedEnter();

            IsInit = true;

            Refresh();
        }

        public void Tick(float deltatime)
        {
            if(ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.WasPressedThisFrame())
            {
                this.CurSelected.Interact();
            }
            if(Input.InputManager.PlayerInput.PlayerUI.AlterSelected.WasPressedThisFrame())
            {
                var vec2 = Input.InputManager.PlayerInput.PlayerUI.AlterSelected.ReadValue<Vector2>();
                if(vec2.y > 0.1f)
                {
                    this.CurSelected.OnSelectedExit();
                    this.CurSelected = this.CurSelected.UpUI;
                    this.CurSelected.OnSelectedEnter();
                }
                else if(vec2.y < -0.1f)
                {
                    this.CurSelected.OnSelectedExit();
                    this.CurSelected = this.CurSelected.DownUI;
                    this.CurSelected.OnSelectedEnter();
                }
            }
            if (ML.Engine.Input.InputManager.Instance.Common.Common.Back.WasPressedThisFrame())
            {
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
            }
        }

        private void OnDestroy()
        {
            (this as ML.Engine.Timer.ITickComponent).DisposeTick();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Input.InputManager.PlayerInput.PlayerUI.Enable();
            Refresh();
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        }

        public override void OnPause()
        {
            base.OnPause();
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
            Input.InputManager.PlayerInput.PlayerUI.Disable();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            Input.InputManager.PlayerInput.PlayerUI.Enable();
            Refresh();
        }

        public override void OnExit()
        {
            base.OnExit();
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
            Input.InputManager.PlayerInput.PlayerUI.Disable();
        }

        public static Dictionary<string, TextTip> TipDict = new Dictionary<string, TextTip>();

        public static ML.Engine.ABResources.ABJsonAssetProcessor<ML.Engine.TextContent.TextTip[]> ABJAProcessor;

        public void InitUITextContents()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ML.Engine.TextContent.TextTip[]>("Binary/TextContent/Player", "PlayerUIPanel", (datas) =>
                {
                    foreach (var tip in datas)
                    {
                        Debug.Log(tip.name+tip);
                        TipDict.Add(tip.name, tip);
                    }
                    this.Refresh();
                    this.enabled = false;
                }, null, "PlayerUIPanel");
                ABJAProcessor.StartLoadJsonAssetData();
            }
            else
            {
                this.Refresh();
                this.enabled = false;
            }
        }

        private void Refresh()
        {
            

            if(ABJAProcessor != null && ABJAProcessor.IsLoaded && IsInit)
            {
                this.EnterBuildBtn.text.text = TipDict["enterbuild"].GetDescription();
                this.EnterTechTreeBtn.text.text = TipDict["entertechtree"].GetDescription();
                this.EnterInventoryBtn.text.text = TipDict["enterinventory"].GetDescription();
                this.EnterResonanceWheelBtn.text.text = TipDict["enterresonancewheel"].GetDescription();
            }
        }
    }
}

