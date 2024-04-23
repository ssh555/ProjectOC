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
            Normal
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
        /// �ѳнӶ����ṹ��
        /// </summary>
        [Serializable]
        public class AcceptedOrder : IComparable<AcceptedOrder>
        {
            public Order order;
            public OrderType orderType;
            public bool canBeCommit;
            public int acceptOrder;

            public AcceptedOrder(Order order, OrderType orderType)
            {
                this.order = order;
                this.orderType = orderType;
                this.canBeCommit = false;
                this.acceptOrder = curAcceptOrder++;
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
        [ShowInInspector]
        private List<AcceptedOrder> acceptedList = new List<AcceptedOrder>();
        private Dictionary<string, AcceptedOrder> acceptedListDic = new Dictionary<string, AcceptedOrder>();

        public List<AcceptedOrder> AcceptedOrders { get { return acceptedList; } }
        private static int curAcceptOrder = 0;

        /// <summary>
        /// ��������ˢ�¼�ʱ��
        /// </summary>
        private CounterDownTimer UrgentOrderRefreshTimer;

        /// <summary>
        /// �ѳнӶ�������״̬ˢ�¼�ʱ��
        /// </summary>
        private CounterDownTimer CanBeCommitRefreshTimer;

        public event Action OnCanBeCommitRefresh;

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
                this.RefreshAcceptedList();
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
        private void RefreshAcceptedList()
        {
            for (int i = 0; i < acceptedList.Count; i++) 
            {
                
                Order order = acceptedList[i].order;
                OrderTableData orderTableData = OrderTableDataDic[order.OrderID];
                List<Formula> formulaList = new List<Formula>();
                List<Formula> AddedItems = new List<Formula>();
                foreach (var requireItem in acceptedList[i].order.RemainRequireItemDic)
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
                        acceptedList[i].canBeCommit = true;
                    }
                }
            }
            //
            acceptedList.Sort();
            OnCanBeCommitRefresh?.Invoke();
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
                OrderUrgent orderUrgent = new OrderUrgent(orderTableData.ID, orderTableData.RequireList, orderTableData.ReceiveDDL, orderTableData.DeliverDDL);
                orderUrgent.StartReceiveDDLTimer();
                
                List<Order> orders = new List<Order>(this.OrderUrgentDelegationMap[ClanID].ToArray());
                for (int i = 0; i < orders.Count; i++)
                {
                    if (orders[i] == null)
                    {
                        Debug.Log(orderUrgent);
                        orders[i] = orderUrgent;
                        //֪ͨUI
                        GM.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.SideBarUI, new UIManager.SideBarUIData("<color=yellow>" + ClanID + "</color>  �����˽�������", orderTableData.OrderName));
                        break; 
                    }
                }
/*                this.OrderUrgentDelegationMap[ClanID].Clear();*/
                /*                foreach (var order in orders)
                                {
                                    this.OrderUrgentDelegationMap[ClanID].Add(order);
                                }*/
            }
            else if(orderTableData.OrderType == OrderType.Normal)
            {
                OrderNormalDelegationMap[ClanID].Add(new OrderNormal(orderTableData.ID, orderTableData.RequireList, orderTableData.CD));
            }
            else if(orderTableData.OrderType == OrderType.Special)
            {
                OrderSpecialDelegationMap[ClanID].Add(new OrderSpecial(orderTableData.ID, orderTableData.RequireList));
            }
            
        }



        /// <summary>
        /// �ܾ��������� �����ھܾ���������  ��Ӧ����ί�оܾ�������ť
        /// </summary>
        public void RefuseOrder(string OrderId)
        {
            if (OrderId == null) return;
            if (!OrderTableDataDic.ContainsKey(OrderId)) return;
            OrderTableData orderTableData = OrderTableDataDic[OrderId];
            if (orderTableData.OrderType != OrderType.Urgent) return;
            string ClanID = OrderIDToClanIDDic[orderTableData.ID];
            var olist = OrderUrgentDelegationMap[ClanID];
            for(int i = 0; i < olist.Count; i++)
            {
                if (olist[i]?.OrderID == OrderId)
                {
                    Debug.Log("������ʱ " + olist[i].OrderID+" "+ LocalGameManager.Instance.DispatchTimeManager.CurrentHour.ToString() + " : " + LocalGameManager.Instance.DispatchTimeManager.CurrentMinute.ToString());
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
            if(OrderId == null) return;
            if (!OrderTableDataDic.ContainsKey(OrderId)) return;
            OrderTableData orderTableData = OrderTableDataDic[OrderId];
            string ClanID = OrderIDToClanIDDic[orderTableData.ID];
            List<Order> olist = new List<Order>();
            if (orderTableData.OrderType == OrderType.Urgent)
            {
                olist = OrderUrgentDelegationMap[ClanID];
            }
            else if(orderTableData.OrderType == OrderType.Normal)
            {
                olist = OrderNormalDelegationMap[ClanID];
            }
            else if(orderTableData.OrderType == OrderType.Special)
            {
                olist = OrderSpecialDelegationMap[ClanID];
            }

            
            for (int i = 0; i < olist.Count; i++)
            {
                if (olist[i] != null && olist[i].OrderID == OrderId) 
                {
                    //�����ѳн��б�
                    AcceptedOrder acceptedOrder = null;
                    if(olist[i] is OrderUrgent)
                    {
                        OrderUrgent orderUrgent = (OrderUrgent)olist[i];
                        orderUrgent.StartDeliverDDLTimer();
                        acceptedOrder = new AcceptedOrder(orderUrgent, orderTableData.OrderType);
                        //���ý�������״̬
                        orderUrgent.Reset();
                        //�������ղ�
                        olist[i] = null;
                    }
                    else if(olist[i] is OrderSpecial)
                    {
                        acceptedOrder = new AcceptedOrder((OrderSpecial)olist[i], orderTableData.OrderType);
                    }
                    else if (olist[i] is OrderNormal)
                    {
                        acceptedOrder = new AcceptedOrder((OrderNormal)olist[i], orderTableData.OrderType);
                        
                        //����ֱ��ɾ
                        olist.Remove(olist[i]);
                    }
                    acceptedList.Add(acceptedOrder);
                    acceptedListDic.Add(orderTableData.ID, acceptedOrder);
                    //�����ѳн��б������ִ��һ��RefreshAcceptedList����

                    break;
                }
            }
        }

        /// <summary>
        /// ȡ����������
        /// </summary>
        public void CancleOrder(string OrderId)
        {
            if (OrderId == null) return;
            for (int i = 0; i < acceptedList.Count; i++)
            {
                if (acceptedList[i].order.OrderID == OrderId)
                {
                    //TODO �ѿ۳�������ұ��� ��ʱ���Ǳ�������

                    OrderTableData orderTableData = OrderTableDataDic[acceptedList[i].order.OrderID];
                    foreach (var addedItem in acceptedList[i].order.AddedItemDic)
                    {
                        foreach (var item in ItemManager.Instance.SpawnItems(addedItem.Key, addedItem.Value))
                        {
                            PlayerInventory.AddItem(item);
                        }
                        
                    }

                    //TODO �ܵ��ͷ�
                    Debug.Log("�ܵ��ͷ�");
                    acceptedListDic.Remove(acceptedList[i].order.OrderID);
                    acceptedList.Remove(acceptedList[i]);
                    
                    this.RefreshAcceptedList();
                    break;
                }
            }


        }

        /// <summary>
        /// �ύ��������
        /// </summary>
        public void CommitOrder(string OrderId)
        {
            Debug.Log("�ύ���� " + OrderId);
            if (OrderId == null) return;
            if (!OrderTableDataDic.ContainsKey(OrderId)) return;
            OrderTableData orderTableData = OrderTableDataDic[OrderId];

            for (int i = 0; i < acceptedList.Count; i++)
            {
                if (acceptedList[i].order.OrderID == OrderId)
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

                    if (acceptedList[i].order is OrderNormal)
                    {
                        OrderNormal orderNormal = (OrderNormal)acceptedList[i].order;
                        orderNormal.StartRefreshTimer();
                        this.WaitingForRefreshNormalOrders.Add(orderNormal);
                    }
                    Debug.Log("1�ύ���� " + acceptedList[i].order.OrderID);
                    acceptedListDic.Remove(acceptedList[i].order.OrderID);
                    acceptedList.Remove(acceptedList[i]);
                    
                    this.RefreshAcceptedList();
                }
            }
        }

        /// <summary>
        /// ���ٶ���ʵ��
        /// </summary>
        //public void int

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
        public OrderTableData GetOrderTableData(string orderId)
        {
            if(this.OrderTableDataDic.ContainsKey((orderId)))
            {
                return this.OrderTableDataDic[orderId];
            }
            return OrderTableData.None;
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

        public bool IsValidOrderID(string orderID)
        {
            if (orderID == null) return false;
            if (this.OrderTableDataDic.ContainsKey(orderID)) return true;
            return false;
        }

        public AcceptedOrder GetAcceptedOrder(string orderID)
        {
            if(this.acceptedListDic.ContainsKey(orderID))
            {
                return this.acceptedListDic[orderID];
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


