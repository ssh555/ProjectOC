using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.Manager;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public class StartPoint : IStartPoint
    {
        
        private void Awake()
        {
            if (this.startPointType == StartPointType.Player)
            {
                GameManager.Instance.CharacterManager.playerStartPoints.Add(this);    
            }
            this.enabled = false;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                if (startPointType == StartPointType.Player)
                {
                    GameManager.Instance.CharacterManager.playerStartPoints.Remove(this);    
                }   
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position,PosRange);
        }
    }
}
