using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace ProjectOC.RestaurantNS
{
    [LabelText("餐厅座位"), Serializable]
    public class RestaurantSeat : WorkerNS.IWorkerContainer
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
                    timer = new ML.Engine.Timer.CounterDownTimer(Worker.RealEatTime, false, false);
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
            Timer?.Reset(Worker.RealEatTime);
        }

        private void EndActionForTimer()
        {
            RestaurantManager manager = ManagerNS.LocalGameManager.Instance.RestaurantManager;
            Worker.AlterAP(manager.Food_AlterAP(FoodID));
            var mood = manager.Food_AlterMoodOdds(FoodID);

            if (UnityEngine.Random.Range(0f, 1f) <= mood.Item1)
            {
                Worker.AlterMood(mood.Item2);
            }
            if (Worker.APCurrent >= Worker.APRelaxThreshold || !Restaurant.HaveFood)
            {
                WorkerNS.Worker worker = Worker;
                (this as WorkerNS.IWorkerContainer).RemoveWorker();
                if (!Restaurant.HaveFood && Worker.APCurrent < Worker.APRelaxThreshold)
                {
                    manager.AddWorker(worker);
                }
            }
            else
            {
                string foodID = Restaurant.EatFood(Worker);
                if (manager.Food_IsValidID(foodID))
                {
                    SetFood(foodID);
                }
            }
        }

        #region IWorkerContainer
        public WorkerNS.Worker Worker { get; set; }
        public bool IsArrive { get; set; }
        public bool HaveWorker => Worker != null && !string.IsNullOrEmpty(Worker.ID);
        public Action<WorkerNS.Worker> OnSetWorkerEvent { get; set; }
        public Action<bool, WorkerNS.Worker> OnRemoveWorkerEvent { get; set; }

        public string GetUID() { return Restaurant.UID; }
        public WorkerNS.WorkerContainerType GetContainerType() { return WorkerNS.WorkerContainerType.Relax; }
        public Transform GetTransform() { return Socket; }

        public void RemoveWorkerRelateData()
        {
            if (IsEat)
            {
                Timer?.End();
                ML.Engine.InventorySystem.Item item = ML.Engine.InventorySystem.ItemManager.Instance.SpawnItem
                    (ManagerNS.LocalGameManager.Instance.RestaurantManager.Food_ItemID(FoodID));
                ManagerNS.LocalGameManager.Instance.Player.GetInventory().AddItem(item);
            }
            FoodID = "";
        }

        public void OnArriveEvent(WorkerNS.Worker worker)
        {
            if (worker != null && Restaurant.HaveFood)
            {
                string foodID = Restaurant.EatFood(Worker);
                if (ManagerNS.LocalGameManager.Instance.RestaurantManager.Food_IsValidID(foodID))
                {
                    (this as WorkerNS.IWorkerContainer).OnArriveSetPosition(worker);
                    SetFood(foodID);
                    return;
                }
            }
            // 没食物就移除刁民
            (this as WorkerNS.IWorkerContainer).RemoveWorker();
            ManagerNS.LocalGameManager.Instance.RestaurantManager.AddWorker(worker);
        }
        #endregion
    }
}
