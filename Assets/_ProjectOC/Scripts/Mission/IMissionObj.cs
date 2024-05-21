using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    public interface IMissionObj
    {
        [LabelText("搬运优先级"), ShowInInspector, ReadOnly]
        public TransportPriority TransportPriority { get; set; }
        [LabelText("对应的搬运"), ShowInInspector, ReadOnly]
        public List<Transport> Transports { get; set; }
        [LabelText("对应的搬运任务"), ShowInInspector, ReadOnly]
        public List<MissionTransport> Missions { get; set; }

        #region Get
        public Transform GetTransform();
        public string GetUID();
        public MissionObjType GetMissionObjType();
        public int GetAmount(string id, DataNS.DataOpType type, bool needCanIn = false, bool needCanOut = false);
        public Dictionary<string, int> GetAmount(DataNS.DataOpType type, bool needCanIn = false, bool needCanOut = false);
        public int GetReservePutIn(string itemID) { return GetAmount(itemID, DataNS.DataOpType.EmptyReserve); }
        public int GetReservePutOut(string itemID) { return GetAmount(itemID, DataNS.DataOpType.StorageReserve); }
        public Dictionary<string, int> GetReservePutIn() { return GetAmount(DataNS.DataOpType.EmptyReserve); }
        public Dictionary<string, int> GetReservePutOut() { return GetAmount(DataNS.DataOpType.StorageReserve); }
        public TransportPriority GetTransportPriority() { return TransportPriority; }
        public int GetMissionNum(string itemID, bool isPutIn = true)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (MissionTransport mission in Missions.ToArray())
                {
                    if (mission != null && mission.ID == itemID)
                    {
                        bool flag = isPutIn ? mission.MissionInitiatorType == MissionInitiatorType.PutIn_Initiator 
                            : mission.MissionInitiatorType == MissionInitiatorType.PutOut_Initiator;
                        if (flag)
                        {
                            result += mission.MissionNum;
                        }
                    }
                }
            }
            return result;
        }
        public int GetNeedAssignNum(string itemID, bool isPutIn = true)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (MissionTransport mission in Missions.ToArray())
                {
                    if (mission != null && mission.ID == itemID)
                    {
                        bool flag = isPutIn ? mission.MissionInitiatorType == MissionInitiatorType.PutIn_Initiator
                            : mission.MissionInitiatorType == MissionInitiatorType.PutOut_Initiator;
                        if (flag)
                        {
                            result += mission.NeedAssignNum;
                        }
                    }
                }
            }
            return result;
        }
        public List<MissionTransport> GetMissions(string itemID, bool isPutIn = true)
        {
            List<MissionTransport> result = new List<MissionTransport>();
            if (!string.IsNullOrEmpty(itemID))
            {
                foreach (MissionTransport mission in Missions.ToArray())
                {
                    if (mission != null && mission.ID == itemID)
                    {
                        bool flag = isPutIn ? mission.MissionInitiatorType == MissionInitiatorType.PutIn_Initiator
                            : mission.MissionInitiatorType == MissionInitiatorType.PutOut_Initiator;
                        if (flag)
                        {
                            result.Add(mission);
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region Set
        public int ChangeAmount(string id, int amount, DataNS.DataOpType addType, DataNS.DataOpType removeType, bool exceed = false, bool complete = true, bool needCanIn = false, bool needCanOut = false);
        public bool PutIn(string itemID, int amount)
        {
            return ChangeAmount(itemID, amount, DataNS.DataOpType.Storage, DataNS.DataOpType.EmptyReserve, exceed: true) == amount;
        }
        public int PutOut(string itemID, int amount)
        {
            return ChangeAmount(itemID, amount, DataNS.DataOpType.Empty, DataNS.DataOpType.StorageReserve, complete: false);
        }
        public int ReservePutIn(string itemID, int amount)
        {
            return ChangeAmount(itemID, amount, DataNS.DataOpType.EmptyReserve, DataNS.DataOpType.Empty, exceed: true, needCanIn: true);
        }
        public int ReservePutOut(string itemID, int amount)
        {
            return ChangeAmount(itemID, amount, DataNS.DataOpType.StorageReserve, DataNS.DataOpType.Storage, complete: false, needCanOut: true);
        }
        public int RemoveReservePutIn(string itemID, int amount)
        {
            return ChangeAmount(itemID, amount, DataNS.DataOpType.Empty, DataNS.DataOpType.EmptyReserve);
        }
        public int RemoveReservePutOut(string itemID, int amount)
        {
            return ChangeAmount(itemID, amount, DataNS.DataOpType.Storage, DataNS.DataOpType.StorageReserve);
        }
        #endregion

        #region Transports Missions
        public void AddTransport(Transport transport) { Transports.Add(transport); }
        public void RemoveTranport(Transport transport) { Transports.Remove(transport); }
        public void AddMissionTranport(MissionTransport mission) { Missions.Add(mission); }
        public void RemoveMissionTranport(MissionTransport mission) { Missions.Remove(mission); }

        public void OnPositionChangeTransport()
        {
            foreach (var transport in Transports.ToArray())
            {
                transport?.UpdateDestination();
            }
            foreach (var mission in Missions.ToArray())
            {
                mission?.UpdateDestionation();
            }
        }

        public void UpdateTransport(string itemID)
        {
            if (!string.IsNullOrEmpty(itemID))
            {
                int storageReserve = GetReservePutOut(itemID);
                int emptyReserve = GetReservePutIn(itemID);
                foreach (Transport transport in Transports.ToArray())
                {
                    if (transport != null && transport.ID == itemID)
                    {
                        if (transport.Source == this && !transport.ArriveSource)
                        {
                            if (storageReserve <= 0)
                            {
                                transport.End();
                            }
                            else
                            {
                                storageReserve -= transport.SoureceReserveNum;
                            }
                        }
                        else if (transport.Target == this && emptyReserve <= 0)
                        {
                            transport.End();
                        }
                    }
                }
            }
        }

        public void UpdateTransport()
        {
            Dictionary<string, int> storageReserve = GetReservePutOut();
            Dictionary<string, int> emptyReserve = GetReservePutIn();
            foreach (Transport transport in Transports.ToArray())
            {
                if (transport != null && storageReserve.ContainsKey(transport.ID))
                {
                    if (transport.Source == this && !transport.ArriveSource)
                    {
                        if (storageReserve[transport.ID] <= 0)
                        {
                            transport.End();
                        }
                        else
                        {
                            storageReserve[transport.ID] -= transport.SoureceReserveNum;
                        }
                    }
                    else if (transport.Target == this && emptyReserve[transport.ID] <= 0)
                    {
                        transport.End();
                    }
                }
                else
                {
                    transport?.End();
                }
            }
        }

        public void Clear()
        {
            if (Transports != null)
            {
                foreach (Transport transport in Transports.ToArray())
                {
                    if (transport.Target == this || !transport.ArriveSource)
                    {
                        transport?.End();
                    }
                }
            }
            if (Missions != null)
            {
                foreach (MissionTransport mission in Missions.ToArray())
                {
                    mission?.End(true, true);
                }
            }
        }
        #endregion
    }
}
