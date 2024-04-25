using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using ML.Engine.Timer;
using ML.Engine.UI;
using ML.Example.InventorySystem;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using ProjectOC.TechTree;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static ProjectOC.Order.OrderManager;
using static ProjectOC.TechTree.TechTreeManager;
using Formula = ML.Engine.InventorySystem.CompositeSystem.Formula;



namespace ProjectOC.Order
{
    [System.Serializable]
    public sealed class OrderManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        /// <summary>
        /// 订单种类
        /// </summary>
        public enum OrderType
        {
            Urgent = 0,
            Special,
            Normal,
            None,
        }

        /// <summary>
        /// 当前每个种类的已经解锁的订单ID 订单类型，该类型已解锁的订单ID数组
        /// </summary>
        [ShowInInspector]
        private Dictionary<OrderType, List<string>>  UnlockedOrderMap = new Dictionary<OrderType, List<string>>();

        /// <summary>
        /// 当前每个氏族已经解锁的紧急订单
        /// </summary>
        [ShowInInspector]
        private Dictionary<string, List<string>> UnlockedUrgentOrderMap = new Dictionary<string, List<string>>();

        /// <summary>
        /// 当前订单委托中存在的订单 string 氏族ID，List<Order>
        /// </summary>
        [ShowInInspector]
        private Dictionary<string, List<Order>> OrderNormalDelegationMap = new Dictionary<string, List<Order>>();

        [ShowInInspector]
        private Dictionary<string, List<Order>> OrderSpecialDelegationMap = new Dictionary<string, List<Order>>();

        [ShowInInspector]
        private Dictionary<string, List<Order>> OrderUrgentDelegationMap = new Dictionary<string, List<Order>>();
        /// <summary>
        /// 当前正在等待刷新的常规订单 (string 氏族ID,OrderType type)，List<Order>
        /// </summary>
        [ShowInInspector]
        private List<OrderNormal> WaitingForRefreshNormalOrders = new List<OrderNormal>();


        /// <summary>
        /// 已承接列表的订单 
        /// </summary>
        [ShowInInspector]
        private List<Order> AcceptedOrderList = new List<Order>();
        /// <summary>
        /// OrderInstanceID,AcceptedOrder
        /// </summary>
        [ShowInInspector]
        private Dictionary<string, Order> AcceptedOrderListDic = new Dictionary<string, Order>();

        /// <summary>
        /// OrderInstanceID,AcceptedOrder
        /// </summary>
        [ShowInInspector]
        private Dictionary<string, Order> OrderDelegationListDic = new Dictionary<string, Order>();

        public List<Order> AcceptedOrders { get { return AcceptedOrderList; } }
        private static int curAcceptOrder = 0;

        /// <summary>
        /// 紧急订单刷新计时器
        /// </summary>
        private CounterDownTimer UrgentOrderRefreshTimer;

        /// <summary>
        /// 已承接订单交付状态刷新计时器
        /// </summary>
        private CounterDownTimer CanBeCommitRefreshTimer;

        public event Action OrderPanelRefreshOrderUrgentDelegation;
        public event Action OrderPanelRefreshAcceptedOrder;

        private IInventory PlayerInventory;

        #region 策划配置项
        /*        [LabelText("紧急订单刷新时间间隔（现实时间的min为单位）")]*/
        [ShowInInspector]
        public float UrgentRefreshInterval = 0.1f;
        #endregion

        #region Base

        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;

        /// <summary>
        /// 单例管理
        /// </summary>
        public static OrderManager Instance { get { return instance; } }

        private static OrderManager instance;
        public void Init()
        {
            //TODO 之后由氏族模块处理
            OrderUrgentDelegationMap.Add("Clan1", new List<Order>() { null, null });
            OrderSpecialDelegationMap.Add("Clan1", new List<Order>());
            OrderNormalDelegationMap.Add("Clan1", new List<Order>());


            OrderUrgentDelegationMap.Add("Clan2", new List<Order>() { null, null });
            OrderSpecialDelegationMap.Add("Clan2", new List<Order>());
            OrderNormalDelegationMap.Add("Clan2", new List<Order>());


            OrderUrgentDelegationMap.Add("Clan3", new List<Order>() { null, null });
            OrderSpecialDelegationMap.Add("Clan3", new List<Order>());
            OrderNormalDelegationMap.Add("Clan3", new List<Order>());


            UnlockedOrderMap.Add(OrderType.Urgent,new List<string>());
            UnlockedOrderMap.Add(OrderType.Special, new List<string>());
            UnlockedOrderMap.Add(OrderType.Normal, new List<string>());



            //初始化Timer
            this.UrgentOrderRefreshTimer = new CounterDownTimer(0.1f * 60 * LocalGameManager.Instance.DispatchTimeManager.TimeScale, autocycle: false, autoStart: true);
            this.CanBeCommitRefreshTimer = new CounterDownTimer(5, autocycle: true, autoStart: true);
            this.UrgentOrderRefreshTimer.OnEndEvent += () =>
            {
                this.RefreshUrgentOrder();
            };
            this.CanBeCommitRefreshTimer.OnEndEvent += () =>
            {
                //this.RefreshAcceptedList();
            };

            //初始化背包
            this.PlayerInventory = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).OCState.Inventory;
            Debug.Log($"初始化背包 {PlayerInventory!= null}");
            // 载入表格数据 
            LoadTableData();

        }
        public void OnRegister()
        {
            if (instance == null)
            {
                instance = this;
                Init();
            }
        }

        public void OnUnregister()
        {
            if (instance == this)
            {
                instance = null;
            }
        }


        #endregion

        #region Internal
        /// <summary>
        /// 刷新紧急订单函数
        /// </summary>
        private void RefreshUrgentOrder()
        {
            //1-8号槽是否有空位？

            List<Tuple<string, Order>> emptyList = new List<Tuple<string, Order>>();
            HashSet<string> alreadyHaveOrderId = new HashSet<string>();

            foreach ( (string ClanID,List<Order> orders) in OrderUrgentDelegationMap)
            {
                if (orders[0] == null) emptyList.Add(new Tuple<string, Order>(ClanID, orders[0]));
                else alreadyHaveOrderId.Add(orders[0].OrderID);
                if (orders[1] == null) emptyList.Add(new Tuple<string, Order>(ClanID, orders[1]));
                else alreadyHaveOrderId.Add(orders[1].OrderID);
            }
            if(emptyList.Count == 0)
            {
                //重新开始计时
                Debug.Log("没有空位，重新开始计时");
                this.UrgentOrderRefreshTimer.Start();
                return;
            }
            //随机选定一个槽位
            System.Random rand = new System.Random();

            (string clanId,Order order) = emptyList[rand.Next(0, emptyList.Count)];

            //抽选对应氏族紧急订单
            string extractOrderId = null;

            List<string> strings = null;
            if (UnlockedUrgentOrderMap.ContainsKey(clanId))
            {
                strings = UnlockedUrgentOrderMap[clanId];
            }

            if (strings == null || strings.Count == 0)
            {
                //重新开始计时
                Debug.Log("对应氏族没有可解锁订单，重新开始计时");
                this.UrgentOrderRefreshTimer.Start();
                return;
            }

            do
            {
                extractOrderId = strings[rand.Next(0, strings.Count)];
            } while (alreadyHaveOrderId.Contains(extractOrderId));

            AddOrderToOrderDelegationMap(extractOrderId);

            //重新开始计时
            this.UrgentOrderRefreshTimer.Start();
            return;
        }

        /// <summary>
        /// 刷新已承接列表函数
        /// </summary>
        public void RefreshAcceptedList()
        {
            for (int i = 0; i < AcceptedOrderList.Count; i++) 
            {
                
                Order order = AcceptedOrderList[i];
                OrderTableData orderTableData = OrderTableDataDic[order.OrderID];
                List<Formula> formulaList = new List<Formula>();
                List<Formula> AddedItems = new List<Formula>();
                foreach (var requireItem in AcceptedOrderList[i].RemainRequireItemDic)
                {
                    formulaList.Add(new Formula() { id = requireItem.Key,num = requireItem.Value});
                }

                //扣除
                AddedItems = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).InventoryCostItems(formulaList, priority:-1);

                foreach (var AddedItem in AddedItems)
                {
                    bool isFinish = order.ChangeRequireItemDic(AddedItem.id, AddedItem.num);
                    if(isFinish)
                    {
                        Debug.Log("isFinish");
                        AcceptedOrderList[i].canBeCommit = true;
                    }
                }
            }
            //
            AcceptedOrderList.Sort();
        }

        /// <summary>
        /// 解锁订单函数
        /// </summary>
        public void UnlockOrder(string OrderId)
        {
            if (OrderId == null) return;
            if (!OrderTableDataDic.ContainsKey(OrderId)) return;
            //Debug.Log("UnlockOrder " + OrderId);
            OrderTableData orderTableData = OrderTableDataDic[OrderId];
            string ClanID = OrderIDToClanIDDic[orderTableData.ID];
            UnlockedOrderMap[orderTableData.OrderType].Add(OrderId);

            if(orderTableData.OrderType == OrderType.Normal)
            {
                AddOrderToOrderDelegationMap(OrderId);
            }
            else if(orderTableData.OrderType == OrderType.Urgent)
            {
                if(UnlockedUrgentOrderMap.ContainsKey(ClanID))
                {
                    UnlockedUrgentOrderMap[ClanID].Add(OrderId);
                }
                else
                {
                    UnlockedUrgentOrderMap.Add(ClanID,new List<string> { OrderId});
                }
                
            }
            else if(orderTableData.OrderType == OrderType.Special)
            {
                AddOrderToOrderDelegationMap(OrderId);
            }
        }

        /// <summary>
        /// 添加订单函数
        /// </summary>
        public void AddOrderToOrderDelegationMap(string OrderId)
        {
            if (OrderId == null) return;
            if (!OrderTableDataDic.ContainsKey(OrderId)) return;
            //Debug.Log("接取订单 " + OrderId + " " + LocalGameManager.Instance.DispatchTimeManager.CurrentHour.ToString() + " : " + LocalGameManager.Instance.DispatchTimeManager.CurrentMinute.ToString());
            OrderTableData orderTableData = OrderTableDataDic[OrderId];
            string ClanID = OrderIDToClanIDDic[orderTableData.ID];

            if (orderTableData.OrderType == OrderType.Urgent)
            {
                OrderUrgent orderUrgent = new OrderUrgent(orderTableData);
                orderUrgent.StartReceiveDDLTimer();
                List<Order> orders = this.OrderUrgentDelegationMap[ClanID];
                for (int i = 0; i < orders.Count; i++)
                {
                    if (orders[i] == null)
                    {
                        orders[i] = orderUrgent;
                        //通知UI
                        GM.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.SideBarUI, new UIManager.SideBarUIData("<color=yellow>" + ClanID + "</color>  发布了紧急征求", orderTableData.OrderName));
                        break; 
                    }
                }
                OrderDelegationListDic.Add(orderUrgent.OrderInstanceID, orderUrgent);
                //紧急订单需要刷新UI
                OrderPanelRefreshOrderUrgentDelegation?.Invoke();
            }
            else if(orderTableData.OrderType == OrderType.Normal)
            {
                Debug.Log("常规订单有刷新！" + orderTableData.ID);
                OrderNormal orderNormal = new OrderNormal(orderTableData);
                OrderNormalDelegationMap[ClanID].Add(orderNormal);
                OrderDelegationListDic.Add(orderNormal.OrderInstanceID, orderNormal);
            }
            else if(orderTableData.OrderType == OrderType.Special)
            {
                OrderSpecial orderSpecial = new OrderSpecial(orderTableData);
                OrderSpecialDelegationMap[ClanID].Add(new OrderSpecial(orderTableData));
                OrderDelegationListDic.Add(orderSpecial.OrderInstanceID, orderSpecial);
            }
        }

        /// <summary>
        /// 拒绝订单函数 仅限于拒绝紧急订单  对应订单委托拒绝订单按钮
        /// </summary>
        public void RefuseOrder(string OrderInstanceID)
        {
            Debug.Log("OrderInstanceID " + OrderInstanceID + " " + OrderDelegationListDic.ContainsKey(OrderInstanceID));
            if (OrderInstanceID == null || !OrderDelegationListDic.ContainsKey(OrderInstanceID)) return;
            Order order = OrderDelegationListDic[OrderInstanceID];

            if (order.orderType != OrderType.Urgent) return;
            string ClanID = OrderIDToClanIDDic[order.OrderID];
            var olist = OrderUrgentDelegationMap[ClanID];
            for(int i = 0; i < olist.Count; i++)
            {
                if (olist[i]?.OrderInstanceID == order.OrderInstanceID)
                {
                    Debug.Log("订单超时 " + olist[i].OrderID+" "+ LocalGameManager.Instance.DispatchTimeManager.CurrentHour.ToString() + " : " + LocalGameManager.Instance.DispatchTimeManager.CurrentMinute.ToString());
                    OrderDelegationListDic.Remove(olist[i].OrderInstanceID);
                    olist[i] = null;
                    break;
                }
            }
            //紧急订单需要刷新UI
            OrderPanelRefreshOrderUrgentDelegation?.Invoke();
        }

        /// <summary>
        /// 接取订单函数
        /// </summary>
        public void ReceiveOrder(string OrderInstanceID)
        {
            if (OrderInstanceID == null || !OrderDelegationListDic.ContainsKey(OrderInstanceID)) return;
            Order order = OrderDelegationListDic[OrderInstanceID];


            string ClanID = OrderIDToClanIDDic[order.OrderID];
            List<Order> olist = new List<Order>();
            if (order.orderType == OrderType.Urgent)
            {
                olist = OrderUrgentDelegationMap[ClanID];
            }
            else if(order.orderType == OrderType.Normal)
            {
                olist = OrderNormalDelegationMap[ClanID];
            }
            else if(order.orderType == OrderType.Special)
            {
                olist = OrderSpecialDelegationMap[ClanID];
            }

            
            for (int i = 0; i < olist.Count; i++)
            {
                if (olist[i] != null && olist[i].OrderInstanceID == order.OrderInstanceID) 
                {
                    //加入已承接列表
                    Order acceptedOrder = null;
                    if(olist[i] is OrderUrgent)
                    {
                        OrderUrgent orderUrgent = (OrderUrgent)olist[i];
                        orderUrgent.StartDeliverDDLTimer();
                        acceptedOrder = orderUrgent;
                        //重置紧急订单状态
                        orderUrgent.Reset();
                        //紧急留空槽
                        olist[i] = null;
                    }
                    else if(olist[i] is OrderSpecial)
                    {
                        OrderSpecial orderSpecial = (OrderSpecial)olist[i];
                        acceptedOrder = orderSpecial;
                    }
                    else if (olist[i] is OrderNormal)
                    {
                        OrderNormal orderNormal = (OrderNormal)olist[i];
                        acceptedOrder = orderNormal;
                        
                        //常规直接删
                        olist.Remove(olist[i]);
                    }
                    OrderDelegationListDic.Remove(order.OrderInstanceID);
                    AcceptedOrderList.Add(acceptedOrder);
                    AcceptedOrderListDic.Add(acceptedOrder.OrderInstanceID, acceptedOrder);
                    //加入已承接列表后立即执行一次RefreshAcceptedList函数

                    break;
                }
            }

        }

        /// <summary>
        /// 取消订单函数
        /// </summary>
        public void CancleOrder(string OrderInstanceID)
        {
            if (OrderInstanceID == null || !AcceptedOrderListDic.ContainsKey(OrderInstanceID)) return;
            Order acceptedOrder = AcceptedOrderListDic[OrderInstanceID];
            if (acceptedOrder.canBeCancled == false) return;

            //TODO 已扣除返还玩家背包 暂时考虑背包无限

            foreach (var addedItem in acceptedOrder.AddedItemDic)
            {
                foreach (var item in ItemManager.Instance.SpawnItems(addedItem.Key, addedItem.Value))
                {
                    PlayerInventory.AddItem(item);
                }
                        
            }
            //TODO 受到惩罚
            Debug.Log("受到惩罚");

            //移除唯一命名
            AcceptedOrderListDic.Remove(acceptedOrder.OrderInstanceID);

            AcceptedOrderList.Remove(acceptedOrder);
                    
            this.RefreshAcceptedList();
        }

        /// <summary>
        /// 提交订单函数
        /// </summary>
        public void CommitOrder(string OrderInstanceID)
        {
            Debug.Log("提交订单 " + OrderInstanceID);
            if (OrderInstanceID == null || !AcceptedOrderListDic.ContainsKey(OrderInstanceID)) return;
            Order acceptedOrder = AcceptedOrderListDic[OrderInstanceID];
            if (acceptedOrder.canBeCommit == false) return;

            OrderTableData orderTableData = OrderTableDataDic[acceptedOrder.OrderID];

            //获得Item奖励

            foreach (var ordermap in orderTableData.ItemReward)
            {
                foreach (var item in ItemManager.Instance.SpawnItems(ordermap.id, ordermap.num))
                {
                    PlayerInventory.AddItem(item);
                }
            }

            //TODO 获得氏族信赖奖励

            //TODO 获得角色信赖奖励

            //加入等待重新进入的常规订单列表

            if (acceptedOrder is OrderNormal)
            {
                OrderNormal orderNormal = (OrderNormal)acceptedOrder;
                orderNormal.StartRefreshTimer();
                this.WaitingForRefreshNormalOrders.Add(orderNormal);
            }
            Debug.Log("1提交订单 " + acceptedOrder.OrderInstanceID);

            //移除唯一命名
            AcceptedOrderListDic.Remove(acceptedOrder.OrderInstanceID);

            AcceptedOrderList.Remove(acceptedOrder);
                    
            this.RefreshAcceptedList();
        }

        public string GenerateOrderInstanceID(string orderID)
        {
            return orderID+"|"+ML.Engine.Utility.OSTime.OSCurMilliSeconedTime.ToString();
        }
        

        #endregion

        #region DataFetch
        public List<Order> GetOrderDelegationOrders(string ClanId, OrderType orderType)
        {
            /*string debugDict = "";
            foreach (var item in this.OrderDelegationMap)
            {
                string itemValue = "";
                item.Value.Where(e => e != null).Select(e => e.OrderID).ForEach(e => itemValue += e);
                debugDict += item.Key + "--" + "Count: " + item.Value.Where(e => e != null).Count() + " " + "itemValue: " + itemValue + "\n";
            }
            Debug.Log($"{Time.frameCount} GetOrderDelegationOrders ClanId {ClanId} orderType {orderType} debugDict {debugDict}");
            foreach (var item in this.OrderDelegationMap)
            {
                if(item.Key.Item1 == ClanId && item.Key.Item2 == orderType)
                {
                    return item.Value;
                }
            }*/
            if (orderType == OrderType.Urgent)
            {
                if(OrderUrgentDelegationMap.ContainsKey(ClanId))
                {
                    return OrderUrgentDelegationMap[ClanId];
                }
            }
            else if (orderType == OrderType.Normal)
            {
                if (OrderNormalDelegationMap.ContainsKey(ClanId))
                {
                    return OrderNormalDelegationMap[ClanId];
                }
            }
            else if (orderType == OrderType.Special)
            {
                if (OrderSpecialDelegationMap.ContainsKey(ClanId))
                {
                    return OrderSpecialDelegationMap[ClanId];
                }
            }

            return new List<Order>();
        }
        public OrderTableData GetOrderTableData(string OrderInstanceID)
        {
            string id = GetOrderID(OrderInstanceID);
            if (id != null) 
            {
                return this.OrderTableDataDic[id];
            }
            return OrderTableData.None;
        }

        public string GetOrderID(string OrderInstanceID)
        {
            if (OrderInstanceID != null && AcceptedOrderListDic.ContainsKey(OrderInstanceID)) return AcceptedOrderListDic[OrderInstanceID].OrderID;
            if (OrderInstanceID != null && OrderDelegationListDic.ContainsKey(OrderInstanceID)) return OrderDelegationListDic[OrderInstanceID].OrderID;
            return null;
        }
        public OrderTableData GetOrderTableData(Order order)
        {
            if (order == null) return OrderTableData.None;
            if (this.OrderTableDataDic.ContainsKey(order.OrderID))
            {
                return this.OrderTableDataDic[order.OrderID];
            }
            return OrderTableData.None;
        }

        public OrderType GetOrderTypeInOrderDelegation(string OrderInstanceID)
        {
            if (OrderInstanceID == null || !OrderDelegationListDic.ContainsKey(OrderInstanceID)) return OrderType.None;
            return OrderDelegationListDic[OrderInstanceID].orderType;
        }

        public bool IsValidOrderIDInOrderDelegation(string OrderInstanceID)
        {
            if (OrderInstanceID == null || !OrderDelegationListDic.ContainsKey(OrderInstanceID)) return false;
            return true;
        }

        public bool IsValidOrderIDInAcceptedOrder(string OrderInstanceID)
        {
            if (OrderInstanceID == null || !AcceptedOrderListDic.ContainsKey(OrderInstanceID)) return false;
            return true;
        }

        public Order GetAcceptedOrder(string OrderInstanceID)
        {
            if(this.AcceptedOrderListDic.ContainsKey(OrderInstanceID))
            {
                return this.AcceptedOrderListDic[OrderInstanceID];
            }
            return null;
        }

        public IInventory GetInventory()
        {
            return this.PlayerInventory;
        }
        #endregion

        #region TableData
        [System.Serializable]
        public struct OrderMap
        {
            public string id;
            public int num;
        }

        [System.Serializable]
        public struct OrderTableData
        {
            public string ID;//键
            public OrderType OrderType;//类型
            public string OrderName;//名称
            public string OrderDescription;//描述
            public List<OrderMap> RequireList;//物资清单
            public List<OrderMap> ItemReward;//物品报酬
            public List<OrderMap> ClanReward;//TODO 暂时用Formula 氏族信赖报酬
            public List<OrderMap> CharaReward;//TODO 暂时用Formula 角色信赖报酬
            public bool IsFirstOrder;//首项
            public int CD;//需求周期
            public int ReceiveDDL;//接取时限
            public int DeliverDDL;//交付时限
            public List<OrderMap> PayBack;//TODO 暂时用Formula 偿还
            public string[] Contacter;//接洽人
            //事件函数
            public static OrderTableData None = new OrderTableData();
        }


        //TODO此表应在氏族模块中读
        [System.Serializable]
        public struct OrderUnlock
        {
            public string ID;//氏族ID
            public string[] UnlockOrders_LV0;//解锁订单ID数组
            public string[] UnlockOrders_LV1;//解锁订单ID数组
            public string[] UnlockOrders_LV2;//解锁订单ID数组
            public string[] UnlockOrders_LV3;//解锁订单ID数组
            public string[] UnlockOrders_LV4;//解锁订单ID数组
            public string[] UnlockOrders_LV5;//解锁订单ID数组
        }

        [ShowInInspector]
        private Dictionary<string,OrderTableData> OrderTableDataDic = new Dictionary<string,OrderTableData>();
        private List<OrderUnlock> OrderUnlockTableDataList = new List<OrderUnlock>();
        [ShowInInspector]
        private Dictionary<string,string> OrderIDToClanIDDic = new Dictionary<string,string>();
        #endregion

        #region Load
        private void LoadTableData()
        {
            ML.Engine.ABResources.ABJsonAssetProcessor<OrderTableData[]> ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<OrderTableData[]>("OC/Json/TableData", "Order", (datas) =>
            {
                //TODO 之后从氏族模块获取
                OrderIDToClanIDDic.Add("Order_Urgent_LiYuan_1", "Clan1");
                OrderIDToClanIDDic.Add("Order_Urgent_LiYuan_2", "Clan2");
                OrderIDToClanIDDic.Add("Order_Normal_LiYuan_1", "Clan3");
                OrderIDToClanIDDic.Add("Order_Special_LiYuan_1", "Clan1");
                OrderIDToClanIDDic.Add("Order_Urgent_LiYuan_3", "Clan1");
                OrderIDToClanIDDic.Add("Order_Urgent_LiYuan_4", "Clan1");
                OrderIDToClanIDDic.Add("Order_Normal_LiYuan_2", "Clan2");
                OrderIDToClanIDDic.Add("Order_Special_LiYuan_2", "Clan1");
                OrderIDToClanIDDic.Add("Order_Normal_LiYuan_3", "Clan1");
                OrderIDToClanIDDic.Add("Order_Normal_LiYuan_4", "Clan2");
                OrderIDToClanIDDic.Add("Order_Normal_LiYuan_5", "Clan3");
                OrderIDToClanIDDic.Add("Order_Normal_LiYuan_6", "Clan1");
                OrderIDToClanIDDic.Add("Order_Urgent_LiYuan_5", "Clan2");
                OrderIDToClanIDDic.Add("Order_Urgent_LiYuan_6", "Clan3");
                OrderIDToClanIDDic.Add("Order_Urgent_LiYuan_7", "Clan3");
                OrderIDToClanIDDic.Add("Order_Urgent_LiYuan_8", "Clan1");
                foreach (var data in datas)
                {
                    //TODO 暂时直接解锁
                    this.OrderTableDataDic.Add(data.ID, data);
                    UnlockOrder(data.ID);
                }


                
            }, "订单数据");
            ABJAProcessor.StartLoadJsonAssetData();

        }
        #endregion


    }


}


