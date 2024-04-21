using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;


namespace ProjectOC.RestaurantNS
{
    [LabelText("�����洢����"), Serializable]
    public struct RestaurantData
    {
        #region ��ǰ����
        [LabelText("ʳ��ID"), ReadOnly]
        public string ID;
        [LabelText("ʳ��ʳ�����ȼ�"), ReadOnly]
        public FoodPriority Priority;
        [LabelText("��ǰʳ������"), ReadOnly]
        public int Amount;
        #endregion

        #region ���û��������
        [LabelText("���ʳ������"), ShowInInspector, ReadOnly]
        public int MaxCapacity => LocalGameManager.Instance != null ? LocalGameManager.Instance.RestaurantManager.MaxCapacity : 0;
        [LabelText("��Ӧ��ƷID"), ShowInInspector, ReadOnly]
        public string ItemID { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.RestaurantManager.WorkerFood_ItemID(ID) : ""; }
        [LabelText("ʳ��ʱ��"), ShowInInspector, ReadOnly]
        public int EatTime { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.RestaurantManager.WorkerFood_EatTime(ID) : 0; }
        [LabelText("�������ֵ"), ShowInInspector, ReadOnly]
        public int AlterAP { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.RestaurantManager.WorkerFood_AlterAP(ID) : 0; }
        [LabelText("���������ʺ�ֵ"), ShowInInspector, ReadOnly]
        public Tuple<float, int> AlterMoodOdds { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.RestaurantManager.WorkerFood_AlterMoodOdds(ID) : new Tuple<float, int>(0, 0); }
        #endregion

        #region ����
        [LabelText("�Ƿ�������ʳ��"), ShowInInspector, ReadOnly]
        public bool HasSetFood => !string.IsNullOrEmpty(ID) && LocalGameManager.Instance != null && LocalGameManager.Instance.RestaurantManager.WorkerFood_IsValidID(ID);
        [LabelText("�Ƿ���ʳ��"), ShowInInspector, ReadOnly]
        public bool HasFood => HasSetFood && Amount > 0;
        #endregion

        public class Sort : IComparer<RestaurantData>
        {
            public int Compare(RestaurantData x, RestaurantData y)
            {
                //1.����ʳ�������ǰ��
                if (x.HasSetFood != y.HasSetFood)
                {
                    return y.HasSetFood.CompareTo(x.HasSetFood);
                }
                //2.���ȼ��������ǰ��
                if (x.Priority != y.Priority)
                {
                    return y.Priority.CompareTo(x.Priority);
                }
                //3.��ʳ�������ǰ��
                if (x.HasFood != y.HasFood)
                {
                    return y.HasFood.CompareTo(x.HasFood);
                }
                //4.�������ֵС������ǰ��
                if (x.AlterAP != y.AlterAP)
                {
                    return x.AlterAP.CompareTo(y.AlterAP);
                }
                //5.IDС������ǰ��
                return x.ID.CompareTo(y.ID);
            }
        }
    }
}
