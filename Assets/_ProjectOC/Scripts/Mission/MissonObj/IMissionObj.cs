using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    public interface IMissionObj { }
    public interface IMissionObj<T> : IMissionObj
    {
        #region Data
        [LabelText("存储数据是否独立"), ShowInInspector, ReadOnly]
        public bool IsUnique { get; }
        [LabelText("搬运优先级"), ShowInInspector, ReadOnly]
        public TransportPriority TransportPriority { get; set; }
        [LabelText("对应的搬运"), ShowInInspector, ReadOnly]
        public List<Transport<T>> Transports { get; set; }
        [LabelText("对应的搬运任务"), ShowInInspector, ReadOnly]
        public List<MissionTransport<T>> Missions { get; set; }
        #endregion

        #region Abstract
        public Transform GetTransform();
        public string GetUID();
        public MissionObjType GetMissionObjType();
        public int GetAmount(T data, DataNS.DataOpType type, bool needCanIn = false, bool needCanOut = false);
        public Dictionary<T, int> GetAmount(DataNS.DataOpType type, bool needCanIn = false, bool needCanOut = false);
        public int ChangeAmount(T data, int amount, DataNS.DataOpType addType, DataNS.DataOpType removeType, bool exceed = false, bool complete = true, bool needCanIn = false, bool needCanOut = false);
        #endregion

        #region Get
        public int GetReservePutIn(T data) { return GetAmount(data, DataNS.DataOpType.EmptyReserve); }
        public int GetReservePutOut(T data) { return GetAmount(data, DataNS.DataOpType.StorageReserve); }
        public Dictionary<T, int> GetReservePutIn() { return GetAmount(DataNS.DataOpType.EmptyReserve); }
        public Dictionary<T, int> GetReservePutOut() { return GetAmount(DataNS.DataOpType.StorageReserve); }
        public TransportPriority GetTransportPriority() { return TransportPriority; }
        public int GetMissionNum(T data, bool isPutIn = true)
        {
            int result = 0;
            if (data != null)
            {
                foreach (MissionTransport<T> mission in Missions.ToArray())
                {
                    if (mission != null && mission.Data.Equals(data))
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
        public int GetNeedAssignNum(T data, bool isPutIn = true)
        {
            int result = 0;
            if (data != null)
            {
                foreach (MissionTransport<T> mission in Missions.ToArray())
                {
                    if (mission != null && mission.Equals(data))
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
        public List<MissionTransport<T>> GetMissions(T data, bool isPutIn = true)
        {
            List<MissionTransport<T>> result = new List<MissionTransport<T>>();
            if (data != null)
            {
                foreach (MissionTransport<T> mission in Missions.ToArray())
                {
                    if (mission != null && mission.Data.Equals(data))
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
        public bool PutIn(T data, int amount)
        {
            amount = IsUnique ? 1 : amount;
            bool exceed = !IsUnique;
            return ChangeAmount(data, amount, DataNS.DataOpType.Storage, DataNS.DataOpType.EmptyReserve, exceed: exceed) == amount;
        }
        public int PutOut(T data, int amount)
        {
            amount = IsUnique ? 1 : amount;
            bool complete = IsUnique;
            return ChangeAmount(data, amount, DataNS.DataOpType.Empty, DataNS.DataOpType.StorageReserve, complete: complete);
        }
        public int ReservePutIn(T data, int amount)
        {
            amount = IsUnique ? 1 : amount;
            bool exceed = !IsUnique;
            return ChangeAmount(data, amount, DataNS.DataOpType.EmptyReserve, DataNS.DataOpType.Empty, exceed: exceed, needCanIn: true);
        }
        public int ReservePutOut(T data, int amount)
        {
            amount = IsUnique ? 1 : amount;
            bool complete = IsUnique;
            return ChangeAmount(data, amount, DataNS.DataOpType.StorageReserve, DataNS.DataOpType.Storage, complete: complete, needCanOut: true);
        }
        public int RemoveReservePutIn(T data, int amount)
        {
            return ChangeAmount(data, amount, DataNS.DataOpType.Empty, DataNS.DataOpType.EmptyReserve);
        }
        public int RemoveReservePutOut(T data, int amount)
        {
            return ChangeAmount(data, amount, DataNS.DataOpType.Storage, DataNS.DataOpType.StorageReserve);
        }
        #endregion

        #region Transports Missions
        public void AddTransport(Transport<T> transport) { Transports.Add(transport); }
        public void RemoveTranport(Transport<T> transport) { Transports.Remove(transport); }
        public void AddMissionTranport(MissionTransport<T> mission) { Missions.Add(mission); }
        public void RemoveMissionTranport(MissionTransport<T> mission) { Missions.Remove(mission); }
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
        public void UpdateTransport(T data)
        {
            if (data != null)
            {
                int storageReserve = GetReservePutOut(data);
                int emptyReserve = GetReservePutIn(data);
                foreach (Transport<T> transport in Transports.ToArray())
                {
                    if (transport != null && transport.Data.Equals(data))
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
            Dictionary<T, int> storageReserve = GetReservePutOut();
            Dictionary<T, int> emptyReserve = GetReservePutIn();
            bool unique = IsUnique;
            foreach (Transport<T> transport in Transports.ToArray())
            {
                if (transport == null) { continue; }
                var key = transport.Data;
                if (transport.Source == this && !transport.ArriveSource)
                {
                    if (storageReserve.ContainsKey(key) && storageReserve[key] > 0)
                    {
                        storageReserve[key] -= transport.SoureceReserveNum;
                    }
                    else
                    {
                        transport.End();
                    }
                }
                else if (transport.Target == this && !transport.ArriveTarget)
                {
                    if (!emptyReserve.ContainsKey(key) || emptyReserve[key] <= 0)
                    {
                        transport.End();
                    }
                    else if (unique)
                    {
                        emptyReserve[key] -= transport.TargetReserveNum;
                    }
                }
            }
        }
        public void Clear()
        {
            if (Transports != null)
            {
                foreach (Transport<T> transport in Transports.ToArray())
                {
                    if (transport.Target == this || !transport.ArriveSource)
                    {
                        transport?.End();
                    }
                }
            }
            if (Missions != null)
            {
                foreach (MissionTransport<T> mission in Missions.ToArray())
                {
                    mission?.End(true, true);
                }
            }
        }
        #endregion
    }
}