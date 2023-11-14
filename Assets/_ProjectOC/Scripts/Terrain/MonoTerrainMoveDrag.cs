using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Terrain
{
    [RequireComponent(typeof(Collider))]
    public class MonoTerrainMoveDrag : MonoBehaviour
    {
        [LabelText("地面阻尼系数")]

        public float TerrainDrag = 0;

        private void Awake()
        {
            this.enabled = false;
        }
    }
}

