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
        /// 存入
        /// </summary>
        public bool PutIn(string itemID, int amount);
        /// <summary>
        /// 取出
        /// </summary>
        public int PutOut(string itemID, int amount);
        /// <summary>
        /// 预留存入量
        /// </summary>
        public virtual int ReservePutIn(string itemID, int amount, Transport transport) { return 0; }
        /// <summary>
        /// 预留取出量
        /// </summary>
        public virtual int ReservePutOut(string itemID, int amount, Transport transport) { return 0; }
        /// <summary>
        /// 移除预留存入量
        /// </summary>
        public virtual int RemoveReservePutIn(string itemID, int amount) { return 0; }
        /// <summary>
        /// 移除预留取出量
        /// </summary>
        public virtual int RemoveReservePutOut(string itemID, int amount) { return 0; }
    }
}
