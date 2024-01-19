using JetBrains.Annotations;
using ML.Engine.ABResources;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.Manager;
using ML.Engine.Timer;
using ProjectOC.Player;
using ProjectOC.WorkerNS;
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
        public ExternWorker(string WorkerID, float time,BuildingPart buildingPart)
        {

            timer = new CounterDownTimer(time);
            GameManager.Instance.GetLocalManager<WorkerManager>().OnlyCostResource(GameObject.Find("PlayerCharacter").GetComponent<PlayerCharacter>().Inventory, WorkerID);
            timer.OnEndEvent += () =>
            {
                GameManager.Instance.GetLocalManager<WorkerManager>().SpawnWorker(buildingPart.transform.position,Quaternion.identity,WorkerID);
            };
        }
    }
    [System.Serializable]
    public sealed class WorkerEcho : ML.Engine.Manager.LocalManager.ILocalManager
    {
        private int Level = 1;
        ExternWorker[] Workers = new ExternWorker[5];
        BuildingPart BuildingPart = null;

        public WorkerEcho(BuildingPart buildingPart)
        {
            this.BuildingPart = buildingPart;
        }
        public ExternWorker SummonWorker(string id,int index)
        {
            if(this.Level==1)
            {
                id = GameManager.Instance.GetLocalManager<WorkerEchoManager>().GetRandomID();
            }
            ExternWorker externWorker = new ExternWorker(id, GameManager.Instance.GetLocalManager<WorkerEchoManager>().GetTimeCost(id), BuildingPart);
            return externWorker;
        }
        public void SpawnWorker(int index)
        {
            Workers[index].worker.transform.position = Vector3.zero;
            Workers[index] = null;
        }
        public void ExpelWorker(int index)
        {
            GameManager.Instance.GetLocalManager<WorkerManager>().RemoveWorker(Workers[index].worker);
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
            IInventory inventory = GameObject.Find("PlayerCharacter").GetComponent<PlayerCharacter>().Inventory;
            List<ML.Engine.InventorySystem.CompositeSystem.Formula> dict = GameManager.Instance.GetLocalManager<WorkerEchoManager>().GetRaw(Workers[index].WorkerID);
            foreach(var pair in dict)
            {
                foreach (Item item in ItemManager.Instance.SpawnItems(pair.id, pair.num))
                {
                    if (inventory.AddItem(item))
                    {
                        inventory.AddItem(item);
                    }
                }

            }

        }
    }
}