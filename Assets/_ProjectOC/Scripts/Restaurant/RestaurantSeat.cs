using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace ProjectOC.RestaurantNS
{
    [LabelText("餐厅座位"), Serializable]
    public class RestaurantSeat : IWorkerContainer
    {
        [LabelText("对应的餐厅"), ReadOnly, NonSerialized]
        public Restaurant Restaurant;
        [LabelText("对应的Socket"), ReadOnly, NonSerialized]
        public Transform Socket;
        [LabelText("刁民正在吃的食物"), ShowInInspector, ReadOnly]
        public string FoodID { get; private set; }
        [LabelText("是否正在吃东西"), ShowInInspector, ReadOnly]
        public bool IsEat => !string.IsNullOrEmpty(FoodID) && timer != null && !timer.IsStoped;
        [NonSerialized]
        private ML.Engine.Timer.CounterDownTimer timer;
        [LabelText("进食计时器")]
        public ML.Engine.Timer.CounterDownTimer Timer
        {
            get 
            {
                if (timer == null && HaveWorker && IsArrive && !string.IsNullOrEmpty(FoodID))
                {
                    timer = new ML.Engine.Timer.CounterDownTimer(ManagerNS.LocalGameManager.Instance.RestaurantManager.WorkerFood_EatTime(FoodID), false, false);
                    timer.OnEndEvent += EndActionForTimer;
                }
                return timer; 
            }
        }

        public RestaurantSeat(Restaurant restaurant, Transform socket)
        {
            Restaurant = restaurant;
            Socket = socket;
        }

        public void SetFood(string foodID)
        {
            FoodID = foodID;
            Timer?.Reset(ManagerNS.LocalGameManager.Instance.RestaurantManager.WorkerFood_EatTime(FoodID));
        }

        private void EndActionForTimer()
        {
            var restaurantManager = ManagerNS.LocalGameManager.Instance.RestaurantManager;
            Worker.AlterAP(restaurantManager.WorkerFood_AlterAP(FoodID));
            var mood = restaurantManager.WorkerFood_AlterMoodOdds(FoodID);

            System.Random random = new System.Random();
            if (random.NextDouble() <= mood.Item1)
            {
                Worker.AlterMood(mood.Item2);
            }

            if (Worker.APCurrent >= Worker.APRelaxThreshold || !Restaurant.HaveFood)
            {
                Worker worker = Worker;
                (this as IWorkerContainer).RemoveWorker();
                if (!Restaurant.HaveFood && Worker.APCurrent < Worker.APRelaxThreshold)
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

        #region IWorkerContainer
        public Worker Worker { get; set; }
        public bool IsArrive { get; set; }
        public bool HaveWorker => Worker != null && !string.IsNullOrEmpty(Worker.InstanceID);
        public Action<Worker> OnSetWorkerEvent { get; set; }
        public Action<bool> OnRemoveWorkerEvent { get; set; }

        public string GetUID() { return Restaurant.UID; }
        public WorkerContainerType GetContainerType() { return WorkerContainerType.Relax; }
        public Transform GetTransform() { return Socket; }

        public void RemoveWorkerRelateData()
        {
            if (IsEat)
            {
                Timer?.End();
                ML.Engine.InventorySystem.Item item = ML.Engine.InventorySystem.ItemManager.Instance.SpawnItem(ManagerNS.LocalGameManager.Instance.RestaurantManager.WorkerFood_ItemID(FoodID));
                (ML.Engine.Manager.GameManager.Instance.CharacterManager.GetLocalController() as Player.OCPlayerController).OCState.Inventory.AddItem(item);
            }
            FoodID = "";
        }

        public void OnArriveEvent(Worker worker)
        {
            if (worker != null && Restaurant.HaveFood)
            {
                string foodID = Restaurant.EatFood(Worker);
                if (ManagerNS.LocalGameManager.Instance.RestaurantManager.WorkerFood_IsValidID(foodID))
                {
                    (this as IWorkerContainer).OnArriveSetPosition(worker);
                    SetFood(foodID);
                    return;
                }
            }
            // 没食物就移除刁民
            (this as IWorkerContainer).RemoveWorker();
            ManagerNS.LocalGameManager.Instance.RestaurantManager.AddWorker(worker);
        }
        #endregion
    }
}
