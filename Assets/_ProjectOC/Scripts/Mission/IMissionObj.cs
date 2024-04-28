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
        public Transform GetTransform();
        public string GetUID();
        public bool PutIn(string itemID, int amount);
        public int PutOut(string itemID, int amount);

        public virtual int ReservePutIn(string itemID, int amount) { return 0; }
        public virtual int ReservePutOut(string itemID, int amount) { return 0; }
        public virtual int RemoveReservePutIn(string itemID, int amount) { return 0; }
        public virtual int RemoveReservePutOut(string itemID, int amount) { return 0; }
        public virtual int GetReservePutIn(string itemID) { return 0; }
        public virtual int GetReservePutOut(string itemID) { return 0; }
        public virtual Dictionary<string, int> GetReservePutIn() { return new Dictionary<string, int>(); }
        public virtual Dictionary<string, int> GetReservePutOut() { return new Dictionary<string, int>(); }

        public TransportPriority GetTransportPriority() { return TransportPriority; }
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
                        else if (transport.Target == this && emptyReserve == 0)
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
                    else if (transport.Target == this && emptyReserve[transport.ID] == 0)
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
                    mission?.End();
                }
            }
        }
    }
}
