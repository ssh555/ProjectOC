using System.Collections;
using UnityEngine;

namespace ProjectOC.MissionNS
{
    public interface IMissionObj
    {
        public Transform GetTransform();
        public TransportPriority GetTransportPriority();
        public string GetUID();
        public void AddTransport(Transport transport);
        public void RemoveTranport(Transport transport);
        public void AddMissionTranport(MissionTransport mission);
        public void RemoveMissionTranport(MissionTransport mission);
        public bool PutIn(string itemID, int amount);
        public int PutOut(string itemID, int amount);
    }
}
