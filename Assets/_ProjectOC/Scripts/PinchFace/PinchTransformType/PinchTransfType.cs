using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public abstract class PinchTransfType
    {
        public TransformType transformType;
        public int index;
        
        public enum TransformType
        {
            Bezier,
            Sphere
        }
        public virtual void Init(){}

        public virtual void Release()
        {
            index = -1;
        }

        public PinchTransfType(int _index)
        {
            index = _index;
        }
        
        
        public virtual void ModifyValue(Vector2 _value)
        {
            
        }
    }
}