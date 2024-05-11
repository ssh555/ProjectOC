using System;
using UnityEngine;


namespace ProjectOC.WorkerNS
{
    public interface IWorkerContainer
    {
        #region Data
        public Action<Worker> OnSetWorkerEvent { get; set; }
        public Action<bool, Worker> OnRemoveWorkerEvent { get; set; }
        public Worker Worker { get; set; }
        public bool IsArrive { get; set; }
        public bool HaveWorker => Worker != null && !string.IsNullOrEmpty(Worker.ID);
        #endregion

        public string GetUID();
        public WorkerContainerType GetContainerType();
        public Transform GetTransform();
        public void OnArriveEvent(Worker worker);

        public virtual void SetWorkerRelateData() { }
        public virtual void RemoveWorkerRelateData() { }
        public virtual bool TempRemoveWorker()
        {
            RemoveWorker();
            return true;
        }

        public void SetWorker(Worker worker)
        {
            IWorkerContainer container = worker?.GetContainer(GetContainerType());
            container?.RemoveWorker(true);
            RemoveWorker(true);
            Worker = worker;
            Worker?.SetContainer(this);
            SetWorkerRelateData();
            OnSetWorkerEvent?.Invoke(worker);
        }

        public void RemoveWorker(bool isReset=false)
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
                Worker oldWorker = Worker;
                Worker = null;
                OnRemoveWorkerEvent?.Invoke(isReset, oldWorker);
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
                    Worker.SetDestination(GetTransform().position, OnArriveEvent, GetContainerType());
                }
            }
        }
    }

    public enum WorkerContainerType
    {
        None,
        Work,
        Relax,
        Home
    }
}
