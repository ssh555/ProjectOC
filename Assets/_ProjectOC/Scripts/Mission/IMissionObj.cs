using ProjectOC.StoreNS;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    public interface IMissionObj
    {
        public Transform GetTransform();
        public TransportPriority GetTransportPriority();
        public string GetUID();
        public virtual void AddTransport(Transport transport) { }
        public virtual void RemoveTranport(Transport transport) { }
        public virtual void AddMissionTranport(MissionTransport mission) { }
        public virtual void RemoveMissionTranport(MissionTransport mission) { }
        /// <summary>
        /// ����
        /// </summary>
        public bool PutIn(string itemID, int amount);
        /// <summary>
        /// ȡ��
        /// </summary>
        public int PutOut(string itemID, int amount);
        /// <summary>
        /// Ԥ��������
        /// </summary>
        public virtual int ReservePutIn(string itemID, int amount, Transport transport) { return 0; }
        /// <summary>
        /// Ԥ��ȡ����
        /// </summary>
        public virtual int ReservePutOut(string itemID, int amount, Transport transport) { return 0; }
        /// <summary>
        /// �Ƴ�Ԥ��������
        /// </summary>
        public virtual int RemoveReservePutIn(string itemID, int amount) { return 0; }
        /// <summary>
        /// �Ƴ�Ԥ��ȡ����
        /// </summary>
        public virtual int RemoveReservePutOut(string itemID, int amount) { return 0; }
    }
}
