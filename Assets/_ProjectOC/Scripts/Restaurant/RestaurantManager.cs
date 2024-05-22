using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Linq;

namespace ProjectOC.RestaurantNS
{
    [Serializable]
    public struct WorkerFoodTableData
    {
        public string ID;
        public string ItemID;
        public int AlterAP;
        public float AlterMoodOddsProb;
        public int AlterMoodOddsValue;
    }

    [LabelText("餐厅管理器"), Serializable]
    public class RestaurantManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region ILocalManager
        private Dictionary<string, WorkerFoodTableData> WorkerFoodTableDict = new Dictionary<string, WorkerFoodTableData>();
        private Dictionary<string, string> ItemToFoodDict = new Dictionary<string, string>();
        public ML.Engine.ABResources.ABJsonAssetProcessor<WorkerFoodTableData[]> ABJAProcessor;
        public RestaurantConfig Config;
        public void OnRegister()
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
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<RestaurantConfigAsset>("Config_Restaurant").Completed += (handle) =>
            {
                RestaurantConfigAsset data = handle.Result;
                Config = data.Config;
            };
            Timer = new ML.Engine.Timer.CounterDownTimer(Config.BroadcastTime, true, true);
            Timer.OnEndEvent += EndActionForTimer;
        }

        public void OnUnRegister()
        {
            Timer?.End();
        }
        #endregion

        #region 当前数据
        private HashSet<WorkerNS.Worker> WorkerSets = new HashSet<WorkerNS.Worker>();
        [LabelText("等待去餐厅的刁民队列"), ReadOnly]
        public List<WorkerNS.Worker> Workers = new List<WorkerNS.Worker>();
        [LabelText("实例化的餐厅"), ReadOnly, ShowInInspector]
        public Dictionary<string, WorldRestaurant> WorldRestaurants = new Dictionary<string, WorldRestaurant>();
        [LabelText("分配计时器"), ReadOnly]
        public ML.Engine.Timer.CounterDownTimer Timer;
        #endregion

        #region 方法
        public void AddWorker(WorkerNS.Worker worker)
        {
            if (worker != null && !ContainWorker(worker))
            {
                Workers.RemoveAll(worker => worker == null);
                WorkerSets.RemoveWhere(worker => worker == null);
                Workers.Add(worker);
                WorkerSets.Add(worker);
            }
        }

        public void RemoveWorker(WorkerNS.Worker worker)
        {
            if (ContainWorker(worker))
            {
                Workers.RemoveAll(worker => worker == null);
                WorkerSets.RemoveWhere(worker => worker == null);
                Workers.Remove(worker);
                WorkerSets.Remove(worker);
            }
        }

        public bool ContainWorker(WorkerNS.Worker worker)
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

        public Restaurant GetPutInRestaurant(DataNS.IDataObj data, int amount)
        {
            Restaurant result = null;
            if (data != null && amount > 0)
            {
                List<Restaurant> restaurants = GetRestaurants();
                foreach (var restaurant in restaurants)
                {
                    if (restaurant.DataContainer.HaveSetData(data))
                    {
                        int empty = restaurant.DataContainer.GetAmount(data, DataNS.DataOpType.Empty);
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
                List<WorkerNS.Worker> workers = new List<WorkerNS.Worker>();
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
                
                foreach (WorkerNS.Worker worker in workers)
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

        #region Food Getter
        public bool Food_IsValidID(string id)
        {
            return !string.IsNullOrEmpty(id) ? WorkerFoodTableDict.ContainsKey(id) : false;
        }
        public string Food_ItemID(string id)
        {
            return Food_IsValidID(id) ? WorkerFoodTableDict[id].ItemID : "";
        }
        public int Food_AlterAP(string id)
        {
            return Food_IsValidID(id) ? WorkerFoodTableDict[id].AlterAP : 0;
        }
        public Tuple<float, int> Food_AlterMoodOdds(string id)
        {
            return Food_IsValidID(id) ? 
                new Tuple<float, int>(WorkerFoodTableDict[id].AlterMoodOddsProb, WorkerFoodTableDict[id].AlterMoodOddsValue) : 
                new Tuple<float, int>(0, 0);
        }
        public string ItemIDToFoodID(string itemID)
        {
            return !string.IsNullOrEmpty(itemID) && ItemToFoodDict.ContainsKey(itemID) ? ItemToFoodDict[itemID] : "";
        }
        #endregion
    }
}
