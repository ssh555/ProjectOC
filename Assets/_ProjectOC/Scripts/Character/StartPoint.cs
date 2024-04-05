using System;
using System.Collections;
using System.Collections.Generic;
using ML.PlayerCharacterNS;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public class StartPoint : IStartPoint
    {
        private void Awake()
        {
            this.enabled = false;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position,PosRange);
        }
    }
}
