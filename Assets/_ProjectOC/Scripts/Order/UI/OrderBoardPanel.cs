using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using ML.Engine.UI;
using ML.Engine.Utility;
using ProjectOC.ManagerNS;
using ProjectOC.MissionNS;
using ProjectOC.Order;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;
using UnityEngine.UI;
using static ML.Engine.UI.UIBtnListContainer;
using static OrderBoardPanel;
using static ProjectOC.Order.OrderManager;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class OrderBoardPanel : UIBasePanel<OrderBoardPanelStruct>
{
    #region Unity
    protected override void Awake()
    {
        base.Awake();
        this.Function = transform.Find("FunctionType").Find("Content").Find("Function");
        this.FunctionPanel = transform.Find("FunctionPanel");
        this.FunctionType = this.Function.transform.childCount;

        this.AcceptedOrder = this.FunctionPanel.Find("AcceptedOrder");
        this.OrderDelegation = this.FunctionPanel.Find("OrderDelegation");

        this.AcceptedOrderOrderInfo = this.AcceptedOrder.Find("OrderInfo");
        this.OrderDelegationOrderInfo = this.OrderDelegation.Find("OrderDelegation").Find("Panel1").Find("OrderInfo");
    }

    #endregion

    #region Override
    public override void OnEnter()
    {
        base.OnEnter();
        OrderManager.Instance.OnCanBeCommitRefresh += Refresh;
    }
    public override void OnExit()
    {
        base.OnExit();
        OrderManager.Instance.OnCanBeCommitRefresh -= Refresh;
    }
    protected override void Exit()
    {
        base.Exit();
        ClearTemp();
    }

    #endregion

    #region Internal
    protected override void UnregisterInput()
    {
        // �л���Ŀ
        ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm.performed -= LastTerm_performed;
        ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm.performed -= NextTerm_performed;

        ML.Engine.Input.InputManager.Instance.Common.Common.MainInteract.performed -= MainInteract_performed;
        ML.Engine.Input.InputManager.Instance.Common.Common.SubInteract.performed -= SubInteract_performed;

        this.ClanBtnList.RemoveAllListener();
        this.ClanBtnList.DeBindInputAction();

        this.AcceptedOrderBtnList.RemoveAllListener();
        this.AcceptedOrderBtnList.DeBindInputAction();

        // ����
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

        this.OrderDelegationUIBtnListContainer.DisableUIBtnListContainer();
    }

    protected override void RegisterInput()
    {
        // �л���Ŀ
        ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm.performed += LastTerm_performed;
        ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm.performed += NextTerm_performed;

        ML.Engine.Input.InputManager.Instance.Common.Common.MainInteract.performed += MainInteract_performed;
        ML.Engine.Input.InputManager.Instance.Common.Common.SubInteract.performed += SubInteract_performed;

        this.ClanBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
        this.ClanBtnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnListContainer.BindType.performed);

        this.AcceptedOrderBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
        this.AcceptedOrderBtnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnListContainer.BindType.performed);

        // ����
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        this.OrderDelegationUIBtnListContainer.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, BindType.started);

    }

    private void LastTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        FunctionIndex = (FunctionIndex + FunctionType - 1) % FunctionType;
        this.Refresh();
    }

    private void NextTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        FunctionIndex = (FunctionIndex + 1) % FunctionType;
        this.Refresh();
    }

    private void MainInteract_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
        if(FunctionIndex == 1)
        {
            //�� �ر�������Ϣ
            if (OrderDelegationIndex == 0)
            {
                ClanSelectIndex = ClanSelectIndex == 0 ? 1 : 0;
            }
            else if(OrderDelegationIndex == 1)
            {
                //�нӶ���
                OrderManager.Instance.ReceiveOrder(curSelectedOrderIDInOrderDelegation);
                OrderTableData orderTableData = OrderManager.Instance.GetOrderTableData(curSelectedOrderIDInOrderDelegation);
                if(orderTableData.OrderType == OrderType.Urgent)
                {
                    isNeedRefreshOrderDelegation = true;
                }
                else if(orderTableData.OrderType == OrderType.Normal)
                {
                    this.OrderDelegationUIBtnListContainer.UIBtnLists[1].DeleteButton(curSelectedOrderIDInOrderDelegation);
                }
                
                Debug.Log("�нӶ���");
            }
        }
        else if(FunctionIndex == 0)
        {
            //�㰴�ύ����
            OrderManager.Instance.CommitOrder(curSelectedOrderIDInAcceptedOrder);
            //TODO ��������ύ����
            isNeedRefreshAcceptedOrder = true;
        }
        this.Refresh();
    }

    private void SubInteract_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {

        if (FunctionIndex == 1)
        {
            if (OrderDelegationIndex == 1)
            {
                //�ܾ�����
                Debug.Log("�ܾ����� "+ curSelectedOrderIDInOrderDelegation);
                OrderManager.Instance.RefuseOrder(curSelectedOrderIDInOrderDelegation);
                isNeedRefreshOrderDelegation = true;
            }
        }
        else if (FunctionIndex == 0)
        {
            //ȡ������
            Debug.Log("ȡ������ "+ curSelectedOrderIDInAcceptedOrder);
            OrderManager.Instance.CancleOrder(curSelectedOrderIDInAcceptedOrder);
            isNeedRefreshAcceptedOrder = true;
        }
        this.Refresh();
    }

    private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        UIMgr.PopPanel();
    }
    #endregion


    #region UI
    #region temp
    /*    private Sprite icon_genderfemaleSprite, icon_gendermaleSprite;*/

    private void ClearTemp()
    {
/*        GameManager.DestroyObj(icon_genderfemaleSprite);
        GameManager.DestroyObj(icon_gendermaleSprite);*/
    }

    #endregion

    #region UI��������
    //��ʼѡ�ж���ί��
    [ShowInInspector]
    private int FunctionIndex = 1;
    //��ʼѡ������ѡ��
    [ShowInInspector]
    private int OrderDelegationIndex = 0;
    //��ʼѡ������ѡ��Panel1
    [ShowInInspector]
    private int ClanSelectIndex = 0;

    private int FunctionType;
    private Transform Function;
    private Transform FunctionPanel;

    private Transform AcceptedOrder;
    private Transform OrderDelegation;

    private bool isNeedRefreshAcceptedOrder = true;
    private bool isNeedRefreshOrderDelegation = true;
    private int OrderDelegationCnt = 0;

    private Transform AcceptedOrderOrderInfo;
    private Transform OrderDelegationOrderInfo;

    private bool isInitNormalOrder = false;

    private string curSelectedOrderIDInOrderDelegation 
    {   get 
        {
            SelectedButton btn = null;
            btn = this.OrderDelegationUIBtnListContainer.CurSelectUIBtnList?.GetCurSelected();
            return btn != null ? btn.name : null;
        } 
    }

    private string curSelectedOrderIDInAcceptedOrder
    {
        get
        {
            SelectedButton btn = null;
            btn = this.AcceptedOrderBtnList.GetCurSelected();
            return btn != null ? btn.name : null;
        }
    }


    #endregion

    public override void Refresh()
    {
        if (!this.objectPool.IsLoadFinish())
        {
            return;
        }
        #region FunctionType

        for (int i = 0; i < FunctionType; i++)
        {
            Function.GetChild(i).Find("Selected").gameObject.SetActive(FunctionIndex == i);
            FunctionPanel.GetChild(i).gameObject.SetActive(FunctionIndex == i);
        }
        #endregion

        #region OrderDelegation

        //����ί��ѡ��
        if (FunctionIndex == 1)
        {
            for (int j = 0; j < this.OrderDelegation.childCount; j++)
            {
                this.OrderDelegation.GetChild(j).gameObject.SetActive(this.OrderDelegationIndex == j);
            }
        }

        #region OrderDelegation -> ClanSelect  
        if (FunctionIndex == 1 && OrderDelegationIndex == 0)
        {
            if(ClanSelectIndex == 0)
            {
                this.ClanBtnList.EnableBtnList();
            }
            for (int i = 0; i < this.OrderDelegation.GetChild(0).childCount; i++)
            {
                this.OrderDelegation.GetChild(0).GetChild(i).gameObject.SetActive(this.ClanSelectIndex == i);
            }
        }
        else
        {
            this.ClanBtnList.DisableBtnList();
        }
        #endregion
        #region OrderDelegation -> OrderDelegation 
        
        if (isNeedRefreshOrderDelegation && FunctionIndex == 1 && OrderDelegationIndex == 1)
        {
            //����OrderDelegationUIBtnListContainer
            Debug.Log("����OrderDelegationUIBtnListContainer");
            this.OrderDelegationUIBtnListContainer.SetIsEnableTrue();
            //��ǰ��ѡ�����ID
            string CurSelectedClanID = this.ClanBtnList.GetCurSelected().name;
            UIBtnList.Synchronizer synchronizer = new UIBtnList.Synchronizer(2 + OrderManager.Instance.GetOrderDelegationOrders(CurSelectedClanID, OrderType.Normal).Count, () =>
            {
                this.OrderDelegationUIBtnListContainer.InitBtnlistInfo();
            });

            this.OrderDelegationUIBtnListContainer.SetEmptyAllBtnList(() =>
            {
                #region ����������

                foreach (var order in OrderManager.Instance.GetOrderDelegationOrders(CurSelectedClanID, OrderType.Urgent))
                {
                    OrderUrgent orderUrgent = (OrderUrgent)order;
                    OrderTableData orderTableData = OrderManager.Instance.GetOrderTableData(order);

                    this.OrderDelegationUIBtnListContainer.UIBtnLists[0].AddBtn("OC/Prefabs_OrderBoard_UI/Prefab_Order_UI_UrgentDelegationBtn.prefab", BtnSettingAction: (btn) =>
                    {
                        if (order != null)
                        {
                            //���¼�ʱ��Ϣ
                            btn.transform.Find("StripImage").Find("OrderName").GetComponent<TextMeshProUGUI>().text = orderTableData.OrderName;
                            TextMeshProUGUI RemainTimeText = btn.transform.Find("ReceiveTime").Find("RemainTime").GetComponent<TextMeshProUGUI>();
                            Slider slider = btn.transform.Find("ReceiveTime").Find("Slider").GetComponent<Slider>();
                            CounterDownTimer timer = orderUrgent.ReceiveDDLTimer;
                            timer.OnUpdateEvent += (time) => {
                                RemainTimeText.text = timer.CurrentTime.ToString();
                                slider.value = (float)(timer.CurrentTime / timer.Duration);
                            };
                            timer.OnEndEvent += () => { this.isNeedRefreshOrderDelegation = true; this.Refresh(); };
                            btn.name = orderTableData.ID;
                        }
                        else
                        {
                            //��Ϊ�ղ���ֻ����Selected
                            for (int i = 0; i < btn.transform.childCount; i++)
                            {
                                btn.transform.GetChild(i).gameObject.SetActive(btn.transform.GetChild(i).name == "Selected");
                            }
                            btn.name = btn.GetHashCode().ToString();
                        }
                    },
                    OnFinishAdd: () => {
                        synchronizer.Check();
                    });
                }
                #endregion
                #region ���涩����
                foreach (var order in OrderManager.Instance.GetOrderDelegationOrders(CurSelectedClanID, OrderType.Normal))
                {
                    OrderTableData orderTableData = OrderManager.Instance.GetOrderTableData(order);

                    this.OrderDelegationUIBtnListContainer.UIBtnLists[1].AddBtn("OC/Prefabs_OrderBoard_UI/Prefab_Order_UI_NormalDelegationBtn.prefab", BtnSettingAction: (btn) =>
                    {
                        //������Ϣ
                        btn.transform.Find("Image").Find("Text").GetComponent<TextMeshProUGUI>().text = orderTableData.OrderName;
                        btn.name = orderTableData.ID;
                    },
                    OnFinishAdd: () =>
                    {
                        synchronizer.Check();
                    });
                }
                #endregion

            });
            isNeedRefreshOrderDelegation = false;
        }
        else if(FunctionIndex != 1 || OrderDelegationIndex != 1)
        {
            //�г�
            //ʧ��OrderDelegationUIBtnListContainer
            Debug.Log("ʧ��OrderDelegationUIBtnListContainer");
            this.OrderDelegationUIBtnListContainer.SetIsEnableFalse();

            //�˳� �����н� ����
            isNeedRefreshOrderDelegation = true;
            this.isInitNormalOrder = false;
        }

        #region �Ҳඩ����ϸ��Ϣ

        //���ö����
        this.objectPool.ResetAllObject();
        if (OrderManager.Instance.IsValidOrderID(curSelectedOrderIDInOrderDelegation))
        {
            OrderTableData orderTableData = OrderManager.Instance.GetOrderTableData(curSelectedOrderIDInOrderDelegation);
            this.OrderDelegationOrderInfo.gameObject.SetActive(true);

            //LimitTime
            var LimitTime = this.OrderDelegationOrderInfo.Find("LimitTime");
            if (orderTableData.OrderType == OrderType.Urgent)
            {
                LimitTime.gameObject.SetActive(true);
                //��ʱ X ��
                LimitTime.Find("Text2").GetComponent<TextMeshProUGUI>().text = orderTableData.ReceiveDDL.ToString();
            }
            else
            {
                this.OrderDelegationOrderInfo.Find("LimitTime").gameObject.SetActive(false);
            }
            //Title
            var Title = this.OrderDelegationOrderInfo.Find("Title");
            Title.Find("Text1").GetComponent<TextMeshProUGUI>().text = orderTableData.OrderName;
            Title.Find("Text2").GetComponent<TextMeshProUGUI>().text = orderTableData.OrderDescription;
            //ItemList
            var ItemList = this.OrderDelegationOrderInfo.Find("ItemList");
            var Slots = ItemList.Find("Slots");
            Debug.Log("rtr " + orderTableData.RequireList.Count +" "+ Slots);
            for (int i = 0; i < orderTableData.RequireList.Count; i++)
            {
                var slot = this.objectPool.GetNextObject("SlotPool", Slots);
                slot.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.black;

                int needNum = orderTableData.RequireList[i].num;
                int haveNum = OrderManager.Instance.GetInventory().GetItemAllNum(orderTableData.RequireList[i].id);
                
                if (needNum > haveNum)
                {
                    slot.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.red;
                    slot.transform.Find("ItemNumber").Find("Text").GetComponent<TextMeshProUGUI>().text = needNum.ToString() + "/" + haveNum.ToString();
                }
                else
                {
                    slot.transform.Find("ItemNumber").Find("Text").GetComponent<TextMeshProUGUI>().text = orderTableData.RequireList[i].num.ToString();
                }
                slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(orderTableData.RequireList[i].id);


            }
            //RewardList
            var RewardList = this.OrderDelegationOrderInfo.Find("RewardList");
            var Tokens = RewardList.Find("Tokens");
            for (int i = 0; i < orderTableData.ItemReward.Count; i++)
            {
                var slot = this.objectPool.GetNextObject("SlotPool", Tokens);
                slot.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.black;

                slot.transform.Find("ItemNumber").Find("Text").GetComponent<TextMeshProUGUI>().text = orderTableData.ItemReward[i].num.ToString();
                slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(orderTableData.ItemReward[i].id);
            }
            //��ť����
            Debug.Log(this.OrderDelegationOrderInfo);
            Debug.Log(this.OrderDelegationOrderInfo.Find("CancelBtn"));
            this.OrderDelegationOrderInfo.Find("CancelBtn").gameObject.SetActive(orderTableData.OrderType == OrderType.Urgent);
        }
        else
        {
            //�Ҳඩ����ϢΪ��
            this.OrderDelegationOrderInfo.gameObject.SetActive(false);
        }

        #endregion
        #endregion

        #endregion

        #region AcceptedOrder
        if(FunctionIndex == 0)
        {
            if(isNeedRefreshAcceptedOrder)
            {
                this.AcceptedOrderBtnList.EnableBtnList();
                this.AcceptedOrderBtnList.DeleteAllButton(() =>
                {
                    foreach (var acceptedOrder in OrderManager.Instance.AcceptedOrders)
                    {
                        OrderTableData orderTableData = OrderManager.Instance.GetOrderTableData(acceptedOrder.order);
                        this.AcceptedOrderBtnList.AddBtn("OC/Prefabs_OrderBoard_UI/Prefab_Order_UI_AcceptedOrderListBtn.prefab", BtnSettingAction: (btn) =>
                        {
                            btn.transform.Find("Image").Find("Text").GetComponent<TextMeshProUGUI>().text = orderTableData.OrderName;

                            if(orderTableData.OrderType == OrderType.Urgent)
                            {
                                btn.transform.Find("Image").Find("Image1").GetComponent<Image>().color = Color.red;
                            }
                            else if(orderTableData.OrderType == OrderType.Special)
                            {
                                btn.transform.Find("Image").Find("Image1").GetComponent<Image>().color = Color.blue;
                            }
                            else if(orderTableData.OrderType == OrderType.Normal)
                            {
                                btn.transform.Find("Image").Find("Image1").GetComponent<Image>().color = Color.gray;
                            }

                            

                            btn.transform.Find("Image").Find("Image2").gameObject.SetActive(acceptedOrder.canBeCommit);
                            btn.name = orderTableData.ID;
                        },
                        OnFinishAdd: () => { 
                            //��ť�������ˢ��
                            this.Refresh();
                        });
                    }
                    //��ť�������ˢ��
                    this.Refresh();

                });
                isNeedRefreshAcceptedOrder = false;
            }

        }
        else
        {
            this.AcceptedOrderBtnList.DisableBtnList();
            isNeedRefreshAcceptedOrder = true;
        }

        #region �Ҳඩ����ϸ��Ϣ
        
        if (OrderManager.Instance.IsValidOrderID(curSelectedOrderIDInAcceptedOrder))
        {
            Debug.Log(curSelectedOrderIDInAcceptedOrder);
            OrderTableData orderTableData = OrderManager.Instance.GetOrderTableData(curSelectedOrderIDInAcceptedOrder);
            this.AcceptedOrderOrderInfo.gameObject.SetActive(true);

            //LimitTime
            var LimitTime = this.AcceptedOrderOrderInfo.Find("LimitTime");
            if (orderTableData.OrderType == OrderType.Urgent)
            {
                LimitTime.gameObject.SetActive(true);
                //��ʱ X ��
                LimitTime.Find("Text2").GetComponent<TextMeshProUGUI>().text = orderTableData.ReceiveDDL.ToString();
            }
            else
            {
                this.AcceptedOrderOrderInfo.Find("LimitTime").gameObject.SetActive(false);
            }
            //Title
            var Title = this.AcceptedOrderOrderInfo.Find("Title");
            Title.Find("Text1").GetComponent<TextMeshProUGUI>().text = orderTableData.OrderName;
            Title.Find("Text2").GetComponent<TextMeshProUGUI>().text = orderTableData.OrderDescription;
            //ItemList
            var ItemList = this.AcceptedOrderOrderInfo.Find("ItemList");
            var Slots = ItemList.Find("Slots");

            for (int i = 0; i < orderTableData.RequireList.Count; i++)
            {
                var slot = this.objectPool.GetNextObject("SlotPool", Slots);
                slot.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.black;

                int needNum = orderTableData.RequireList[i].num;
                int haveNum = OrderManager.Instance.GetInventory().GetItemAllNum(orderTableData.RequireList[i].id);

                if (needNum > haveNum)
                {
                    slot.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.red;
                    slot.transform.Find("ItemNumber").Find("Text").GetComponent<TextMeshProUGUI>().text = needNum.ToString() + "/" + haveNum.ToString();
                }
                else
                {
                    slot.transform.Find("ItemNumber").Find("Text").GetComponent<TextMeshProUGUI>().text = orderTableData.RequireList[i].num.ToString();
                }
                slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(orderTableData.RequireList[i].id);
            }
            //RewardList
            var RewardList = this.AcceptedOrderOrderInfo.Find("RewardList");
            var Tokens = RewardList.Find("Tokens");
            for (int i = 0; i < orderTableData.ItemReward.Count; i++)
            {
                var slot = this.objectPool.GetNextObject("SlotPool", Tokens);
                slot.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.black;

                slot.transform.Find("ItemNumber").Find("Text").GetComponent<TextMeshProUGUI>().text = orderTableData.ItemReward[i].num.ToString();
                slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(orderTableData.ItemReward[i].id);
            }

            //��ť����
            this.OrderDelegationOrderInfo.Find("CancelBtn").gameObject.SetActive(!OrderManager.Instance.GetAcceptedOrder(curSelectedOrderIDInAcceptedOrder).canBeCommit);
        }
        else
        {
            //�Ҳඩ����ϢΪ��
            this.AcceptedOrderOrderInfo.gameObject.SetActive(false);
        }

        #endregion
        #endregion
    }
    #endregion


    #region Resource

    #region TextContent
    [System.Serializable]
    public struct OrderBoardPanelStruct
    {
        public KeyTip AcceptedOrderCancel;
        public KeyTip AcceptedOrderConfirm;
        public KeyTip OrderDelegationCancel;
        public KeyTip OrderDelegationConfirm;

        public KeyTip CommitOrder;
        public KeyTip ViewMoreInformation;
        public KeyTip Back;
    }
    protected override void InitTextContentPathData()
    {
        this.abpath = "OC/Json/TextContent/Order";
        this.abname = "OrderBoardPanel";
        this.description = "OrderBoardPanel���ݼ������";
    }
    #endregion

    protected override void InitObjectPool()
    {
        this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "SlotPool", 10, "Prefab_Order_UIPrefab/Prefab_Order_UI_Slot.prefab");
        base.InitObjectPool();
    }
    [ShowInInspector]
    //����ѡ��
    private UIBtnList ClanBtnList;
    //����ί�� �������� ���涩��
    [ShowInInspector]
    private UIBtnListContainer OrderDelegationUIBtnListContainer;

    [ShowInInspector]
    //�ѳнӶ���
    private UIBtnList AcceptedOrderBtnList;

    protected override void InitBtnInfo()
    {
        UIBtnListInitor ClanBtnListInitor = this.OrderDelegation.Find("ClanSelect").GetComponentInChildren<UIBtnListInitor>(true);
        ClanBtnList = new UIBtnList(ClanBtnListInitor);
        ClanBtnList.SetAllBtnAction(() =>
        {
            FunctionIndex = 1;
            OrderDelegationIndex = 1;
            this.Refresh();
        });

        UIBtnListContainerInitor OrderDelegationUIBtnListContainerInitor = this.OrderDelegation.Find("OrderDelegation").Find("Panel1").GetComponent<UIBtnListContainerInitor>();
        this.OrderDelegationUIBtnListContainer = new UIBtnListContainer(OrderDelegationUIBtnListContainerInitor);
        this.OrderDelegationUIBtnListContainer.AddOnSelectButtonChangedAction(() => { this.Refresh(); });

        UIBtnListInitor AcceptedOrderBtnListInitor = this.AcceptedOrder.Find("AcceptedOrderList").GetComponentInChildren<UIBtnListInitor>(true);
        this.AcceptedOrderBtnList = new UIBtnList(AcceptedOrderBtnListInitor);
        this.AcceptedOrderBtnList.OnSelectButtonChanged += () => { this.Refresh(); };

    }

    #endregion
}
