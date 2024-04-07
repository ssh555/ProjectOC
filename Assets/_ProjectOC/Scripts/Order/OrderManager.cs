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
        /// ��������
        /// </summary>
        public enum OrderType
        {
            Urgent = 0,
            Special,
            Normal
        }

        /// <summary>
        /// ��ǰÿ��������Ѿ������Ķ���ID �������ͣ��������ѽ����Ķ���ID����
        /// </summary>
        private Dictionary<OrderType, List<string>>  UnlockedOrderMap = new Dictionary<OrderType, List<string>>();

        /// <summary>
        /// ��ǰÿ�������Ѿ������Ľ�������
        /// </summary>
        private Dictionary<string, List<string>> UnlockedUrgentOrderMap = new Dictionary<string, List<string>>();

        /// <summary>
        /// ��ǰ����ί���д��ڵĶ���(string ����ID,OrderType type)��List<Order>
        /// </summary>
        private Dictionary<(string, OrderType), List<Order>> OrderDelegationMap = new Dictionary<(string, OrderType), List<Order>>();

        /// <summary>
        /// ��ǰ���ڵȴ�ˢ�µĳ��涩�� (string ����ID,OrderType type)��List<Order>
        /// </summary>
        private List<OrderNormal> WaitingForRefreshNormalOrders = new List<OrderNormal>();

        /// <summary>
        /// �ѳнӶ����ṹ��
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

            // ʵ�� IComparable �ӿ��е� CompareTo ����
            public int CompareTo(AcceptedOrder other)
            {
                // ���Ȱ��� orderType ����
                int result = orderType.CompareTo(other.orderType);
                if (result != 0)
                {
                    return result;
                }

                // ��� orderType ��ͬ������ canBeCommit ����
                result = canBeCommit.CompareTo(other.canBeCommit);
                if (result != 0)
                {
                    return result;
                }

                // ��� canBeCommit Ҳ��ͬ������ acceptOrder ����
                return acceptOrder.CompareTo(other.acceptOrder);
            }
        }

        /// <summary>
        /// �ѳн��б�Ķ��� (string ����ID,OrderType type)��List<Order>
        /// </summary>
        private List<AcceptedOrder> AcceptedList = new List<AcceptedOrder>();
        private static int curAcceptOrder = 0;

        /// <summary>
        /// ��������ˢ�¼�ʱ��
        /// </summary>
        private CounterDownTimer UrgentOrderRefreshTimer;

        /// <summary>
        /// �ѳнӶ�������״̬ˢ�¼�ʱ��
        /// </summary>
        private CounterDownTimer CanBeCommitRefreshTimer;

        private IInventory PlayerInventory;

        #region �߻�������
        [LabelText("��������ˢ��ʱ��������ʵʱ���minΪ��λ��")]
        public int UrgentRefreshInterval = 1;
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

            //��ʼ��Timer
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
            // ���������� 
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
        /// ˢ�½�����������
        /// </summary>
        private void RefreshUrgentOrder()
        {
            //1-8�Ų��Ƿ��п�λ��

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
                //���¿�ʼ��ʱ
                this.UrgentOrderRefreshTimer.Start();
                return;
            }
            //���ѡ��һ����λ
            System.Random rand = new System.Random();

            (string clanId,Order order) = emptyList[rand.Next(0, emptyList.Count)];

            //��ѡ��Ӧ�����������
            string extractOrderId = null;
            List<string> strings = UnlockedUrgentOrderMap[clanId];

            if (strings.Count == 0)
            {
                //���¿�ʼ��ʱ
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
        private void RefreshAcceptedList()
        {
            for (int i = 0; i < AcceptedList.Count; i++) 
            {
                OrderTableData orderTableData = OrderTableDataDic[AcceptedList[i].order.OrderID];

                // �ܿ��ȿ�

                //�۱���
                foreach (var requireItem in orderTableData.RequireList)
                {
                    //����ȫ��
                    if (PlayerInventory.RemoveItem(requireItem.id, requireItem.num))
                    {
                        AcceptedList[i].order.ChangeRequireItemDic(requireItem.id, requireItem.num);
                    }
                    else//�������еĿ�
                    {
                        AcceptedList[i].SetCanBeCommit(false);
                        var hasnum = PlayerInventory.GetItemAllNum(requireItem.id);
                        PlayerInventory.RemoveItem(requireItem.id, hasnum);
                        AcceptedList[i].order.ChangeRequireItemDic(requireItem.id, hasnum);
                    }
                }

                //TODO��˳��۲ֿ�
                if (!AcceptedList[i].canBeCommit) 
                {
                    //LocalGameManager.Instance.StoreManager.GetStores
                }
            }

            
            //

            AcceptedList.Sort();
        }

        /// <summary>
        /// ������������
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
        /// ��Ӷ�������
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
                //֪ͨUI
                GM.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.SideBarUI, new UIManager.SideBarUIData("<color=yellow>" + ClanID + "</color>  �����˽�������", orderTableData.OrderName));
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
        /// �ܾ��������� �����ھܾ���������  ��Ӧ����ί�оܾ�������ť
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
        /// ��ȡ��������
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
                    //�����ѳн��б�
                    
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

                    //�����ѳн��б������ִ��һ��RefreshAcceptedList����

                    olist[i] = null;
                    break;
                }
            }
        }

        /// <summary>
        /// ȡ����������
        /// </summary>
        public void CancleOrder(string OrderId)
        {
            for (int i = 0; i < AcceptedList.Count; i++)
            {
                if (AcceptedList[i].order.OrderID == OrderId)
                {
                    //TODO �ѿ۳�������ұ��� ��ʱ���Ǳ�������

                    OrderTableData orderTableData = OrderTableDataDic[AcceptedList[i].order.OrderID];
                    
                    foreach (var addedItem in AcceptedList[i].order.AddedItemDic)
                    {
                        foreach (var item in ItemManager.Instance.SpawnItems(addedItem.Key, addedItem.Value))
                        {
                            PlayerInventory.AddItem(item);
                        }
                        
                    }


                    //TODO �ܵ��ͷ�

                    AcceptedList.Remove(AcceptedList[i]);
                    this.RefreshAcceptedList();
                }
            }


        }

        /// <summary>
        /// �ύ��������
        /// </summary>
        public void CommitOrder(string OrderId)
        {
            if (!OrderTableDataDic.ContainsKey(OrderId)) return;
            OrderTableData orderTableData = OrderTableDataDic[OrderId];

            for (int i = 0; i < AcceptedList.Count; i++)
            {
                if (AcceptedList[i].order.OrderID == OrderId)
                {
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
        /// ���ٶ���ʵ��
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


