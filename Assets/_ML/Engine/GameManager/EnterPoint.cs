using ML.Engine.UI;
using ProjectOC.MainInteract.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace ML.Engine.Manager
{
    public class EnterPoint
    {
        private string StartMenuPanelPrefab = "Prefab_MainInteract_UIPanel/Prefab_MainInteract_UI_StartMenuPanel.prefab";
        private string LoadingScenePanelPrefab = "Prefab_MainInteract_UIPanel/Prefab_MainInteract_UI_LoadingScenePanel.prefab";
        private string OptionPanelPrefab = "Prefab_MainInteract_UIPanel/Prefab_MainInteract_UI_OptionPanel.prefab";

        public EnterPoint()
        {
            //InitUIPrefabs();


            System.Action<string, string> postCallback = (string s1,string s2) => {
                this.GetStartMenuPanelInstance().Completed += (handle) =>
                {
                    // สตภýปฏ
                    var panel = handle.Result.GetComponent<StartMenuPanel>();

                    panel.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform, false);

                    GameManager.Instance.UIManager.PushPanel(panel);

                    //panel.OnEnter();
                };

            };

            GameManager.Instance.StartCoroutine(GameManager.Instance.LevelSwitchManager.LoadSceneAsync("Scene_PreScene", null, postCallback));
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
        

        public AsyncOperationHandle<GameObject> GetStartMenuPanelInstance()
        {
            return Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(this.StartMenuPanelPrefab, isGlobal: true);
        }
        public AsyncOperationHandle<GameObject> GetLoadingScenePanelInstance()
        {
            return Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(this.LoadingScenePanelPrefab, isGlobal: true);
        }
        public AsyncOperationHandle<GameObject> GetOptionPanelInstance()
        {
            return Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(this.OptionPanelPrefab, isGlobal: true);
        }
        #endregion
    }

}


