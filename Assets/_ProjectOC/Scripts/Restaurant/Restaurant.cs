using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ProjectOC.RestaurantNS
{
    [LabelText("餐厅"), Serializable]
    public class Restaurant : DataNS.ItemContainerOwner
    {
        #region Data
        [LabelText("世界餐厅"), ReadOnly, NonSerialized]
        public WorldRestaurant WorldRestaurant;
        [ShowInInspector, ReadOnly]
        public string UID => WorldRestaurant?.InstanceID ?? "";
        [LabelText("座位"), ShowInInspector, ReadOnly]
        private RestaurantSeat[] Seats;
        #endregion

        #region Property
        [LabelText("是否有座位"), ShowInInspector, ReadOnly]
        public bool HaveSeat
        {
            get
            {
                if (Seats != null)
                {
                    foreach (RestaurantSeat seat in Seats)
                    {
                        if (!seat.HaveWorker)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        [LabelText("是否有食物"), ShowInInspector, ReadOnly]
        public bool HaveFood => DataContainer?.HaveAnyData(DataNS.DataOpType.Storage) ?? false;
        #endregion

        public void Init()
        {
            Seats = new RestaurantSeat[ManagerNS.LocalGameManager.Instance.RestaurantManager.Config.SeatNum];
            for (int i = 0; i < Seats.Length; i++)
            {
                Seats[i] = new RestaurantSeat(this, WorldRestaurant.transform.Find($"seat{i + 1}"));
            }
            (this as DataNS.IContainerOwner<string>).InitData(ManagerNS.LocalGameManager.Instance.RestaurantManager.Config.DataNum, ManagerNS.LocalGameManager.Instance.RestaurantManager.Config.MaxCapacity);
        }

        public void Destroy()
        {
            foreach (var seat in Seats)
            {
                (seat as WorkerNS.IWorkerContainer).RemoveWorker();
            }
            ClearData();
        }

        public void OnPositionChange(Vector3 differ)
        {
            (this as MissionNS.IMissionObj<string>).OnPositionChangeTransport();
            foreach (var seat in Seats)
            {
                (seat as WorkerNS.IWorkerContainer).OnPositionChange(differ);
            }
        }

        #region 方法
        /// <summary>
        /// 先查找空座位，找不到空座位返回false，更新该座位的信息，让刁民寻路到该座位。
        /// </summary>
        public bool AddWorker(WorkerNS.Worker worker)
        {
            if (worker != null)
            {
                for (int i = 0; i < Seats.Length; i++)
                {
                    if (!Seats[i].HaveWorker)
                    {
                        (Seats[i] as WorkerNS.IWorkerContainer).SetWorker(worker);
                        worker.SetDestination(Seats[i].Socket.position, Seats[i].OnArriveEvent, Seats[i].GetContainerType());
                        ManagerNS.LocalGameManager.Instance.RestaurantManager.RemoveWorker(worker);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 给玩家分配食物
        /// 将Datas转为列表并排序，遍历该列表，如果有No1和No2的食物，则返回No1或No2，
        /// 否则继续遍历该列表，返回第一个能让刁民体力值溢出的食物，找不到的话就返回体力值最大的食物。
        /// </summary>
        public int FindFood(WorkerNS.Worker worker)
        {
            int result = -1;
            if (worker != null)
            {
                List<RestaurantData> datas = new List<RestaurantData>();
                var items = DataContainer.GetDatas();
                for (int i = 0; i < items.Length; i++)
                {
                    datas.Add(new RestaurantData(items[i].ID, i, items[i].GetAmount(DataNS.DataOpType.Storage)));
                }
                datas.Sort(new RestaurantData());
                foreach (RestaurantData data in datas)
                {
                    if (data.HaveFood)
                    {
                        result = data.Index;
                        if (data.Priority != FoodPriority.None || worker.APCurrent + data.AlterAP >= worker.APMax)
                        {
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public string EatFood(WorkerNS.Worker worker)
        {
            int index = FindFood(worker);
            int flag = DataContainer.ChangeAmount(index, 1, DataNS.DataOpType.Empty, DataNS.DataOpType.Storage);
            if (0 <= index && flag == 1)
            {
                string itemID = DataContainer.GetID(index);
                return ManagerNS.LocalGameManager.Instance.RestaurantManager.ItemIDToFoodID(itemID);
            }
            return null;
        }
        #endregion

        #region IMissionObj
        public override Transform GetTransform() { return WorldRestaurant.transform; }
        public override string GetUID() { return UID; }

        public override MissionNS.MissionObjType GetMissionObjType()
        {
            return MissionNS.MissionObjType.Restaurant;
        }
        #endregion
    }
}
