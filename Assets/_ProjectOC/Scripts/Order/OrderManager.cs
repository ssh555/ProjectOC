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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static ProjectOC.Order.OrderManager;
using static UnityEditor.Timeline.Actions.MenuPriority;


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
            Normal
        }

        /// <summary>
        /// 当前每个种类的已经解锁的订单ID 订单类型，该类型已解锁的订单ID数组
        /// </summary>
        private Dictionary<OrderType, List<string>>  UnlockedOrderMap = new Dictionary<OrderType, List<string>>();

        /// <summary>
        /// 当前每个氏族已经解锁的紧急订单
        /// </summary>
        private Dictionary<string, List<string>> UnlockedUrgentOrderMap = new Dictionary<string, List<string>>();

        /// <summary>
        /// 当前订单委托中存在的订单(string 氏族ID,OrderType type)，List<Order>
        /// </summary>
        private Dictionary<(string, OrderType), List<Order>> OrderDelegationMap = new Dictionary<(string, OrderType), List<Order>>();

        /// <summary>
        /// 当前正在等待刷新的常规订单 (string 氏族ID,OrderType type)，List<Order>
        /// </summary>
        private List<OrderNormal> WaitingForRefreshNormalOrders = new List<OrderNormal>();

        /// <summary>
        /// 已承接订单结构体
        /// </summary>
        private struct AcceptedOrder : IComparable<AcceptedOrder>
        {
            public Order order;
            public OrderType orderType;
            public bool canBeCommit;
            public int acceptOrder;

            public AcceptedOrder(Order order, OrderType orderType)
            {
                this.order = order;
                this.orderType = orderType;
                this.canBeCommit = true;
                this.acceptOrder = curAcceptOrder++;
            }
            
            public void SetCanBeCommit(bool canBeCommit)
            {
                this.canBeCommit = canBeCommit;
            }

            // 实现 IComparable 接口中的 CompareTo 方法
            public int CompareTo(AcceptedOrder other)
            {
                // 首先按照 orderType 升序
                int result = orderType.CompareTo(other.orderType);
                if (result != 0)
                {
                    return result;
                }

                // 如果 orderType 相同，按照 canBeCommit 升序
                result = canBeCommit.CompareTo(other.canBeCommit);
                if (result != 0)
                {
                    return result;
                }

                // 如果 canBeCommit 也相同，按照 acceptOrder 升序
                return acceptOrder.CompareTo(other.acceptOrder);
            }
        }

        /// <summary>
        /// 已承接列表的订单 (string 氏族ID,OrderType type)，List<Order>
        /// </summary>
        private List<AcceptedOrder> AcceptedList = new List<AcceptedOrder>();
        private static int curAcceptOrder = 0;

        /// <summary>
        /// 紧急订单刷新计时器
        /// </summary>
        private CounterDownTimer UrgentOrderRefreshTimer;

        /// <summary>
        /// 已承接订单交付状态刷新计时器
        /// </summary>
        private CounterDownTimer CanBeCommitRefreshTimer;

        private IInventory PlayerInventory;

        #region 策划配置项
        [LabelText("紧急订单刷新时间间隔（现实时间的min为单位）")]
        public int UrgentRefreshInterval = 1;
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
            OrderDelegationMap.Add(("ID1",OrderType.Urgent), new List<Order>(2));
            OrderDelegationMap.Add(("ID1", OrderType.Special), new List<Order>());
            OrderDelegationMap.Add(("ID1", OrderType.Normal), new List<Order>());


            OrderDelegationMap.Add(("ID2", OrderType.Urgent), new List<Order>(2));
            OrderDelegationMap.Add(("ID2", OrderType.Special), new List<Order>());
            OrderDelegationMap.Add(("ID2", OrderType.Normal), new List<Order>());


            OrderDelegationMap.Add(("ID3", OrderType.Urgent), new List<Order>(2));
            OrderDelegationMap.Add(("ID3", OrderType.Special), new List<Order>());
            OrderDelegationMap.Add(("ID3", OrderType.Normal), new List<Order>());


            UnlockedOrderMap.Add(OrderType.Urgent,new List<string>());
            UnlockedOrderMap.Add(OrderType.Special, new List<string>());
            UnlockedOrderMap.Add(OrderType.Normal, new List<string>());

            UnlockedUrgentOrderMap.Add("ID1",new List<string>());
            UnlockedUrgentOrderMap.Add("ID2", new List<string>());
            UnlockedUrgentOrderMap.Add("ID3", new List<string>());

            //初始化Timer
            this.UrgentOrderRefreshTimer = new CounterDownTimer(this.UrgentRefreshInterval * 60, autocycle: false, autoStart: true);
            this.CanBeCommitRefreshTimer = new CounterDownTimer(5, autocycle: true, autoStart: true);
            this.UrgentOrderRefreshTimer.OnEndEvent += () =>
            {
                this.RefreshUrgentOrder();
            };
            this.CanBeCommitRefreshTimer.OnEndEvent += () =>
            {
                this.RefreshAcceptedList();
            };

            PlayerInventory = GameObject.Find("PlayerCharacter(Clone)").GetComponent<PlayerCharacter>().Inventory;
            // 载入表格数据 
            LoadTableData();

        }
        public void OnRegister()
        {
            if (instance == null)
            {
                instance = this;
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

            foreach ( ((string ClanID,OrderType OrderType),List<Order> orders) in OrderDelegationMap)
            {
                if(OrderType == OrderType.Urgent)
                {
                    if (orders[0] == null) emptyList.Add(new Tuple<string, Order>(ClanID, orders[0]));
                    else alreadyHaveOrderId.Add(orders[0].OrderID);
                    if (orders[1] == null) emptyList.Add(new Tuple<string, Order>(ClanID, orders[1]));
                    else alreadyHaveOrderId.Add(orders[1].OrderID);
                }
            }
            if(emptyList.Count == 0)
            {
                //重新开始计时
                this.UrgentOrderRefreshTimer.Start();
                return;
            }
            //随机选定一个槽位
            System.Random rand = new System.Random();

            (string clanId,Order order) = emptyList[rand.Next(0, emptyList.Count)];

            //抽选对应氏族紧急订单
            string extractOrderId = null;
            List<string> strings = UnlockedUrgentOrderMap[clanId];

            if (strings.Count == 0)
            {
                //重新开始计时
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
        private void RefreshAcceptedList()
        {
            for (int i = 0; i < AcceptedList.Count; i++) 
            {
                OrderTableData orderTableData = OrderTableDataDic[AcceptedList[i].order.OrderID];

                // 能扣先扣

                //扣背包
                foreach (var requireItem in orderTableData.RequireList)
                {
                    //够扣全扣
                    if (PlayerInventory.RemoveItem(requireItem.id, requireItem.num))
                    {
                        AcceptedList[i].order.ChangeRequireItemDic(requireItem.id, requireItem.num);
                    }
                    else//不够把有的扣
                    {
                        AcceptedList[i].SetCanBeCommit(false);
                        var hasnum = PlayerInventory.GetItemAllNum(requireItem.id);
                        PlayerInventory.RemoveItem(requireItem.id, hasnum);
                        AcceptedList[i].order.ChangeRequireItemDic(requireItem.id, hasnum);
                    }
                }

                //TODO按顺序扣仓库
                if (!AcceptedList[i].canBeCommit) 
                {
                    //LocalGameManager.Instance.StoreManager.GetStores
                }
            }

            
            //

            AcceptedList.Sort();
        }

        /// <summary>
        /// 解锁订单函数
        /// </summary>
        public void UnlockOrder(string OrderId)
        {
            if (!OrderTableDataDic.ContainsKey(OrderId)) return;
            OrderTableData orderTableData = OrderTableDataDic[OrderId];
            string ClanID = OrderIDToClanIDDic[orderTableData.ID];
            UnlockedOrderMap[orderTableData.OrderType].Add(OrderId);
            UnlockedUrgentOrderMap[ClanID].Add(OrderId);
        }

        /// <summary>
        /// 添加订单函数
        /// </summary>
        public void AddOrderToOrderDelegationMap(string OrderId)
        {
            if (!OrderTableDataDic.ContainsKey(OrderId)) return;
            OrderTableData orderTableData = OrderTableDataDic[OrderId];
            string ClanID = OrderIDToClanIDDic[orderTableData.ID];
            if (orderTableData.OrderType == OrderType.Urgent)
            {
                OrderUrgent orderUrgent = new OrderUrgent(orderTableData.ID, orderTableData.RequireList, orderTableData.ReceiveDDL, orderTableData.DeliverDDL);
                orderUrgent.StartReceiveDDLTimer();
                OrderDelegationMap[(ClanID, orderTableData.OrderType)].Add(orderUrgent);
                //通知UI
                GM.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.SideBarUI, new UIManager.SideBarUIData("<color=yellow>" + ClanID + "</color>  发布了紧急征求", orderTableData.OrderName));
            }
            else if(orderTableData.OrderType == OrderType.Normal)
            {
                OrderDelegationMap[(ClanID, orderTableData.OrderType)].Add(new OrderNormal(orderTableData.ID, orderTableData.RequireList, orderTableData.CD));
            }
            else if(orderTableData.OrderType == OrderType.Special)
            {
                OrderDelegationMap[(ClanID, orderTableData.OrderType)].Add(new OrderSpecial(orderTableData.ID, orderTableData.RequireList));
            }
            
        }



        /// <summary>
        /// 拒绝订单函数 仅限于拒绝紧急订单  对应订单委托拒绝订单按钮
        /// </summary>
        public void RefuseOrder(string OrderId)
        {
            if (!OrderTableDataDic.ContainsKey(OrderId)) return;
            OrderTableData orderTableData = OrderTableDataDic[OrderId];
            string ClanID = OrderIDToClanIDDic[orderTableData.ID];
            var olist = OrderDelegationMap[(ClanID, OrderType.Urgent)];
            for(int i = 0; i < olist.Count; i++)
            {
                if (olist[i].OrderID == OrderId)
                {
                    olist[i] = null;
                    break;
                }
            }
        }

        /// <summary>
        /// 接取订单函数
        /// </summary>
        public void ReceiveOrder(string OrderId)
        {
            if (!OrderTableDataDic.ContainsKey(OrderId)) return;
            OrderTableData orderTableData = OrderTableDataDic[OrderId];
            string ClanID = OrderIDToClanIDDic[orderTableData.ID];

            var olist = OrderDelegationMap[(ClanID, orderTableData.OrderType)];
            for (int i = 0; i < olist.Count; i++)
            {
                if (olist[i].OrderID == OrderId)
                {
                    //加入已承接列表
                    
                    if(olist[i] is OrderUrgent)
                    {
                        OrderUrgent orderUrgent = (OrderUrgent)olist[i];
                        orderUrgent.StartDeliverDDLTimer();
                        AcceptedList.Add(new AcceptedOrder(orderUrgent, orderTableData.OrderType));
                    }
                    else if(olist[i] is OrderSpecial)
                    {
                        AcceptedList.Add(new AcceptedOrder((OrderSpecial)olist[i], orderTableData.OrderType));
                    }
                    else if (olist[i] is OrderNormal)
                    {
                        AcceptedList.Add(new AcceptedOrder((OrderNormal)olist[i], orderTableData.OrderType));
                    }

                    //加入已承接列表后立即执行一次RefreshAcceptedList函数

                    olist[i] = null;
                    break;
                }
            }
        }

        /// <summary>
        /// 取消订单函数
        /// </summary>
        public void CancleOrder(string OrderId)
        {
            for (int i = 0; i < AcceptedList.Count; i++)
            {
                if (AcceptedList[i].order.OrderID == OrderId)
                {
                    //TODO 已扣除返还玩家背包 暂时考虑背包无限

                    OrderTableData orderTableData = OrderTableDataDic[AcceptedList[i].order.OrderID];
                    
                    foreach (var addedItem in AcceptedList[i].order.AddedItemDic)
                    {
                        foreach (var item in ItemManager.Instance.SpawnItems(addedItem.Key, addedItem.Value))
                        {
                            PlayerInventory.AddItem(item);
                        }
                        
                    }


                    //TODO 受到惩罚

                    AcceptedList.Remove(AcceptedList[i]);
                    this.RefreshAcceptedList();
                }
            }


        }

        /// <summary>
        /// 提交订单函数
        /// </summary>
        public void CommitOrder(string OrderId)
        {
            if (!OrderTableDataDic.ContainsKey(OrderId)) return;
            OrderTableData orderTableData = OrderTableDataDic[OrderId];

            for (int i = 0; i < AcceptedList.Count; i++)
            {
                if (AcceptedList[i].order.OrderID == OrderId)
                {
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

                    if (AcceptedList[i].order is OrderNormal)
                    {
                        OrderNormal orderNormal = (OrderNormal)AcceptedList[i].order;
                        orderNormal.StartRefreshTimer();
                        this.WaitingForRefreshNormalOrders.Add(orderNormal);
                    }

                    AcceptedList.Remove(AcceptedList[i]);
                    this.RefreshAcceptedList();
                }
            }
        }

        /// <summary>
        /// 销毁订单实例
        /// </summary>
        //public void int

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


        private Dictionary<string,OrderTableData> OrderTableDataDic = new Dictionary<string,OrderTableData>();
        private List<OrderUnlock> OrderUnlockTableDataList = new List<OrderUnlock>();

        private Dictionary<string,string> OrderIDToClanIDDic = new Dictionary<string,string>();
        #endregion

        #region Load
        private void LoadTableData()
        {
            

        }
        #endregion


    }


}


