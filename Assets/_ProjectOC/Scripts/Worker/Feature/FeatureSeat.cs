using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
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
        public List<string> WorkerFeatureIDs = new List<string>();
        public List<string> FeatureIDs = new List<string>();
        public List<bool> IsChanged = new List<bool>();
        public event Action OnArriveInvokeEvent;

        public FeatureSeat(FeatureBuilding featBuild, Transform socket)
        {
            FeatBuild = featBuild;
            Socket = socket;
        }
        public void ClearFeatureID()
        {
            FeatureIDs.Clear();
            WorkerFeatureIDs.Clear();
            IsChanged.Clear();
        }
        public void SetFeatureID()
        {
            ClearFeatureID();
            if (Worker != null)
            {
                FeatureIDs.AddRange(Worker.GetFeatureIDs(true));
                WorkerFeatureIDs.AddRange(FeatureIDs);
                for (int i = 0; i < FeatureIDs.Count; i++)
                {
                    IsChanged.Add(false);
                }
            }
        }
        public void ChangerWorkerFeature()
        {
            for (int i = 0; i < FeatureIDs.Count; i++)
            {
                string oldID = WorkerFeatureIDs[i];
                string newID = FeatureIDs[i];
                if (oldID != newID)
                {
                    if (Worker.Feature.ContainsKey(oldID))
                    {
                        Worker.Feature[oldID].ClearOwner();
                    }
                    if (ManagerNS.LocalGameManager.Instance.FeatureManager.IsValidID(newID))
                    {
                        var feature = ManagerNS.LocalGameManager.Instance.FeatureManager.SpawnFeature(newID);
                        feature.SetOwner(Worker);
                    }
                }
            }
            SetFeatureID();
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
            (this as IWorkerContainer).OnArriveSetPosition(worker, Socket.position);
            worker.LastPosition = Socket.position;
            OnArriveInvokeEvent?.Invoke();
        }
        public void SetWorkerRelateData() 
        { 
            SetFeatureID();
            if (Worker != null) { Worker.StopHomeTimer(); }
        }
        public void RemoveWorkerRelateData() 
        {
            ClearFeatureID();
            if (Worker != null) { Worker.CheckHome(); }
        }
        #endregion
    }
}