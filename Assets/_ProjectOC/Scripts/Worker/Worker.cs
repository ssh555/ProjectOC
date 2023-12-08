using ML.Engine.FSM;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using ProjectOC.MissionNS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// ����
    /// </summary>
    [System.Serializable]
    public class Worker : MonoBehaviour
    {
        /// <summary>
        /// ���֣��������
        /// </summary>
        public string Name = "";
        /// <summary>
        /// ְҵ����
        /// </summary>
        public WorkType WorkType;
        /// <summary>
        /// ��ǰ����ֵ
        /// </summary>
        public int APCurrent;
        /// <summary>
        /// ��������
        /// </summary>
        public int APMax;
        /// <summary>
        /// ����������ֵ���жϹ���״̬�����Ƿ���Ҫǿ����Ϣ����ֵ
        /// </summary>
        public int APWorkThreshold;
        /// <summary>
        /// ������Ϣ��ֵ���ж���Ϣ״̬�����Ƿ���Ҫ�������͵���ֵ
        /// </summary>
        public int APRelaxThreshold;
        /// <summary>
        /// �������һ���������ĵ�����ֵ
        /// </summary>
        public int APCost;
        /// <summary>
        /// ÿ�ΰ��˽������ĵ�����ֵ
        /// </summary>
        public int APCostTransport;
        /// <summary>
        /// �ƶ��ٶȣ���λΪ m/s
        /// </summary>
        public float WalkSpeed;
        /// <summary>
        /// ��ǰ����
        /// </summary>
        public int BURCurrent;
        /// <summary>
        /// ��������
        /// </summary>
        public int BURMax;
        /// <summary>
        /// ְҵ�����ȡ�ٶȣ���λΪ%
        /// </summary>
        public Dictionary<WorkType, int> ExpRate = new Dictionary<WorkType, int>();
        /// <summary>
        /// ְҵЧ�ʼӳɣ���λΪ%
        /// </summary>
        public Dictionary<WorkType, int> Eff = new Dictionary<WorkType, int>();
        /// <summary>
        /// ����
        /// </summary>
        public List<Feature> Features = new List<Feature>();
        /// <summary>
        /// ����
        /// </summary>
        public Dictionary<WorkType, WorkerAbility> Skills = new Dictionary<WorkType, WorkerAbility>();


        /// <summary>
        /// ÿ��ʱ�εİ���
        /// </summary>
        protected TimeArrangement TimeArrangement;
        /// <summary>
        /// ��ǰʱ�εİ���Ӧ��������״̬
        /// </summary>
        public TimeStatus CurTimeFrameStatus 
        { 
            get 
            {
                return TimeArrangement[GameManager.Instance.GetLocalManager<DispatchTimeManager>().CurrentTimeFrame];
            } 
        }
        /// <summary>
        /// ��ǰʵ��״̬
        /// </summary>
        private Status status;
        /// <summary>
        /// ��ǰʵ��״̬
        /// </summary>
        public Status Status
        {
            get => status;
            set
            {
                GameManager.Instance.GetLocalManager<MissionBroadCastManager>().UpdateWorkerStatusList(this, status, value);
                status = value;
            }
        }
        /// <summary>
        /// �Ƿ���ֵ��
        /// </summary>
        public bool IsOnDuty;
        /// <summary>
        /// ״̬��������
        /// </summary>
        protected StateController stateController;
        /// <summary>
        /// ״̬��
        /// </summary>
        protected WorkerStateMachine stateMachine;


        public Worker()
        {
            // TODO: �߻����ó�ʼ��ֵ
            this.Name = "";
            this.WorkType = WorkType.Transport;
            this.APCurrent = 10;
            this.APMax = 10;
            this.APWorkThreshold = 0;
            this.APRelaxThreshold = 3;
            this.APCost = 1;
            this.APCostTransport = 1;
            this.WalkSpeed = 10;
            this.BURCurrent = 0;
            this.BURMax = 10;
            this.ExpRate.Add(WorkType.Cook, 100);
            this.ExpRate.Add(WorkType.HandCraft, 100);
            this.ExpRate.Add(WorkType.Industry, 100);
            this.ExpRate.Add(WorkType.Science, 100);
            this.ExpRate.Add(WorkType.Magic, 100);
            this.ExpRate.Add(WorkType.Transport, 100);

            this.Eff.Add(WorkType.Cook, 10);
            this.Eff.Add(WorkType.HandCraft, 10);
            this.Eff.Add(WorkType.Industry, 10);
            this.Eff.Add(WorkType.Science, 10);
            this.Eff.Add(WorkType.Magic, 10);
            this.Eff.Add(WorkType.Transport, 50);

            FeatureManager featureManager = GameManager.Instance.GetLocalManager<FeatureManager>();
            if (featureManager != null)
            {
                this.Features = featureManager.CreateFeatureForWorker();
            }
            foreach (Feature feature in this.Features)
            {
                feature.ApplyEffectToWorker(this);
            }
            this.Skills.Add(WorkType.Cook, new WorkerAbility(""));
            this.Skills.Add(WorkType.HandCraft, new WorkerAbility(""));
            this.Skills.Add(WorkType.Industry, new WorkerAbility(""));
            this.Skills.Add(WorkType.Science, new WorkerAbility(""));
            this.Skills.Add(WorkType.Magic, new WorkerAbility(""));
            this.Skills.Add(WorkType.Transport, new WorkerAbility(""));
            // TODO:�ӱ�����skill id����skill
            foreach (WorkerAbility skill in this.Skills.Values)
            {
                skill.ApplyEffectToWorker(this);
            }

            // ��ʼ��״̬��
            stateController = new StateController(0);
            stateMachine = new WorkerStateMachine();
            stateController.SetStateMachine(stateMachine);
        }
        /// <summary>
        /// �޸ľ���ֵ
        /// </summary>
        /// <param name="workType">��������</param>
        /// <param name="value">����ֵ</param>
        public void AlterExp(WorkType workType, int value)
        {
            this.Skills[workType].AlterExp(value);
            // TODO:��������ʱЧ���仯
        }

        /// <summary>
        /// �޸�����ֵ
        /// </summary>
        /// <param name="value">����ֵ</param>
        public void AlterAP(int value)
        {

        }

        public void SetTimeStatus(int time, TimeStatus timeStatus)
        {
            this.TimeArrangement[time] = timeStatus;
        }
        public void SetTimeArrangement(Worker worker)
        {
            if (worker != null)
            {
                this.TimeArrangement.SetTimeArrangement(worker.TimeArrangement);
            }
        }
        public void SetTimeStatusAll(TimeStatus timeStatus)
        {
            this.TimeArrangement.SetTimeStatusAll(timeStatus);
        }

    }
}
