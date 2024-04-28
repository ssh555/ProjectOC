using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using ProjectOC.WorkerNS;
using System.Linq;


namespace ProjectOC.RestaurantNS
{
    [Serializable]
    public struct WorkerFoodTableData
    {
        public string ID;
        public string ItemID;
        public int EatTime;
        public int AlterAP;
        public float AlterMoodOddsProb;
        public int AlterMoodOddsValue;
    }

    [LabelText("餐厅管理器"), Serializable]
    public class RestaurantManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        public void OnRegister()
        {
            LoadTableData();
            Timer = new ML.Engine.Timer.CounterDownTimer(BroadcastTime, true, true);
            Timer.OnEndEvent += EndActionForTimer;
        }

        public void OnUnRegister()
        {
            Timer?.End();
        }

        #region 配置数据
        [LabelText("分配一次的时间"), FoldoutGroup("配置")]
        public int BroadcastTime { get; private set; } = 5;
        [LabelText("位置数量"), FoldoutGroup("配置")]
        public int SeatNum { get; private set; } = 4;
        [LabelText("数据数量"), FoldoutGroup("配置")]
        public int DataNum { get; private set; } = 5;
        [LabelText("存储上限"), FoldoutGroup("配置")]
        public int MaxCapacity { get; private set; } = 100;
        #endregion

        #region 当前数据
        private HashSet<Worker> WorkerSets = new HashSet<Worker>();
        [LabelText("等待去餐厅的刁民队列"), ReadOnly]
        public List<Worker> Workers = new List<Worker>();
        [LabelText("实例化的餐厅"), ReadOnly, ShowInInspector]
        public Dictionary<string, WorldRestaurant> WorldRestaurants = new Dictionary<string, WorldRestaurant>();
        [LabelText("分配计时器"), ReadOnly]
        public ML.Engine.Timer.CounterDownTimer Timer;
        #endregion

        #region 方法
        public void AddWorker(Worker worker)
        {
            if (worker != null && !ContainWorker(worker))
            {
                Workers.RemoveAll(worker => worker == null);
                WorkerSets.RemoveWhere(worker => worker == null);
                Workers.Add(worker);
                WorkerSets.Add(worker);
            }
        }

        public void RemoveWorker(Worker worker)
        {
            if (ContainWorker(worker))
            {
                Workers.RemoveAll(worker => worker == null);
                WorkerSets.RemoveWhere(worker => worker == null);
                Workers.Remove(worker);
                WorkerSets.Remove(worker);
            }
        }

        public bool ContainWorker(Worker worker)
        {
            return worker != null && WorkerSets.Contains(worker);
        }

        public List<Restaurant> GetRestaurants()
        {
            List<Restaurant> restaurants = new List<Restaurant>();
            foreach (WorldRestaurant world in WorldRestaurants.Values)
            {
                if (world != null)
                {
                    restaurants.Add(world.Restaurant);
                }
            }
            return restaurants;
        }

        public Restaurant GetPutInRestaurant(string itemID, int amount)
        {
            Restaurant result = null;
            if (!string.IsNullOrEmpty(foodID) && amount > 0)
            {
                List<Restaurant> restaurants = GetRestaurants();
                foreach (var restaurant in restaurants)
                {
                    if (restaurant.DataContainer.HaveSetData(itemID))
                    {
                        int empty = restaurant.DataContainer.GetAmount(itemID, DataNS.DataOpType.Empty);
                        if (result == null && empty > 0)
                        {
                            result = restaurant;
                        }
                        if (empty >= amount)
                        {
                            result = restaurant;
                            break;
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region Spawn
        public void WorldRestaurantSetData(WorldRestaurant worldRestaurant)
        {
            if (worldRestaurant != null)
            {
                if (!WorldRestaurants.ContainsKey(worldRestaurant.InstanceID))
                {
                    WorldRestaurants.Add(worldRestaurant.InstanceID, worldRestaurant);
                }
                else
                {
                    WorldRestaurants[worldRestaurant.InstanceID] = worldRestaurant;
                }

                Restaurant restaurant = new Restaurant();
                if (restaurant != null)
                {
                    if (worldRestaurant.Restaurant != null)
                    {
                        worldRestaurant.Restaurant.WorldRestaurant = null;
                    }
                    worldRestaurant.Restaurant = restaurant;
                    restaurant.WorldRestaurant = worldRestaurant;
                }
                restaurant.Init();
            }
        }
        #endregion

        #region 分配
        private void EndActionForTimer()
        {
            Workers.RemoveAll(x => x == null);
            WorkerSets.RemoveWhere(x => x == null);
            List<WorldRestaurant> worldRestaurants = WorldRestaurants.Values.Where(worldRestaurant => worldRestaurant != null && worldRestaurant.Restaurant.HaveFood && worldRestaurant.Restaurant.HaveSeat).ToList();
            if (Workers.Count > 0 && worldRestaurants.Count > 0)
            {
                List<Worker> workers = new List<Worker>();
                workers.AddRange(Workers);

                List<Vector3> positions = new List<Vector3>();
                foreach (var core in ManagerNS.LocalGameManager.Instance.BuildPowerIslandManager.powerCores)
                {
                    if (core.GetType() == typeof(LandMassExpand.BuildPowerCore))
                    {
                        positions.Add(core.transform.position);
                    }
                }

                Dictionary<int, HashSet<Restaurant>> dict = new Dictionary<int, HashSet<Restaurant>>();

                foreach (WorldRestaurant world in worldRestaurants)
                {
                    if (world != null)
                    {
                        float minDist = float.MaxValue;
                        int index = 0;
                        for (int i = 0; i < positions.Count; i++)
                        {
                            float dist = Vector3.Distance(world.transform.position, positions[i]);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                index = i;
                            }
                        }
                        if (!dict.ContainsKey(index))
                        {
                            dict[index] = new HashSet<Restaurant>();
                        }
                        dict[index].Add(world.Restaurant);
                    }
                }
                
                foreach (Worker worker in workers)
                {
                    if (worker != null)
                    {
                        List<Tuple<float, int>> dists = positions.Select((position, index) => Tuple.Create(Vector3.Distance(worker.transform.position, position), index)).ToList();
                        List<int> indexs = dists.OrderBy(tuple => tuple.Item1).Select(tuple => tuple.Item2).ToList();
                        bool flag = false;
                        foreach (int index in indexs)
                        {
                            if (flag)
                            {
                                break;
                            }
                            if (dict.ContainsKey(index))
                            {
                                List<Restaurant> removes = new List<Restaurant>();
                                foreach (var restaurant in dict[index])
                                {
                                    if (restaurant != null && restaurant.HaveFood && restaurant.HaveSeat && restaurant.AddWorker(worker))
                                    {
                                        flag = true;
                                        break;
                                    }
                                    else
                                    {
                                        removes.Add(restaurant);
                                    }
                                }
                                foreach (var remove in removes)
                                {
                                    dict[index].Remove(remove);
                                }
                            }
                        }

                        if (!flag)
                        {
                            break;
                        }
                    }
                }
            }
        }
        #endregion

        #region Load And Data
        private Dictionary<string, WorkerFoodTableData> WorkerFoodTableDict = new Dictionary<string, WorkerFoodTableData>();
        private Dictionary<string, string> ItemToFoodDict = new Dictionary<string, string>();

        public ML.Engine.ABResources.ABJsonAssetProcessor<WorkerFoodTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<WorkerFoodTableData[]>("OCTableData", "WorkerFood", (datas) =>
            {
                foreach (var data in datas)
                {
                    WorkerFoodTableDict.Add(data.ID, data);
                    ItemToFoodDict.Add(data.ItemID, data.ID);
                }
            }, "隐兽食物表数据");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion

        #region Worker Food Getter
        public bool WorkerFood_IsValidID(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return WorkerFoodTableDict.ContainsKey(id);
            }
            return false;
        }

        public string WorkerFood_ItemID(string id)
        {
            if (WorkerFood_IsValidID(id))
            {
                return WorkerFoodTableDict[id].ItemID;
            }
            return "";
        }

        public int WorkerFood_EatTime(string id)
        {
            if (WorkerFood_IsValidID(id))
            {
                return WorkerFoodTableDict[id].EatTime;
            }
            return 0;
        }

        public int WorkerFood_AlterAP(string id)
        {
            if (WorkerFood_IsValidID(id))
            {
                return WorkerFoodTableDict[id].AlterAP;
            }
            return 0;
        }

        public Tuple<float, int> WorkerFood_AlterMoodOdds(string id)
        {
            if (WorkerFood_IsValidID(id))
            {
                return new Tuple<float, int>(WorkerFoodTableDict[id].AlterMoodOddsProb, WorkerFoodTableDict[id].AlterMoodOddsValue);
            }
            return new Tuple<float, int>(0, 0);
        }

        public string ItemIDToFoodID(string itemID)
        {
            if (!string.IsNullOrEmpty(itemID) && ItemToFoodDict.ContainsKey(itemID))
            {
                return ItemToFoodDict[itemID];
            }
            return "";
        }
        #endregion
    }
}
