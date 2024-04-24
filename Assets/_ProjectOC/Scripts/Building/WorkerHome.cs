using ML.Engine.Timer;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ProjectOC.Building
{
    public class WorkerHome : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, ML.Engine.InteractSystem.IInteraction, IWorkerContainer
    {
        #region Config
        [LabelText("ÿTime��ظ�Mood����ֵ"), FoldoutGroup("����")]
        public int Time { get; private set; } = 1;
        [LabelText("�ظ�����ֵ"), FoldoutGroup("����")]
        public int Mood { get; private set; } = 5;
        #endregion

        #region Data
        [LabelText("�ƶ���ʱ��ԭ���󶨵ĵ���"), ShowInInspector, ReadOnly]
        private Worker TempWorker;
        [LabelText("�ƶ���ʱ��ԭ���ĵ����Ƿ�������"), ShowInInspector, ReadOnly]
        private bool TempHasArrive;
        /// <summary>
        /// �а󶨵����ҵ�������ʱѭ��ִ�У�ʱ��ΪTime�룬ִ��һ�ν�����Ըõ�������Mood������ֵ
        /// </summary>
        private CounterDownTimer Timer;
        #endregion

        #region BuildingPart IInteraction
        public string InteractType { get; set; } = "WorkerHome";
        public Vector3 PosOffset { get; set; } = Vector3.zero;
        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                BindWorkerDefault();
                Timer = new CounterDownTimer(Time, true, false);
                Timer.OnEndEvent += EndActionForTimer;
            }
            if (!isFirstBuild && oldPos != newPos)
            {
                if (TempWorker != null)
                {
                    (this as IWorkerContainer).SetWorker(TempWorker);
                    if (TempHasArrive)
                    {
                        OnArriveEvent(TempWorker);
                    }
                }
                else
                {
                    BindWorkerDefault();
                }
            }
            base.OnChangePlaceEvent(oldPos, newPos);
        }

        public override void OnEnterEdit()
        {
            //��ʼ�ƶ�ʱ����TempWorker����ΪWorker����TempHasArrive����ΪHasArrive������UnBindWorker()�����Worker��
            this.TempWorker = Worker;
            this.TempHasArrive = IsArrive;
            (this as IWorkerContainer).RemoveWorker();
        }

        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIWorkerHomePanel.prefab", ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).Completed += (handle) =>
            {
                ProjectOC.Building.UI.UIWorkerHome uiPanel = (handle.Result).GetComponent<ProjectOC.Building.UI.UIWorkerHome>();
                uiPanel.Home = this;
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);
            };
        }
        #endregion

        #region Mono
        protected override void Start()
        {
            OnSetWorkerEvent += (worker) =>
            {
                if (worker != null)
                {
                    ManagerNS.LocalGameManager.Instance.WorkerManager.OnAddWokerEvent -= OnManagerAddWorkerEvent;
                }
            };
            OnRemoveWorkerEvent += () => { ManagerNS.LocalGameManager.Instance.WorkerManager.OnAddWokerEvent += OnManagerAddWorkerEvent; };
            this.enabled = false;
        }

        public void OnDestroy()
        {
            (this as IWorkerContainer).RemoveWorker();
        }

        public void OnManagerAddWorkerEvent(Worker worker)
        {
            if (!worker.HasHome && !HaveWorker)
            {
                (this as IWorkerContainer).SetWorker(worker);
            }
        }
        #endregion

        #region Method
        private void EndActionForTimer()
        {
            if (Worker != null && Worker.Mood < Worker.MoodMax)
            {
                Worker.AlterMood(Mood);
            }
        }

        public void BindWorkerDefault()
        {
            List<Worker> workers = ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorkers();
            workers.RemoveAll(x => x == null);
            foreach (Worker worker in workers.OrderBy(worker => worker.InstanceID).ToList())
            {
                if (!worker.HasHome)
                {
                    (this as IWorkerContainer).SetWorker(worker);
                }
            }
        }
        #endregion

        #region IWorkerContainer
        public Worker Worker { get; set; }
        [LabelText("�����Ƿ�������"), ShowInInspector, ReadOnly]
        private bool isArrive;
        public bool IsArrive
        {
            get { return isArrive; }
            set
            {
                isArrive = value;
                if (isArrive) { Timer?.Start(); }
                else { Timer?.End(); }
            }
        }
        public bool HaveWorker => Worker != null && !string.IsNullOrEmpty(Worker.InstanceID);
        public Action<Worker> OnSetWorkerEvent { get; set; }
        public Action OnRemoveWorkerEvent { get; set; }

        public string GetUID() { return InstanceID; }
        public Transform GetTransform() { return transform; }
        public WorkerContainerType GetContainerType() { return WorkerContainerType.Home; }

        public void OnArriveEvent(Worker worker)
        {
            (this as IWorkerContainer).OnArriveSetPosition(worker, transform.position + new Vector3(0, 2f, 0));
        }
        #endregion
    }
}