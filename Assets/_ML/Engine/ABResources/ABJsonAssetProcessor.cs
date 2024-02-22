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
