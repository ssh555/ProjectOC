using ML.Engine.Timer;
using ProjectOC.ManagerNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ProjectOC.Building
{
    public class WorkerHome : ML.Engine.BuildingSystem.BuildingPart.BuildingPart, ML.Engine.InteractSystem.IInteraction
    {
        #region ��������
        [LabelText("ÿTime��ظ�Mood����ֵ"), FoldoutGroup("����"), ReadOnly]
        public int Time = 1;
        [LabelText("�ظ�����ֵ"), FoldoutGroup("����"), ReadOnly]
        public int Mood = 5;
        #endregion

        #region ��ǰ����
        [LabelText("��������"), ReadOnly]
        public Worker Worker;
        [LabelText("�ƶ���ʱ��ԭ���󶨵ĵ���"), ShowInInspector, ReadOnly]
        private Worker TempWorker;
        [LabelText("�ƶ���ʱ��ԭ���ĵ����Ƿ�������"), ShowInInspector, ReadOnly]
        private bool TempHasArrive;

        /// <summary>
        /// �а󶨵����ҵ�������ʱѭ��ִ�У�ʱ��ΪTime�룬ִ��һ�ν�����Ըõ�������Mood������ֵ
        /// </summary>
        private CounterDownTimer Timer;

        public string InteractType { get; set; } = "WorkerHome";
        public Vector3 PosOffset { get; set; } = Vector3.zero;
        #endregion

        #region ����
        public string UID => InstanceID;
        [LabelText("�Ƿ��й�������"), ShowInInspector, ReadOnly]
        public bool HasWorker => Worker != null && !string.IsNullOrEmpty(Worker.InstanceID);

        private bool hasArrive;
        [LabelText("�����Ƿ�������"), ShowInInspector, ReadOnly]
        public bool HasArrive
        {
            get { return hasArrive; }
            set
            {
                hasArrive = value;
                if (hasArrive) { Timer?.Start(); }
                else { Timer?.End(); }
            }
        }
        #endregion

        #region �ӿڷ���
        /// <summary>
        /// ֹͣTimer������UnBindWorker()��������ޡ�
        /// </summary>
        public void OnDestroy()
        {
            if (LocalGameManager.Instance != null && !HasWorker)
            {
                LocalGameManager.Instance.WorkerManager.OnAddWokerEvent -= OnAddWorkerEvent;
            }
            UnBindWorker();
            Timer?.End();
        }
        
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
                //�ƶ��������ж�TempWorker�Ƿ�Ϊ�գ���Ϊ�յ���BindWorker(TempWorker)��
                if (TempWorker != null)
                {
                    BindWorker(TempWorker);
                    HasArrive = TempHasArrive;
                    if (HasArrive)
                    {
                        Worker.transform.position = transform.position + new Vector3(0, 2f, 0);
                    }
                }
                //����HasArriveΪTempHasArrive��Ϊ�������BindWorkerDefalt()��
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
            this.TempHasArrive = HasArrive;
            UnBindWorker();
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

        public void OnAddWorkerEvent(Worker worker)
        {
            if (!worker.HasHome && !HasWorker)
            {
                BindWorker(worker);
            }
        }

        public void ManageAddWorkerEvent()
        {
            if (HasWorker)
            {
                LocalGameManager.Instance.WorkerManager.OnAddWokerEvent -= OnAddWorkerEvent;
            }
            else
            {
                LocalGameManager.Instance.WorkerManager.OnAddWokerEvent += OnAddWorkerEvent;
            }
        }

        private void EndActionForTimer()
        {
            if (Worker != null && Worker.Mood < Worker.MoodMax)
            {
                Worker.AlterMood(Mood);
            }
        }

        public void BindWorker(Worker worker)
        {
            UnBindWorker();
            if (worker != null)
            {
                worker.Home?.UnBindWorker(true);
                Worker = worker;
                worker.Home = this;
            }
            ManageAddWorkerEvent();
        }

        public void BindWorkerDefault()
        {
            List<Worker> workers = ManagerNS.LocalGameManager.Instance.WorkerManager.GetWorkers();
            foreach (Worker worker in workers.OrderBy(worker => worker.InstanceID).ToList())
            {
                if (!worker.HasHome)
                {
                    BindWorker(worker);
                }
            }
            ManageAddWorkerEvent();
        }

        public void UnBindWorker(bool addListener = false)
        {
            if (HasWorker)
            {
                if (HasArrive)
                {
                    Worker.RecoverLastPosition();
                }
                Worker.Home = null;
            }
            HasArrive = false;
            Worker = null;
            if (addListener)
            {
                ManageAddWorkerEvent();
            }
        }
    }
}