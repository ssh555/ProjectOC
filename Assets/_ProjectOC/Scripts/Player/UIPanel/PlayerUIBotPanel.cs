using ML.Engine.BuildingSystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using ML.Engine.UI;
using ProjectOC.InventorySystem.UI;
using ProjectOC.MainInteract.UI;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using ProjectOC.ResonanceWheelSystem.UI;
using ProjectOC.TechTree.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ML.Engine.UI.UIBtnListContainer;
using static ProjectOC.Player.UI.PlayerUIBotPanel;
using Vector3 = UnityEngine.Vector3;



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
            this.Ring.gameObject.SetActive(false);
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
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            this.player.interactComponent.Enable();
            base.Enter();   
        }

        protected override void Exit()
        {
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
                TimeText.text = LocalGameManager.Instance.DispatchTimeManager.CurrentHour.ToString()+" : "+ LocalGameManager.Instance.DispatchTimeManager.CurrentMinute.ToString();
        }

        #endregion

        #region Internal

        protected override void UnregisterInput()
        {
            this.UIBtnListContainer.DisableUIBtnListContainer();
            ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm.performed -= LastTerm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm.performed -= NextTerm_performed;


            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.Disable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Disable();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.OpenMenu.started -= OpenMenu_started;
            ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.OpenMap.started -= OpenMap_started;


            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }



        protected override void RegisterInput()
        {
            ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm.performed += LastTerm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm.performed += NextTerm_performed;

            this.UIBtnListContainer.SetBtnAction("����",
            () =>
            {
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefabs_UIInfiniteInventoryPanel", this.transform.parent, true).Completed += (handle) =>
                {
                    var panel = handle.Result.GetComponent<UIInfiniteInventory>();
                    panel.transform.localScale = Vector3.one;
                    panel.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                    panel.inventory = this.player.Inventory as ML.Engine.InventorySystem.InfiniteInventory;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                };
            }                                                                         
            );
            this.UIBtnListContainer.SetBtnAction("ѡ��",
            () =>
            {
                GameManager.Instance.EnterPoint.GetOptionPanelInstance().Completed += (handle) =>
                {
                    // ʵ����
                    var panel = handle.Result.GetComponent<OptionPanel>();

                    panel.transform.SetParent(GameManager.Instance.UIManager.NormalPanel, false);

                    GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            this.UIBtnListContainer.SetBtnAction("����",
            () =>
            {
                // GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, new UIManager.FloatTextUIData("����"));
                // ��ʱ��������
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
                LocalGameManager.Instance.PinchFaceManager.GeneratePinchRaceUI();
            }
            );
            this.UIBtnListContainer.SetBtnAction("�ҵ�����",
            () =>
            {
                //GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.FloatTextUI, new UIManager.FloatTextUIData("�ҵ�����"));
                GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Mine_UIPanel/Prefab_Mine_UI_SmallMapPanel.prefab").Completed += (handle) =>
                {
                    var panel = handle.Result.GetComponent<UISmallMapPanel>();
                    panel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false);
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            this.UIBtnListContainer.SetBtnAction("��������",
            () =>
            {
                GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Order_UIPanel/Prefab_OrderSystem_UI_OrderBoardPanel.prefab").Completed += (handle) =>
                {
                    UIOrderBoardPanel orderBoardPanel = handle.Result.GetComponent<UIOrderBoardPanel>();

                    orderBoardPanel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false);
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(orderBoardPanel);
                };
            }
            );
            

            this.UIBtnListContainer.SetBtnAction("��������",
            () =>
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.SideBarUI, new UIManager.SideBarUIData("<color=yellow>��������</color>  ��������", "��������",null));
            }
            );
            this.UIBtnListContainer.SetBtnAction("�Ƽ���",
            () =>
            {
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefabs_TechTree_UI/Prefab_TechTree_UI_TechPointPanel.prefab", this.transform.parent, true).Completed += (handle) =>
                {
                    var panel = handle.Result.GetComponent<UITechPointPanel>();
                    panel.transform.localScale = Vector3.one;
                    panel.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                    panel.inventory = this.player.Inventory;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            this.UIBtnListContainer.SetBtnAction("����",
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

            this.UIBtnListContainer.SetBtnAction("ͨѶ",
            () =>
            {
                Debug.Log("ͨѶ");
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
            this.UIBtnListContainer.SetIsEnableTrue();
            this.UIBtnListContainer.SetAllBtnListDoSomething((btnlist) => { btnlist.BindNavigationInputAction(ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.SelectGrid, UIBtnListContainer.BindType.performed); });
            this.UIBtnListContainer.FindEnterableUIBtnList();
            this.UIBtnListContainer.BindButtonInteractInputAction(ProjectOC.Input.InputManager.PlayerInput.PlayerUIBot.SelectGrid, UIBtnListContainer.BindType.canceled,
                () => { 
                    this.UIBtnListContainer.SetIsEnableFalse();
                }, () => {
                    Ring.gameObject.SetActive(false);
                    (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).currentCharacter.interactComponent.Enable();
                    ProjectOC.Input.InputManager.PlayerInput.Player.Enable();
                    this.UIBtnListContainer.SetAllBtnListDoSomething((btnlist) => { btnlist.SetCurSelectedNull(); });
                    this.UIBtnListContainer.DisableUIBtnListContainer();
                });
            BtnListIndex = 0;
            for (int i = 0; i < UIBtnListContainer.Parent.childCount; ++i)
            {
                UIBtnListContainer.Parent.GetChild(i).gameObject.SetActive(i == BtnListIndex);
            }
            this.Ring.gameObject.SetActive(true);
            (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).currentCharacter.interactComponent.Disable();
            this.UIKeyTipList?.RefreshKeyTip();
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
                (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).currentCharacter.interactComponent.Enable();
            }
        }

        private void LastTerm_performed(InputAction.CallbackContext context)
        {
            BtnListIndex = (BtnListIndex + UIBtnListContainer.UIBtnLists.Count - 1) % UIBtnListContainer.UIBtnLists.Count;
            this.Refresh();
        }

        private void NextTerm_performed(InputAction.CallbackContext context)
        {
            BtnListIndex = (BtnListIndex + 1) % UIBtnListContainer.UIBtnLists.Count;
            this.Refresh();
        }
        #endregion

        #region UI��������
        private Transform Ring;
        //��ǰѡ��btnlist��index
        private int BtnListIndex = 0;

        public override void Refresh()
        {
            this.UIBtnListContainer.MoveToBtnList(UIBtnListContainer.UIBtnLists[BtnListIndex]);
            for(int i=0;i< UIBtnListContainer.Parent.childCount;++i)
            {
                UIBtnListContainer.Parent.GetChild(i).gameObject.SetActive(i == BtnListIndex);
            }
        }

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
            this.abpath = "OCTextContent/PlayerUIBotPanel";
            this.abname = "PlayerUIBotPanel";
            this.description = "PlayerUIBotPanel���ݼ������";
        }
        [ShowInInspector]
        private UIBtnListContainer UIBtnListContainer;
        protected override void InitBtnInfo()
        {
            UIBtnListContainerInitor uiBtnListContainerInitor = this.transform.GetComponentInChildren<UIBtnListContainerInitor>(true);
            this.UIBtnListContainer = new UIBtnListContainer(uiBtnListContainerInitor);
        }
        private void InitBtnData(PlayerUIBotPanelStruct datas)
        {
            foreach (var tt in datas.Btns)
            {
                this.UIBtnListContainer.SetAllBtnListDoSomething((btnlist) => { btnlist.SetBtnText(tt.name, tt.description.GetText()); });
            }
        }
        #endregion
    }

}
