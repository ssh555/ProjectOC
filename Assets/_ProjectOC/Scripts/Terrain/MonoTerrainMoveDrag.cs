using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Terrain
{
    [RequireComponent(typeof(Collider))]
    public class MonoTerrainMoveDrag : MonoBehaviour
    {
        [LabelText("��������ϵ��")]

        public float TerrainDrag = 0;

        private void Awake()
        {
            this.enabled = false;
        }
    }
}

