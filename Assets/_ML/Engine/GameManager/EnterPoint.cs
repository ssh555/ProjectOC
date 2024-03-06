using ML.Engine.UI;
using ProjectOC.InventorySystem.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace ML.Engine.Manager
{
    public class EnterPoint
    {
        private GameObject StartMenuPanelPrefab;
        public GameObject LoadingScenePanelPrefab;
        public GameObject OptionPanelPrefab;
        private bool isInit;

        public EnterPoint()
        {
            this.isInit = false;
            GameManager.Instance.StartCoroutine(InitUIPrefabs());
        }

        public bool EnterGame()
        {
            if(isInit == false) 
            {
                return false;
            }

            // สตภýปฏ
            var panel = GameObject.Instantiate(StartMenuPanelPrefab).GetComponent<StartMenuPanel>();

            panel.transform.SetParent(GameObject.Find("Canvas").transform, false);

            // Push
            GameManager.Instance.UIManager.PushPanel(panel);
            return true;
        }

        #region Prefab
        public AssetBundle PrefabsAB;
        public IEnumerator InitUIPrefabs()
        {
            this.PrefabsAB = null;
            var abmgr = GameManager.Instance.ABResourceManager;

            var crequest = abmgr.LoadLocalABAsync("ui/baseuipanel", null, out var PrefabsAB);

            if (crequest != null)
            {
                yield return crequest;
                PrefabsAB = crequest.assetBundle;

                this.PrefabsAB = PrefabsAB;
                StartMenuPanelPrefab = this.PrefabsAB.LoadAsset<GameObject>("StartMenuPanel");
                LoadingScenePanelPrefab = this.PrefabsAB.LoadAsset<GameObject>("LoadingScenePanel");
                OptionPanelPrefab = this.PrefabsAB.LoadAsset<GameObject>("OptionPanel");
                this.isInit = true;
                EnterGame();
            }  
        }
        #endregion
    }

}


