using ML.Engine.BuildingSystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using ML.Engine.UI;
using ProjectOC.InventorySystem.UI;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using ProjectOC.TechTree.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.Player.UI.PlayerUIBotPanel;



namespace ProjectOC.Player.UI
{
    public class PlayerUIBotPanel : ML.Engine.UI.UIBasePanel<PlayerUIBotPanelStruct>, ITickComponent
    {
        #region Unity
        public bool IsInit = false;
        public PlayerCharacter player;
        protected override void Awake()
        {
            base.Awake();
            Ring = this.transform.Find("Ring");
            btnList = Ring.Find("ButtonList");
            TimeText = this.transform.Find("Time").Find("Text").GetComponent<TextMeshProUGUI>();
        }

        protected override void Start()
        {
            IsInit = true;
            Refresh();
            base.Start();
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            base.OnEnter();
            this.Enter();
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
            base.OnPause();
            this.Exit();
        }

        public override void OnRecovery()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            base.OnRecovery();
            this.Enter();
        }
        protected override void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.Enable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Enable();
            this.player.interactComponent.Enable();
            base.Enter();   
        }

        protected override void Exit()
        {
            this.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.Disable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Disable();
            //TODO player null
            this.player?.interactComponent.Disable();
            base.Exit();
        }
        #endregion
        #region Tick
        private TextMeshProUGUI TimeText;
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void Tick(float deltatime)
        {
            TimeText.text = LocalGameManager.Instance.DispatchTimeManager.CurrentTimeFrame.ToString()+" : "+"分";
        }

        #endregion
        #region Internal

        private void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.OpenMenu.started -= OpenMenu_started;
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.OpenMap.started -= OpenMap_started;
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.SelectGrid.performed -= SelectGrid_performed;
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.SelectGrid.canceled -= SelectGrid_canceled;
        }

        private void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.OpenMenu.started += OpenMenu_started;
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.OpenMap.started += OpenMap_started;
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.SelectGrid.performed += SelectGrid_performed;
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.SelectGrid.canceled += SelectGrid_canceled;
        }

        private void OpenMenu_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.Ring.gameObject.SetActive(true);
        }

        private void OpenMap_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, "打开地图！");
        }


        private void SelectGrid_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (Ring.gameObject.activeInHierarchy == false) return;
            Vector2 vector2 = obj.ReadValue<Vector2>();

            float angle = Mathf.Atan2(vector2.x, vector2.y);

            angle = angle * 180 / Mathf.PI;
            if (angle < 0)
            {
                angle = angle + 360;
            }

            if (angle < 22.5 || angle > 337.5) this.UIBtnList.MoveIndexIUISelected(0);
            else if (angle > 22.5 && angle < 67.5) this.UIBtnList.MoveIndexIUISelected(7);
            else if (angle > 67.5 && angle < 112.5) this.UIBtnList.MoveIndexIUISelected(6);
            else if (angle > 112.5 && angle < 157.5) this.UIBtnList.MoveIndexIUISelected(5);
            else if (angle > 157.5 && angle < 202.5) this.UIBtnList.MoveIndexIUISelected(4);
            else if (angle > 202.5 && angle < 247.5) this.UIBtnList.MoveIndexIUISelected(3);
            else if (angle > 247.5 && angle < 292.5) this.UIBtnList.MoveIndexIUISelected(2);
            else if (angle > 292.5 && angle < 337.5) this.UIBtnList.MoveIndexIUISelected(1);
        }
        private void SelectGrid_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (Ring.gameObject.activeInHierarchy == false) return;
            this.UIBtnList.GetCurSelected().Interact();
            this.UIBtnList.SetCurSelectedNull();
            Ring.gameObject.SetActive(false);
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
        private Transform Ring;
        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }


        }
        #endregion



        #region TextContent
        [System.Serializable]
        public struct PlayerUIBotPanelStruct
        {
            public TextTip[] Btns;
        }

        protected override void OnLoadJsonAssetComplete(PlayerUIBotPanelStruct datas)
        {
            InitBtnData(datas);
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/PlayerUIBotPanel";
            this.abname = "PlayerUIBotPanel";
            this.description = "PlayerUIBotPanel数据加载完成";
        }
        private Transform btnList;
        private UIBtnList UIBtnList;
        private void InitBtnData(PlayerUIBotPanelStruct datas)
        {
            UIBtnList = new UIBtnList(parent: btnList,hasInitSelect: false);
            foreach (var tt in datas.Btns)
            {
                this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            }

            this.UIBtnList.SetBtnAction("背包",
            () =>
            {
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIInfiniteInventoryPanel.prefab", this.transform.parent, true).Completed += (handle) =>
                {
                    var panel = handle.Result.GetComponent<UIInfiniteInventory>();
                    panel.transform.localScale = Vector3.one;
                    panel.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                    panel.inventory = this.player.Inventory as ML.Engine.InventorySystem.InfiniteInventory;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            this.UIBtnList.SetBtnAction("选项",
            () =>
            {
                GameManager.Instance.EnterPoint.GetOptionPanelInstance().Completed += (handle) =>
                {
                    // 实例化
                    var panel = handle.Result.GetComponent<OptionPanel>();

                    panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                    GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            this.UIBtnList.SetBtnAction("友人",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, "友人！");
            }
            );
            this.UIBtnList.SetBtnAction("我的氏族",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, "我的氏族！");
            }
            );
            this.UIBtnList.SetBtnAction("订单管理",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, "订单管理！");
            }
            );
            this.UIBtnList.SetBtnAction("生产管理",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, "生产管理！");
            }
            );
            this.UIBtnList.SetBtnAction("科技树",
            () =>
            {
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/TechPointPanel.prefab", this.transform.parent, true).Completed += (handle) =>
                {
                    var panel = handle.Result.GetComponent<UITechPointPanel>();
                    panel.transform.localScale = Vector3.one;
                    panel.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                    panel.inventory = this.player.Inventory;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            this.UIBtnList.SetBtnAction("建造",
            () =>
            {
                if (BuildingManager.Instance.Mode == BuildingMode.None)
                {
                    if (BuildingManager.Instance.GetRegisterBPartCount() > 0)
                    {
                        BuildingManager.Instance.Mode = BuildingMode.Interact;
                    }
                    else
                    {
                        Debug.LogWarning("当前建筑物数量为0，无法进入建造模式!");
                    }
                }
            }
            );

        }
        #endregion



    }

}
