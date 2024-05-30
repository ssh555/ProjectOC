using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class PinchBraid : PinchTransfType
    {
        public PinchBraid(int _index) : base(_index)
        {
            transformType = TransformType.Sphere;
        }
    }
}