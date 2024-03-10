using ML.Engine.UI;
using ProjectOC.InventorySystem.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace ML.Engine.Manager
{
    public class EnterPoint
    {
        private string StartMenuPanelPrefab = "";
        private string LoadingScenePanelPrefab = "";
        private string OptionPanelPrefab = "";

        public EnterPoint()
        {
            //InitUIPrefabs();


            System.Action<string, string> postCallback = (string s1,string s2) => {
                this.GetStartMenuPanelInstance().Completed += (handle) =>
                {
                    // สตภýปฏ
                    var panel = handle.Result.GetComponent<StartMenuPanel>();

                    panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                    // Push
                    GameManager.Instance.UIManager.PushPanel(panel);
                };

            };

            GameManager.Instance.StartCoroutine(GameManager.Instance.LevelSwitchManager.LoadSceneAsync("EnterPointScene", null, postCallback));
        }

        #region Prefab
        //public void InitUIPrefabs()
        //{
        //    this.PrefabsAB = null;
        //    var abmgr = GameManager.Instance.ABResourceManager;

        //    var crequest = abmgr.LoadLocalABAsync("ui/baseuipanel", null, out var PrefabsAB);


        //    PrefabsAB = crequest.assetBundle;

        //    this.PrefabsAB = PrefabsAB;
        //    StartMenuPanelPrefab = this.PrefabsAB.LoadAsset<GameObject>("StartMenuPanel");
        //    LoadingScenePanelPrefab = this.PrefabsAB.LoadAsset<GameObject>("LoadingScenePanel");
        //    OptionPanelPrefab = this.PrefabsAB.LoadAsset<GameObject>("OptionPanel");
        //    this.isInit = true;
           
        //}
        #endregion

        public AsyncOperationHandle<GameObject> GetStartMenuPanelInstance()
        {
            return Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(this.StartMenuPanelPrefab);
        }
        public AsyncOperationHandle<GameObject> GetLoadingScenePanelInstance()
        {
            return Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(this.LoadingScenePanelPrefab);
        }
        public AsyncOperationHandle<GameObject> GetOptionPanelInstance()
        {
            return Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(this.OptionPanelPrefab);
        }
    }

}


