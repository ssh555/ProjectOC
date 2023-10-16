using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ML.Engine.Level
{
    public sealed class LevelSwitchManager :Manager.GlobalManager.IGlobalManager, Timer.ITickComponent
    {
        #region Base
        private const float gridXSize = 1000f;
        private const float gridYSize = 1000f;
        private const float gridZSize = 2000f;
        private const float gridCenterRadius = 500f;
        private const string levelResPath = "csv/LevelResources";

        private struct LevelResource
        {
            public string[] globalResourceList;
            public string[] locallResourceList;
        }

        /// <summary>
        /// �����л�ʱʹ�õĳ�����Դ�嵥
        /// <LevelName, LevelResource>
        /// </summary>
        private Dictionary<string, LevelResource> levelResourcesDict = new Dictionary<string, LevelResource>();

        /// <summary>
        /// �����ڸ�����Դ�嵥
        /// </summary>
        private Dictionary<Vector2Int, string[]> scenegridResourcesDict = new Dictionary<Vector2Int, string[]>();

        /// <summary>
        /// �л�����ǰ
        /// <preLevel, postLevel>
        /// </summary>
        public event System.Action<string, string> OnLoadLevelPre;
        /// <summary>
        /// �л�������
        /// <preLevel, postLevel>
        /// </summary>
        public event System.Action<string, string> OnLoadLevelPost;
        /// <summary>
        /// ������Դ������ɺ�
        /// </summary>
        public event System.Action OnDispatchLevelResPost;


        public ABResources.ABResourceManager ABRManager;

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
            // ע��Tick
            Manager.GameManager.Instance.TickManager.RegisterLateTick(-1, this);

            // ��������� => ��Դ�嵥
            this.Internal_LoadResourceList();
            // ���뵱ǰ��������Դ
            this.CurSceneName = SceneManager.GetActiveScene().name;
            Manager.GameManager.Instance.StartCoroutine(this.DispatchLevelResources(null, this.CurSceneName));
        }

        #endregion

        #region �����л�
        private AsyncOperation ao = null;
        /// <summary>
        /// �첽���س���
        /// </summary>
        public IEnumerator LoadSceneAsync(string sceneName, System.Action<string, string> preCallback = null, System.Action<string, string> postCallback = null)
        {
            string preSceneName = this.CurSceneName;
            this.CurSceneName = sceneName;

            // PreCallback
            //yield return preCallback?.BeginInvoke(preSceneName, CurSceneName, null, null);
            preCallback?.Invoke(preSceneName, CurSceneName);
            // OnLoadLevelPre
            OnLoadLevelPre?.Invoke(preSceneName, CurSceneName);

            // �����л� && ��Դ����
            var loadResource = Manager.GameManager.Instance.StartCoroutine(DispatchLevelResources(preSceneName, CurSceneName));

            ao = SceneManager.LoadSceneAsync(CurSceneName, LoadSceneMode.Single);
            ao.allowSceneActivation = false;
            // �ȴ������������
            while (!ao.isDone)
            {
                if (Mathf.Approximately(ao.progress, 0.9f))
                {
                    break;
                }
                yield return null;
            }

            // �ȴ���Դ�������
            yield return loadResource;
            // PostCallback
            postCallback?.Invoke(preSceneName, CurSceneName);

            // OnLoadLevelPost
            OnLoadLevelPost?.Invoke(preSceneName, CurSceneName);

            // ����һ�γ�������Դ����
            yield return Manager.GameManager.Instance.StartCoroutine(DispatchSceneResources());

            // ���س���
            ao.allowSceneActivation = true;
            yield return ao;

            ao = null;

            // ������Э��
            yield break;
        }



        private DispatchGrids dispatchGrids;

        /// <summary>
        /// ������Դ�嵥
        /// </summary>
        private void Internal_LoadResourceList()
        {
            var datas = ML.Engine.Utility.CSVUtils.ParseCSV(levelResPath, 1);
            foreach(var row in datas)
            {
                LevelResource res = new LevelResource();
                // 0 => level name
                // 1 => global
                res.globalResourceList = row[1].Split('|', System.StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i < res.globalResourceList.Length; ++i)
                {
                    res.globalResourceList[i] = res.globalResourceList[i].Trim();
                }
                // 2 => local
                res.locallResourceList = row[2].Split('|', System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < res.locallResourceList.Length; ++i)
                {
                    res.locallResourceList[i] = res.locallResourceList[i].Trim();
                }
                this.levelResourcesDict.Add(row[0], res);
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
        /// �����л���Դ����
        /// </summary>
        private IEnumerator DispatchLevelResources(string pre, string cur)
        {
            LevelResource resList = this.levelResourcesDict[cur];

            Queue<AssetBundleCreateRequest> a = new Queue<AssetBundleCreateRequest>();
            Queue<AsyncOperation> b = new Queue<AsyncOperation>();

            // �ͷ�֮ǰ������ LocalAB
            if (pre != null && this.levelResourcesDict.TryGetValue(pre, out LevelResource preList))
            {
                foreach(var res in preList.locallResourceList)
                {
                    bool release = true;
                    foreach(var r in resList.locallResourceList)
                    {
                        if (r == res)
                            release = false;
                    }
                    if (release)
                    {
                        b.Enqueue(this.ABRManager.UnLoadLocalABAsync(res, true, null));
                    }
                }
            }

            // ���ص�ǰ��������Դ
            AssetBundle ab;
            // Global
            foreach (var res in resList.globalResourceList)
            {
                var t = this.ABRManager.LoadGlobalABAsync(res, null, out ab);
                if(t != null)
                {
                    a.Enqueue(t);
                }
            }
            // Local
            foreach (var res in resList.locallResourceList)
            {
                var t = this.ABRManager.LoadLocalABAsync(res, null, out ab);
                if (t != null)
                {
                    a.Enqueue(t);
                }
            }
           
            // �ȴ� Level ��Դ�������ͷ����
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

            // ���뵱ǰ�����ڵĳ�����Դ�嵥
            Internal_LoadGridResourcesList(CurSceneName);
            this.OnDispatchLevelResPost?.Invoke();
            //yield return this.OnDispatchLevelResPost?.BeginInvoke(null, null);
            yield break;
        }

        #endregion

        #region ������Դ����

        private struct DispatchGrids
        {
            public DispatchGrids(Vector2Int CurGird)
            {
                this.LoadBufferingCount = 0;
                this.ReleaseBufferingCount = 0;
                this.centerGird = CurGird;

                // ���µ�ǰ��Χ����
                this.loadGrids = new Vector2Int[8];
                // ��Ȧ
                for (int x = -1, i = 0; x < 2; ++x, ++i)
                {
                    for (int y = -1; y < 2; ++y, ++i)
                    {
                        this.loadGrids[i].Set(this.centerGird.x + x, this.centerGird.y + y);
                    }
                }
                this.waitReleaseGrids = new List<Vector2Int>(16);
                // ��Ȧ
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
            /// ���ڻ�����ص���Դ��
            /// </summary>uffered { get; private set
            public int LoadBufferingCount { get; private set; }
            /// <summary>
            /// ���ڻ����ͷŵ���Դ��
            /// </summary>
            public int ReleaseBufferingCount { get; private set; }

            /// <summary>
            /// ���ػ������
            /// </summary>
            private Queue<Vector2Int> loadQueue;
            /// <summary>
            /// �ͷŻ������
            /// </summary>
            private Queue<Vector2Int> releaseQueue;

            /// <summary>
            /// ��ǰ���ڸ���
            /// </summary>
            private Vector2Int centerGird;
            /// <summary>
            /// ��Ȧ => ����AB
            /// </summary>
            private Vector2Int[] loadGrids;
            /// <summary>
            /// ��Ȧ => �ȴ��ͷ�
            /// </summary>
            private List<Vector2Int> waitReleaseGrids;

            /// <summary>
            /// ���õ�ǰλ�ڵ����ĸ��� => ���µ�ǰ���ӡ�����Ȧ���ӡ�������У�����������Դ�����ͷ�
            /// </summary>
            public Vector2Int CenterGrid
            {
                get => this.centerGird;
                set
                {
                    this.centerGird = value;
                    // ���µ�ǰ��Χ����
                    // ��Ȧ
                    for (int x = -1, i = 0; x < 2; ++x, ++i)
                    {
                        for (int y = -1; y < 2; ++y, ++i)
                        {
                            this.loadGrids[i].Set(this.centerGird.x + x, this.centerGird.y + y);
                        }
                    }
                    // ��Ȧ
                    // ��ֵ
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

                    // ���µ�ǰ������� 
                    this.loadQueue.Clear();
                    foreach(var grid in this.loadGrids)
                        this.loadQueue.Enqueue(grid);

                    // ����ȴ��ͷŵ���Դ
                    this.releaseQueue.Clear();
                    foreach (var pos in vector2Ints)
                    {
                        // ���ڵ�ǰ��Ȧ => �����ͷ�
                        if (!this.waitReleaseGrids.Contains(pos))
                        {
                            this.releaseQueue.Enqueue(pos);
                        }
                    }
                    // ��Դ�����ͷ��� DispatchSceneResources ���ú������
                }
            }

            /// <summary>
            /// Э���첽����&�ͷŻ�������Դ
            /// </summary>
            public IEnumerator BeginBufferAsync()
            {
                // �첽������Դ
                Vector2Int vec2;
                while ((vec2 = this.loadQueue.Dequeue()) != null)
                {
                    ++this.LoadBufferingCount;
                    Manager.GameManager.Instance.StartCoroutine(Manager.GameManager.Instance.LevelSwitchManager.LoadSceneGridResource(vec2, LoadBufferCallback));
                }
                // �첽�ͷ���Դ
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
        /// ��������Դ����
        /// </summary>
        private IEnumerator DispatchSceneResources()
        {
            if(this.DispatchResourceTarget == null)
            {
                yield break;
            }
            // ��ȡ��ǰλ��
            Vector2Int pos = this.GetDRTargetToGrid();
            // ����λ���и���
            if(pos != this.dispatchGrids.CenterGrid)
            {
                // ����λ���Լ��������
                this.dispatchGrids.CenterGrid = pos;
            }
            // ���ڸ�������λ�� => ��������Դ����
            if (this.IsInGridCenter())
            {
                this.dispatchGrids.BeginBufferAsync();
            }
        }

        /// <summary>
        /// �����������Դ
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
        /// �ͷŸ�������Դ
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

