using ML.Engine.FSM;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using ProjectOC.MissionNS;
using ProjectOC.ProductionNodeNS;
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
        /// ����ID
        /// </summary>
        public string ID = "";
        /// <summary>
        /// ���֣�TODO:�������
        /// </summary>
        public string Name = "";
        /// <summary>
        /// ְҵ����
        /// </summary>
        public WorkType WorkType;
        /// <summary>
        /// ��ǰ����ֵ
        /// </summary>
        public int APCurrent = 10;
        /// <summary>
        /// ��������
        /// </summary>
        public int APMax = 10;
        /// <summary>
        /// ����������ֵ���жϹ���״̬�����Ƿ���Ҫǿ����Ϣ����ֵ
        /// </summary>
        public int APWorkThreshold = 3;
        /// <summary>
        /// ������Ϣ��ֵ���ж���Ϣ״̬�����Ƿ���Ҫ�������͵���ֵ
        /// </summary>
        public int APRelaxThreshold = 5;
        /// <summary>
        /// �������һ���������ĵ�����ֵ
        /// </summary>
        public int APCost = 1;
        /// <summary>
        /// ÿ�ΰ��˽������ĵ�����ֵ
        /// </summary>
        public int APCostTransport = 1;
        /// <summary>
        /// ��ǰ����
        /// </summary>
        public int BURCurrent = 0;
        /// <summary>
        /// ��������
        /// </summary>
        public int BURMax = 100;
        /// <summary>
        /// ����
        /// </summary>
        public Dictionary<WorkType, Skill> Skills = new Dictionary<WorkType, Skill>();

        #region ������
        /// <summary>
        /// �ƶ��ٶȣ���λΪ m/s
        /// </summary>
        public float WalkSpeed = 10;
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
                DispatchTimeManager timeManager = GameManager.Instance.GetLocalManager<DispatchTimeManager>();
                if (timeManager != null)
                {
                    return TimeArrangement[timeManager.CurrentTimeFrame];
                }
                return TimeStatus.None;
            } 
        }
        /// <summary>
        /// ��ǰʵ��״̬
        /// </summary>
        public Status Status;
        /// <summary>
        /// ״̬��������
        /// </summary>
        protected StateController StateController;
        /// <summary>
        /// ״̬��
        /// </summary>
        protected WorkerStateMachine StateMachine;
        /// <summary>
        /// �Ƿ���ֵ��
        /// </summary>
        public bool IsOnDuty;
        public ProductionNode DutyProductionNode;
        public MissionTransport MissionTransport;
        #endregion
        public Worker()
        {
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

            this.Features = FeatureManager.Instance.CreateFeatureForWorker();
            foreach (Feature feature in this.Features)
            {
                feature.ApplyEffectToWorker(this);
            }
            // ��ʼ��״̬��
            StateController = new StateController(0);
            StateMachine = new WorkerStateMachine();
            StateController.SetStateMachine(StateMachine);
        }
        public void Init(WorkerManager.WorkerTableJsonData config)
        {
            this.ID = config.id;
            this.Name = config.name;
            this.WorkType = config.workType;
            this.APCurrent = config.apMax;
            this.APMax = config.apMax;
            this.APWorkThreshold = config.apWorkThreshold;
            this.APRelaxThreshold = config.apRelaxThreshold;
            this.APCost = config.apCost;
            this.APCostTransport = config.apCostTransport;
            this.BURMax = config.burMax;
            foreach (var kv in config.skillDict)
            {
                Skill skill = SkillManager.Instance.SpawnSkill(kv.Value);
                if (skill != null)
                {
                    skill.ApplyEffectToWorker(this);
                    this.Skills.Add(kv.Key, skill);
                }
            }
        }
        public void Init(Worker worker)
        {
            this.ID = worker.ID;
            this.Name = worker.Name;
            this.WorkType = worker.WorkType;
            this.APCurrent = worker.APCurrent;
            this.APMax = worker.APMax;
            this.APWorkThreshold = worker.APWorkThreshold;
            this.APRelaxThreshold = worker.APRelaxThreshold;
            this.APCost = worker.APCost;
            this.APCostTransport = worker.APCostTransport;
            this.WalkSpeed = worker.WalkSpeed;
            this.BURCurrent = worker.BURCurrent;
            this.BURMax = worker.BURMax;
            this.ExpRate = new Dictionary<WorkType, int>(worker.ExpRate);
            this.Eff = new Dictionary<WorkType, int>(worker.Eff);
            foreach (Feature feature in this.Features)
            {
                feature.RemoveEffectToWorker(this);
            }
            this.Features.Clear();
            foreach (Feature feature in worker.Features)
            {
                Feature featureNew = new Feature();
                featureNew.Init(feature);
                featureNew.ApplyEffectToWorker(this);
                this.Features.Add(featureNew);
            }
            foreach (Skill skill in this.Skills.Values)
            {
                skill.RemoveEffectToWorker(this);
            }
            this.Skills.Clear();
            foreach (var kv in worker.Skills)
            {
                Skill skill = new Skill();
                skill.Init(kv.Value);
                skill.ApplyEffectToWorker(this);
                this.Skills.Add(kv.Key, skill);
            }
            this.TimeArrangement.SetTimeArrangement(worker.TimeArrangement);
        }

        /// <summary>
        /// �޸ľ���ֵ
        /// </summary>
        /// <param name="workType">��������</param>
        /// <param name="value">����ֵ</param>
        public void AlterExp(WorkType workType, int value)
        {
            this.Skills[workType].AlterExp(value);
        }
        /// <summary>
        /// �޸�����ֵ
        /// </summary>
        /// <param name="value">����ֵ</param>
        public bool AlterAP(int value)
        {
            int ap = this.APCurrent + value;
            if (ap >= 0 && ap <= this.APMax)
            {
                this.APCurrent = ap;
                return true;
            }
            return false;
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
