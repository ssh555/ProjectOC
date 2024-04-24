using System;
using UnityEngine;


namespace ProjectOC.WorkerNS
{
    public interface IWorkerContainer
    {
        #region Data
        // 可选初始化
        public Action<Worker> OnSetWorkerEvent { get; set; }
        public Action OnRemoveWorkerEvent { get; set; }
        // 不需要初始化
        public Worker Worker { get; set; }
        public bool IsArrive { get; set; }
        // 属性
        public bool HaveWorker => Worker != null && !string.IsNullOrEmpty(Worker.InstanceID);
        #endregion

        public string GetUID();
        public WorkerContainerType GetContainerType();
        public Transform GetTransform();
        public void OnArriveEvent(Worker worker);

        public virtual void SetWorkerRelateData() { }
        public virtual void RemoveWorkerRelateData() { }
        public virtual void TempRemoveWorker()
        {
            RemoveWorker();
        }

        public void SetWorker(Worker worker)
        {
            IWorkerContainer container = Worker?.GetContainer(GetContainerType());
            container?.RemoveWorker();
            RemoveWorker();
            Worker = worker;
            Worker?.SetContainer(this);
            SetWorkerRelateData();
            OnSetWorkerEvent?.Invoke(worker);
        }

        public void RemoveWorker()
        {
            RemoveWorkerRelateData();
            if (HaveWorker)
            {
                Worker.ClearDestination();
                Worker.RemoveContainer(GetContainerType());
                if (IsArrive)
                {
                    Worker.RecoverLastPosition();
                }
                Worker = null;
                OnRemoveWorkerEvent?.Invoke();
            }
            IsArrive = false;
        }

        public void OnArriveSetPosition(Worker worker)
        {
            OnArriveSetPosition(worker, GetTransform().position);
        }
        public void OnArriveSetPosition(Vector3 pos)
        {
            OnArriveSetPosition(Worker, pos);
        }
        public void OnArriveSetPosition(Worker worker, Vector3 pos)
        {
            if (worker != null)
            {
                IsArrive = true;
                worker.Agent.enabled = false;
                worker.LastPosition = worker.transform.position;
                worker.transform.position = pos;
            }
        }

        public void OnPositionChange(Vector3 differ)
        {
            if (HaveWorker)
            {
                if (IsArrive)
                {
                    Worker.transform.position += differ;
                }
                else
                {
                    Worker.SetDestination(GetTransform().position, OnArriveEvent);
                }
            }
        }
    }

    public enum WorkerContainerType
    {
        Work,
        Relax,
        Home
    }
}
