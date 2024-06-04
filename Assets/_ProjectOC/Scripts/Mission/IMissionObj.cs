using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using ProjectOC.DataNS;

namespace ProjectOC.MissionNS
{
    public interface IMissionObj
    {
        #region Data
        [LabelText("搬运优先级"), ShowInInspector, ReadOnly]
        public TransportPriority TransportPriority { get; set; }
        [LabelText("对应的搬运"), ShowInInspector, ReadOnly]
        public List<Transport> Transports { get; set; }
        [LabelText("对应的搬运任务"), ShowInInspector, ReadOnly]
        public List<MissionTransport> Missions { get; set; }
        #endregion

        #region Abstract
        public Transform GetTransform();
        public string GetUID();
        public MissionObjType GetMissionObjType();
        #endregion

        #region Get
        public int GetReservePutIn(IDataObj data);
        public int GetReservePutOut(IDataObj data);
        public Dictionary<IDataObj, int> GetReservePutIn();
        public Dictionary<IDataObj, int> GetReservePutOut();
        public TransportPriority GetTransportPriority() { return TransportPriority; }
        public int GetMissionNum(IDataObj data, bool isPutIn = true)
        {
            int result = 0;
            if (data != null)
            {
                foreach (MissionTransport mission in Missions.ToArray())
                {
                    if (mission != null && mission.Data.DataEquales(data))
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
        public int GetNeedAssignNum(IDataObj data, bool isPutIn = true)
        {
            int result = 0;
            if (data != null)
            {
                foreach (MissionTransport mission in Missions.ToArray())
                {
                    if (mission != null && mission.Data.DataEquales(data))
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
        public List<MissionTransport> GetMissions(IDataObj data, bool isPutIn = true)
        {
            List<MissionTransport> result = new List<MissionTransport>();
            if (data != null)
            {
                foreach (MissionTransport mission in Missions.ToArray())
                {
                    if (mission != null && mission.Data.DataEquales(data))
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
        public void PutIn(int index, IDataObj data, int amount);
        public bool PutIn(IDataObj data, int amount);
        public int ReservePutIn(IDataObj data, int amount, bool reserveEmpty = false);
        public int RemoveReservePutIn(IDataObj data, int amount, bool removeEmpty = false);
        public int PutOut(IDataObj data, int amount, bool removeEmpty = false);
        public int ReservePutOut(IDataObj data, int amount, bool recplaceData = false, Transport transport = null);
        public int RemoveReservePutOut(IDataObj data, int amount);
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
        public void UpdateTransport(IDataObj data)
        {
            if (data != null)
            {
                int storageReserve = GetReservePutOut(data);
                int emptyReserve = GetReservePutIn(data);
                foreach (Transport transport in Transports.ToArray())
                {
                    if (transport != null && transport.Data.DataEquales(data))
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
            Dictionary<IDataObj, int> storageReserve = GetReservePutOut();
            Dictionary<IDataObj, int> emptyReserve = GetReservePutIn();
            foreach (Transport transport in Transports.ToArray())
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