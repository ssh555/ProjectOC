using ML.Engine.Timer;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    public class ItemIcon : MonoBehaviour, ITickComponent
    {
        private UnityEngine.UI.Image Image;
        public Transform Target;
        private SpriteRenderer Renderer;
        private bool IsShow;

        #region ITickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        #endregion

        public void Awake()
        {
            Renderer = GetComponent<SpriteRenderer>();
            Image = GetComponentInChildren<UnityEngine.UI.Image>();
        }

        private void Start()
        {
            Image.transform.SetParent(Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
            Image.enabled = false;
            Manager.GameManager.Instance.TickManager.RegisterLateTick(0, this);
            this.enabled = false;
        }

        public void LateTick(float deltatime)
        {
            if (Renderer.isVisible && Image.sprite != null && Target != null && Vector3.Distance(transform.position, Target.position) <= 5f)
            {
                Image.enabled = true;
                IsShow = true;
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
                Image.transform.position = screenPosition;
            }
            else if(IsShow)
            {
                Image.enabled = false;
                IsShow = false;
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
            if (Image != null)
            {
                Image.enabled = false;
            }
            (this as ML.Engine.Timer.ITickComponent).DisposeTick();
            if (Manager.GameManager.Instance != null && Image != null)
                Manager.GameManager.Instance.ABResourceManager.ReleaseInstance(Image.transform.gameObject);
        }
    }
}