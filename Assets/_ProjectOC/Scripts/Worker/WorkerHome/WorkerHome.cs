using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ProjectOC.WorkerNS
{
    public class WorkerHome : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, ML.Engine.InteractSystem.IInteraction, IWorkerContainer
    {
        #region Data
        [LabelText("�ƶ���ʱ��ԭ���󶨵ĵ���"), ShowInInspector, ReadOnly]
        private Worker TempWorker;
        [LabelText("�ƶ���ʱ��ԭ���ĵ����Ƿ�������"), ShowInInspector, ReadOnly]
        private bool TempHasArrive;
        private Vector3 TempLastPos;
        private ML.Engine.Timer.CounterDownTimer timer;
        /// <summary>
        /// �а󶨵����ҵ�������ʱѭ��ִ�У�ʱ��ΪTime�룬ִ��һ�ν�����Ըõ�������Mood������ֵ
        /// </summary>
        private ML.Engine.Timer.CounterDownTimer Timer
        {
            get
            {
                if (timer == null)
                {
                    timer = new ML.Engine.Timer.CounterDownTimer(1, true, false);
                    timer.OnEndEvent += EndActionForTimer;
                }
                return timer;
            }
        }
        #endregion

        #region BuildingPart IInteraction
        public string InteractType { get; set; } = "WorkerHome";
        public Vector3 PosOffset { get; set; } = Vector3.zero;
        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                Init();
            }
            if (!isFirstBuild && oldPos != newPos)
            {
                if (TempWorker != null)
                {
                    (this as IWorkerContainer).SetWorker(TempWorker);
                    if (TempHasArrive)
                    {
                        OnArriveEvent(TempWorker);
                        TempWorker.LastPosition = TempLastPos + newPos - oldPos;
                    }
                }
            }
            if (!HaveWorker)
            {
                BindWorkerDefault();
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }

        public override void OnEnterEdit()
        {
            //��ʼ�ƶ�ʱ����TempWorker����ΪWorker����TempHasArrive����ΪHasArrive������UnBindWorker()�����Worker��
            this.TempWorker = Worker;
            this.TempHasArrive = IsArrive;
            this.TempLastPos = Worker.LastPosition;
            (this as IWorkerContainer).RemoveWorker();
        }

        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Worker_UI/Prefab_Worker_UI_WorkerHomePanel.prefab", ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                UI.UIWorkerHome uiPanel = (handle.Result).GetComponent<UI.UIWorkerHome>();
                uiPanel.Home = this;
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }
        #endregion

        #region Mono
        public void Init()
        {
            if (!HaveWorker && ManagerNS.LocalGameManager.Instance != null)
            {
                ManagerNS.LocalGameManager.Instance.WorkerManager.OnAddWokerEvent += OnManagerAddWorkerEvent;
            }
            OnSetWorkerEvent += (worker) =>
            {
                if (worker != null)
                {
                    ManagerNS.LocalGameManager.Instance.WorkerManager.OnAddWokerEvent -= OnManagerAddWorkerEvent;
                }
            };
            OnRemoveWorkerEvent += (isReset, oldWorker) =>
            {
                oldWorker = isReset ? oldWorker : null;
                if (ManagerNS.LocalGameManager.Instance != null && !BindWorkerDefault(oldWorker))
                {
                    ManagerNS.LocalGameManager.Instance.WorkerManager.OnAddWokerEvent += OnManagerAddWorkerEvent;
                }
            };
        }
        public void OnDestroy()
        {
            OnSetWorkerEvent = null;
            OnRemoveWorkerEvent = null;
            (this as IWorkerContainer).RemoveWorker();
        }

        public void OnManagerAddWorkerEvent(Worker worker)
        {
            if (!worker.HaveHome && !HaveWorker)
            {
                (this as IWorkerContainer).SetWorker(worker);
            }
        }
        #endregion

        #region Method
        private void EndActionForTimer()
        {
            if (HaveWorker)
            {
                Worker.AlterMood(Worker.EMRecover);
            }
        }

        public bool BindWorkerDefault(Worker oldWorker = null)
        {
            if (!HaveWorker)
            {
                List<Worker> workers = ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorkers();
                workers.RemoveAll(x => x == null);
                foreach (Worker worker in workers.OrderBy(worker => worker.ID).ToList())
                {
                    if (!worker.HaveHome && (oldWorker== null || worker != oldWorker))
                    {
                        (this as IWorkerContainer).SetWorker(worker);
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region IWorkerContainer
        public Worker Worker { get; set; }
        [LabelText("�����Ƿ�������"), ShowInInspector, ReadOnly]
        private bool isArrive;
        public bool IsArrive
        {
            get { return isArrive; }
            set
            {
                isArrive = value;
                if (isArrive) { Timer?.Start(); }
                else 
                {
                    if (timer != null && !timer.IsStoped)
                    {
                        timer?.End();
                    }
                }
            }
        }
        public bool HaveWorker => Worker != null && !string.IsNullOrEmpty(Worker.ID);
        public Action<Worker> OnSetWorkerEvent { get; set; }
        public Action<bool, Worker> OnRemoveWorkerEvent { get; set; }

        public string GetUID() { return InstanceID; }
        public Transform GetTransform() { return transform; }
        public WorkerContainerType GetContainerType() { return WorkerContainerType.Home; }

        public void OnArriveEvent(Worker worker)
        {
            (this as IWorkerContainer).OnArriveSetPosition(worker, transform.position + new Vector3(0, 2f, 0));
        }
        public Action<int> OnWorkerMoodChangeEvent;
        public void SetWorkerRelateData()
        {
            if (HaveWorker)
            {
                Worker.OnAPChangeEvent += OnWorkerMoodChangeEvent;
            }
        }
        public void RemoveWorkerRelateData()
        {
            if (HaveWorker)
            {
                Worker.OnAPChangeEvent -= OnWorkerMoodChangeEvent;
            }
        }

        public bool TempRemoveWorker()
        {
            if (Worker != null && IsArrive)
            {
                Worker.RecoverLastPosition();
                IsArrive = false;
                return true;
            }
            return false;
        }
        #endregion
    }
}