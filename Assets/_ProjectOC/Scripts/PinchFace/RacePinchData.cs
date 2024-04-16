using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    [System.Serializable]
    public class RacePinchData
    {
        public string raceName;
        public string raceDescription;
        public List<PinchPartType3> pinchPartType3s;
        public bool isDefault = true;
        RacePinchData(string _raceName, string _raceDescription, List<PinchPartType3> _pinchPartType3s)
        {
            this.raceName = _raceName;
            this.raceDescription = _raceDescription;
            this.pinchPartType3s = _pinchPartType3s;
        }
    }
}