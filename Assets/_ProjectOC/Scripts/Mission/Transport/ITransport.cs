using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    public interface ITransport 
    {
        #region Property
        [LabelText("����ð��˵ĵ���"), ReadOnly, ShowInInspector]
        public WorkerNS.Worker Worker { get; set; }
        [LabelText("��Ҫ���˵�����"), ReadOnly, ShowInInspector]
        public int MissionNum { get; set; }
        [LabelText("��ǰ�õ�������"), ReadOnly, ShowInInspector]
        public int CurNum { get; set; }
        [LabelText("��ɵ�����"), ReadOnly, ShowInInspector]
        public int FinishNum { get; set; }
        [LabelText("ȡ����Ԥ��������"), ReadOnly, ShowInInspector]
        public int SoureceReserveNum { get; set; }
        [LabelText("�ͻ���Ԥ��������"), ReadOnly, ShowInInspector]
        public int TargetReserveNum { get; set; }
        [LabelText("�Ƿ񵽴�ȡ����"), ReadOnly, ShowInInspector]
        public bool ArriveSource { get; set; }
        [LabelText("�Ƿ񵽴�Ŀ�ĵ�"), ReadOnly, ShowInInspector]
        public bool ArriveTarget { get; set; }
        [LabelText("�Ƿ�����Ч��"), ReadOnly, ShowInInspector]
        public bool IsValid { get; }
        #endregion

        #region Method
        public Transform GetSourceTransform();
        public Transform GetTargetTransform();
        public void UpdateDestination();
        /// <summary>
        /// ��ȡ�����ó�
        /// </summary>
        public void PutOutSource();
        /// <summary>
        /// �����ͻ���
        /// </summary>
        public void PutInTarget();
        /// <summary>
        /// ��������
        /// </summary>
        public void End();
        #endregion
    }
}