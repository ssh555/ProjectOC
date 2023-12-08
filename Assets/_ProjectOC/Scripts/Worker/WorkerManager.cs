using ML.Engine.Manager.LocalManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public sealed class WorkerManager:ILocalManager
    {
        /// <summary>
        /// 刁民数量
        /// </summary>
        public int WorkerNum { get {return Workers.Count;} }
        /// <summary>
        /// 刁民列表
        /// </summary>
        private HashSet<Worker> Workers = new HashSet<Worker>();

        public Worker CreateWorker()
        {
            Worker worker = new Worker();
            this.Workers.Add(worker);
            return worker;
        }

        public List<Worker> GetWorkers()
        {
            this.Workers.RemoveWhere(item => item == null);
            return this.Workers.ToList();
        }
        /// <summary>
        /// 获取能执行帮运任务的刁民
        /// </summary>
        /// <returns></returns>
        public Worker GetCanTransportWorker()
        {
            Worker result = null;
            foreach (Worker worker in this.Workers)
            {
                if (worker != null && worker.Status == Status.Fishing && worker.IsOnDuty == false)
                {
                    result = worker;
                    break;
                }
            }
            return result;
        }

        public void RemoveWorker(Worker worker)
        {
            this.Workers.Remove(worker);
        }
    }
}

