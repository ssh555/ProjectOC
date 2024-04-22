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
    [LabelText("餐厅座位"), Serializable]
    public struct RestaurantSeat
    {
        [LabelText("对应的餐厅"), ReadOnly]
        public Restaurant Restaurant;
        [LabelText("对应的Socket"), ReadOnly]
        public Transform Socket;

        [LabelText("绑定的刁民"), ShowInInspector, ReadOnly]
        public Worker Worker { get; private set; }
        [LabelText("刁民正在吃的食物"), ShowInInspector, ReadOnly]
        public string EatFoodID { get; private set; }

        [LabelText("刁民是否到达"), ReadOnly]
        public bool HasArrive;
        [LabelText("是否有绑定刁民"), ShowInInspector, ReadOnly]
        public bool HasWorker => !string.IsNullOrEmpty(Worker?.InstanceID);

        public bool IsEat => !string.IsNullOrEmpty(EatFoodID) && timer != null && !timer.IsStoped;

        private CounterDownTimer timer;
        [LabelText("进食计时器"), ReadOnly]
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
            //  刁民座位计时器结束后执行，增加刁民的体力值和心情值。
            //  如果刁民的体力大等于休息阈值，则移除该刁民，清空刁民的座位数据。
            //  如果餐厅的HasFood为false，则将刁民加入Manager队列中，清空刁民的座位数据。
            //  否则继续设置刁民的进食数据为FindFood()，并调用EatFood()。
        }
    }
}
