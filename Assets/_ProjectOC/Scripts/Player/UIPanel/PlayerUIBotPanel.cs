using ML.Engine.BuildingSystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using ML.Engine.UI;
using ProjectOC.InventorySystem.UI;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using ProjectOC.ResonanceWheelSystem.UI;
using ProjectOC.TechTree.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            TimeText = this.transform.Find("Time").Find("Text").GetComponent<TextMeshProUGUI>();
            this.Ring = this.transform.Find("Ring");
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
            this.UIBtnList.EnableBtnList();
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            this.player.interactComponent.Enable();
            base.Enter();   
        }

        protected override void Exit()
        {
            this.UIBtnList.DisableBtnList();
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
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
            if(LocalGameManager.Instance!=null)
                TimeText.text = LocalGameManager.Instance.DispatchTimeManager.CurrentTimeFrame.ToString()+" : "+ LocalGameManager.Instance.DispatchTimeManager.CurrentMinute.ToString();
        }

        #endregion
        #region Internal

        protected override void UnregisterInput()
        {
            this.UIBtnList.RemoveAllListener();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.Disable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Disable();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.OpenMenu.started -= OpenMenu_started;
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.OpenMap.started -= OpenMap_started;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        protected override void RegisterInput()
        {
            this.UIBtnList.SetBtnAction("����",
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
            this.UIBtnList.SetBtnAction("ѡ��",
            () =>
            {
                GameManager.Instance.EnterPoint.GetOptionPanelInstance().Completed += (handle) =>
                {
                    // ʵ����
                    var panel = handle.Result.GetComponent<OptionPanel>();

                    panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                    GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            this.UIBtnList.SetBtnAction("����",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, new UIManager.FloatTextUIData("����"));
            }
            );
            this.UIBtnList.SetBtnAction("�ҵ�����",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, new UIManager.FloatTextUIData("�ҵ�����"));
            }
            );
            this.UIBtnList.SetBtnAction("��������",
            () =>
            {
                GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/OrderBoardPanel.prefab").Completed += (handle) =>
                {
                    OrderBoardPanel orderBoardPanel = handle.Result.GetComponent<OrderBoardPanel>();

                    orderBoardPanel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(orderBoardPanel);
                };
            }
            );
            this.UIBtnList.SetBtnAction("��������",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.SideBarUI, new UIManager.SideBarUIData("<color=yellow>��������</color>  ��������", "��������"));
            }
            );
            this.UIBtnList.SetBtnAction("�Ƽ���",
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
            this.UIBtnList.SetBtnAction("����",
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
                        Debug.LogWarning("��ǰ����������Ϊ0���޷����뽨��ģʽ!");
                    }
                }
            }
            );
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.Enable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Enable();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.OpenMenu.started += OpenMenu_started;
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.OpenMap.started += OpenMap_started;


            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }

        private void OpenMenu_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.UIBtnList.BindNavigationInputAction(ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.SelectGrid, UIBtnListContainer.BindType.performed);
            this.UIBtnList.BindButtonInteractInputAction(ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.SelectGrid, UIBtnListContainer.BindType.canceled,
                null, () => { 
                    Ring.gameObject.SetActive(false);
                    ProjectOC.Input.InputManager.PlayerInput.Player.Enable();
                    this.UIBtnList.SetCurSelectedNull();
                    this.UIBtnList.DeBindInputAction();
                });
            this.Ring.gameObject.SetActive(true);
            Debug.Log("OpenMenu_started " + UIKeyTipList);
            this.UIKeyTipList.RefreshKetTip();
            ProjectOC.Input.InputManager.PlayerInput.Player.Disable();

        }

        private void OpenMap_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, new UIManager.FloatTextUIData("�򿪵�ͼ"));
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(this.Ring.gameObject.activeInHierarchy == true)
            {
                Ring.gameObject.SetActive(false);
                ProjectOC.Input.InputManager.PlayerInput.Player.Enable();
                this.UIBtnList.SetCurSelectedNull();
                this.UIBtnList.DeBindInputAction();
            }
        }

        #endregion

        #region UI��������
        private Transform Ring;
        #endregion

        #region TextContent
        [System.Serializable]
        public struct PlayerUIBotPanelStruct
        {
            public TextTip[] Btns;
            public KeyTip SelectGrid;
        }

        protected override void OnLoadJsonAssetComplete(PlayerUIBotPanelStruct datas)
        {
            base.OnLoadJsonAssetComplete(datas);
            InitBtnData(datas);
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/PlayerUIBotPanel";
            this.abname = "PlayerUIBotPanel";
            this.description = "PlayerUIBotPanel���ݼ������";
        }
        private UIBtnList UIBtnList;
        protected override void InitBtnInfo()
        {
            UIBtnListInitor uIBtnListInitor = this.transform.GetComponentInChildren<UIBtnListInitor>(true);
            this.UIBtnList = new UIBtnList(uIBtnListInitor.transform, uIBtnListInitor.btnListInitData);
        }
        private void InitBtnData(PlayerUIBotPanelStruct datas)
        {
            foreach (var tt in datas.Btns)
            {
                this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            }
        }
        #endregion
    }

}
