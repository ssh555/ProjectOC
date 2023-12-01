using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.ABResources
{
    /// <summary>
    /// Ŀǰ������ Windows
    /// AB��Ŀǰ���� StreamingAssets/Windows ��
    /// </summary>
    public sealed class ABResourceManager : Manager.GlobalManager.IGlobalManager
    {
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
                return null;
            }
            // ��ѯ Dependency
            if (abStatus == ABStatus.Dependence)
            {
                this.dependedResourcesDict.Remove(name);
                this.localResourcesDict.Add(name, assetBundle);
                return null;
            }
            // δ����
            var ans = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(ABPath, name));
            this.localResourcesDict.Add(name, ans.assetBundle);
            ans.completed += (asyncOpt) =>
            {
                this.localResourcesDict[name] = ans.assetBundle;
                Internal_LoadDependencies(name);
            };
            if(callback != null)
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
    }
}
