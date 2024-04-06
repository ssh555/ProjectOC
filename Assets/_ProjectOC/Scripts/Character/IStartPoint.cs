using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public abstract class IStartPoint:MonoBehaviour
    {
        [ShowInInspector]
        public bool EnablePosRange;
        [ShowInInspector,ShowIf("@EnablePosRange == true")] 
        public float PosRange;
        
        [ShowInInspector]
        public bool EnableRotRange;
        [ShowInInspector,ShowIf("@EnableRotRange == true"),Range(0,180f)] 
        public float RotRange;   //应该只需要旋转Y轴吧
    }
}