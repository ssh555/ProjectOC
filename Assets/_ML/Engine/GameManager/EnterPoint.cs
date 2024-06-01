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
            this.GetStartMenuPanelInstance().Completed += (handle) =>
            {
                // สตภýปฏ
                var panel = handle.Result.GetComponent<StartMenuPanel>();

                panel.transform.SetParent(GameManager.Instance.UIManager.NormalPanel, false);

                GameManager.Instance.UIManager.PushPanel(panel);
            };

            //System.Action<string, string> postCallback = (string s1,string s2) => {
            //    this.GetStartMenuPanelInstance().Completed += (handle) =>
            //    {
            //        var panel = handle.Result.GetComponent<StartMenuPanel>();

            //        panel.transform.SetParent(GameManager.Instance.UIManager.NormalPanel, false);

            //        GameManager.Instance.UIManager.PushPanel(panel);
            //    };

            //};

            //GameManager.Instance.StartCoroutine(GameManager.Instance.LevelSwitchManager.LoadSceneAsync("Scene_PreScene", null, postCallback));
        }

        #region Prefab
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


