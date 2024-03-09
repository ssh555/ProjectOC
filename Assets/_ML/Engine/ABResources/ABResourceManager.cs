using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using static UnityEditor.FilePathAttribute;
using UnityEngine.UIElements;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.InputSystem;
using System.Xml;
using System;
using static UnityEngine.AddressableAssets.Addressables;
using UnityEngine.SceneManagement;
using UnityEditor.SearchService;
using System.Xml.Linq;

namespace ML.Engine.ABResources
{
    /// <summary>
    /// Ŀǰ������ Windows
    /// AB��Ŀǰ���� StreamingAssets/Windows ��
    /// </summary>
    public sealed class ABResourceManager : Manager.GlobalManager.IGlobalManager
    {
#if false
        /// <summary>
        /// ��ʼ������ʱĬ�������Global AB
        /// �� | ����
        /// </summary>
#if UNITY_EDITOR
        private const string DEFAULTAB = "scene|testscene";
#else
        private const string DEFAULTAB = "scene";
#endif

        /// <summary>
        /// Windows �� AB ������·��
        /// </summary>
        private readonly static string WindowsPath = Application.streamingAssetsPath + "/Windows";

        /// <summary>
        /// ʵ��ʹ��·��
        /// </summary>
        public static string ABPath
        {
            get
            {
#if UNITY_STANDALONE_WIN
                return WindowsPath;
#endif
            }
        }

        public static string MainABName
        {
            get
            {
#if UNITY_STANDALONE_WIN
                return "Windows";
#endif
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        private static AssetBundle mainAB;
        /// <summary>
        /// ����Mainifest
        /// </summary>
        private static AssetBundleManifest mainABManifest;

        public ABResourceManager()
        {
            // ���� AB ����
            if(mainAB == null)
            {
                mainAB = AssetBundle.LoadFromFile(System.IO.Path.Combine(ABPath, MainABName));
                mainABManifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                this.LoadDefaultAB();
            }
        }

        ~ABResourceManager()
        {
            mainAB.Unload(true);
        }

        /// <summary>
        /// �ڲ�ʹ�õ�AB�������ṹ��
        /// </summary>
        private struct DependedAB
        {
            /// <summary>
            /// ���ü���
            /// </summary>
            public int referCount;
            /// <summary>
            /// ����AB��
            /// </summary>
            public AssetBundle dependedAB;
        }

        /// <summary>
        /// ȫ����ԴAB��
        /// <namePath, AssetBundle>
        /// �����ֶ��ͷ�
        /// ��߲㼶
        /// </summary>
        private Dictionary<string, AssetBundle> globalResourcesDict = new Dictionary<string, AssetBundle>();

        /// <summary>
        /// �ֲ���ԴAB��
        /// <namePath, AssetBundle>
        /// �����л�ʱ�Զ��ͷ�
        /// �β㼶 -> LoadGlobalʱ����Ϊ Global�㼶
        /// </summary>
        private Dictionary<string, AssetBundle> localResourcesDict = new Dictionary<string, AssetBundle>();

        /// <summary>
        /// ������ԴAB��
        /// ���������������ô���
        /// ���ô���Ϊ0ʱ�Զ��ͷ�
        /// ��Ͳ㼶 -> Loadʱ����Ϊ Global/Local �㼶
        /// </summary>
        private Dictionary<string, DependedAB> dependedResourcesDict = new Dictionary<string, DependedAB>();

        public enum ABStatus
        {
            Null = 0,
            Global,
            Local,
            Dependence,
        }

        public ABStatus IsLoadedAB(string name)
        {
            name = name.ToLower();
            // ��ѯ Global
            if (this.globalResourcesDict.ContainsKey(name))
            {
                return ABStatus.Global;
            }
            // ��ѯ Local
            if (this.localResourcesDict.ContainsKey(name))
            {
                return ABStatus.Local;
            }
            // ��ѯ Dependency
            if (this.dependedResourcesDict.ContainsKey(name))
            {
                return ABStatus.Dependence;
            }
            return ABStatus.Null;
        }

        private ABStatus Internal_GetAB(string name, out AssetBundle ab)
        {
            name = name.ToLower();
            // ��ѯ Global
            if (this.globalResourcesDict.TryGetValue(name, out ab))
            {
                return ABStatus.Global;
            }
            // ��ѯ Local
            if (this.localResourcesDict.TryGetValue(name, out ab))
            {
                return ABStatus.Local;
            }
            // ��ѯ Dependency
            if (this.dependedResourcesDict.TryGetValue(name, out DependedAB dependedAB))
            {
                ab = dependedAB.dependedAB;
                return ABStatus.Dependence;
            }
            return ABStatus.Null;
        }

        /// <summary>
        /// ͬ������
        /// </summary>
        /// <param name="abname"></param>
        /// <returns></returns>
        private AssetBundle[] Internal_LoadDependencies(string abname)
        {
            abname = abname.ToLower();

            string[] depenceies = mainABManifest.GetAllDependencies(abname);
            AssetBundle[] ans = new AssetBundle[depenceies.Length];

            for (int i = 0; i < depenceies.Length; ++i)
            {
                ABStatus abStatus = Internal_GetAB(depenceies[i], out ans[i]);
                // ��ѯ�Ƿ�������������
                if (abStatus == ABStatus.Null)
                {
                    ans[i] = AssetBundle.LoadFromFile(System.IO.Path.Combine(ABPath, depenceies[i]));
                    DependedAB dependedAB = new DependedAB();
                    dependedAB.dependedAB = ans[i];
                    dependedAB.referCount = 1;
                    this.dependedResourcesDict.Add(depenceies[i], dependedAB);
                }
                else if (abStatus == ABStatus.Dependence)
                {
                    DependedAB dependedAB = this.dependedResourcesDict[depenceies[i]];
                    dependedAB.referCount++;
                }
            }

            return ans;
        }

        ///// <summary>
        ///// ͬ��ж��
        ///// </summary>
        ///// <param name="abname"></param>
        //private void Internal_UnLoadDependencies(string abname)
        //{

        //}

        /// <summary>
        /// �첽ж�� => ����dependedResourcesDict��������
        /// Local��Global�е�������Ҫ�����ֶ��ͷ�
        /// </summary>
        /// <param name="abname"></param>
        private void Internal_UnLoadDependencies(string abname)
        {
            abname = abname.ToLower();

            string[] depenceies = mainABManifest.GetAllDependencies(abname);

            for (int i = 0; i < depenceies.Length; ++i)
            {
                ABStatus abStatus = Internal_GetAB(depenceies[i], out AssetBundle ab);
                // ��ѯ�Ƿ�������������
                if (abStatus == ABStatus.Dependence)
                {
                    // �����ô���
                    var dab = this.dependedResourcesDict[depenceies[i]];
                    dab.referCount--;
                    if(dab.referCount == 0)
                    {
                        this.dependedResourcesDict.Remove(depenceies[i]);
                    }
                    // �첽ж��
                    ab.UnloadAsync(true);
                }
            }
        }

        /// <summary>
        /// ͬ������ GlobalAB
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AssetBundle LoadGlobalAB(string name)
        {
            name = name.ToLower();

            ABStatus abStatus = Internal_GetAB(name, out AssetBundle ab);
            // ��ѯ Global
            if (abStatus == ABStatus.Global)
            {
                return ab;
            }
            // ��ѯ Local
            if (abStatus == ABStatus.Local)
            {
                this.localResourcesDict.Remove(name);
                this.globalResourcesDict.Add(name, ab);
                return ab;
            }
            // ��ѯ Dependency
            if (abStatus == ABStatus.Dependence)
            {
                this.dependedResourcesDict.Remove(name);
                this.globalResourcesDict.Add(name, ab);
                return ab;
            }

            Internal_LoadDependencies(name);

            // δ����
            ab = AssetBundle.LoadFromFile(System.IO.Path.Combine(ABPath, name));


            this.globalResourcesDict.Add(name, ab);
            return ab;
        }
        /// <summary>
        /// ����ֵΪnull���ʾ�Ѿ����ؽ���AB�� => ����ֵΪ assetBundle
        /// ����ֵ��Ϊnull���ʾδ���ؽ���AB�� => ʹ�ó����첽�ص��ȷ�����ȡ��Ӧ AssetBundle
        /// to-do : [����ͬ������������? => if completed��������Ļ�]
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="assetBundle"></param>
        /// <returns></returns>
        public AssetBundleCreateRequest LoadGlobalABAsync(string name, System.Action<AsyncOperation> callback, out AssetBundle assetBundle)
        {
            name = name.ToLower();

            ABStatus abStatus = Internal_GetAB(name, out assetBundle);
            // ��ѯ Global
            if (abStatus == ABStatus.Global)
            {
                return null;
            }
            // ��ѯ Local
            if (abStatus == ABStatus.Local)
            {
                this.localResourcesDict.Remove(name);
                this.globalResourcesDict.Add(name, assetBundle);
                return null;
            }
            // ��ѯ Dependency
            if (abStatus == ABStatus.Dependence)
            {
                this.dependedResourcesDict.Remove(name);
                this.globalResourcesDict.Add(name, assetBundle);
                return null;
            }

            // δ����
            var ans = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(ABPath, name));
            this.globalResourcesDict.Add(name, ans.assetBundle);
            ans.completed += (asyncOpt) =>
            {
                this.globalResourcesDict[name] = ans.assetBundle;
                Internal_LoadDependencies(name);
            };
            ans.completed += callback;

            assetBundle = null;

            return ans;
        }

        /// <summary>
        /// �ͷ� AB�� �������Asset
        /// ֮��Ҫʹ�ñ����ٴ����� AB��
        /// </summary>
        /// <param name="name"></param>
        /// <param name="unloadAllLoadedObjects"></param>
        public void UnLoadGlobalAB(string name, bool unloadAllLoadedObjects)
        {
            name = name.ToLower();

            if (this.globalResourcesDict.TryGetValue(name, out AssetBundle ab))
            {
                this.globalResourcesDict.Remove(name);
                Internal_UnLoadDependencies(name);
                ab.Unload(unloadAllLoadedObjects);
            }
        }

        /// <summary>
        /// �ͷ� AB�� �������Asset
        /// ֮��Ҫʹ�ñ����ٴ����� AB��
        /// </summary>
        /// <param name="name"></param>
        /// <param name="unloadAllLoadedObjects"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public AsyncOperation UnLoadGlobalABAsync(string name, bool unloadAllLoadedObjects, System.Action<AsyncOperation> callback)
        {
            name = name.ToLower();

            if (this.globalResourcesDict.TryGetValue(name, out AssetBundle ab))
            {
                this.globalResourcesDict.Remove(name);
                var ans = ab.UnloadAsync(unloadAllLoadedObjects);
                ans.completed += callback;
                Internal_UnLoadDependencies(name);
                return ans;
            }
            return null;
        }
    
        /// <summary>
        /// �����Ҫ����ʹ�õ���ԴΪ Global, ��Ҳ�᷵��
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AssetBundle LoadLocalAB(string name)
        {
            name = name.ToLower();

            ABStatus abStatus = Internal_GetAB(name, out AssetBundle ab);
            // ��ѯ Global | ��ѯ Local
            if (abStatus == ABStatus.Global || abStatus == ABStatus.Local)
            {
                return ab;
            }
            // ��ѯ Dependency
            if (abStatus == ABStatus.Dependence)
            {
                this.dependedResourcesDict.Remove(name);
                this.localResourcesDict.Add(name, ab);
                return ab;
            }
            // δ����
            Internal_LoadDependencies(name);
            ab = AssetBundle.LoadFromFile(System.IO.Path.Combine(ABPath, name));

            this.localResourcesDict.Add(name, ab);

            return ab;
        }

        /// <summary>
        /// �����Ҫ����ʹ�õ���ԴΪ Global, ��Ҳ�᷵��
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AssetBundleCreateRequest LoadLocalABAsync(string name, System.Action<AsyncOperation> callback, out AssetBundle assetBundle)
        {
            name = name.ToLower();

            ABStatus abStatus = Internal_GetAB(name, out assetBundle);
            // ��ѯ Global | ��ѯ Local
            if (abStatus == ABStatus.Global || abStatus == ABStatus.Local)
            {
                callback?.Invoke(null);
                return null;
            }
            // ��ѯ Dependency
            if (abStatus == ABStatus.Dependence)
            {
                this.dependedResourcesDict.Remove(name);
                this.localResourcesDict.Add(name, assetBundle);
                callback?.Invoke(null);
                return null;
            }
            // δ����
            var ans = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(ABPath, name));
            this.localResourcesDict.Add(name, ans.assetBundle);
            assetBundle = ans.assetBundle;
            ans.completed += (asyncOpt) =>
            {
                this.localResourcesDict[name] = ans.assetBundle;
                Internal_LoadDependencies(name);
            };
            if(callback != null)
                ans.completed += callback;

            return ans;
        }


        /// <summary>
        /// �ͷ� AB�� �������Asset
        /// ֮��Ҫʹ�ñ����ٴ����� AB��
        /// </summary>
        /// <param name="name"></param>
        /// <param name="unloadAllLoadedObjects"></param>
        public void UnLoadLocalAB(string name, bool unloadAllLoadedObjects)
        {
            name = name.ToLower();

            if (this.localResourcesDict.TryGetValue(name, out AssetBundle ab))
            {
                this.localResourcesDict.Remove(name);
                Internal_UnLoadDependencies(name);
                ab.Unload(unloadAllLoadedObjects);
            }
        }

        /// <summary>
        /// �ͷ� AB�� �������Asset
        /// ֮��Ҫʹ�ñ����ٴ����� AB��
        /// </summary>
        /// <param name="name"></param>
        /// <param name="unloadAllLoadedObjects"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public AsyncOperation UnLoadLocalABAsync(string name, bool unloadAllLoadedObjects, System.Action<AsyncOperation> callback)
        {
            name = name.ToLower();

            if (this.localResourcesDict.TryGetValue(name, out AssetBundle ab))
            {
                this.localResourcesDict.Remove(name);
                var ans = ab.UnloadAsync(unloadAllLoadedObjects);
                ans.completed += callback;
                Internal_UnLoadDependencies(name);
                return ans;
            }
            return null;
        }

        /// <summary>
        /// TempIfNull => true:��������������ʱAB�� false:�������򷵻�null
        /// </summary>
        /// <param name="abname"></param>
        /// <param name="assetname"></param>
        /// <param name="TempIfNull"></param>
        /// <returns></returns>
        public Object LoadAsset(string abname, string assetname, bool TempIfNull = false)
        {
            abname = abname.ToLower();
            assetname = assetname.ToLower();

            ABStatus abStatus = Internal_GetAB(abname, out AssetBundle ab);
            if(abStatus != ABStatus.Null)
            {
                Object obj = ab.LoadAsset(assetname);
                return obj;
            }
            if (TempIfNull)
            {
                ab = AssetBundle.LoadFromFile(System.IO.Path.Combine(ABPath, abname));
                Object obj = ab.LoadAsset(assetname);
                ab.Unload(false);
                return obj;
            }
            return null;
        }




        /// <summary>
        /// TempIfNull => true:��������������ʱAB�� false:�������򷵻�null
        /// </summary>
        /// <param name="abname"></param>
        /// <param name="assetname"></param>
        /// <param name="TempIfNull"></param>
        /// <returns></returns>
        public T LoadAsset<T>(string abname, string assetname, bool TempIfNull = false) where T : Object
        {
            abname = abname.ToLower();
            assetname = assetname.ToLower();
            ABStatus abStatus = Internal_GetAB(abname, out AssetBundle ab);
            if (abStatus != ABStatus.Null)
            {
                T obj = ab.LoadAsset<T>(assetname);
                return obj;
            }
            if (TempIfNull)
            {
                ab = AssetBundle.LoadFromFile(System.IO.Path.Combine(ABPath, abname));
                T obj = ab.LoadAsset<T>(assetname);
                ab.Unload(false);
                return obj;
            }
            return null;
        }

        /// <summary>
        /// TempIfNull => true:��������������ʱAB�� false:�������򷵻�null
        /// </summary>
        /// <param name="abname"></param>
        /// <param name="assetname"></param>
        /// <param name="TempIfNull"></param>
        /// <returns></returns>
        public Object LoadAsset(string abname, string assetname, System.Type type, bool TempIfNull = false)
        {
            abname = abname.ToLower();
            assetname = assetname.ToLower();

            ABStatus abStatus = Internal_GetAB(abname, out AssetBundle ab);
            if (abStatus != ABStatus.Null)
            {
                Object obj = ab.LoadAsset(assetname, type);
                return obj;
            }
            if (TempIfNull)
            {
                ab = AssetBundle.LoadFromFile(System.IO.Path.Combine(ABPath, abname));
                Object obj = ab.LoadAsset(assetname, type);
                ab.Unload(false);
                return obj;
            }
            return null;
        }

        public AssetBundleRequest LoadAssetAsync(string abname, string assetname, System.Action<AsyncOperation> callback)
        {
            abname = abname.ToLower();
            assetname = assetname.ToLower();

            ABStatus abStatus = Internal_GetAB(abname, out AssetBundle ab);
            if (abStatus != ABStatus.Null)
            {
                var ans = ab.LoadAssetAsync(assetname);
                ans.completed += callback;
                return ans;
            }
            return null;
        }

        public AssetBundleRequest LoadAssetAsync<T>(string abname, string assetname, System.Action<AsyncOperation> callback)
        {
            abname = abname.ToLower();
            assetname = assetname.ToLower();

            ABStatus abStatus = Internal_GetAB(abname, out AssetBundle ab);
            if (abStatus != ABStatus.Null)
            {
                var ans = ab.LoadAssetAsync<T>(assetname);
                ans.completed += callback;
                return ans;
            }
            return null;
        }
        
        public AssetBundleRequest LoadAssetAsync(string abname, string assetname, System.Action<AsyncOperation> callback, System.Type type)
        {
            abname = abname.ToLower();
            assetname = assetname.ToLower();

            ABStatus abStatus = Internal_GetAB(abname, out AssetBundle ab);
            if (abStatus != ABStatus.Null)
            {
                var ans = ab.LoadAssetAsync(assetname, type);
                ans.completed += callback;
                return ans;
            }
            return null;
        }


        /// <summary>
        /// ����Ĭ�ϵ�Global AB��
        /// </summary>
        private void LoadDefaultAB()
        {
            var abs = DEFAULTAB.Split("|");
            foreach(var ab in abs)
            {
                this.LoadGlobalAB(ab.Trim());
            }
        }
#else

        #region Property |Field
        /// <summary>
        /// ͨ��ʵ����������GameObject��Handle
        /// Local -> �ڳ����л� | ���Լ���������������� | �ֶ����� -> ReleaseInstance
        /// </summary>
        private Dictionary<AsyncOperationHandle, object> localInstances;
        /// <summary>
        /// ͨ��ʵ����������GameObject��Handle
        /// Global -> �����ֶ����� ReleaseInstance
        /// </summary>
        private Dictionary<AsyncOperationHandle, object> globalInstances;
        /// <summary>
        /// ͨ��LoadAsset������ʲ���Handle
        /// Local -> �ڳ����л� | ���Լ���������������� | �ֶ����� -> Release
        /// </summary>
        private Dictionary<AsyncOperationHandle, object> localHandles;
        /// <summary>
        /// ͨ��LoadAsset������ʲ���Handle
        /// Global -> �����ֶ����� Release
        /// </summary>
        private Dictionary<AsyncOperationHandle, object> globalHandles;
        #endregion

        #region Construction
        public ABResourceManager()
        {
            this.localInstances = new Dictionary<AsyncOperationHandle, object>();
            this.globalInstances = new Dictionary<AsyncOperationHandle, object>();
            this.localHandles = new Dictionary<AsyncOperationHandle, object>();
            this.globalHandles = new Dictionary<AsyncOperationHandle, object>();

            // TODO : ʹ��Addressable���
            //Addressables.UnloadSceneAsync

            //AssetReference
            //AssetLabelReference
        }

        ~ABResourceManager()
        {
            this.ReleaseAll();
        }

        #endregion

        #region API
        #region Instance

        public AsyncOperationHandle<GameObject> InstantiateAsync(IResourceLocation location, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true, bool isGlobal = false)
        {
            var handle = Addressables.InstantiateAsync(location, parent, instantiateInWorldSpace, trackHandle);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localInstances.Add(handle, handle.Result);
                }
            };
            return handle;
        }
        public AsyncOperationHandle<GameObject> InstantiateAsync(IResourceLocation location, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true, bool isGlobal = false)
        {
            var handle = Addressables.InstantiateAsync(location, position, rotation, parent, trackHandle);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localInstances.Add(handle, handle.Result);
                }
            };
            return handle;
        }
        public AsyncOperationHandle<GameObject> InstantiateAsync(object key, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true, bool isGlobal = false)
        {
            var handle = Addressables.InstantiateAsync(key, parent, instantiateInWorldSpace, trackHandle);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localInstances.Add(handle, handle.Result);
                }
            };
            return handle;
        }
        public AsyncOperationHandle<GameObject> InstantiateAsync(object key, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true, bool isGlobal = false)
        {
            var handle = Addressables.InstantiateAsync(key, position, rotation, parent, trackHandle);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localInstances.Add(handle, handle.Result);
                }
            };
            return handle;
        }
        public AsyncOperationHandle<GameObject> InstantiateAsync(object key, InstantiationParameters instantiateParameters, bool trackHandle = true, bool isGlobal = false)
        {
            var handle = Addressables.InstantiateAsync(key, instantiateParameters, trackHandle);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localInstances.Add(handle, handle.Result);
                }
            };
            return handle;
        }
        public AsyncOperationHandle<GameObject> InstantiateAsync(IResourceLocation location, InstantiationParameters instantiateParameters, bool trackHandle = true, bool isGlobal = false)
        {
            var handle = Addressables.InstantiateAsync(location, instantiateParameters, trackHandle);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localInstances.Add(handle, handle.Result);
                }
            };
            return handle;
        }

        public bool ReleaseInstance(GameObject instance)
        {
            var b = Addressables.ReleaseInstance(instance);
            if(b)
            {
                if(this.localInstances.ContainsValue(instance))
                {
                    this.RemoveFromDictionary<AsyncOperationHandle>(this.localInstances, instance);
                }
                else if(this.globalInstances.ContainsValue(instance))
                {
                    this.RemoveFromDictionary<AsyncOperationHandle>(this.globalInstances, instance);
                }
                return true;
            }
            return false;
        }
        public bool ReleaseInstance(AsyncOperationHandle handle)
        {
            var b = Addressables.ReleaseInstance(handle);
            if (b)
            {
                this.localInstances.Remove(handle);
                this.globalInstances.Remove(handle);
                return true;
            }
            return false;
        }
        public bool ReleaseInstance(AsyncOperationHandle<GameObject> handle)
        {
            var b = Addressables.ReleaseInstance(handle);
            if (b)
            {
                this.localInstances.Remove(handle);
                this.globalInstances.Remove(handle);
                return true;
            }
            return false;
        }

        #endregion

        #region Asset
        public AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(IResourceLocation location, bool isGlobal = false)
        {
            var handle = Addressables.LoadAssetAsync<TObject>(location);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localHandles.Add(handle, handle.Result);
                }
            };
            return handle;
        }
        public AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(object key, bool isGlobal = false)
        {
            var handle = Addressables.LoadAssetAsync<TObject>(key);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localHandles.Add(handle, handle.Result);
                }
            };
            return handle;
        }

        public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IList<IResourceLocation> locations, Action<TObject> callback, bool isGlobal = false)
        {
            var handle = Addressables.LoadAssetsAsync<TObject>(locations, callback);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localHandles.Add(handle, handle.Result);
                }
            };
            return handle;
        }
        public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IList<IResourceLocation> locations, Action<TObject> callback, bool releaseDependenciesOnFailure, bool isGlobal = false)
        {
            var handle = Addressables.LoadAssetsAsync<TObject>(locations, callback, releaseDependenciesOnFailure);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localHandles.Add(handle, handle.Result);
                }
            };
            return handle;
        }
        public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IEnumerable keys, Action<TObject> callback, MergeMode mode, bool isGlobal = false)
        {
            var handle = Addressables.LoadAssetsAsync<TObject>(keys, callback, mode);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localHandles.Add(handle, handle.Result);
                }
            };
            return handle;
        }
        public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IEnumerable keys, Action<TObject> callback, MergeMode mode, bool releaseDependenciesOnFailure, bool isGlobal = false)
        {
            var handle = Addressables.LoadAssetsAsync<TObject>(keys, callback, mode, releaseDependenciesOnFailure);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localHandles.Add(handle, handle.Result);
                }
            };
            return handle;
        }
        public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(object key, Action<TObject> callback, bool isGlobal = false)
        {
            var handle = Addressables.LoadAssetsAsync<TObject>(key, callback);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localHandles.Add(handle, handle.Result);
                }
            };
            return handle;
        }
        public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(object key, Action<TObject> callback, bool releaseDependenciesOnFailure, bool isGlobal = false)
        {
            var handle = Addressables.LoadAssetsAsync<TObject>(key, callback, releaseDependenciesOnFailure);
            handle.Completed += (ash) =>
            {
                if (isGlobal)
                {
                    this.globalHandles.Add(handle, handle.Result);
                }
                else
                {
                    this.localHandles.Add(handle, handle.Result);
                }
            };
            return handle;
        }


        // �Ƽ�ʹ��Release(handle) -> ֻ�ͷ���һ��
        // Release(handle.asset) -> �ͷ�asset��Ӧ������handle
        public void Release<TObject>(TObject obj)
        {
            Addressables.Release<TObject>(obj);
            if(this.localHandles.ContainsValue(obj))
            {
                this.RemoveFromDictionary(this.localHandles, obj);
            }
            else if (this.globalHandles.ContainsValue(obj))
            {
                this.RemoveFromDictionary(this.globalHandles, obj);
            }
        }
        public void Release<TObject>(AsyncOperationHandle<TObject> handle)
        {
            Addressables.Release<TObject>(handle);
            this.localHandles.Remove(handle);
            this.globalHandles.Remove(handle);
        }
        public void Release(AsyncOperationHandle handle)
        {
            Addressables.Release(handle);
            this.localHandles.Remove(handle);
            this.globalHandles.Remove(handle);
        }
        #endregion

        #region Scene
        public AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
        }
        public AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneParameters loadSceneParameters, bool activateOnLoad = true, int priority = 100)
        {
            return Addressables.LoadSceneAsync(key, loadSceneParameters, activateOnLoad, priority);
        }
        public AsyncOperationHandle<SceneInstance> LoadSceneAsync(IResourceLocation location, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return Addressables.LoadSceneAsync(location, loadMode, activateOnLoad, priority);
        }
        public AsyncOperationHandle<SceneInstance> LoadSceneAsync(IResourceLocation location, LoadSceneParameters loadSceneParameters, bool activateOnLoad = true, int priority = 100)
        {
            return Addressables.LoadSceneAsync(location, loadSceneParameters, activateOnLoad, priority);
        }

        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(SceneInstance scene, UnloadSceneOptions unloadOptions, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(scene, unloadOptions, autoReleaseHandle);
        }
        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle handle, UnloadSceneOptions unloadOptions, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(handle, unloadOptions, autoReleaseHandle);
        }
        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(SceneInstance scene, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
        }
        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle handle, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(handle, autoReleaseHandle);
        }
        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle<SceneInstance> handle, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(handle, autoReleaseHandle);
        }
        #endregion

        public void ReleaseAllLocal()
        {
            foreach(var instance in this.localInstances)
            {
                Addressables.ReleaseInstance(instance.Key);
            }
            foreach (var instance in this.localHandles)
            {
                Addressables.ReleaseInstance(instance.Key);
            }
            this.localInstances.Clear();
            this.localHandles.Clear();
        }

        #endregion

        #region Internal
        private void ReleaseAll()
        {
            foreach (GameObject handle in this.localInstances.Values)
            {
                Addressables.ReleaseInstance(handle);
            }
            foreach (GameObject handle in this.globalInstances.Values)
            {
                Addressables.ReleaseInstance(handle);
            }
            foreach (var handle in this.localHandles.Values)
            {
                Addressables.Release(handle);
            }
            foreach (var handle in this.globalHandles.Values)
            {
                Addressables.Release(handle);
            }
        }

        void RemoveFromDictionary<K>(Dictionary<K, object> myDictionary, object valueToRemove)
        {
            // ����һ����ʱ�б����洢��Ҫ�Ƴ��ļ�
            List<K> keysToRemove = new List<K>();

            foreach (var pair in myDictionary)
            {
                if (pair.Value == valueToRemove)
                {
                    // ����Ҫ�Ƴ��ļ���ӵ���ʱ�б���
                    keysToRemove.Add(pair.Key);
                }
            }

            // ʹ����ʱ�б��еļ����Ƴ���Ӧ�ļ�ֵ��
            foreach (var key in keysToRemove)
            {
                myDictionary.Remove(key);
            }
        }
        #endregion
#endif
    }
}

/*
// �ȸ���
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.UI;

// �����²�������Դ
public class CheckUpdateAndDownload : MonoBehaviour
{
    /// <summary>
    /// ��ʾ����״̬�ͽ���
    /// </summary>
    public Text updateText;

    /// <summary>
    /// ���԰�ť
    /// </summary>
    public Button retryBtn;

    void Start()
    {
        retryBtn.gameObject.SetActive(false);
        retryBtn.onClick.AddListener(() =>
        {
            StartCoroutine(DoUpdateAddressadble());
        });

        // Ĭ���Զ�ִ��һ�θ��¼��
        StartCoroutine(DoUpdateAddressadble());
    }

    IEnumerator DoUpdateAddressadble()
    {
        AsyncOperationHandle<IResourceLocator> initHandle = Addressables.InitializeAsync();
        yield return initHandle;

        // ������
        var checkHandle = Addressables.CheckForCatalogUpdates(true);
        yield return checkHandle;
        if (checkHandle.Status != AsyncOperationStatus.Succeeded)
        {
            OnError("CheckForCatalogUpdates Error\n" +  checkHandle.OperationException.ToString());
            yield break;
        }

        if (checkHandle.Result.Count > 0)
        {
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, true);
            yield return updateHandle;

            if (updateHandle.Status != AsyncOperationStatus.Succeeded)
            {
                OnError("UpdateCatalogs Error\n" + updateHandle.OperationException.ToString());
                yield break;
            }

            // �����б������
            List<IResourceLocator> locators = updateHandle.Result;
            foreach (var locator in locators)
            {
                List<object> keys = new List<object>();
                keys.AddRange(locator.Keys);
                // ��ȡ�����ص��ļ��ܴ�С
                var sizeHandle = Addressables.GetDownloadSizeAsync(keys.GetEnumerator());
                yield return sizeHandle;
                if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    OnError("GetDownloadSizeAsync Error\n" + sizeHandle.OperationException.ToString());
                    yield break;
                }

                long totalDownloadSize = sizeHandle.Result;
                updateText.text = updateText.text + "\ndownload size : " + totalDownloadSize;
                Debug.Log("download size : " + totalDownloadSize);
                if (totalDownloadSize > 0)
                {
                    // ����
                    var downloadHandle = Addressables.DownloadDependenciesAsync(keys, true);
                    while (!downloadHandle.IsDone)
                    {
                        if (downloadHandle.Status == AsyncOperationStatus.Failed)
                        {
                            OnError("DownloadDependenciesAsync Error\n"  + downloadHandle.OperationException.ToString());
                            yield break;
                        }
                        // ���ؽ���
                        float percentage = downloadHandle.PercentComplete;
                        Debug.Log($"������: {percentage}");
                        updateText.text = updateText.text + $"\n������: {percentage}";
                        yield return null;
                    }
                    if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Debug.Log("�������!");
                        updateText.text = updateText.text + "\n�������";
                    }
                }
            }
        }
        else
        {
            updateText.text = updateText.text + "\nû�м�⵽����";
        }

        // ������Ϸ
        EnterGame();
    }

    // �쳣��ʾ
    private void OnError(string msg)
    {
        updateText.text = updateText.text + $"\n{msg}\n������! ";
        // ��ʾ���԰�ť
        retryBtn.gameObject.SetActive(true);
    }


    // ������Ϸ
    void EnterGame()
    {
        // TODO
    }
}

 */