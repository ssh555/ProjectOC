using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace ML.Engine.BuildingSystem.Config
{
    [CreateAssetMenu(fileName = "Socket2SocketMatchConfig", menuName = "ML/BuildingSystem/Socket2SocketMatchConfig", order = 1)]
    public sealed class BuildingSocket2SocketMatchAsset : SerializedScriptableObject
    {
        [Serializable]
        public struct SocketConfig
        {
            [LabelText("ID")]
            public BuildingSocket.BuildingSocketType ID;
            [LabelText("¿ÉÎü¸½µã")]
            public List<SocketData> ToSocket;

            [Serializable]
            public struct SocketData
            {
                [LabelText("ID")]
                public BuildingSocket.BuildingSocketType ID;
                [LabelText("Î»ÖÃÆ«ÒÆ")]
                public Vector3 offsetPosition;
                [LabelText("Ðý×ªÆ«ÒÆ")]
                public Quaternion offsetRotation;
            }
        }

        /// <summary>
        /// [self][target]
        /// </summary>
        [SerializeField]
        private List<SocketConfig> configs;

        public bool IsMatch(BuildingSocket.BuildingSocketType id, BuildingSocket.BuildingSocketType target, out Vector3 offsetPos, out Quaternion offsetRot)
        {
            offsetPos = Vector3.zero;
            offsetRot = Quaternion.identity;
            if (id == 0 || target == 0)
            {
                return false;
            }
            int index = configs.FindIndex((s) => s.ID == id);
            if(index == -1)
            {
                return false;
            }
            int ti = configs[index].ToSocket.FindIndex((s) => s.ID == target);
            if(ti == -1)
            {
                return false;
            }
            offsetPos = configs[index].ToSocket[ti].offsetPosition;
            offsetRot = configs[index].ToSocket[ti].offsetRotation;
            if(offsetRot.x == 0 && offsetRot.y == 0 && offsetRot.z == 0 && offsetRot.w == 0)
            {
                offsetRot = Quaternion.identity;
            }
            return true;
        }
    }
}
