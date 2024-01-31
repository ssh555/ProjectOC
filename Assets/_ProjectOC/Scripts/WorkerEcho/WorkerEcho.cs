using JetBrains.Annotations;
using ML.Engine.ABResources;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.Manager;
using ML.Engine.Timer;
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

                this.worker = GameManager.Instance.GetLocalManager<WorkerManager>().SpawnWorker(buildingPart.transform.position,Quaternion.identity,WorkerID);
                this.worker.gameObject.transform.position = this.worker.gameObject.transform.position += new Vector3((float)(3 *Math.Cos(2 * 3.1415926 * index / 5)),0, (float)(3 * Math.Sin(2 * 3.1415926 * index / 5)));
                (buildingPart as WorkerEchoBuilding).workerEcho.AddWorker(worker, index);          
            };
        }
    }
    [System.Serializable]
    public sealed class WorkerEcho : ML.Engine.Manager.LocalManager.ILocalManager
    {
        public int Level = 1;
        ExternWorker[] Workers = new ExternWorker[5];
        BuildingPart BuildingPart = null;

        public WorkerEcho(BuildingPart buildingPart)
        {
            this.BuildingPart = buildingPart;
        }

        public ExternWorker SummonWorker(string id,int index)
        {
            IInventory inventory = GameObject.Find("PlayerCharacter").GetComponent<PlayerCharacter>().Inventory;
            
            if (this.Level==1)
            {
                Debug.Log("id " + id);
                GameManager.Instance.GetLocalManager<WorkerManager>().OnlyCostResource(inventory, id);
                id = GameManager.Instance.GetLocalManager<WorkerEchoManager>().GetRandomID();
                
            }
            else
            {
                foreach (var item in GameManager.Instance.GetLocalManager<WorkerEchoManager>().GetRaw(id))
                {
                    if (item.num > inventory.GetItemAllNum(item.id))
                    {
                        Debug.Log("材料不足！无法召唤！");
                        return null;
                    }
                }
                GameManager.Instance.GetLocalManager<WorkerManager>().OnlyCostResource(inventory, id);
            }

            
            
            ExternWorker externWorker = new ExternWorker(id, GameManager.Instance.GetLocalManager<WorkerEchoManager>().GetTimeCost(id), BuildingPart,index);
            Workers[index] = externWorker;
           
            return externWorker;
        }

        public void SpawnWorker(int index)
        {
            Workers[index].worker.transform.position = new Vector3(2,2,2);
            Workers[index] = null;
        }

        public void ExpelWorker(int index)
        {
            GameObject.Destroy(Workers[index].worker.gameObject);
           
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

        public void StopEcho(string id,int index)
        {
            IInventory inventory = GameObject.Find("PlayerCharacter").GetComponent<PlayerCharacter>().Inventory;
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