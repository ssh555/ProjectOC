using ML.Engine.Timer;
using ProjectOC.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    public class ItemIcon : MonoBehaviour, ITickComponent
    {
        private UnityEngine.UI.Image Image;
        public PlayerCharacter Player;

        #region ITickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        #endregion

        private void Start()
        {
            Image = GetComponentInChildren<UnityEngine.UI.Image>();
            Image.transform.SetParent(Manager.GameManager.Instance.UIManager.GetCanvas.transform);
            Image.enabled = false;
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterLateTick(0, this);
            this.enabled = false;
        }

        public void LateTick(float deltatime)
        {
            if (Image.sprite != null && Vector3.Distance(transform.position, Player.transform.position) <= 5f)
            {
                Image.enabled = true;
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
                screenPosition.x = Mathf.Clamp(screenPosition.x, 0, Screen.width);
                screenPosition.y = Mathf.Clamp(screenPosition.y, 0, Screen.height);
                Image.transform.position = screenPosition;
            }
            else
            {
                Image.enabled = false;
            }
        }

        public Sprite GetSprite()
        {
            return Image?.sprite;
        }
        public void SetSprite(Sprite sprite)
        {
            Image.sprite = sprite;
        }

        private void OnDestroy()
        {
            (this as ML.Engine.Timer.ITickComponent).DisposeTick();
            if (Manager.GameManager.Instance != null && Image != null)
                Manager.GameManager.Instance.ABResourceManager.ReleaseInstance(Image.transform.gameObject);
        }
    }
}