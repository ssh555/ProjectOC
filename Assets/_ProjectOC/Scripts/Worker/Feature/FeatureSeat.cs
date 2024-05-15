using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [LabelText("喵喵窝座位"), Serializable]
    public class FeatureSeat : IWorkerContainer
    {
        [LabelText("对应的喵喵窝"), ReadOnly, NonSerialized]
        public FeatureBuilding FeatBuild;
        [LabelText("对应的Socket"), ReadOnly, NonSerialized]
        public Transform Socket;
        public List<string> FeatureIDs;

        public FeatureSeat(FeatureBuilding featBuild, Transform socket)
        {
            FeatBuild = featBuild;
            Socket = socket;
            FeatureIDs = new List<string>();
        }
        public void SetFeatureID()
        {
            FeatureIDs.Clear();
            foreach (Feature feature in Worker.GetFeatures(true))
            {
                FeatureIDs.Add(feature.ID);
            }
        }
        public void ChangerWorkerFeature()
        {
            HashSet<string> set = new HashSet<string>();
            foreach(Feature feature in Worker.GetFeatures(true, false))
            {
                set.Add(feature.ID);
            }
            var newSet = FeatureIDs.ToHashSet();
            newSet.Remove("");
            newSet.SymmetricExceptWith(set);
            foreach (var id in set)
            {
                Worker.Feature[id].ClearOwner();
            }
            foreach (string id in newSet)
            {
                var feature = ManagerNS.LocalGameManager.Instance.FeatureManager.SpawnFeature(id);
                if (!string.IsNullOrEmpty(feature.ID) && 
                    ManagerNS.LocalGameManager.Instance.FeatureManager.IsValidID(feature.ID))
                {
                    feature.SetOwner(Worker);
                }
            }
        }

        #region IWorkerContainer
        public Worker Worker { get; set; }
        public bool IsArrive { get; set; }
        public bool HaveWorker => Worker != null && !string.IsNullOrEmpty(Worker.ID);
        public Action<Worker> OnSetWorkerEvent { get; set; }
        public Action<bool, Worker> OnRemoveWorkerEvent { get; set; }
        public string GetUID() { return FeatBuild.InstanceID; }
        public WorkerContainerType GetContainerType() { return WorkerContainerType.Feature; }
        public Transform GetTransform() { return Socket; }
        public void OnArriveEvent(Worker worker)
        {
            (this as IWorkerContainer).OnArriveSetPosition(worker, Socket.transform.position);
            worker.LastPosition = Socket.transform.position;
        }
        public void SetWorkerRelateData() 
        { 
            SetFeatureID();
            Worker.StopHomeTimer();
        }
        public void RemoveWorkerRelateData() 
        { 
            FeatureIDs.Clear();
            Worker.CheckHome();
        }
        #endregion
    }
}
