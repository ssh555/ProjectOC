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
        public bool isDefault = false;
        public List<PinchFaceTemplateData> PinchFaceTemplate = new List<PinchFaceTemplateData>();
        public RacePinchData()
        {
            pinchPartType3s = new List<PinchPartType3>();
        }
        public RacePinchData(string _raceName, string _raceDescription, List<PinchPartType3> _pinchPartType3s)
        {
            this.raceName = _raceName;
            this.raceDescription = _raceDescription;
            this.pinchPartType3s = _pinchPartType3s;
        }

        public class PinchFaceTemplateData
        {
            public string faceTemplateName;
            public List<PinchPart.PinchPartData> PinchPartDatas;

            public PinchFaceTemplateData(string _faceTemplateName,List<PinchPart.PinchPartData> _pinchPartDatas)
            {
                faceTemplateName = _faceTemplateName;
                PinchPartDatas = _pinchPartDatas;
            }
        }
    }
}