using ML.Engine.Manager;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.WorkerNS;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace ML.Engine.Utility
{
    /// <summary>
    /// 管理整个UI的资源，包括TextContent,Texture2D,Prefabs
    /// </summary>
    public class ObjectPool
    {

        /// <summary>
        /// 资源加载器
        /// </summary>
        private FunctionExecutor<List<AsyncOperationHandle>> functionExecutor;
        /// <summary>
        /// Prefabs对象池字典
        /// </summary>
        private Dictionary<string, PoolStruct<GameObject>> goPoolDic;
        /// <summary>
        /// Texture2D对象池字典
        /// </summary>
        private Dictionary<string, PoolStruct<SpriteAtlas>> saPoolDic;

        /// <summary>
        /// GameObjectHandle
        /// </summary>
        private List<AsyncOperationHandle<GameObject>> goHandle;
        /// <summary>
        /// SpriteAtlasHandle
        /// </summary>
        private List<AsyncOperationHandle<SpriteAtlas>> saHandle;

        private GameObject RootGameObject;
        private struct PoolStruct<T>
        {
            public int index;
            public List<T> Pool;
        }

        public enum HandleType
        {
            Texture2D=0,
            Prefab,
        }


        public ObjectPool()
        {
            functionExecutor = new FunctionExecutor<List<AsyncOperationHandle>>();
            goPoolDic = new Dictionary<string, PoolStruct<GameObject>>();
            saPoolDic = new Dictionary<string, PoolStruct<SpriteAtlas>>();
            goHandle = new List<AsyncOperationHandle<GameObject>>();
            saHandle = new List<AsyncOperationHandle<SpriteAtlas>>();
            RootGameObject = new GameObject("RootGameObject");
            RootGameObject.transform.position = Vector3.zero;
        }

        /// <summary>
        /// 该函数用于加入资源加载器，由资源加载器执行
        /// </summary>
        private List<AsyncOperationHandle> GetHandles(HandleType handleType, string poolName, int n, string path, Action<AsyncOperationHandle> OnCompleted = null)
        {
            var handles = new List<AsyncOperationHandle>();
            for (int i = 0; i < n; i++)
            {
                
                switch (handleType)
                {
                    case HandleType.Texture2D:
                        var handle1 = GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(path);
                        handle1.Completed += (handle1) =>
                        {
                            this.saHandle.Add(handle1);
                            if (!this.saPoolDic.ContainsKey(poolName))
                            {
                                PoolStruct<SpriteAtlas> poolStruct = new PoolStruct<SpriteAtlas>();
                                poolStruct.index = 0;
                                poolStruct.Pool = new List<SpriteAtlas>();
                                this.saPoolDic[poolName] = poolStruct;
                            }

                            this.saPoolDic[poolName].Pool.Add(handle1.Result);
                            OnCompleted?.Invoke(handle1);
                        };
                        handles.Add(handle1);
                        break;
                    case HandleType.Prefab:
                        var handle2 = GameManager.Instance.ABResourceManager.InstantiateAsync(path);
                        handle2.Completed += (handle2) =>
                        {
                            this.goHandle.Add(handle2);
                            if (!this.goPoolDic.ContainsKey(poolName))
                            {
                                PoolStruct<GameObject> poolStruct = new PoolStruct<GameObject>();
                                poolStruct.index = 0;
                                poolStruct.Pool = new List<GameObject>();
                                this.goPoolDic[poolName] = poolStruct;
                            }

                            this.goPoolDic[poolName].Pool.Add(handle2.Result);
                            OnCompleted?.Invoke(handle2);
                        };
                        handles.Add(handle2);
                        break;
                }  
            }
            return handles;
        }

        /// <summary>
        /// 该函数用于注册对象池
        /// </summary>
        public void RegisterPool(HandleType handleType, string poolName, int n, string path, Action<AsyncOperationHandle> OnCompleted = null)
        {
            this.functionExecutor.AddFunction(() => { return GetHandles(handleType, poolName, n, path, OnCompleted); });

        }
        /// <summary>
        /// 该函数用于释放对象池中所有资源
        /// </summary>
        public void OnDestroy()
        {
            foreach (var handle in goHandle)
            {
                GameManager.Instance.ABResourceManager.ReleaseInstance(handle);
            }

            foreach (var handle in saHandle)
            {
                GameManager.Instance.ABResourceManager.Release(handle);
            }

            if(RootGameObject != null)
                GameManager.DestroyObj(RootGameObject, false);
        }

        /// <summary>
        /// 该函数用于获取某个Prefabs对象池的下一个可用实例
        /// </summary>
        public GameObject GetNextObject(string poolName, Transform parent = null)
        {
            //Debug.Log("GetNextObject "+goPoolDic.Count);
            if (this.goPoolDic.ContainsKey(poolName))
            {
                PoolStruct<GameObject> poolStruct = this.goPoolDic[poolName];
                GameObject go = poolStruct.Pool[poolStruct.index];
                poolStruct.index = (poolStruct.index + 1) % poolStruct.Pool.Count;
                this.goPoolDic[poolName] = poolStruct;
                if (parent != null) go.transform.SetParent(parent, false);
                return go;
            }
            return null;
        }
        /// <summary>
        /// 该函数用于获取资源加载器
        /// </summary>
        public FunctionExecutor<List<AsyncOperationHandle>> GetFunctionExecutor()
        {
            return this.functionExecutor;
        }

        public void ResetAllObject()
        {
            foreach(var pool in goPoolDic)
            {
                foreach (var go in pool.Value.Pool)
                {
                    go.transform.SetParent(RootGameObject.transform, false);
                }
            }
        }

    }

}


