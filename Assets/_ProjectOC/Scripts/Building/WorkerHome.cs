using ML.Engine.Timer;
using ProjectOC.ManagerNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ProjectOC.Building
{
    public class WorkerHome : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, ML.Engine.InteractSystem.IInteraction
    {
        #region 配置数据
        [LabelText("每Time秒回复Mood心情值"), FoldoutGroup("配置"), ReadOnly]
        public int Time = 1;
        [LabelText("回复心情值"), FoldoutGroup("配置"), ReadOnly]
        public int Mood = 5;
        #endregion

        #region 当前数据
        [LabelText("关联刁民"), ReadOnly]
        public Worker Worker;
        [LabelText("移动窝时，原来绑定的刁民"), ShowInInspector, ReadOnly]
        private Worker TempWorker;
        [LabelText("移动窝时，原来的刁民是否在窝里"), ShowInInspector, ReadOnly]
        private bool TempHasArrive;

        /// <summary>
        /// 有绑定刁民且刁民在窝时循环执行，时间为Time秒，执行一次结束后对该刁民增加Mood点心情值
        /// </summary>
        private CounterDownTimer Timer;

        public string InteractType { get; set; } = "WorkerHome";
        public Vector3 PosOffset { get; set; } = Vector3.zero;
        #endregion

        #region 属性
        public string UID => InstanceID;
        [LabelText("是否有关联刁民"), ShowInInspector, ReadOnly]
        public bool HasWorker => Worker != null && !string.IsNullOrEmpty(Worker.InstanceID);

        private bool hasArrive;
        [LabelText("刁民是否在窝里"), ShowInInspector, ReadOnly]
        public bool HasArrive
        {
            get { return hasArrive; }
            set
            {
                hasArrive = value;
                if (hasArrive) { Timer?.Start(); }
                else { Timer?.End(); }
            }
        }
        #endregion

        #region 接口方法
        /// <summary>
        /// 停止Timer，调用UnBindWorker()，解绑隐兽。
        /// </summary>
        public void OnDestroy()
        {
            if (LocalGameManager.Instance != null && !HasWorker)
            {
                LocalGameManager.Instance.WorkerManager.OnAddWokerEvent -= OnAddWorkerEvent;
            }
            UnBindWorker();
            Timer?.End();
        }
        
        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                BindWorkerDefault();
                Timer = new CounterDownTimer(Time, true, false);
                Timer.OnEndEvent += EndActionForTimer;
            }
            if (!isFirstBuild && oldPos != newPos)
            {
                //移动结束后，判断TempWorker是否为空，不为空调用BindWorker(TempWorker)，
                if (TempWorker != null)
                {
                    BindWorker(TempWorker);
                    HasArrive = TempHasArrive;
                    if (HasArrive)
                    {
                        Worker.transform.position = transform.position + new Vector3(0, 2f, 0);
                    }
                }
                //设置HasArrive为TempHasArrive；为空则调用BindWorkerDefalt()。
                else
                {
                    BindWorkerDefault();
                }
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }

        public override void OnEnterEdit()
        {
            //开始移动时，将TempWorker设置为Worker，将TempHasArrive设置为HasArrive，调用UnBindWorker()，解绑Worker。
            this.TempWorker = Worker;
            this.TempHasArrive = HasArrive;
            UnBindWorker();
        }

        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIWorkerHomePanel.prefab", ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                ProjectOC.Building.UI.UIWorkerHome uiPanel = (handle.Result).GetComponent<ProjectOC.Building.UI.UIWorkerHome>();
                uiPanel.Home = this;
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }
        #endregion

        public void OnAddWorkerEvent(Worker worker)
        {
            if (!worker.HasHome && !HasWorker)
            {
                BindWorker(worker);
            }
        }

        public void ManageAddWorkerEvent()
        {
            if (HasWorker)
            {
                LocalGameManager.Instance.WorkerManager.OnAddWokerEvent -= OnAddWorkerEvent;
            }
            else
            {
                LocalGameManager.Instance.WorkerManager.OnAddWokerEvent += OnAddWorkerEvent;
            }
        }

        private void EndActionForTimer()
        {
            if (Worker != null && Worker.Mood < Worker.MoodMax)
            {
                Worker.AlterMood(Mood);
            }
        }

        public void BindWorker(Worker worker)
        {
            UnBindWorker();
            if (worker != null)
            {
                worker.Home?.UnBindWorker(true);
                Worker = worker;
                worker.Home = this;
            }
            ManageAddWorkerEvent();
        }

        public void BindWorkerDefault()
        {
            List<Worker> workers = ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorkers();
            foreach (Worker worker in workers.OrderBy(worker => worker.InstanceID).ToList())
            {
                if (!worker.HasHome)
                {
                    BindWorker(worker);
                }
            }
            ManageAddWorkerEvent();
        }

        public void UnBindWorker(bool addListener = false)
        {
            if (HasWorker)
            {
                if (HasArrive)
                {
                    Worker.RecoverLastPosition();
                }
                Worker.Home = null;
            }
            HasArrive = false;
            Worker = null;
            if (addListener)
            {
                ManageAddWorkerEvent();
            }
        }
    }
}