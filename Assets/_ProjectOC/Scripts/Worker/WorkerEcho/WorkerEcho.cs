using ML.Engine.Timer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    public class ExternWorker
    {
        public string WorkerID;
        public CounterDownTimer Timer;
        public Worker Worker;
        public ExternWorker(string workerID, float time, WorkerEchoBuilding workerEchoBuilding, int index)
        {
            WorkerID = workerID;
            Timer = new CounterDownTimer(time);
            Timer.OnEndEvent += () =>
            {
                ManagerNS.LocalGameManager.Instance.WorkerManager.SpawnWorker(workerEchoBuilding.transform.position, Quaternion.identity, false, workerEchoBuilding.WorkerEcho).Completed += (handle) =>
                {
                    Worker = handle.Result.GetComponent<Worker>();
                    Worker.gameObject.transform.position += new Vector3((float)(3 * Math.Cos(2 * 3.1415926 * index / 5)), 0, (float)(3 * Math.Sin(2 * 3.1415926 * index / 5)));
                };
            };
        }
    }
    [System.Serializable]
    public class WorkerEcho
    {
        public int Level = 2;
        public ExternWorker[] Workers = new ExternWorker[5];
        public WorkerEchoBuilding WorkerEchoBuilding = null;
        public List<int> FeatureMax = new List<int>() { 100, 100, 100, 100};
        public List<int> FeatureOdds = new List<int>() { 100, 100 };

        public WorkerEcho(WorkerEchoBuilding workerEchoBuilding)
        {
            WorkerEchoBuilding = workerEchoBuilding;
        }

        public ExternWorker SummonWorker(string id,int index, ML.Engine.InventorySystem.IInventory inventory)
        {
            if (!ManagerNS.LocalGameManager.Instance.WorkerManager.OnlyCostResource(inventory, id)) return null;

            if (Level == 1)
            {
                id = ManagerNS.LocalGameManager.Instance.WorkerEchoManager.GetRandomID();
            }
            
            ExternWorker externWorker = new ExternWorker(id, ManagerNS.LocalGameManager.Instance.WorkerEchoManager.GetTimeCost(id), WorkerEchoBuilding, index);
            Workers[index] = externWorker;
           
            return externWorker;
        }

        public void SpawnWorker(int index)
        {
            ManagerNS.LocalGameManager.Instance.WorkerManager.AddToWorkers(Workers[index].Worker);
            WorkerEchoBuilding.WorkerEcho.AddWorker(Workers[index].Worker, index);

            ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(Workers[index].Timer);
            Workers[index] = null;
        }

        public void ExpelWorker(int index)
        {
            ManagerNS.LocalGameManager.Instance.WorkerManager.DeleteWorker(Workers[index].Worker);
            ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(Workers[index].Timer);
            Workers[index] = null;
        }

        public void LevelUp()
        {
            if (Level == 2) return;
            Level = 2;
        }

        public EchoStatusType GetStatus()
        {
            bool isNone = true;
            for (int i = 0; i < 5; i++)
            { 
                if (Workers[i] != null)
                {
                    isNone = false;
                    break;
                }
            }

            if (isNone) return EchoStatusType.None;
            foreach (ExternWorker worker in Workers)
            {

                if (worker!=null && !worker.Timer.IsTimeUp)
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

        public void StopEcho(string id,int index, ML.Engine.InventorySystem.IInventory inventory)
        {
            foreach(var pair in ManagerNS.LocalGameManager.Instance.WorkerEchoManager.GetRaw(id))
            {
                foreach (var item in ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(pair.id, pair.num))
                {
                    inventory.AddItem(item);
                }
            }
            Workers[index] = null;
        }

        public void AddWorker(Worker worker,int index)
        {
            this.Workers[index].Worker = worker;
        }
    }
}