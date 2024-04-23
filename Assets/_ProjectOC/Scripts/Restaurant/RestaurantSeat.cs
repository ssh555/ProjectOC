using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace ProjectOC.RestaurantNS
{
    [LabelText("������λ"), Serializable]
    public struct RestaurantSeat
    {
        [LabelText("��Ӧ�Ĳ���"), ReadOnly]
        public Restaurant Restaurant;
        [LabelText("��Ӧ��Socket"), ReadOnly]
        public Transform Socket;

        [LabelText("�󶨵ĵ���"), ShowInInspector, ReadOnly]
        public WorkerNS.Worker Worker { get; private set; }
        [LabelText("�������ڳԵ�ʳ��"), ShowInInspector, ReadOnly]
        public string EatFoodID { get; private set; }

        [LabelText("�����Ƿ񵽴�"), ReadOnly]
        public bool HasArrive;
        [LabelText("�Ƿ��а󶨵���"), ShowInInspector, ReadOnly]
        public bool HasWorker => !string.IsNullOrEmpty(Worker?.InstanceID);
        [LabelText("�Ƿ����ڳԶ���"), ShowInInspector, ReadOnly]
        public bool IsEat => !string.IsNullOrEmpty(EatFoodID) && timer != null && !timer.IsStoped;

        private ML.Engine.Timer.CounterDownTimer timer;
        [LabelText("��ʳ��ʱ��"), ReadOnly]
        public ML.Engine.Timer.CounterDownTimer Timer
        {
            get 
            {
                if (timer == null && Restaurant != null && HasWorker && HasArrive)
                {
                    if (LocalGameManager.Instance.RestaurantManager.WorkerFood_IsValidID(EatFoodID))
                    {
                        timer = new ML.Engine.Timer.CounterDownTimer(LocalGameManager.Instance.RestaurantManager.WorkerFood_EatTime(EatFoodID), false, false);
                        timer.OnEndEvent += EndActionForTimer;
                    }
                }
                return timer; 
            }
        }

        public void SetWorker(WorkerNS.Worker worker)
        {
            Worker = worker;
            Worker.Restaurant = Restaurant;
        }

        public void SetFood(string foodID)
        {
            EatFoodID = foodID;
            if (timer == null)
            {
                Timer?.Start();
            }
            else
            {
                Timer.Reset(LocalGameManager.Instance.RestaurantManager.WorkerFood_EatTime(EatFoodID));
            }
        }

        public void ClearData()
        {
            if (HasWorker)
            {
                Worker.ClearDestination();
                Worker.Restaurant = null;
                Worker.RecoverLastPosition();
                Worker = null;
            }
            if (IsEat)
            {
                timer?.End();
                ML.Engine.InventorySystem.Item item = ML.Engine.InventorySystem.ItemManager.Instance.SpawnItem(LocalGameManager.Instance.RestaurantManager.WorkerFood_ItemID(EatFoodID));
                item.Amount = 1;
                (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as ProjectOC.Player.OCPlayerController).OCState.Inventory.AddItem(item);
            }

            HasArrive = false;
            EatFoodID = "";
        }

        private void EndActionForTimer()
        {
            var restaurantManager = LocalGameManager.Instance.RestaurantManager;
            if (restaurantManager.WorkerFood_IsValidID(EatFoodID) && Worker != null)
            {
                Worker.AlterAP(restaurantManager.WorkerFood_AlterAP(EatFoodID));
                var mood = restaurantManager.WorkerFood_AlterMoodOdds(EatFoodID);

                System.Random random = new System.Random();
                if (random.NextDouble() <= mood.Item1)
                {
                    Worker.AlterMood(mood.Item2);
                }

                if (Worker.APCurrent >= Worker.APMax || !Restaurant.HasFood)
                {
                    WorkerNS.Worker worker = Worker;
                    ClearData();
                    if (!Restaurant.HasFood && Worker.APCurrent < Worker.APMax)
                    {
                        restaurantManager.AddWorker(worker);
                    }
                }
                else
                {
                    string foodID = Restaurant.EatFood(Worker);
                    if (restaurantManager.WorkerFood_IsValidID(foodID))
                    {
                        SetFood(foodID);
                    }
                }
            }
        }
    }
}
