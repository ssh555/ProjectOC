using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.ABResources
{
    /// <summary>
    /// 目前仅考虑 Windows
    /// AB包目前均在 StreamingAssets/Windows 下
    /// </summary>
    public sealed class ABResourceManager : Manager.GlobalManager.IGlobalManager
    {
        /// <summary>
        /// 初始化此项时默认载入的Global AB
        /// 用 | 隔开
        /// </summary>
        #if UNITY_EDITOR
        private const string DEFAULTAB = "scene|testscene";
        #else
        private const string DEFAULTAB = "scene";
        #endif

        /// <summary>
        /// Windows 下 AB 包所在路径
        /// </summary>
        private readonly static string WindowsPath = Application.streamingAssetsPath + "/Windows";

        /// <summary>
        /// 实际使用路径
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
        /// 主包
        /// </summary>
        private static AssetBundle mainAB;
        /// <summary>
        /// 主包Mainifest
        /// </summary>
        private static AssetBundleManifest mainABManifest;

        public ABResourceManager()
        {
            // 载入 AB 主包
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
        /// 内部使用的AB依赖包结构体
        /// </summary>
        private struct DependedAB
        {
            /// <summary>
            /// 引用计数
            /// </summary>
            public int referCount;
            /// <summary>
            /// 依赖AB包
            /// </summary>
            public AssetBundle dependedAB;
        }

        /// <summary>
        /// 全局资源AB包
        /// <namePath, AssetBundle>
        /// 必须手动释放
        /// 最高层级
        /// </summary>
        private Dictionary<string, AssetBundle> globalResourcesDict = new Dictionary<string, AssetBundle>();

        /// <summary>
        /// 局部资源AB包
        /// <namePath, AssetBundle>
        /// 场景切换时自动释放
        /// 次层级 -> LoadGlobal时升级为 Global层级
        /// </summary>
        private Dictionary<string, AssetBundle> localResourcesDict = new Dictionary<string, AssetBundle>();

        /// <summary>
        /// 依赖资源AB包
        /// 随依赖主体变更引用次数
        /// 引用次数为0时自动释放
        /// 最低层级 -> Load时升级为 Global/Local 层级
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
            // 查询 Global
            if (this.globalResourcesDict.ContainsKey(name))
            {
                return ABStatus.Global;
            }
            // 查询 Local
            if (this.localResourcesDict.ContainsKey(name))
            {
                return ABStatus.Local;
            }
            // 查询 Dependency
            if (this.dependedResourcesDict.ContainsKey(name))
            {
                return ABStatus.Dependence;
            }
            return ABStatus.Null;
        }

        private ABStatus Internal_GetAB(string name, out AssetBundle ab)
        {
            name = name.ToLower();
            // 查询 Global
            if (this.globalResourcesDict.TryGetValue(name, out ab))
            {
                return ABStatus.Global;
            }
            // 查询 Local
            if (this.localResourcesDict.TryGetValue(name, out ab))
            {
                return ABStatus.Local;
            }
            // 查询 Dependency
            if (this.dependedResourcesDict.TryGetValue(name, out DependedAB dependedAB))
            {
                ab = dependedAB.dependedAB;
                return ABStatus.Dependence;
            }
            return ABStatus.Null;
        }

        /// <summary>
        /// 同步载入
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
                // 查询是否已载入依赖包
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
        ///// 同步卸载
        ///// </summary>
        ///// <param name="abname"></param>
        //private void Internal_UnLoadDependencies(string abname)
        //{

        //}

        /// <summary>
        /// 异步卸载 => 仅限dependedResourcesDict的依赖包
        /// Local和Global中的依赖需要额外手动释放
        /// </summary>
        /// <param name="abname"></param>
        private void Internal_UnLoadDependencies(string abname)
        {
            abname = abname.ToLower();

            string[] depenceies = mainABManifest.GetAllDependencies(abname);

            for (int i = 0; i < depenceies.Length; ++i)
            {
                ABStatus abStatus = Internal_GetAB(depenceies[i], out AssetBundle ab);
                // 查询是否已载入依赖包
                if (abStatus == ABStatus.Dependence)
                {
                    // 减引用次数
                    var dab = this.dependedResourcesDict[depenceies[i]];
                    dab.referCount--;
                    if(dab.referCount == 0)
                    {
                        this.dependedResourcesDict.Remove(depenceies[i]);
                    }
                    // 异步卸载
                    ab.UnloadAsync(true);
                }
            }
        }

        /// <summary>
        /// 同步载入 GlobalAB
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AssetBundle LoadGlobalAB(string name)
        {
            name = name.ToLower();

            ABStatus abStatus = Internal_GetAB(name, out AssetBundle ab);
            // 查询 Global
            if (abStatus == ABStatus.Global)
            {
                return ab;
            }
            // 查询 Local
            if (abStatus == ABStatus.Local)
            {
                this.localResourcesDict.Remove(name);
                this.globalResourcesDict.Add(name, ab);
                return ab;
            }
            // 查询 Dependency
            if (abStatus == ABStatus.Dependence)
            {
                this.dependedResourcesDict.Remove(name);
                this.globalResourcesDict.Add(name, ab);
                return ab;
            }

            Internal_LoadDependencies(name);

            // 未载入
            ab = AssetBundle.LoadFromFile(System.IO.Path.Combine(ABPath, name));


            this.globalResourcesDict.Add(name, ab);
            return ab;
        }
        
        /// <summary>
        /// 返回值为null则表示已经加载进入AB包 => 返回值为 assetBundle
        /// 返回值不为null则表示未加载进入AB包 => 使用常规异步回调等方法获取对应 AssetBundle
        /// to-do : [但会同步载入依赖包? => if completed不能载入的话]
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="assetBundle"></param>
        /// <returns></returns>
        public AssetBundleCreateRequest LoadGlobalABAsync(string name, System.Action<AsyncOperation> callback, out AssetBundle assetBundle)
        {
            name = name.ToLower();

            ABStatus abStatus = Internal_GetAB(name, out assetBundle);
            // 查询 Global
            if (abStatus == ABStatus.Global)
            {
                return null;
            }
            // 查询 Local
            if (abStatus == ABStatus.Local)
            {
                this.localResourcesDict.Remove(name);
                this.globalResourcesDict.Add(name, assetBundle);
                return null;
            }
            // 查询 Dependency
            if (abStatus == ABStatus.Dependence)
            {
                this.dependedResourcesDict.Remove(name);
                this.globalResourcesDict.Add(name, assetBundle);
                return null;
            }

            // 未载入
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
        /// 释放 AB包 及其相关Asset
        /// 之后要使用必须再次载入 AB包
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
        /// 释放 AB包 及其相关Asset
        /// 之后要使用必须再次载入 AB包
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
        /// 如果需要载入使用的资源为 Global, 则也会返回
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AssetBundle LoadLocalAB(string name)
        {
            name = name.ToLower();

            ABStatus abStatus = Internal_GetAB(name, out AssetBundle ab);
            // 查询 Global | 查询 Local
            if (abStatus == ABStatus.Global || abStatus == ABStatus.Local)
            {
                return ab;
            }
            // 查询 Dependency
            if (abStatus == ABStatus.Dependence)
            {
                this.dependedResourcesDict.Remove(name);
                this.localResourcesDict.Add(name, ab);
                return ab;
            }
            // 未载入
            Internal_LoadDependencies(name);
            ab = AssetBundle.LoadFromFile(System.IO.Path.Combine(ABPath, name));

            this.localResourcesDict.Add(name, ab);

            return ab;
        }

        /// <summary>
        /// 如果需要载入使用的资源为 Global, 则也会返回
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AssetBundleCreateRequest LoadLocalABAsync(string name, System.Action<AsyncOperation> callback, out AssetBundle assetBundle)
        {
            name = name.ToLower();

            ABStatus abStatus = Internal_GetAB(name, out assetBundle);
            // 查询 Global | 查询 Local
            if (abStatus == ABStatus.Global || abStatus == ABStatus.Local)
            {
                return null;
            }
            // 查询 Dependency
            if (abStatus == ABStatus.Dependence)
            {
                this.dependedResourcesDict.Remove(name);
                this.localResourcesDict.Add(name, assetBundle);
                return null;
            }
            // 未载入
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
        /// 释放 AB包 及其相关Asset
        /// 之后要使用必须再次载入 AB包
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
        /// 释放 AB包 及其相关Asset
        /// 之后要使用必须再次载入 AB包
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
        /// TempIfNull => true:不存在则载入临时AB包 false:不存在则返回null
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
        /// TempIfNull => true:不存在则载入临时AB包 false:不存在则返回null
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
        /// TempIfNull => true:不存在则载入临时AB包 false:不存在则返回null
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
        /// 载入默认的Global AB包
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
