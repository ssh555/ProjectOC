using ML.Engine.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    public class ItemIcon : MonoBehaviour, ITickComponent
    {
        private Transform m_Camera;

        #region ITickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        #endregion

        private void Awake()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterLateTick(0, this);
        }
        private void Start()
        {
            m_Camera = Camera.main.transform;
            this.enabled = false;
        }

        public void LateTick(float deltatime)
        {
            if (m_Camera == null)
            {
                return;
            }
            transform.rotation = Quaternion.LookRotation(transform.position - m_Camera.position);
        }

        private void OnDestroy()
        {
            (this as ML.Engine.Timer.ITickComponent).DisposeTick();
        }
    }

}