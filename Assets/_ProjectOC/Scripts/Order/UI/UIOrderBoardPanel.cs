using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using ML.Engine.UI;
using ML.Engine.Utility;
using ProjectOC.Order;
using ProjectOC.Player;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ML.Engine.UI.UIBtnListContainer;
using static UIOrderBoardPanel;
using static ProjectOC.Order.OrderManager;

public class UIOrderBoardPanel : UIBasePanel<OrderBoardPanelStruct>
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

        this.KeyTips = this.transform.Find("BotKeyTips").Find("KeyTips");
        this.KT_CommitAllOrder = this.KeyTips.Find("KT_CommitAllOrder");
        this.KT_ViewMoreInformation = this.KeyTips.Find("KT_ViewMoreInformation");
        this.KT_CancelMoreInformation = this.KeyTips.Find("KT_CancelMoreInformation");
        this.KT_Back = this.KeyTips.Find("KT_Back");
    }

    #endregion

    #region Override
    private void OrderPanelRefreshOrderUrgentDelegationAction()
    {
        isNeedRefreshOrderUrgentDelegation = true; Refresh();
    }
    private void OrderPanelRefreshAcceptedOrderAction()
    {
        OrderUrgentInAcceptedOrderTimer = null;
        isNeedRefreshAcceptedOrder = true; Refresh();
    }
    public override void OnEnter()
    {
        base.OnEnter();
        OrderManager.Instance.OrderPanelRefreshOrderUrgentDelegation += OrderPanelRefreshOrderUrgentDelegationAction;
    }
    public override void OnExit()
    {
        base.OnExit();
        OrderManager.Instance.OrderPanelRefreshOrderUrgentDelegation -= OrderPanelRefreshOrderUrgentDelegationAction;
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
        // 切换类目
        ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm.performed -= LastTerm_performed;
        ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm.performed -= NextTerm_performed;

        ML.Engine.Input.InputManager.Instance.Common.Common.MainInteract.performed -= MainInteract_performed;
        ML.Engine.Input.InputManager.Instance.Common.Common.MainInteractHold.performed -= MainInteractHold_performed;
        ML.Engine.Input.InputManager.Instance.Common.Common.SubInteract.performed -= SubInteract_performed;

        this.ClanBtnList.RemoveAllListener();
        this.ClanBtnList.DeBindInputAction();

        this.AcceptedOrderBtnList.RemoveAllListener();
        this.AcceptedOrderBtnList.DeBindInputAction();

        // 返回
        ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        this.OrderDelegationUIBtnListContainer.DisableUIBtnListContainer();
    }

    protected override void RegisterInput()
    {
        // 切换类目
        ML.Engine.Input.InputManager.Instance.Common.Common.LastTerm.performed += LastTerm_performed;
        ML.Engine.Input.InputManager.Instance.Common.Common.NextTerm.performed += NextTerm_performed;

        ML.Engine.Input.InputManager.Instance.Common.Common.MainInteract.performed += MainInteract_performed;
        ML.Engine.Input.InputManager.Instance.Common.Common.MainInteractHold.performed += MainInteractHold_performed;
        ML.Engine.Input.InputManager.Instance.Common.Common.SubInteract.performed += SubInteract_performed;

        this.ClanBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
        this.ClanBtnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnListContainer.BindType.performed);

        this.AcceptedOrderBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
        this.AcceptedOrderBtnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnListContainer.BindType.performed);

        // 返回
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
            //打开 关闭氏族信息
            if (OrderDelegationIndex == 0)
            {
                ClanSelectIndex = ClanSelectIndex == 0 ? 1 : 0;
                this.Refresh();
            }
            else if(OrderDelegationIndex == 1)
            {
                //承接订单
                OrderType orderType = OrderManager.Instance.GetOrderTypeInOrderDelegation(curSelectedOrderInstanceIDInOrderDelegation);
                OrderManager.Instance.ReceiveOrder(curSelectedOrderInstanceIDInOrderDelegation);
                if(orderType == OrderType.Urgent)
                {
                    isNeedRefreshOrderUrgentDelegation = true;
                    this.Refresh();
                }
                else if(orderType == OrderType.Normal)
                {
                    this.OrderDelegationUIBtnListContainer.UIBtnLists[1].DeleteButton(curSelectedOrderInstanceIDInOrderDelegation);
                }
            }
        }
        else if(FunctionIndex == 0)
        {
            //点按提交订单
            bool isCommitSuccess = OrderManager.Instance.CommitOrder(curSelectedOrderInstanceIDInAcceptedOrder);
            if(isCommitSuccess)
            {
                this.AcceptedOrderBtnList.DeleteButton(curSelectedOrderInstanceIDInAcceptedOrder, () => { Refresh(); });
            }
        }
    }

    private void MainInteractHold_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (FunctionIndex == 0)
        {
            //长按快捷提交订单
            OrderManager.Instance.CommitAllOrder();
            
            //手动取消订单以后手动刷新面板
            OrderPanelRefreshAcceptedOrderAction();
        }
    }

    private void SubInteract_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (FunctionIndex == 1)
        {
            if (OrderDelegationIndex == 1)
            {
                //拒绝订单
                //Debug.Log("拒绝订单 "+ curSelectedOrderInstanceIDInOrderDelegation);
                OrderManager.Instance.RefuseOrder(curSelectedOrderInstanceIDInOrderDelegation);
            }
        }
        else if (FunctionIndex == 0)
        {
            //取消订单
            //Debug.Log("取消订单 "+ curSelectedOrderInstanceIDInAcceptedOrder);
            var AcceptedOrder = OrderManager.Instance.GetAcceptedOrder(curSelectedOrderInstanceIDInAcceptedOrder);
            if (AcceptedOrder != null && AcceptedOrder.canBeCancled)
            {
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, new UIManager.PopUpUIData("确定取消该订单吗？", "您将面临违约惩罚", null, () => {
                    bool isCancleOrderSuccess = OrderManager.Instance.CancleOrder(curSelectedOrderInstanceIDInAcceptedOrder);
                    if (isCancleOrderSuccess)
                    {
                        this.AcceptedOrderBtnList.DeleteButton(curSelectedOrderInstanceIDInAcceptedOrder);
                    }
                    //手动取消订单以后手动刷新面板
                    OrderPanelRefreshAcceptedOrderAction();
                }));
            }
        }
    }

    private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (FunctionIndex == 1 && OrderDelegationIndex == 1)
        {
            OrderDelegationIndex = 0;
            this.Refresh();
        }
        else
        {
            UIMgr.PopPanel();
        }
        
    }
    #endregion

    #region UI
    #region temp

    private CounterDownTimer OrderUrgentInAcceptedOrderTimer = null;

    private void ClearTemp()
    {
/*        GameManager.DestroyObj(icon_genderfemaleSprite);
        GameManager.DestroyObj(icon_gendermaleSprite);*/
    }

    #endregion

    #region UI对象引用
    //初始选中订单委托
    [ShowInInspector]
    private int FunctionIndex = 1;
    //初始选中氏族选择
    [ShowInInspector]
    private int OrderDelegationIndex = 0;
    //初始选中氏族选择Panel1
    [ShowInInspector]
    private int ClanSelectIndex = 0;

    private int FunctionType;
    private Transform Function;
    private Transform FunctionPanel;

    private Transform AcceptedOrder;
    private Transform OrderDelegation;

    private bool isNeedRefreshAcceptedOrder = true;
    private bool isNeedRefreshOrderUrgentDelegation = true;
    private bool isNeedRefreshOrderNormalDelegation = true;
    //private int OrderDelegationCnt = 0;

    private Transform AcceptedOrderOrderInfo;
    private Transform OrderDelegationOrderInfo;

    private Transform KeyTips;
    private Transform KT_CommitAllOrder;
    private Transform KT_ViewMoreInformation;
    private Transform KT_CancelMoreInformation;
    private Transform KT_Back;



    //[ShowInInspector]
    private string curSelectedOrderInstanceIDInOrderDelegation 
    {   get 
        {
            SelectedButton btn = null;
            btn = this.OrderDelegationUIBtnListContainer.CurSelectUIBtnList?.GetCurSelected();
            return btn != null ? btn.name : null;
        } 
    }

    //[ShowInInspector]
    private string curSelectedOrderInstanceIDInAcceptedOrder
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

        //订单委托选择
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
        
        if (FunctionIndex == 1 && OrderDelegationIndex == 1)
        {
            //激活OrderDelegationUIBtnListContainer
            this.OrderDelegationUIBtnListContainer.SetIsEnableTrue();
            //当前所选氏族的ID
            string CurSelectedClanID = this.ClanBtnList.GetCurSelected().name;

            if(isNeedRefreshOrderUrgentDelegation && !UrgentBtnListIsBusy)
            {
                Synchronizer synchronizer = new Synchronizer(2 , () =>
                {
                    this.OrderDelegationUIBtnListContainer.InitBtnlistInfo();
                    this.OrderDelegationUIBtnListContainer.FindEnterableUIBtnList();
                    this.OrderDelegationUIBtnListContainer.CurSelectUIBtnList.GetCurSelected()?.OnSelect(null);
                    UrgentBtnListIsBusy = false;
                });

                this.OrderDelegationUIBtnListContainer.UIBtnLists[0].DeleteAllButton(() =>
                {
                    UrgentBtnListIsBusy = true;
                    #region 紧急订单槽

                    foreach (var order in OrderManager.Instance.GetOrderDelegationOrders(CurSelectedClanID, OrderType.Urgent))
                    {
                        OrderUrgent orderUrgent = (OrderUrgent)order;
                        OrderTableData orderTableData = OrderManager.Instance.GetOrderTableData(order);

                        this.OrderDelegationUIBtnListContainer.UIBtnLists[0].AddBtn("Prefab_Order_UIPrefab/Prefab_Order_UI_UrgentDelegationBtn.prefab", BtnSettingAction: (btn) =>
                        {
                            if (order != null)
                            {
                                //更新计时信息
                                btn.transform.Find("StripImage").Find("OrderName").GetComponent<TextMeshProUGUI>().text = orderTableData.OrderName;
                                TextMeshProUGUI RemainTimeText = btn.transform.Find("ReceiveTime").Find("RemainTime").GetComponent<TextMeshProUGUI>();
                                Slider slider = btn.transform.Find("ReceiveTime").Find("Slider").GetComponent<Slider>();
                                CounterDownTimer timer = orderUrgent.ReceiveDDLTimer;

                                timer.OnUpdateEvent = null;
                                timer.OnUpdateEvent += (time) => {
                                    RemainTimeText.text = timer.currentTimeInMForm.ToString() + " MIN";
                                    slider.value = (float)(timer.CurrentTime / timer.Duration);
                                };
                                btn.name = order.OrderInstanceID;
                            }
                            else
                            {
                                //若为空槽则只激活Selected
                                for (int i = 0; i < btn.transform.childCount; i++)
                                {
                                    //btn.transform.GetChild(i).gameObject.SetActive(btn.transform.GetChild(i).name == "Selected");
                                    btn.transform.GetChild(i).gameObject.SetActive(false);
                                }
                                btn.name = btn.GetHashCode().ToString();
                            }
                        },
                        OnFinishAdd: () => {
                            synchronizer.Check();
                        });
                    }
                    #endregion
                });
                isNeedRefreshOrderUrgentDelegation = false;
            }
            
            if(isNeedRefreshOrderNormalDelegation)
            {
                Synchronizer synchronizer = new Synchronizer(OrderManager.Instance.GetOrderDelegationOrders(CurSelectedClanID, OrderType.Normal).Count, () =>
                {
                    this.OrderDelegationUIBtnListContainer.InitBtnlistInfo();
                    this.OrderDelegationUIBtnListContainer.FindEnterableUIBtnList();
                    this.OrderDelegationUIBtnListContainer.CurSelectUIBtnList.GetCurSelected()?.OnSelect(null);
                });
                this.OrderDelegationUIBtnListContainer.SetEmptyAllBtnList(() =>
                {

                    #region 常规订单槽
                    foreach (var order in OrderManager.Instance.GetOrderDelegationOrders(CurSelectedClanID, OrderType.Normal))
                    {
                        OrderTableData orderTableData = OrderManager.Instance.GetOrderTableData(order);

                        this.OrderDelegationUIBtnListContainer.UIBtnLists[1].AddBtn("Prefab_Order_UIPrefab/Prefab_Order_UI_NormalDelegationBtn.prefab", BtnSettingAction: (btn) =>
                        {
                            //更新信息
                            btn.transform.Find("Image").Find("Text").GetComponent<TextMeshProUGUI>().text = orderTableData.OrderName;
                            btn.name = order.OrderInstanceID;
                        },
                        OnFinishAdd: () =>
                        {
                            synchronizer.Check();
                        });
                    }
                    #endregion

                });
                isNeedRefreshOrderNormalDelegation = false;
            }
        }
        else if(FunctionIndex != 1 || OrderDelegationIndex != 1)
        {
            //切出
            //失活OrderDelegationUIBtnListContainer
            this.OrderDelegationUIBtnListContainer.SetIsEnableFalse();

            //退出 订单承接 界面
            isNeedRefreshOrderUrgentDelegation = true;
            isNeedRefreshOrderNormalDelegation = true;
        }

        #region 右侧订单详细信息

        //重置对象池
        this.objectPool.ResetAllObject();
        if (OrderManager.Instance.IsValidOrderIDInOrderDelegation(curSelectedOrderInstanceIDInOrderDelegation))
        {
            OrderTableData orderTableData = OrderManager.Instance.GetOrderTableData(curSelectedOrderInstanceIDInOrderDelegation);
            this.OrderDelegationOrderInfo.gameObject.SetActive(true);

            //LimitTime
            var LimitTime = this.OrderDelegationOrderInfo.Find("LimitTime");
            if (orderTableData.OrderType == OrderType.Urgent)
            {
                LimitTime.gameObject.SetActive(true);
                //限时 X 日
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
            for (int i = 0; i < orderTableData.RequireList.Count; i++)
            {
                var slot = this.objectPool.GetNextObject("SlotPool", Slots);
                slot.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.black;

                int needNum = orderTableData.RequireList[i].num;
                int haveNum = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).InventoryItemAmount(orderTableData.RequireList[i].id);
                
                if (needNum > haveNum)
                {
                    slot.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.red;
                    slot.transform.Find("ItemNumber").Find("Text").GetComponent<TextMeshProUGUI>().text = haveNum.ToString() + "/" + needNum.ToString();
                }
                else
                {
                    slot.transform.Find("ItemNumber").Find("Text").GetComponent<TextMeshProUGUI>().text = needNum.ToString();
                }
                slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(orderTableData.RequireList[i].id);
                slot.transform.Find("ItemIcon").GetComponent<Image>().sprite = ItemManager.Instance.GetItemSprite(orderTableData.RequireList[i].id);
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
                slot.transform.Find("ItemIcon").GetComponent<Image>().sprite = ItemManager.Instance.GetItemSprite(orderTableData.ItemReward[i].id);
            }
            //按钮隐藏

            this.OrderDelegationOrderInfo.Find("CancelBtn").gameObject.SetActive(orderTableData.OrderType == OrderType.Urgent);
        }
        else
        {
            //右侧订单信息为空
            this.OrderDelegationOrderInfo.gameObject.SetActive(false);
        }

        #endregion
        #endregion

        #endregion

        #region AcceptedOrder
        if(FunctionIndex == 0)
        {
            if(isNeedRefreshAcceptedOrder && !AcceptedOrderBtnListIsBusy)
            {
                OrderManager.Instance.RefreshAcceptedList();
                Synchronizer synchronizer = new Synchronizer(OrderManager.Instance.AcceptedOrders.Count, () =>
                {
                    this.AcceptedOrderBtnList.InitBtnInfo();
                    this.AcceptedOrderBtnList.InitSelectBtn();
                    AcceptedOrderBtnListIsBusy = false;
                });

                this.AcceptedOrderBtnList.EnableBtnList();
                this.AcceptedOrderBtnList.DeleteAllButton(() =>
                {
                    AcceptedOrderBtnListIsBusy = true;
                    foreach (var acceptedOrder in OrderManager.Instance.AcceptedOrders)
                    {
                        OrderTableData orderTableData = OrderManager.Instance.GetOrderTableData(acceptedOrder);
                        this.AcceptedOrderBtnList.AddBtn("Prefab_Order_UIPrefab/Prefab_Order_UI_AcceptedOrderListBtn.prefab", BtnSettingAction: (btn) =>
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
                            btn.name = acceptedOrder.OrderInstanceID;
                        },
                        OnFinishAdd: () => {
                            synchronizer.Check();
                        },NeedRefreshBtnInfo: false);
                    }
                    

                });
                isNeedRefreshAcceptedOrder = false;
            }

        }
        else
        {
            this.AcceptedOrderBtnList.DisableBtnList();
            OrderUrgentInAcceptedOrderTimer = null;
            isNeedRefreshAcceptedOrder = true;
        }

        #region 右侧订单详细信息
        
        if (OrderManager.Instance.IsValidOrderIDInAcceptedOrder(curSelectedOrderInstanceIDInAcceptedOrder))
        {
            OrderTableData orderTableData = OrderManager.Instance.GetOrderTableData(curSelectedOrderInstanceIDInAcceptedOrder);
            var AcceptedOrder = OrderManager.Instance.GetAcceptedOrder(curSelectedOrderInstanceIDInAcceptedOrder);
            this.AcceptedOrderOrderInfo.gameObject.SetActive(true);

            //LimitTime
            var LimitTime = this.AcceptedOrderOrderInfo.Find("LimitTime");
            if (orderTableData.OrderType == OrderType.Urgent)
            {
                OrderUrgent orderUrgent = (OrderUrgent)AcceptedOrder;
                LimitTime.gameObject.SetActive(true);
                var LimitToday = LimitTime.Find("LimitToday");
                var LimitFuture = LimitTime.Find("LimitFuture");
                //今日截止
                int remainDays = orderUrgent.GetDeliverDDLTimerRemainGameDays();
                LimitToday.gameObject.SetActive(remainDays <= 1);
                LimitFuture.gameObject.SetActive(remainDays > 1);
                if(remainDays <= 1)
                {
                    var textmesh = LimitToday.Find("Text3").GetComponent<TextMeshProUGUI>();

                    if(OrderUrgentInAcceptedOrderTimer != orderUrgent.DeliverDDLTimer)
                    {
                        if (OrderUrgentInAcceptedOrderTimer != null)
                        {
                            OrderUrgentInAcceptedOrderTimer.OnUpdateEvent = null;
                        }
                        orderUrgent.DeliverDDLTimer.OnUpdateEvent = null;
                        orderUrgent.DeliverDDLTimer.OnUpdateEvent += (curTime) =>
                        {
                            textmesh.text = orderUrgent.GetDeliverDDLTimerRemainGameHourAndMin();
                        };
                        OrderUrgentInAcceptedOrderTimer = orderUrgent.DeliverDDLTimer;
                    }
                }
                else
                {
                    var textmesh = LimitFuture.Find("Text2").GetComponent<TextMeshProUGUI>();
                    if (OrderUrgentInAcceptedOrderTimer != orderUrgent.DeliverDDLTimer) 
                    {
                        if (OrderUrgentInAcceptedOrderTimer != null)
                        {
                            OrderUrgentInAcceptedOrderTimer.OnUpdateEvent = null;
                        }
                        orderUrgent.DeliverDDLTimer.OnUpdateEvent = null;
                        orderUrgent.DeliverDDLTimer.OnUpdateEvent += (curTime) =>
                        {
                            textmesh.text = remainDays.ToString();
                        };
                        OrderUrgentInAcceptedOrderTimer = orderUrgent.DeliverDDLTimer;
                    }
                }
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

            foreach (var (id,num) in AcceptedOrder?.RequireItem)
            {
                var slot = this.objectPool.GetNextObject("SlotPool", Slots);
                slot.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.black;

                if (AcceptedOrder.AddedItemDic[id] < num) 
                {
                    slot.transform.Find("ItemNumber").Find("Background").GetComponent<Image>().color = UnityEngine.Color.red;
                    slot.transform.Find("ItemNumber").Find("Text").GetComponent<TextMeshProUGUI>().text = AcceptedOrder.AddedItemDic[id].ToString() + "/" + num.ToString();
                }
                else
                {
                    slot.transform.Find("ItemNumber").Find("Text").GetComponent<TextMeshProUGUI>().text = num.ToString();
                }
                slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = ItemManager.Instance.GetItemName(id);
                slot.transform.Find("ItemIcon").GetComponent<Image>().sprite = ItemManager.Instance.GetItemSprite(id);
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
                slot.transform.Find("ItemIcon").GetComponent<Image>().sprite = ItemManager.Instance.GetItemSprite(orderTableData.ItemReward[i].id);
            }
            RewardList.Find("Text2").gameObject.SetActive(orderTableData.OrderType == OrderType.Urgent);
            RewardList.Find("Text2").GetComponent<TextMeshProUGUI>().text = "x2";

            //按钮隐藏
            
            if (AcceptedOrder != null)
            {
                this.AcceptedOrderOrderInfo.Find("ConfirmBtn").Find("Image").gameObject.SetActive(!AcceptedOrder.canBeCommit);
                this.AcceptedOrderOrderInfo.Find("CancelBtn").gameObject.SetActive(AcceptedOrder.canBeCancled);
            }
        }
        else
        {
            //右侧订单信息为空
            this.AcceptedOrderOrderInfo.gameObject.SetActive(false);
        }
        #endregion
        #endregion

        #region BotKeyTips
        this.KT_ViewMoreInformation.gameObject.SetActive(FunctionIndex == 1 && OrderDelegationIndex == 0 && ClanSelectIndex == 0);
        this.KT_CancelMoreInformation.gameObject.SetActive(FunctionIndex == 1 && OrderDelegationIndex == 0 && ClanSelectIndex == 1);
        this.KT_CommitAllOrder.gameObject.SetActive(FunctionIndex == 0);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.KeyTips.GetComponent<RectTransform>());
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
        this.abpath = "OCTextContent/Order";
        this.abname = "OrderBoardPanel";
        this.description = "OrderBoardPanel数据加载完成";
    }
    #endregion

    protected override void InitObjectPool()
    {
        this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "SlotPool", 10, "Prefab_Order_UIPrefab/Prefab_Order_UI_Slot.prefab");
        base.InitObjectPool();
    }
    [ShowInInspector]
    //氏族选择
    private UIBtnList ClanBtnList;
    //订单委托 紧急订单 常规订单
    [ShowInInspector]
    private UIBtnListContainer OrderDelegationUIBtnListContainer;
    private bool UrgentBtnListIsBusy = false;

    [ShowInInspector]
    //已承接订单
    private UIBtnList AcceptedOrderBtnList;
    private bool AcceptedOrderBtnListIsBusy = false;

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

        UIBtnListContainerInitor OrderDelegationUIBtnListContainerInitor = this.OrderDelegation.Find("OrderDelegation").Find("Panel1").Find("BtnContainer").GetComponent<UIBtnListContainerInitor>();
        this.OrderDelegationUIBtnListContainer = new UIBtnListContainer(OrderDelegationUIBtnListContainerInitor);
        this.OrderDelegationUIBtnListContainer.AddOnSelectButtonChangedAction(() => { this.Refresh(); });

        UIBtnListInitor AcceptedOrderBtnListInitor = this.AcceptedOrder.Find("AcceptedOrderList").GetComponentInChildren<UIBtnListInitor>(true);
        this.AcceptedOrderBtnList = new UIBtnList(AcceptedOrderBtnListInitor);
        this.AcceptedOrderBtnList.OnSelectButtonChanged += () => { this.Refresh(); };
    }

    #endregion
}
