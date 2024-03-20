using ML.Engine.BuildingSystem;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.TextContent;
using ML.Engine.UI;
using Newtonsoft.Json;
using ProjectOC.InventorySystem.UI;
using ProjectOC.ResonanceWheelSystem.UI;
using ProjectOC.TechTree.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Player.UI
{
    public class PlayerUIPanel : UIBasePanel, ML.Engine.Timer.ITickComponent
    {
        public PlayerCharacter player;

        private IUISelected CurSelected;

        private SelectedButton EnterBuildBtn;
        private SelectedButton EnterTechTreeBtn;
        private SelectedButton EnterInventoryBtn;
        private SelectedButton EnterBeastPanelBtn;
        private SelectedButton CreateWorkerBtn;
        private SelectedButton SaveSystemBtn;

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
                if (ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.WasPressedThisFrame() && BM.Mode == BuildingMode.None)
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
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/TechPointPanel.prefab", this.transform.parent, true).Completed += (handle) =>
                 {
                     var panel = handle.Result.GetComponent<UITechPointPanel>();
                     panel.transform.localScale = Vector3.one;
                     panel.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                     panel.inventory = this.player.Inventory;
                     ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                 };
            };

            this.EnterInventoryBtn = btnList.Find("EnterInventory").GetComponent<SelectedButton>();
            this.EnterInventoryBtn.OnInteract += () =>
            {
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIInfiniteInventoryPanel.prefab", this.transform.parent, true).Completed += (handle) =>
                {
                    var panel = handle.Result.GetComponent<UIInfiniteInventory>();
                    panel.transform.localScale = Vector3.one;
                    panel.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                    panel.inventory = this.player.Inventory as ML.Engine.InventorySystem.InfiniteInventory;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                };
            };

            this.EnterBeastPanelBtn = btnList.Find("EnterBeastPanel").GetComponent<SelectedButton>();
            this.EnterBeastPanelBtn.OnInteract += () =>
            {
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/BeastPanel.prefab", this.transform.parent, true).Completed += (handle) =>
                {
                    var panel = handle.Result.GetComponent<BeastPanel>();
                    panel.transform.localScale = Vector3.one;
                    panel.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                };
            };


            this.CreateWorkerBtn = btnList.Find("CreateWorker").GetComponent<SelectedButton>();
            this.CreateWorkerBtn.OnInteract += () =>
            {
                ProjectOC.ManagerNS.LocalGameManager.Instance.WorkerManager.SpawnWorker(player.transform.position, player.transform.rotation);
            };

            this.SaveSystemBtn = btnList.Find("SaveSystem").GetComponent<SelectedButton>();
            this.SaveSystemBtn.OnInteract += () =>
            {
                if (GameManager.Instance.SaveManager.SaveController.GetSaveData<TestSaveData>() == null)
                {
                    GameManager.Instance.SaveManager.SaveController.SelectSaveDataFolder(0, null);
                    TestSaveData data = new TestSaveData();
                    data.AddToSaveSystem();
                }
            };


            var btns = btnList.GetComponentsInChildren<SelectedButton>();

            for (int i = 0; i < btns.Length; ++i)
            {
                int last = (i - 1 + btns.Length) % btns.Length;
                int next = (i + 1 + btns.Length) % btns.Length;

                btns[i].UpUI = btns[last];
                btns[i].DownUI = btns[next];



            }

            foreach (var btn in btns)
            {
                btn.OnSelectedEnter += () => { btn.image.color = Color.red; };
                btn.OnSelectedExit += () => { btn.image.color = Color.white; };
            }

            this.CurSelected = EnterBuildBtn;
            this.CurSelected.SelectedEnter();

            IsInit = true;

            Refresh();
        }

        public void Tick(float deltatime)
        {
            if(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.WasPressedThisFrame())
            {
                this.CurSelected.Interact();
            }
            if(Input.InputManager.PlayerInput.PlayerUI.AlterSelected.WasPressedThisFrame())
            {
                var vec2 = Input.InputManager.PlayerInput.PlayerUI.AlterSelected.ReadValue<Vector2>();
                if(vec2.y > 0.1f)
                {
                    this.CurSelected.SelectedExit();
                    this.CurSelected = this.CurSelected.UpUI;
                    this.CurSelected.SelectedEnter();
                }
                else if(vec2.y < -0.1f)
                {
                    this.CurSelected.SelectedExit();
                    this.CurSelected = this.CurSelected.DownUI;
                    this.CurSelected.SelectedEnter();
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

        public Dictionary<string, TextTip> TipDict = new Dictionary<string, TextTip>();

        public ML.Engine.ABResources.ABJsonAssetProcessor<ML.Engine.TextContent.TextTip[]> ABJAProcessor;

        public void InitUITextContents()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ML.Engine.TextContent.TextTip[]>("OC/Json/TextContent/PlayerUIPanel", "PlayerUIPanel", (datas) =>
            {
                foreach (var tip in datas)
                {
                    TipDict.Add(tip.name, tip);
                }
                this.Refresh();
                this.enabled = false;
            }, "PlayerUIPanel");
            ABJAProcessor.StartLoadJsonAssetData();
        }

        public override void Refresh()
        {
            if(ABJAProcessor != null && ABJAProcessor.IsLoaded && IsInit)
            {
                this.EnterBuildBtn.text.text = TipDict["enterbuild"].GetDescription();
                this.EnterTechTreeBtn.text.text = TipDict["techtree"].GetDescription();
                this.EnterInventoryBtn.text.text = TipDict["inventory"].GetDescription();
                this.EnterBeastPanelBtn.text.text = TipDict["beastpanel"].GetDescription();
                this.CreateWorkerBtn.text.text = TipDict["worker"].GetDescription();
                this.SaveSystemBtn.text.text = TipDict["save"].GetDescription();
            }
        }
    }
}

