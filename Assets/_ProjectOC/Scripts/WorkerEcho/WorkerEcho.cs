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
    [System.Serializable]
    public sealed class WorkerEcho : ML.Engine.Manager.LocalManager.ILocalManager
    {
        public int Level = 2;
        private List<ExternWorker> Workers = new List<ExternWorker>(5);
        /// <summary>
        /// 一级召唤
        /// </summary>
        public ExternWorker SummonWorker1(int index)
        {
            
            ExternWorker worker = new ExternWorker(null, 5);
            return worker;
        }
        /// <summary>
        /// 二级召唤
        /// </summary>
        public ExternWorker SummonWorker2(int index,string id)
        {
            ExternWorker worker = new ExternWorker(null, 5);
            Debug.Log("SummonWorker2:  " + id);
            return worker;
        }

        public void SpawnWorker(int index)
        {
            //收留
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
            //to-do 返还材料 
        }
    }
}