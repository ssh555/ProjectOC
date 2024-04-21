using ML.Engine.Timer;
using ProjectOC.ManagerNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public Worker Worker { get; private set; }
        [LabelText("�������ڳԵ�ʳ��"), ShowInInspector, ReadOnly]
        public string EatFoodID { get; private set; }

        [LabelText("�����Ƿ񵽴�"), ReadOnly]
        public bool HasArrive;
        [LabelText("�Ƿ��а󶨵���"), ShowInInspector, ReadOnly]
        public bool HasWorker => !string.IsNullOrEmpty(Worker?.InstanceID);

        public bool IsEat => !string.IsNullOrEmpty(EatFoodID) && timer != null && !timer.IsStoped;

        private CounterDownTimer timer;
        [LabelText("��ʳ��ʱ��"), ReadOnly]
        public CounterDownTimer Timer
        {
            get 
            {
                if (timer == null && HasWorker && HasArrive && Restaurant != null)
                {
                    if (LocalGameManager.Instance.RestaurantManager.WorkerFood_IsValidID(EatFoodID))
                    {
                        timer = new CounterDownTimer(LocalGameManager.Instance.RestaurantManager.WorkerFood_EatTime(EatFoodID), false, true);
                    }
                }
                return timer; 
            }
        }

        public void SetWorker(Worker worker)
        {
            Worker = worker;
            Worker.Restaurant = Restaurant;
        }

        public void SetFood(string foodID)
        {
            EatFoodID = foodID;
        }

        public void ClearData()
        {
            Worker.Restaurant = null;
            Worker.RecoverLastPosition();
        }

        private void EndActionForTimer()
        {
            if (LocalGameManager.Instance.RestaurantManager.WorkerFood_IsValidID(EatFoodID))
            {
                
            }
            //  ������λ��ʱ��������ִ�У����ӵ��������ֵ������ֵ��
            //  �������������������Ϣ��ֵ�����Ƴ��õ�����յ������λ���ݡ�
            //  ���������HasFoodΪfalse���򽫵������Manager�����У���յ������λ���ݡ�
            //  ����������õ���Ľ�ʳ����ΪFindFood()��������EatFood()��
        }
    }
}
