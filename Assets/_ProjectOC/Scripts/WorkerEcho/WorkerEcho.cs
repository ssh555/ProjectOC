using JetBrains.Annotations;
using ML.Engine.ABResources;
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
        public string WorkerID;
        public CounterDownTimer timer;
        public Worker worker;
        public ExternWorker(string WorkerID, float time)
        {

            timer = new CounterDownTimer(time);
            timer.OnEndEvent += () =>
            {
                //GameManager.Instance.GetLocalManager<WorkerManager>().SpawnWorker();
            };
        }
    }
    [System.Serializable]
    public sealed class WorkerEcho : ML.Engine.Manager.LocalManager.ILocalManager
    {
        private int Level = 1;
        ExternWorker[] Workers = new ExternWorker[5];
 
        public ExternWorker SummonWorker1(string id,int index)
        {
            if(this.Level==1)
            {
                id = GameManager.Instance.GetLocalManager<WorkerEchoManager>().GetRandomID();
            }
            //调用WorkerManager
            return null;
        }
        /// <summary>
        /// 收留隐兽
        /// </summary>
        /// <param name="index"></param>
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
            if (Workers == null)
            {
                return EchoStatusType.None;
            }
            foreach (ExternWorker worker in Workers)
            {
                if (!worker.timer.IsTimeUp)
                {
                    return EchoStatusType.Echoing;
                }
            }
            return EchoStatusType.Waiting;
        }
        public ExternWorker[] GetExternWorkers()
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