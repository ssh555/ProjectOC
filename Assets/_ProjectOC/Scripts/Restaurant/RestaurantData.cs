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
        public bool HaveSetFood => !string.IsNullOrEmpty(ID) && LocalGameManager.Instance != null && LocalGameManager.Instance.RestaurantManager.WorkerFood_IsValidID(ID);
        [LabelText("�Ƿ���ʳ��"), ShowInInspector, ReadOnly]
        public bool HaveFood => HaveSetFood && Amount > 0;
        #endregion

        public class Sort : IComparer<RestaurantData>
        {
            public int Compare(RestaurantData x, RestaurantData y)
            {
                //1.����ʳ�������ǰ��
                if (x.HaveSetFood != y.HaveSetFood)
                {
                    return y.HaveSetFood.CompareTo(x.HaveSetFood);
                }
                //2.���ȼ��������ǰ��
                if (x.Priority != y.Priority)
                {
                    return y.Priority.CompareTo(x.Priority);
                }
                //3.��ʳ�������ǰ��
                if (x.HaveFood != y.HaveFood)
                {
                    return y.HaveFood.CompareTo(x.HaveFood);
                }
                //4.�������ֵС������ǰ��
                if (x.AlterAP != y.AlterAP)
                {
                    return x.AlterAP.CompareTo(y.AlterAP);
                }
                string idx = x.ID ?? "";
                string idy = y.ID ?? "";
                //5.IDС������ǰ��
                return idx.CompareTo(idy);
            }
        }
    }
}
