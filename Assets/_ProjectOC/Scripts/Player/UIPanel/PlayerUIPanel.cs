using ML.Engine.BuildingSystem;
using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using ML.Engine.UI;
using Newtonsoft.Json;
using ProjectOC.InventorySystem.UI;
using ProjectOC.ResonanceWheelSystem.UI;
using ProjectOC.StoreNS;
using ProjectOC.TechTree.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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
                AssetBundleRequest request = null;
                request = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<GameObject>("UI/UIPanel", "TechPointPanel", (ao) =>
                 {
                     var panel = GameObject.Instantiate((request.asset as GameObject).GetComponent<UITechPointPanel>());
                     panel.transform.SetParent(this.transform.parent, false);
                     panel.inventory = this.player.Inventory;
                     ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                 });
            };

            this.EnterInventoryBtn = btnList.Find("EnterInventory").GetComponent<SelectedButton>();
            this.EnterInventoryBtn.OnInteract += () =>
            {
                AssetBundleRequest request = null;
                request = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<GameObject>("UI/UIPanel", "UIInfiniteInventoryPanel", (ao) =>
                {
                    // 实例化
                    var panel = GameObject.Instantiate((request.asset as GameObject).GetComponent<UIInfiniteInventory>());
                    panel.transform.SetParent(this.transform.parent, false);

                    // 初始化
                    panel.inventory = this.player.Inventory as ML.Engine.InventorySystem.InfiniteInventory;

                    // Push
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                });

            };

            this.EnterBeastPanelBtn = btnList.Find("EnterBeastPanel").GetComponent<SelectedButton>();
            this.EnterBeastPanelBtn.OnInteract += () =>
            {
                AssetBundleRequest request = null;
                request = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<GameObject>("UI/UIPanel", "BeastPanel", (ao) =>
                {
                    // 实例化
                    var panel = GameObject.Instantiate((request.asset as GameObject).GetComponent<BeastPanel>());

                    // Push
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(GameObject.Instantiate(panel.gameObject, GameObject.Find("Canvas").transform, false).GetComponent<ML.Engine.UI.UIBasePanel>());
                    //ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                });

            };


            this.CreateWorkerBtn = btnList.Find("CreateWorker").GetComponent<SelectedButton>();
            this.CreateWorkerBtn.OnInteract += () =>
            {
                ProjectOC.ManagerNS.LocalGameManager.Instance.WorkerManager.SpawnWorker(player.transform.position, player.transform.rotation);
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
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ML.Engine.TextContent.TextTip[]>("Json/TextContent/Player", "PlayerUIPanel", (datas) =>
                {
                    foreach (var tip in datas)
                    {
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

        public override void Refresh()
        {
            if(ABJAProcessor != null && ABJAProcessor.IsLoaded && IsInit)
            {
                this.EnterBuildBtn.text.text = TipDict["enterbuild"].GetDescription();
                this.EnterTechTreeBtn.text.text = TipDict["techtree"].GetDescription();
                this.EnterInventoryBtn.text.text = TipDict["inventory"].GetDescription();
                this.EnterBeastPanelBtn.text.text = TipDict["beastpanel"].GetDescription();
                this.CreateWorkerBtn.text.text = TipDict["worker"].GetDescription();
            }
        }
    }
}

