using ML.Engine.Manager;
using ML.Engine.Timer;
using ProjectOC.WorkerNS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerEchoNS
{
    public struct ExternWorker
    {
        public Worker worker;
        public CounterDownTimer timer;
        public ExternWorker(Worker worker, float time)
        {
            this.worker = worker;
            timer = new CounterDownTimer(time);
        }
    }
    public sealed class WorkerEcho : ML.Engine.Manager.LocalManager.ILocalManager
    {
        private int Level = 1;
        private List<ExternWorker> Workers = new List<ExternWorker>(5);
        public void SummonWorker()
        {
            if(this.Level==1)
            {

            }

        }
        public void SpawnWorker(Worker worker)
        {
            foreach (ExternWorker externWorker in Workers)
            {
                if (externWorker.worker == worker)
                {
                    Workers.Remove(externWorker);
                    GameManager.Instance.GetLocalManager<WorkerManager>().SpawnWorker(Vector3.zero, Quaternion.identity);
                }
            }
        }
        public void ExpelWorker(Worker worker)
        {
            foreach (ExternWorker externWorker in Workers)
            {
                if (externWorker.worker == worker)
                {
                    Workers.Remove(externWorker);
                }
            }
        }
        public void LevelUp()
        {
            if (Level == 2) return;
            Level = 2;
        }
        public EchoStatusType GetStatus()
        {
            if (Workers.Count == 0)
                return EchoStatusType.None;
            foreach (ExternWorker worker in Workers)
            {
                if (!worker.timer.IsTimeUp)
                {
                    return EchoStatusType.Echoing;
                }
            }
            return EchoStatusType.Waiting;
        }
    }
}