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
        public WorkerCategory workerCategory;
        public ExternWorker(WorkerCategory workerCategory, float time, WorkerEchoBuilding workerEchoBuilding, int index)
        {
            this.workerCategory = workerCategory;
            Timer = new CounterDownTimer(time);
            Timer.OnEndEvent += () =>
            {
                ManagerNS.LocalGameManager.Instance.WorkerManager.SpawnWorker(workerEchoBuilding.transform.position, Quaternion.identity, false, workerEchoBuilding.WorkerEcho).Completed += (handle) =>
                {
                    Worker = handle.Result.GetComponent<Worker>();
                    
                    Worker.Category = workerCategory;
                    Worker.gameObject.transform.position += new Vector3((float)(3 * Math.Cos(2 * 3.1415926 * index / 5)), 0, (float)(3 * Math.Sin(2 * 3.1415926 * index / 5)));
                };
            };
        }
    }
    [System.Serializable]
    public class WorkerEcho : IEffectObj
    {
        public ExternWorker[] Workers = new ExternWorker[5];
        public WorkerEchoBuilding WorkerEchoBuilding = null;
        public List<int> FeatureMax = new List<int>() { 100, 100, 100, 100};
        public List<int> FeatureOdds = new List<int>() { 100, 100 };
        public float FactorTimeCost = 1;
        public int ModifyTimeCost;
        public int GetRealTimeCost(string id)
        {
            int timeCost = ManagerNS.LocalGameManager.Instance.WorkerEchoManager.GetTimeCost(id);
            return (int)(timeCost * FactorTimeCost + ModifyTimeCost);
        }

        public (int,int) GetRealTimeCostInMSForm(WorkerCategory workerCategory)
        {
            float time = GetRealTimeCost("WorkerEcho_"+workerCategory);
            int min = (int)(time) / 60;
            int sec = (int)(time) - min * 60;
            return (min, sec);
        }

        public WorkerEcho(WorkerEchoBuilding workerEchoBuilding)
        {
            WorkerEchoBuilding = workerEchoBuilding;
        }

        public ExternWorker SummonWorker(WorkerCategory workerCategory, int index, ML.Engine.InventorySystem.IInventory inventory)
        {
  
            string workerid = "WorkerEcho_"+ workerCategory.ToString();
            if (!ManagerNS.LocalGameManager.Instance.WorkerManager.OnlyCostResource(inventory, workerCategory.ToString())) return null;

            if (workerCategory == WorkerCategory.Random)
            {
                workerCategory = ManagerNS.LocalGameManager.Instance.WorkerEchoManager.GetRandomCategory();
                workerid = "WorkerEcho_" + WorkerCategory.Random.ToString();
            }
            ExternWorker externWorker = new ExternWorker(workerCategory, GetRealTimeCost(workerid), WorkerEchoBuilding, index);
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
            foreach(var pair in ManagerNS.LocalGameManager.Instance.WorkerEchoManager.GetRaw("WorkerEcho_"+id))
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
        #region IEffectObj
        public List<Effect> Effects { get; set; } = new List<Effect>();
        public void ApplyEffect(Effect effect)
        {
            if (effect.EffectType != EffectType.AlterEchoVariable) { Debug.Log("type != AlterEchoVariable"); return; }
            bool flag = true;
            if (effect.ParamStr == "FeatureMax[0]")
            {
                FeatureMax[0] += effect.ParamInt;
            }
            else if (effect.ParamStr == "FeatureOdds[0]")
            {
                FeatureOdds[0] += effect.ParamInt;
            }
            else if (effect.ParamStr == "FeatureOdds[1]")
            {
                FeatureOdds[1] += effect.ParamInt;
            }
            else if (effect.ParamStr == "FactorTimeCost")
            {
                FactorTimeCost += effect.ParamFloat;
            }
            else
            {
                flag = false;
            }
            if (flag)
            {
                Effects.Add(effect);
            }
            else
            {
                Debug.Log($"ParamStr Error {effect.ParamStr}");
            }
        }
        public void RemoveEffect(Effect effect)
        {
            if (effect.EffectType != EffectType.AlterEchoVariable) { Debug.Log("type != AlterEchoVariable"); return; }
            Effects.Remove(effect);
            if (effect.ParamStr == "FeatureMax[0]")
            {
                FeatureMax[0] -= effect.ParamInt;
            }
            else if (effect.ParamStr == "FeatureOdds[0]")
            {
                FeatureOdds[0] -= effect.ParamInt;
            }
            else if (effect.ParamStr == "FeatureOdds[1]")
            {
                FeatureOdds[1] -= effect.ParamInt;
            }
            else if (effect.ParamStr == "FactorTimeCost")
            {
                FactorTimeCost -= effect.ParamFloat;
            }
        }
        #endregion
    }
}