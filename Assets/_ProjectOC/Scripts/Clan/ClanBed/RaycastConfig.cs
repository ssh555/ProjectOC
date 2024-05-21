using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.ClanNS
{
    [LabelText("…‰œﬂºÏ≤‚≈‰÷√"), System.Serializable]
    public struct RaycastConfig
    {
        public Vector3 Offset;
        [LabelText("Size (L,H,W)")]
        public Vector3 Size;
        public Vector3 Rotation;
        public Vector3 Scale;
        public RaycastConfig(Vector3 offset, Vector3 size, Vector3 rotation, Vector3 scale)
        {
            Offset = offset;
            Size = size;
            Rotation = rotation;
            Scale = scale;
        }

        public static RaycastConfig defaultTemplate = new RaycastConfig()
        {
            Offset = Vector3.zero,
            Size = Vector3.one,
            Rotation = Vector3.zero,
            Scale = Vector3.one
        };
    }
}
