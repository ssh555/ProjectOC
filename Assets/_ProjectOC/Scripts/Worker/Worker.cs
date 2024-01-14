using ML.Engine.FSM;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using ProjectOC.MissionNS;
using ProjectOC.ProNodeNS;
using Sirenix.OdinInspector;
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
        #region �߻�������
        [LabelText("����")]
        public string Name = "Worker";
        [LabelText("��ǰ����ֵ")]
        public int APCurrent = 10;
        [LabelText("��������")]
        public int APMax = 10;
        [LabelText("����������ֵ")]
        public int APWorkThreshold = 3;
        [LabelText("������Ϣ��ֵ")]
        public int APRelaxThreshold = 5;
        [LabelText("���һ���������ĵ�����ֵ")]
        public int APCost = 1;
        [LabelText("���һ�ΰ������ĵ�����ֵ")]
        public int APCostTransport = 1;
        [LabelText("�ƶ��ٶ�")]
        public float WalkSpeed = 10;
        [LabelText("��ǰ����")]
        public int BURCurrent
        {
            get
            {
                int result = 0;
                foreach (Item item in this.TransportItems)
                {
                    result += item.Weight;
                }
                return result;
            }
        }
        [LabelText("��������")]
        public int BURMax = 100;
        [LabelText("���˾���ֵ")]
        public int ExpTransport = 10;
        [LabelText("��������")]
        public WorkType SkillType;
        public Dictionary<WorkType, string> SkillConfig = new Dictionary<WorkType, string>();
        [LabelText("����")]
        public Dictionary<WorkType, Skill> Skill = new Dictionary<WorkType, Skill>();
        [LabelText("���ܾ����ȡ�ٶ�")]
        public Dictionary<WorkType, int> ExpRate = new Dictionary<WorkType, int>();
        [LabelText("ְҵЧ�ʼӳ�")]
        public Dictionary<WorkType, int> Eff = new Dictionary<WorkType, int>();
        #endregion

        [LabelText("����")]
        public List<Feature> Features = new List<Feature>();

        [LabelText("ÿ��ʱ�εİ���")]
        public TimeArrangement TimeArrangement;

        [LabelText("��ǰʱ�εİ���Ӧ��������״̬")]
        public TimeStatus CurTimeFrameStatus 
        { 
            get 
            {
                DispatchTimeManager timeManager = GameManager.Instance.GetLocalManager<DispatchTimeManager>();
                if (timeManager != null)
                {
                    return TimeArrangement[timeManager.CurrentTimeFrame];
                }
                Debug.LogError("DispatchTimeManager is Null");
                return TimeStatus.None;
            } 
        }
        
        /// <summary>
        /// ״̬��������
        /// </summary>
        protected StateController StateController;

        /// <summary>
        /// ״̬��
        /// </summary>
        protected WorkerStateMachine StateMachine;

        [LabelText("��ǰʵ��״̬")]
        public Status Status;

        [LabelText("�Ƿ���ֵ��")]
        public bool IsOnDuty { get { return this.ProNode != null && this.Status != Status.Relaxing; } }

        [LabelText("�����ڵ�")]
        public ProNode ProNode;

        [LabelText("����")]
        public Transport Transport;

        [LabelText("������Ʒ")]
        public List<Item> TransportItems = new List<Item>();

        public void Start()
        {
            this.enabled = false;
        }
        public Worker()
        {
            //this.ExpRate.Add(WorkType.Cook, "Skill ID");

            this.ExpRate.Add(WorkType.Cook, 100);
            this.ExpRate.Add(WorkType.HandCraft, 100);
            this.ExpRate.Add(WorkType.Industry, 100);
            this.ExpRate.Add(WorkType.Magic, 100);
            this.ExpRate.Add(WorkType.Transport, 100);
            this.ExpRate.Add(WorkType.Collect, 100);

            this.Eff.Add(WorkType.Cook, 0);
            this.Eff.Add(WorkType.HandCraft, 0);
            this.Eff.Add(WorkType.Industry, 0);
            this.Eff.Add(WorkType.Magic, 0);
            this.Eff.Add(WorkType.Transport, 50);
            this.Eff.Add(WorkType.Collect, 0);

            this.Features = FeatureManager.Instance.CreateFeature();
            foreach (Feature feature in this.Features)
            {
                feature.ApplyFeature(this);
            }

            foreach (var kv in SkillConfig)
            {
                Skill skill = SkillManager.Instance.SpawnSkill(kv.Value);
                if (skill != null)
                {
                    skill.ApplySkill(this);
                    this.Skill.Add(kv.Key, skill);
                }
                else
                {
                    Debug.LogError($"Worker {Name} Skill {kv.Value} is Null");
                }
            }

            // ��ʼ��״̬��
            StateController = new StateController(0);
            StateMachine = new WorkerStateMachine(this);
            StateController.SetStateMachine(StateMachine);
        }

        /// <summary>
        /// �޸ľ���ֵ
        /// </summary>
        /// <param name="workType">��������</param>
        /// <param name="value">����ֵ</param>
        public void AlterExp(WorkType workType, int value)
        {
            this.Skill[workType].AlterExp(value * ExpRate[workType]);
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

        /// <summary>
        /// ������˽���
        /// </summary>
        public void SettleTransport()
        {
            this.AlterAP(-1 * APCostTransport);
            this.AlterExp(WorkType.Transport, ExpTransport);
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
