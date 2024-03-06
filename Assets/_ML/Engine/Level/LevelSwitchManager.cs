using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ML.Engine.InventorySystem.CompositeSystem.CompositeManager;

namespace ML.Engine.Level
{
    public sealed class LevelSwitchManager :Manager.GlobalManager.IGlobalManager, Timer.ITickComponent
    {
        #region Base
        private const float gridXSize = 1000f;
        private const float gridYSize = 1000f;
        private const float gridZSize = 2000f;
        private const float gridCenterRadius = 500f;

        private struct LevelResource
        {
            public string scenename;
            public string[] globalres;
            public string[] localres;
        }

        /// <summary>
        /// 场景切换时使用的场景资源清单
        /// <LevelName, LevelResource>
        /// </summary>
        private Dictionary<string, LevelResource> levelResourcesDict = new Dictionary<string, LevelResource>();

        /// <summary>
        /// 场景内格子资源清单
        /// </summary>
        private Dictionary<Vector2Int, string[]> scenegridResourcesDict = new Dictionary<Vector2Int, string[]>();

        /// <summary>
        /// 切换场景前
        /// <preLevel, postLevel>
        /// </summary>
        public event System.Action<string, string> OnLoadLevelPre;
        /// <summary>
        /// 切换场景后
        /// <preLevel, postLevel>
        /// </summary>
        public event System.Action<string, string> OnLoadLevelPost;
        /// <summary>
        /// 场景资源调度完成后
        /// </summary>
        public event System.Action OnDispatchLevelResPost;


        public ABResources.ABResourceManager ABRManager;

        /// <summary>
        /// 当前场景名称
        /// </summary>
        public string CurSceneName { get; private set; }

        public Transform DispatchResourceTarget { get; private set; }


        public void SetDispatchResourceTarget(Transform target)
        {
            this.DispatchResourceTarget = target;
            this.dispatchGrids = new DispatchGrids(GetDRTargetToGrid());
        }
        private Vector2Int GetDRTargetToGrid()
        {
            Vector2Int ans = new Vector2Int();
            ans.Set((int)(this.DispatchResourceTarget.position.x / gridXSize), (int)(this.DispatchResourceTarget.position.y / gridYSize));
            return ans;
        }
        private bool IsInGridCenter()
        {
            return Vector3.Distance(this.DispatchResourceTarget.position, new Vector3(((this.dispatchGrids.CenterGrid.x + 0.5f) * gridXSize), ((this.dispatchGrids.CenterGrid.y + 0.5f) * gridYSize), 0)) <= gridCenterRadius;
        }

        public LevelSwitchManager(ABResources.ABResourceManager ABRManager)
        {
            this.ABRManager = ABRManager;
            // 注册Tick
            Manager.GameManager.Instance.TickManager.RegisterLateTick(-1, this);

            // 载入表数据 => 资源清单
            Manager.GameManager.Instance.StartCoroutine(this.Internal_LoadResourceList());

        }

        #endregion

        #region 场景切换
        public AsyncOperation ao = null;
        /// <summary>
        /// 异步加载场景
        /// </summary>
        public IEnumerator LoadSceneAsync(string sceneName, System.Action<string, string> preCallback = null, System.Action<string, string> postCallback = null)
        {
            yield return null;

            string preSceneName = this.CurSceneName;
            this.CurSceneName = sceneName;

            // PreCallback
            //yield return preCallback?.BeginInvoke(preSceneName, CurSceneName, null, null);
            preCallback?.Invoke(preSceneName, CurSceneName);
            // OnLoadLevelPre
            OnLoadLevelPre?.Invoke(preSceneName, CurSceneName);

            // 场景切换 && 资源调度
            var loadResource = Manager.GameManager.Instance.StartCoroutine(DispatchLevelResources(preSceneName, CurSceneName));

            // 注销所有LocalGameManager
            Manager.GameManager.Instance.UnregisterAllLocalManager();
            ao = SceneManager.LoadSceneAsync(CurSceneName, LoadSceneMode.Single);
            ao.allowSceneActivation = false;
            // 等待场景加载完成
            while (!ao.isDone)
            {
                if (Mathf.Approximately(ao.progress, 0.9f))
                {
                    break;
                }
                yield return null;
            }

            // 等待资源加载完成
            yield return loadResource;
            // PostCallback
            postCallback?.Invoke(preSceneName, CurSceneName);

            // OnLoadLevelPost
            OnLoadLevelPost?.Invoke(preSceneName, CurSceneName);

            // 调度一次场景内资源加载
            yield return Manager.GameManager.Instance.StartCoroutine(DispatchSceneResources());

            // 加载场景
            ao.allowSceneActivation = true;
            yield return ao;

            ao = null;

            // 结束此协程
            yield break;
        }



        private DispatchGrids dispatchGrids;

        public const string LevelTableDataABPath = "Json/TableData";
        public const string TableName = "LevelTableData";

        /// <summary>
        /// 载入资源清单
        /// </summary>
        private IEnumerator Internal_LoadResourceList()
        {
            while (Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }
            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(LevelTableDataABPath, null, out ab);
            yield return crequest;
            if(crequest != null)
                ab = crequest.assetBundle;


            var request = ab.LoadAssetAsync<TextAsset>(TableName);
            yield return request;
            LevelResource[] datas = JsonConvert.DeserializeObject<LevelResource[]>((request.asset as TextAsset).text);
            if(datas != null)
            {
                foreach (var row in datas)
                {
                    LevelResource res = new LevelResource();
                    // 0 => level name
                    // 1 => global
                    res.globalres = row.globalres;
                    // 2 => local
                    res.localres = row.localres;
                    this.levelResourcesDict.Add(row.scenename, res);
                }


                // 载入当前场景的资源
                this.CurSceneName = SceneManager.GetActiveScene().name;
                Manager.GameManager.Instance.StartCoroutine(this.DispatchLevelResources(null, this.CurSceneName));
            }

        }

        private void Internal_LoadGridResourcesList(string name)
        {
            this.scenegridResourcesDict.Clear();
            name = "csv/" + name.ToLower();
            var datas = ML.Engine.Utility.CSVUtils.ParseCSV(name, 1, false);
            if(datas == null)
            {
                return;
            }
            foreach (var row in datas)
            {
                // 0 => grid
                string[] g = row[0].Split('|', System.StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i < g.Length; ++i)
                {
                    g[i] = g[i].Trim();
                }
                Vector2Int grid = new Vector2Int(int.Parse(g[0]), int.Parse(g[1]));
                // 1 => local
                string[] res = row[2].Split('|', System.StringSplitOptions.RemoveEmptyEntries);
                this.scenegridResourcesDict.Add(grid, res);
            }
        }

        /// <summary>
        /// 场景切换资源调度
        /// </summary>
        private IEnumerator DispatchLevelResources(string pre, string cur)
        {
            bool resIsNotNull = this.levelResourcesDict.ContainsKey(cur);
            LevelResource resList = resIsNotNull ? this.levelResourcesDict[cur] : default;

            Queue<AssetBundleCreateRequest> a = new Queue<AssetBundleCreateRequest>();
            Queue<AsyncOperation> b = new Queue<AsyncOperation>();

            // 释放之前场景的 LocalAB
            if (pre != null && this.levelResourcesDict.TryGetValue(pre, out LevelResource preList))
            {
                foreach(var res in preList.localres)
                {
                    bool release = true;
                    if(resIsNotNull)
                    {
                        foreach (var r in resList.localres)
                        {
                            if (r == res)
                                release = false;
                        }
                    }
                    if (release)
                    {
                        b.Enqueue(this.ABRManager.UnLoadLocalABAsync(res, true, null));
                    }
                }
            }

            // 加载当前场景的资源
            AssetBundle ab;
            // Global
            if(resIsNotNull)
            {
                foreach (var res in resList.globalres)
                {
                    var t = this.ABRManager.LoadGlobalABAsync(res, null, out ab);
                    if (t != null)
                    {
                        a.Enqueue(t);
                    }
                }
                // Local
                foreach (var res in resList.localres)
                {
                    var t = this.ABRManager.LoadLocalABAsync(res, null, out ab);
                    if (t != null)
                    {
                        a.Enqueue(t);
                    }
                }
            }
           
            // 等待 Level 资源加载与释放完成
            while (a.Count > 0)
            {
                yield return a.Peek();
                a.Dequeue();
            }
            while (b.Count > 0)
            {
                yield return b.Peek();
                b.Dequeue();
            }

            // 载入当前场景内的场景资源清单
            Internal_LoadGridResourcesList(CurSceneName);
            this.OnDispatchLevelResPost?.Invoke();
            //yield return this.OnDispatchLevelResPost?.BeginInvoke(null, null);
            yield break;
        }

        #endregion

        #region 场景资源调度

        private struct DispatchGrids
        {
            public DispatchGrids(Vector2Int CurGird)
            {
                this.LoadBufferingCount = 0;
                this.ReleaseBufferingCount = 0;
                this.centerGird = CurGird;

                // 更新当前周围格子
                this.loadGrids = new Vector2Int[8];
                // 内圈
                for (int x = -1, i = 0; x < 2; ++x, ++i)
                {
                    for (int y = -1; y < 2; ++y, ++i)
                    {
                        this.loadGrids[i].Set(this.centerGird.x + x, this.centerGird.y + y);
                    }
                }
                this.waitReleaseGrids = new List<Vector2Int>(16);
                // 外圈
                // -2 -2
                this.waitReleaseGrids[0].Set(this.centerGird.x - 2, this.centerGird.y - 2);
                // -2 -1
                this.waitReleaseGrids[1].Set(this.centerGird.x - 2, this.centerGird.y - 1);
                // -2  0
                this.waitReleaseGrids[2].Set(this.centerGird.x - 2, this.centerGird.y);
                // -2  1
                this.waitReleaseGrids[3].Set(this.centerGird.x - 2, this.centerGird.y + 1);
                // -2  2
                this.waitReleaseGrids[4].Set(this.centerGird.x - 2, this.centerGird.y + 2);
                // -1 -2
                this.waitReleaseGrids[5].Set(this.centerGird.x - 1, this.centerGird.y - 2);
                // -1  2
                this.waitReleaseGrids[6].Set(this.centerGird.x - 1, this.centerGird.y + 2);
                //  0 -2
                this.waitReleaseGrids[7].Set(this.centerGird.x, this.centerGird.y - 2);
                //  0  2
                this.waitReleaseGrids[8].Set(this.centerGird.x, this.centerGird.y + 2);
                //  1 -2
                this.waitReleaseGrids[9].Set(this.centerGird.x + 1, this.centerGird.y - 2);
                //  1  2
                this.waitReleaseGrids[10].Set(this.centerGird.x + 1, this.centerGird.y + 2);
                //  2 -2
                this.waitReleaseGrids[11].Set(this.centerGird.x + 2, this.centerGird.y - 2);
                //  2 -1
                this.waitReleaseGrids[12].Set(this.centerGird.x + 2, this.centerGird.y - 1);
                //  2  0
                this.waitReleaseGrids[13].Set(this.centerGird.x + 2, this.centerGird.y);
                //  2  1
                this.waitReleaseGrids[14].Set(this.centerGird.x + 2, this.centerGird.y + 1);
                //  2  2
                this.waitReleaseGrids[15].Set(this.centerGird.x + 2, this.centerGird.y + 2);

                this.loadQueue = new Queue<Vector2Int>();
                this.releaseQueue = new Queue<Vector2Int>();
            }

            /// <summary>
            /// 正在缓存加载的资源数
            /// </summary>uffered { get; private set
            public int LoadBufferingCount { get; private set; }
            /// <summary>
            /// 正在缓存释放的资源数
            /// </summary>
            public int ReleaseBufferingCount { get; private set; }

            /// <summary>
            /// 加载缓冲队列
            /// </summary>
            private Queue<Vector2Int> loadQueue;
            /// <summary>
            /// 释放缓冲队列
            /// </summary>
            private Queue<Vector2Int> releaseQueue;

            /// <summary>
            /// 当前所在格子
            /// </summary>
            private Vector2Int centerGird;
            /// <summary>
            /// 内圈 => 载入AB
            /// </summary>
            private Vector2Int[] loadGrids;
            /// <summary>
            /// 外圈 => 等待释放
            /// </summary>
            private List<Vector2Int> waitReleaseGrids;

            /// <summary>
            /// 设置当前位于的中心格子 => 更新当前格子、内外圈格子、缓冲队列，但不进行资源加载释放
            /// </summary>
            public Vector2Int CenterGrid
            {
                get => this.centerGird;
                set
                {
                    this.centerGird = value;
                    // 更新当前周围格子
                    // 内圈
                    for (int x = -1, i = 0; x < 2; ++x, ++i)
                    {
                        for (int y = -1; y < 2; ++y, ++i)
                        {
                            this.loadGrids[i].Set(this.centerGird.x + x, this.centerGird.y + y);
                        }
                    }
                    // 外圈
                    // 旧值
                    Vector2Int[] vector2Ints = this.waitReleaseGrids.ToArray();
                    // -2 -2
                    this.waitReleaseGrids[0].Set(this.centerGird.x - 2, this.centerGird.y - 2);
                    // -2 -1
                    this.waitReleaseGrids[1].Set(this.centerGird.x - 2, this.centerGird.y - 1);
                    // -2  0
                    this.waitReleaseGrids[2].Set(this.centerGird.x - 2, this.centerGird.y);
                    // -2  1
                    this.waitReleaseGrids[3].Set(this.centerGird.x - 2, this.centerGird.y + 1);
                    // -2  2
                    this.waitReleaseGrids[4].Set(this.centerGird.x - 2, this.centerGird.y + 2);
                    // -1 -2
                    this.waitReleaseGrids[5].Set(this.centerGird.x - 1, this.centerGird.y - 2);
                    // -1  2
                    this.waitReleaseGrids[6].Set(this.centerGird.x - 1, this.centerGird.y + 2);
                    //  0 -2
                    this.waitReleaseGrids[7].Set(this.centerGird.x, this.centerGird.y - 2);
                    //  0  2
                    this.waitReleaseGrids[8].Set(this.centerGird.x, this.centerGird.y + 2);
                    //  1 -2
                    this.waitReleaseGrids[9].Set(this.centerGird.x + 1, this.centerGird.y - 2);
                    //  1  2
                    this.waitReleaseGrids[10].Set(this.centerGird.x + 1, this.centerGird.y + 2);
                    //  2 -2
                    this.waitReleaseGrids[11].Set(this.centerGird.x + 2, this.centerGird.y - 2);
                    //  2 -1
                    this.waitReleaseGrids[12].Set(this.centerGird.x + 2, this.centerGird.y - 1);
                    //  2  0
                    this.waitReleaseGrids[13].Set(this.centerGird.x + 2, this.centerGird.y);
                    //  2  1
                    this.waitReleaseGrids[14].Set(this.centerGird.x + 2, this.centerGird.y + 1);
                    //  2  2
                    this.waitReleaseGrids[15].Set(this.centerGird.x + 2, this.centerGird.y + 2);

                    // 更新当前缓冲队列 
                    this.loadQueue.Clear();
                    foreach(var grid in this.loadGrids)
                        this.loadQueue.Enqueue(grid);

                    // 缓存等待释放的资源
                    this.releaseQueue.Clear();
                    foreach (var pos in vector2Ints)
                    {
                        // 不在当前外圈 => 即将释放
                        if (!this.waitReleaseGrids.Contains(pos))
                        {
                            this.releaseQueue.Enqueue(pos);
                        }
                    }
                    // 资源加载释放由 DispatchSceneResources 调用函数完成
                }
            }

            /// <summary>
            /// 协程异步加载&释放缓冲区资源
            /// </summary>
            public IEnumerator BeginBufferAsync()
            {
                // 异步加载资源
                Vector2Int vec2;
                while ((vec2 = this.loadQueue.Dequeue()) != null)
                {
                    ++this.LoadBufferingCount;
                    Manager.GameManager.Instance.StartCoroutine(Manager.GameManager.Instance.LevelSwitchManager.LoadSceneGridResource(vec2, LoadBufferCallback));
                }
                // 异步释放资源
                while ((vec2 = this.releaseQueue.Dequeue()) != null)
                {
                    ++this.ReleaseBufferingCount;
                    Manager.GameManager.Instance.StartCoroutine(Manager.GameManager.Instance.LevelSwitchManager.ReleaseSceneGridResource(vec2, ReleaseBufferCallback));
                }
                yield break;
            }

            private void LoadBufferCallback()
            {
                --this.LoadBufferingCount;
            }

            private void ReleaseBufferCallback()
            {
                --this.ReleaseBufferingCount;
            }
        }

        /// <summary>
        /// 场景内资源调度
        /// </summary>
        private IEnumerator DispatchSceneResources()
        {
            if(this.DispatchResourceTarget == null)
            {
                yield break;
            }
            // 获取当前位置
            Vector2Int pos = this.GetDRTargetToGrid();
            // 格子位置有更新
            if(pos != this.dispatchGrids.CenterGrid)
            {
                // 更新位置以及缓冲队列
                this.dispatchGrids.CenterGrid = pos;
            }
            // 处于格子中心位置 => 场景内资源调度
            if (this.IsInGridCenter())
            {
                this.dispatchGrids.BeginBufferAsync();
            }
        }

        /// <summary>
        /// 载入格子内资源
        /// </summary>
        /// <param name="vec2"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator LoadSceneGridResource(Vector2Int vec2, System.Action callback)
        {
            if(this.scenegridResourcesDict.TryGetValue(vec2, out string[] rlist))
            {
                Queue<AssetBundleCreateRequest> assetBundleCreateRequests = new Queue<AssetBundleCreateRequest>();
                AssetBundle ab;
                foreach(string res in rlist)
                {
                    var t = this.ABRManager.LoadLocalABAsync(res, null, out ab);
                    if (t != null)
                    {
                        assetBundleCreateRequests.Enqueue(t);
                    }
                }
                while(assetBundleCreateRequests.Count > 0)
                {
                    yield return assetBundleCreateRequests.Peek();
                    assetBundleCreateRequests.Dequeue();
                }
            }

            callback?.Invoke();
            yield break;
        }
        /// <summary>
        /// 释放格子内资源
        /// </summary>
        /// <param name="vec2"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator ReleaseSceneGridResource(Vector2Int vec2, System.Action callback)
        {
            if (this.scenegridResourcesDict.TryGetValue(vec2, out string[] rlist))
            {
                Queue<AsyncOperation> asyncOperations = new Queue<AsyncOperation>();
                foreach (string res in rlist)
                {
                    var t = this.ABRManager.UnLoadLocalABAsync(res, true, null);
                    if (t != null)
                    {
                        asyncOperations.Enqueue(t);
                    }
                }
                while (asyncOperations.Count > 0)
                {
                    yield return asyncOperations.Peek();
                    asyncOperations.Dequeue();
                }
            }
            callback?.Invoke();
            yield break;
        }

        #region ITickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void LateTick(float deltatime)
        {
            this.DispatchSceneResources();
        }
        #endregion
        #endregion
    }
}

