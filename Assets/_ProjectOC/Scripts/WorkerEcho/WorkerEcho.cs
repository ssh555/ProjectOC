using JetBrains.Annotations;
using ML.Engine.Manager;
using ML.Engine.Timer;
using ProjectOC.WorkerNS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerEchoNS
{
    public class ExternWorker
    {
        public Worker worker { get; private set; }
        public CounterDownTimer timer { get;private set; }
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
        public void SummonWorker(int index)
        {
            if(this.Level==1)
            {

            }
            else
            {
                
            }
        }
        public void SpawnWorker(int index)
        {
            // ’¡Ù
            GameManager.Instance.GetLocalManager<WorkerManager>().SpawnWorker(Vector3.zero, Quaternion.identity);
        }
        public void ExpelWorker(int index)
        {
            Workers[index] = null; 
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
        public List<ExternWorker> GetExternWorkers()
        {
            return Workers;
        }
        public void StopEcho(int index)
        {
            Workers[index].timer.End();
            Workers[index] = null;
            //to-do ∑µªπ≤ƒ¡œ 
        }
    }
}