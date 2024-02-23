using ML.Engine.BuildingSystem;
using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using ML.Engine.UI;
using Newtonsoft.Json;
using ProjectOC.InventorySystem.UI;
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
        private SelectedButton CreateWorkerBtn;
        private SelectedButton AddItemBtn;

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

                    // 初始化
                    panel.inventory = this.player.Inventory as ML.Engine.InventorySystem.InfiniteInventory;

                    // Push
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                });

            };

            this.CreateWorkerBtn = btnList.Find("CreateWorker").GetComponent<SelectedButton>();
            this.CreateWorkerBtn.OnInteract += () =>
            {
                ProjectOC.ManagerNS.LocalGameManager.Instance.WorkerManager.SpawnWorker(player.transform.position, player.transform.rotation);
            };

            this.AddItemBtn = btnList.Find("AddItem").GetComponent<SelectedButton>();
            this.AddItemBtn.OnInteract += () =>
            {

                Debug.Log($"Pre {player.Inventory.GetItemAllNum("Item_Material_1")} {player.Inventory.GetItemAllNum("Item_Material_37")}");

                Player.PlayerCharacter Player = GameObject.Find("PlayerCharacter")?.GetComponent<Player.PlayerCharacter>();
                string itemID = "Item_Material_1";
                int amount = 100;
                Debug.Log($"AddItem1 {ItemManager.Instance.IsValidItemID(itemID)}");
                if (ItemManager.Instance.IsValidItemID(itemID))//
                {
                    Debug.Log("AddItem2");
                    List<Item> items = ItemManager.Instance.SpawnItems(itemID, amount);
                    foreach (Item item in items)
                    {
                        Player.Inventory.AddItem(item);
                    }
                }

                itemID = "Item_Material_37";
                if (ItemManager.Instance.IsValidItemID(itemID))
                {
                    Debug.Log("AddItem3");
                    List<Item> items = ItemManager.Instance.SpawnItems(itemID, amount);
                    foreach (Item item in items)
                    {
                        Player.Inventory.AddItem(item);
                    }
                }

                Debug.Log($"Post {player.Inventory.GetItemAllNum("Item_Material_1")} {player.Inventory.GetItemAllNum("Item_Material_37")}");
                IInventory inventory123 = player.Inventory;
                Debug.Log("CanComposite " + inventory123.GetItemAllNum("Item_Material_1") + " " + inventory123.GetItemAllNum("Item_Material_37"));


                foreach (var item in inventory123.GetItemList())
                {
                    if (item != null)
                    {
                        //Debug.Log(item.ID + ": " + inventory123.GetItemAllNum(item.ID));
                        Debug.Log(item.ID + ": " + item.Amount);
                    }
                }

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

        private void Refresh()
        {
            if(ABJAProcessor != null && ABJAProcessor.IsLoaded && IsInit)
            {
                this.EnterBuildBtn.text.text = TipDict["enterbuild"].GetDescription();
                this.EnterTechTreeBtn.text.text = TipDict["techtree"].GetDescription();
                this.EnterInventoryBtn.text.text = TipDict["inventory"].GetDescription();
                this.CreateWorkerBtn.text.text = TipDict["worker"].GetDescription();
                this.AddItemBtn.text.text = TipDict["additem"].GetDescription();
            }
        }
    }
}

