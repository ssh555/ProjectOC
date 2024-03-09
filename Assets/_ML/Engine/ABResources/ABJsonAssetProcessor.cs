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
        /// �������ʱ�Ļص�
        /// </summary>
        /// <param name="datas"></param>
        public delegate void OnLoadOver(T datas);
        /// <summary>
        /// Ϊnull���߷���trueʱ�ŻῪʼ����
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public delegate bool LoadStartCondition();

        /// <summary>
        /// �Ƿ�����|�Ѿ�����
        /// </summary>
        public bool IsLoading = false;
        /// <summary>
        /// �Ƿ�������
        /// </summary>
        public bool IsLoaded = false;

        /// <summary>
        /// ���ص�����
        /// </summary>
        public T Datas;

        /// <summary>
        /// AB��·��
        /// </summary>
        public readonly string ABPath;
        /// <summary>
        /// AB���ʲ�����
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
                Debug.Log(this.description + " JSONText�Ѽ������");
#endif
            };

            return handle;
        }
    }

}
