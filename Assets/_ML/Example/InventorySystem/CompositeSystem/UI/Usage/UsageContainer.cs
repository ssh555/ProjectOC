using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ML.Example.InventorySystem.CompositeSystem.UI
{
    public class UsageContainer : MonoBehaviour
    {
        public PreviewPanel Owner;

        protected UsageItemBtn usageTemplate;

        private void Awake()
        {
            this.usageTemplate = this.transform.GetChild(0).GetComponent<UsageItemBtn>();

        }

        public void RefreshUsageContainer(int[] usage)
        {
            if (usage.Length == 0)
            {
                this.gameObject.SetActive(false);
            }
            else
            {
                this.gameObject.SetActive(true);
                // Refresh
                foreach (Transform child in this.transform)
                {
                    // ²»É¾³ýÄ£°å
                    if (child == this.usageTemplate.transform) continue;
                    Destroy(child.gameObject);
                }

                for (int i = 0; i < usage.Length; ++i)
                {
                    UsageItemBtn rt = GameObject.Instantiate<UsageItemBtn>(this.usageTemplate, this.transform);
                    rt.ID = usage[i];
                    rt.Owner = this;
                    rt.gameObject.SetActive(true);
                }
            }
        }

    }


}
