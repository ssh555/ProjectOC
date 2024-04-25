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
        /// ��������
        /// </summary>
        public enum OrderType
        {
            Urgent = 0,
            Special,
            Normal,
            None,
        }

        /// <summary>
        /// ��ǰÿ��������Ѿ������Ķ���ID �������ͣ��������ѽ����Ķ���ID����
        /// </summary>
        [ShowInInspector]
        private Dictionary<OrderType, List<string>>  UnlockedOrderMap = new Dictionary<OrderType, List<string>>();

        /// <summary>
        /// ��ǰÿ�������Ѿ������Ľ�������
        /// </summary>
        [ShowInInspector]
        private Dictionary<string, List<string>> UnlockedUrgentOrderMap = new Dictionary<string, List<string>>();

        /// <summary>
        /// ��ǰ����ί���д��ڵĶ��� string ����ID��List<Order>
        /// </summary>
        [ShowInInspector]
        private Dictionary<string, List<Order>> OrderNormalDelegationMap = new Dictionary<string, List<Order>>();

        [ShowInInspector]
        private Dictionary<string, List<Order>> OrderSpecialDelegationMap = new Dictionary<string, List<Order>>();

        [ShowInInspector]
        private Dictionary<string, List<Order>> OrderUrgentDelegationMap = new Dictionary<string, List<Order>>();
        /// <summary>
        /// ��ǰ���ڵȴ�ˢ�µĳ��涩�� (string ����ID,OrderType type)��List<Order>
        /// </summary>
        [ShowInInspector]
        private List<OrderNormal> WaitingForRefreshNormalOrders = new List<OrderNormal>();


        /// <summary>
        /// �ѳн��б�Ķ��� 
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
        /// ��������ˢ�¼�ʱ��
        /// </summary>
        private CounterDownTimer UrgentOrderRefreshTimer;

        /// <summary>
        /// �ѳнӶ�������״̬ˢ�¼�ʱ��
        /// </summary>
        private CounterDownTimer CanBeCommitRefreshTimer;

        public event Action OrderPanelRefreshOrderUrgentDelegation;
        public event Action OrderPanelRefreshAcceptedOrder;

        private IInventory PlayerInventory;

        #region �߻�������
        /*        [LabelText("��������ˢ��ʱ��������ʵʱ���minΪ��λ��")]*/
        [ShowInInspector]
        public float UrgentRefreshInterval = 0.1f;
        #endregion

        #region Base

        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;

        /// <summary>
        /// ��������
        /// </summary>
        public static OrderManager Instance { get { return instance; } }

        private static OrderManager instance;
        public void Init()
        {
            //TODO ֮��������ģ�鴦��
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



            //��ʼ��Timer
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

            //��ʼ������
            this.PlayerInventory = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).OCState.Inventory;
            Debug.Log($"��ʼ������ {PlayerInventory!= null}");
            // ���������� 
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
        /// ˢ�½�����������
        /// </summary>
        private void RefreshUrgentOrder()
        {
            //1-8�Ų��Ƿ��п�λ��

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
                //���¿�ʼ��ʱ
                Debug.Log("û�п�λ�����¿�ʼ��ʱ");
                this.UrgentOrderRefreshTimer.Start();
                return;
            }
            //���ѡ��һ����λ
            System.Random rand = new System.Random();

            (string clanId,Order order) = emptyList[rand.Next(0, emptyList.Count)];

            //��ѡ��Ӧ�����������
            string extractOrderId = null;

            List<string> strings = null;
            if (UnlockedUrgentOrderMap.ContainsKey(clanId))
            {
                strings = UnlockedUrgentOrderMap[clanId];
            }

            if (strings == null || strings.Count == 0)
            {
                //���¿�ʼ��ʱ
                Debug.Log("��Ӧ����û�пɽ������������¿�ʼ��ʱ");
                this.UrgentOrderRefreshTimer.Start();
                return;
            }

            do
            {
                extractOrderId = strings[rand.Next(0, strings.Count)];
            } while (alreadyHaveOrderId.Contains(extractOrderId));

            AddOrderToOrderDelegationMap(extractOrderId);

            //���¿�ʼ��ʱ
            this.UrgentOrderRefreshTimer.Start();
            return;
        }

        /// <summary>
        /// ˢ���ѳн��б���
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

                //�۳�
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
        /// ������������
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
        /// ��Ӷ�������
        /// </summary>
        public void AddOrderToOrderDelegationMap(string OrderId)
        {
            if (OrderId == null) return;
            if (!OrderTableDataDic.ContainsKey(OrderId)) return;
            //Debug.Log("��ȡ���� " + OrderId + " " + LocalGameManager.Instance.DispatchTimeManager.CurrentHour.ToString() + " : " + LocalGameManager.Instance.DispatchTimeManager.CurrentMinute.ToString());
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
                        //֪ͨUI
                        GM.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.SideBarUI, new UIManager.SideBarUIData("<color=yellow>" + ClanID + "</color>  �����˽�������", orderTableData.OrderName));
                        break; 
                    }
                }
                OrderDelegationListDic.Add(orderUrgent.OrderInstanceID, orderUrgent);
                //����������Ҫˢ��UI
                OrderPanelRefreshOrderUrgentDelegation?.Invoke();
            }
            else if(orderTableData.OrderType == OrderType.Normal)
            {
                Debug.Log("���涩����ˢ�£�" + orderTableData.ID);
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
        /// �ܾ��������� �����ھܾ���������  ��Ӧ����ί�оܾ�������ť
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
                    Debug.Log("������ʱ " + olist[i].OrderID+" "+ LocalGameManager.Instance.DispatchTimeManager.CurrentHour.ToString() + " : " + LocalGameManager.Instance.DispatchTimeManager.CurrentMinute.ToString());
                    OrderDelegationListDic.Remove(olist[i].OrderInstanceID);
                    olist[i] = null;
                    break;
                }
            }
            //����������Ҫˢ��UI
            OrderPanelRefreshOrderUrgentDelegation?.Invoke();
        }

        /// <summary>
        /// ��ȡ��������
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
                    //�����ѳн��б�
                    Order acceptedOrder = null;
                    if(olist[i] is OrderUrgent)
                    {
                        OrderUrgent orderUrgent = (OrderUrgent)olist[i];
                        orderUrgent.StartDeliverDDLTimer();
                        acceptedOrder = orderUrgent;
                        //���ý�������״̬
                        orderUrgent.Reset();
                        //�������ղ�
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
                        
                        //����ֱ��ɾ
                        olist.Remove(olist[i]);
                    }
                    OrderDelegationListDic.Remove(order.OrderInstanceID);
                    AcceptedOrderList.Add(acceptedOrder);
                    AcceptedOrderListDic.Add(acceptedOrder.OrderInstanceID, acceptedOrder);
                    //�����ѳн��б������ִ��һ��RefreshAcceptedList����

                    break;
                }
            }

        }

        /// <summary>
        /// ȡ����������
        /// </summary>
        public void CancleOrder(string OrderInstanceID)
        {
            if (OrderInstanceID == null || !AcceptedOrderListDic.ContainsKey(OrderInstanceID)) return;
            Order acceptedOrder = AcceptedOrderListDic[OrderInstanceID];
            if (acceptedOrder.canBeCancled == false) return;

            //TODO �ѿ۳�������ұ��� ��ʱ���Ǳ�������

            foreach (var addedItem in acceptedOrder.AddedItemDic)
            {
                foreach (var item in ItemManager.Instance.SpawnItems(addedItem.Key, addedItem.Value))
                {
                    PlayerInventory.AddItem(item);
                }
                        
            }
            //TODO �ܵ��ͷ�
            Debug.Log("�ܵ��ͷ�");

            //�Ƴ�Ψһ����
            AcceptedOrderListDic.Remove(acceptedOrder.OrderInstanceID);

            AcceptedOrderList.Remove(acceptedOrder);
                    
            this.RefreshAcceptedList();
        }

        /// <summary>
        /// �ύ��������
        /// </summary>
        public void CommitOrder(string OrderInstanceID)
        {
            Debug.Log("�ύ���� " + OrderInstanceID);
            if (OrderInstanceID == null || !AcceptedOrderListDic.ContainsKey(OrderInstanceID)) return;
            Order acceptedOrder = AcceptedOrderListDic[OrderInstanceID];
            if (acceptedOrder.canBeCommit == false) return;

            OrderTableData orderTableData = OrderTableDataDic[acceptedOrder.OrderID];

            //���Item����

            foreach (var ordermap in orderTableData.ItemReward)
            {
                foreach (var item in ItemManager.Instance.SpawnItems(ordermap.id, ordermap.num))
                {
                    PlayerInventory.AddItem(item);
                }
            }

            //TODO ���������������

            //TODO ��ý�ɫ��������

            //����ȴ����½���ĳ��涩���б�

            if (acceptedOrder is OrderNormal)
            {
                OrderNormal orderNormal = (OrderNormal)acceptedOrder;
                orderNormal.StartRefreshTimer();
                this.WaitingForRefreshNormalOrders.Add(orderNormal);
            }
            Debug.Log("1�ύ���� " + acceptedOrder.OrderInstanceID);

            //�Ƴ�Ψһ����
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
            public string ID;//��
            public OrderType OrderType;//����
            public string OrderName;//����
            public string OrderDescription;//����
            public List<OrderMap> RequireList;//�����嵥
            public List<OrderMap> ItemReward;//��Ʒ����
            public List<OrderMap> ClanReward;//TODO ��ʱ��Formula ������������
            public List<OrderMap> CharaReward;//TODO ��ʱ��Formula ��ɫ��������
            public bool IsFirstOrder;//����
            public int CD;//��������
            public int ReceiveDDL;//��ȡʱ��
            public int DeliverDDL;//����ʱ��
            public List<OrderMap> PayBack;//TODO ��ʱ��Formula ����
            public string[] Contacter;//��Ǣ��
            //�¼�����
            public static OrderTableData None = new OrderTableData();
        }


        //TODO�˱�Ӧ������ģ���ж�
        [System.Serializable]
        public struct OrderUnlock
        {
            public string ID;//����ID
            public string[] UnlockOrders_LV0;//��������ID����
            public string[] UnlockOrders_LV1;//��������ID����
            public string[] UnlockOrders_LV2;//��������ID����
            public string[] UnlockOrders_LV3;//��������ID����
            public string[] UnlockOrders_LV4;//��������ID����
            public string[] UnlockOrders_LV5;//��������ID����
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
                //TODO ֮�������ģ���ȡ
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
                    //TODO ��ʱֱ�ӽ���
                    this.OrderTableDataDic.Add(data.ID, data);
                    UnlockOrder(data.ID);
                }


                
            }, "��������");
            ABJAProcessor.StartLoadJsonAssetData();

        }
        #endregion


    }


}


