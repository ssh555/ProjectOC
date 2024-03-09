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
using UnityEngine.ResourceManagement.AsyncOperations;

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
        private readonly string description;
        private AsyncOperationHandle handle;

        public ABJsonAssetProcessor(string abpath, string abname, OnLoadOver onLoadOver = null, string description = "")
        {
            this.ABPath = abpath;
            this.ABName = abname;
            this.onLoadOver = onLoadOver;
            this.description = description;
        }

        ~ABJsonAssetProcessor()
        {
            Manager.GameManager.Instance.ABResourceManager.Release(this.handle);
        }

        public AsyncOperationHandle StartLoadJsonAssetData()
        {
            if (IsLoading)
            {
                return handle;
            }
            IsLoading = true;

            var abmgr = ML.Engine.Manager.GameManager.Instance.ABResourceManager;
            handle = abmgr.LoadAssetAsync<TextAsset>(this.ABPath + "/" + this.ABName);
            handle.Completed += (asHandle) =>
            {
                Datas = JsonConvert.DeserializeObject<T>((handle.Result as TextAsset).text);

                IsLoaded = true;

                this.onLoadOver?.Invoke(Datas);

#if UNITY_EDITOR
                Debug.Log(this.description + " JSONText已加载完成");
#endif
            };

            return handle;
        }
    }

}
