using JetBrains.Annotations;
using ML.Engine.ABResources;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.Manager;
using ML.Engine.Timer;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using ProjectOC.ResonanceWheelSystem.UI;
using ProjectOC.WorkerNS;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectOC.WorkerEchoNS
{
    public class ExternWorker
    {
        public string WorkerID;
        public CounterDownTimer timer;
        public Worker worker;
        public ExternWorker(string WorkerID, float time,BuildingPart buildingPart,int index)
        {
            this.WorkerID = WorkerID;
            timer = new CounterDownTimer(time);

            timer.OnEndEvent += () =>
            {
                GameManager.Instance.GetLocalManager<WorkerManager>().SpawnWorker(buildingPart.transform.position, Quaternion.identity, WorkerID, isAdd: false).Completed += (handle) =>
                  {
                      this.worker = handle.Result.GetComponent<Worker>();
                      this.worker.gameObject.transform.position = this.worker.gameObject.transform.position += new Vector3((float)(3 * Math.Cos(2 * 3.1415926 * index / 5)), 0, (float)(3 * Math.Sin(2 * 3.1415926 * index / 5)));
                      //(buildingPart as WorkerEchoBuilding).workerEcho.AddWorker(worker, index);
                  };
        
            };
        }
    }
    [System.Serializable]
    public sealed class WorkerEcho : ML.Engine.Manager.LocalManager.ILocalManager
    {
        public int Level = 2;
        ExternWorker[] Workers = new ExternWorker[5];
        BuildingPart BuildingPart = null;

        public WorkerEcho(BuildingPart buildingPart)
        {
            this.BuildingPart = buildingPart;
        }

        public ExternWorker SummonWorker(string id,int index,IInventory inventory)
        {
            if (!GameManager.Instance.GetLocalManager<WorkerManager>().OnlyCostResource(inventory, id)) return null;

            if (this.Level == 1)
            {
                id = GameManager.Instance.GetLocalManager<WorkerEchoManager>().GetRandomID();
            }
            
            ExternWorker externWorker = new ExternWorker(id, GameManager.Instance.GetLocalManager<WorkerEchoManager>().GetTimeCost(id), BuildingPart,index);
            Workers[index] = externWorker;
           
            return externWorker;
        }

        public void SpawnWorker(int index,Vector3 pos)
        {
            GameManager.Instance.GetLocalManager<WorkerManager>().AddToWorkers(Workers[index].worker);

            Workers[index].worker.transform.position = pos;
            (BuildingPart as WorkerEchoBuilding).workerEcho.AddWorker(Workers[index].worker, index);

            ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(Workers[index].timer);
            Workers[index] = null;
        }

        public void ExpelWorker(int index)
        {
            LocalGameManager.Instance.WorkerManager.DeleteWorker(Workers[index].worker);
            ML.Engine.Manager.GameManager.Instance.CounterDownTimerManager.RemoveTimer(Workers[index].timer);
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

                if (worker!=null && !worker.timer.IsTimeUp)
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

        public void StopEcho(string id,int index,IInventory inventory)
        {
            List<ML.Engine.InventorySystem.CompositeSystem.Formula> dict = GameManager.Instance.GetLocalManager<WorkerEchoManager>().GetRaw(id);
            foreach(var pair in dict)
            {
                foreach (Item item in ItemManager.Instance.SpawnItems(pair.id, pair.num))
                {
                    inventory.AddItem(item);
                }
            }
            Workers[index] = null;
        }

        public void AddWorker(Worker worker,int index)
        {
            this.Workers[index].worker = worker;
        }
    }
}