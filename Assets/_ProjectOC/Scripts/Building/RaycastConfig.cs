using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Building
{
    /// <summary>
    /// …‰œﬂºÏ≤‚≈‰÷√
    /// </summary>
    [System.Serializable]
    public struct RaycastConfig
    {
        public Vector3 Offset;
        /// <summary>
        /// Length, Height, Width
        /// </summary>
        public Vector3 Size;
        public Vector3 Rotation;
        public Vector3 Scale;
        public RaycastConfig(Vector3 offset, Vector3 Size, Vector3 rotation, Vector3 scale)
        {
            this.Offset = offset;
            this.Size = Size;
            this.Rotation = rotation;
            this.Scale = scale;
        }
    }
}
