using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;


namespace ProjectOC.RestaurantNS
{
    [LabelText("餐厅存储数据"), Serializable]
    public struct RestaurantData
    {
        #region Data
        [LabelText("食物ID"), ReadOnly]
        public string ID;
        [LabelText("食用优先级"), ReadOnly]
        public FoodPriority Priority;
        [LabelText("当前数量"), ReadOnly]
        public int Amount;
        #endregion

        #region Property
        [LabelText("是否有设置食物"), ShowInInspector, ReadOnly]
        public bool HaveSetFood => !string.IsNullOrEmpty(ID) && LocalGameManager.Instance != null && LocalGameManager.Instance.RestaurantManager.WorkerFood_IsValidID(ID);
        [LabelText("是否有食物"), ShowInInspector, ReadOnly]
        public bool HaveFood => HaveSetFood && Amount > 0;
        #endregion

        #region Table Property
        [LabelText("最大食物数量"), ShowInInspector, ReadOnly]
        public int MaxCapacity => LocalGameManager.Instance != null ? LocalGameManager.Instance.RestaurantManager.MaxCapacity : 0;
        [LabelText("对应物品ID"), ShowInInspector, ReadOnly]
        public string ItemID { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.RestaurantManager.WorkerFood_ItemID(ID) : ""; }
        [LabelText("食用时间"), ShowInInspector, ReadOnly]
        public int EatTime { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.RestaurantManager.WorkerFood_EatTime(ID) : 0; }
        [LabelText("变更体力值"), ShowInInspector, ReadOnly]
        public int AlterAP { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.RestaurantManager.WorkerFood_AlterAP(ID) : 0; }
        [LabelText("变更心情概率和值"), ShowInInspector, ReadOnly]
        public Tuple<float, int> AlterMoodOdds { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.RestaurantManager.WorkerFood_AlterMoodOdds(ID) : new Tuple<float, int>(0, 0); }
        #endregion

        public class Sort : IComparer<RestaurantData>
        {
            public int Compare(RestaurantData x, RestaurantData y)
            {
                // 1.有食物的排在前面
                if (x.HaveFood != y.HaveFood)
                {
                    return y.HaveFood.CompareTo(x.HaveFood);
                }
                // 2.优先级大的排在前面
                if (x.Priority != y.Priority)
                {
                    return y.Priority.CompareTo(x.Priority);
                }
                // 3.变更体力值小的排在前面
                if (x.AlterAP != y.AlterAP)
                {
                    return x.AlterAP.CompareTo(y.AlterAP);
                }
                // 4.ID小的排在前面
                string idx = x.ID ?? "";
                string idy = y.ID ?? "";
                return idx.CompareTo(idy);
            }
        }
    }
}
