using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public interface IStartPoint
    {
        [ShowInInspector]
        Vector3 CenterPosition { get; set; }
        [ShowInInspector]
        float PosRange { get; set; }
        [ShowInInspector]
        bool EnablePosRange { get; set; }
        [ShowInInspector]
        Quaternion Rotation { get; set; }
        [ShowInInspector]
        Vector3 RotRange { get; set; }
        [ShowInInspector]
        bool EnableRotRange { get; set; }
    }
}