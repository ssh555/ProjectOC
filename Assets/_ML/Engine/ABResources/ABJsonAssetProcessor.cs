using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace ML.Engine.ABResources
{
    public class ABJsonAssetProcessor<T>
    {
        /// <summary>
        /// 加载完成时的回调
        /// </summary>
        /// <param name="datas"></param>
        public delegate void OnLoadOver(T datas);
        /// <summary>
        /// 为null或者返回true时才会开始加载
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public delegate bool LoadStartCondition();

        /// <summary>
        /// 是否正在|已经加载
        /// </summary>
        public bool IsLoading = false;
        /// <summary>
        /// 是否加载完成
        /// </summary>
        public bool IsLoaded = false;

        /// <summary>
        /// 加载的数据
        /// </summary>
        public T Datas;

        /// <summary>
        /// AB包路径
        /// </summary>
        public readonly string ABPath;
        /// <summary>
        /// AB包资产名称
        /// </summary>
        public readonly string ABName;
        private readonly OnLoadOver onLoadOver;
        private readonly LoadStartCondition condition;
        private readonly string description;

        public ABJsonAssetProcessor(string abpath, string abname, OnLoadOver onLoadOver = null, LoadStartCondition condition = null, string description = "")
        {
            this.ABPath = abpath;
            this.ABName = abname;
            this.onLoadOver = onLoadOver;
            this.condition = condition;
            this.description = description;
        }

        public void StartLoadJsonAssetData()
        {
            Manager.GameManager.Instance.StartCoroutine(LoadData());
        }

        private IEnumerator LoadData()
        {
            if (IsLoading)
            {
                yield break;
            }
            IsLoading = true;
            while (ML.Engine.Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }
            if (condition != null)
            {
                while (condition())
                {
                    yield return null;
                }
            }

#if UNITY_EDITOR
            float startT = Time.realtimeSinceStartup;
#endif

            var abmgr = ML.Engine.Manager.GameManager.Instance.ABResourceManager;
            var crequest = abmgr.LoadLocalABAsync(this.ABPath, null, out AssetBundle ab);
            yield return crequest;
            if (crequest != null)
            {
                ab = crequest.assetBundle;
            }

            var request = ab.LoadAssetAsync<TextAsset>(this.ABName);
            yield return request;

            Datas = JsonConvert.DeserializeObject<T>((request.asset as TextAsset).text);


           IsLoaded = true;

            this.onLoadOver?.Invoke(Datas);

#if UNITY_EDITOR
            Debug.Log($"LoadJsonAssetData {description} cost time: {Time.realtimeSinceStartup - startT}");
#endif
        }
    }

}
