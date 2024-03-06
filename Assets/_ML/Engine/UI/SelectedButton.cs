using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.UI
{
    public class SelectedButton : MonoBehaviour, IUISelected
    {
        #region IUISelected
        public IUISelected LeftUI { get; set; }
        public IUISelected RightUI { get; set; }
        public IUISelected UpUI { get; set; }
        public IUISelected DownUI { get; set; }

        public void Interact()
        {
            OnInteract?.Invoke();
        }

        public void SelectedEnter()
        {
            OnSelectedEnter?.Invoke();
            //this.image.color = Color.red;
        }

        public void SelectedExit()
        {
            OnSelectedExit?.Invoke();
            //this.image.color = Color.white;
        }
        #endregion

        public UnityEngine.UI.Image image;
        public TMPro.TextMeshProUGUI text;

        public event System.Action OnInteract;
        public event System.Action OnSelectedEnter;
        public event System.Action OnSelectedExit;

        private void Awake()
        {
            image = this.GetComponentInChildren<UnityEngine.UI.Image>();
            text = this.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        }
    }

}
