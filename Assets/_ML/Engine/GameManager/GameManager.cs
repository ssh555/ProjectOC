using ML.Engine.TextContent;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using ML.PlayerCharacterNS;
using UnityEngine;


namespace ML.Engine.Manager
{
    public sealed class GameManager : MonoBehaviour
    {
        #region static
        /// <summary>
        /// ����
        /// </summary>
        public static GameManager Instance;
        #endregion

        #region ���õ�Manager
        /// <summary>
        /// ���õ�AB����ԴManager
        /// </summary>
        public ABResources.ABResourceManager ABResourceManager { get; private set; }
        
        /// <summary>
        /// ���õ�Level&Scene Manager
        /// </summary>
        public Level.LevelSwitchManager LevelSwitchManager { get; private set; }

        /// <summary>
        /// ���õļ�ʱ��Manager
        /// </summary>
        [ShowInInspector, ReadOnly]
        public Timer.CounterDownTimerManager CounterDownTimerManager { get; private set; }

        /// <summary>
        /// ���õ� TickManager(��Ӧ��unity�е�Update
        /// </summary>
        [ShowInInspector, ReadOnly]
        public Timer.TickManager TickManager { get; private set; }

        /// <summary>
        /// ���õ�ȫ��UIManager
        /// </summary>
        [ShowInInspector, ReadOnly]
        public UI.UIManager UIManager { get; private set; }

        /// <summary>
        /// ���õ�ȫ��InputManager
        /// </summary>
        public Input.InputManager InputManager { get; private set; }

        [ShowInInspector, ReadOnly]
        public SaveSystem.SaveManager SaveManager { get; private set; }

        [ShowInInspector, ReadOnly]
        public Event.EventManager EventManager { get; private set; }

        public EnterPoint EnterPoint { get; private set; }

        [SerializeField]
        private CharacterManager _character;

        public CharacterManager CharacterManager => _character;
        #endregion

        #region ��������
        /// <summary>
        /// ��������
        /// </summary>
        private void Awake()
        {
            if(Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            this.Init();
            EnterPoint = new EnterPoint();
        }

        /// <summary>
        /// ������ʼ��
        /// </summary>
        private void Init()
        {
            this.globalManagers = new List<GlobalManager.IGlobalManager>();
            this.localManagers = new List<LocalManager.ILocalManager>();

            this.CounterDownTimerManager = this.RegisterGlobalManager<Timer.CounterDownTimerManager>();
            this.TickManager = this.RegisterGlobalManager<Timer.TickManager>();
            this.ABResourceManager = this.RegisterGlobalManager<ABResources.ABResourceManager>();
            this.LevelSwitchManager = new Level.LevelSwitchManager(this.ABResourceManager);
            this.RegisterGlobalManager(this.LevelSwitchManager);

            this.UIManager = this.RegisterGlobalManager<UI.UIManager>();
            this.InputManager = this.RegisterGlobalManager<Input.InputManager>();

            this.SaveManager = this.RegisterGlobalManager<SaveSystem.SaveManager>();
            this.EventManager = this.RegisterGlobalManager<Event.EventManager>();
            this.RegisterGlobalManager(this.CharacterManager);
        }
        
        private void OnDestroy()
        {
            if (this == Instance)
            {
                Instance = null;
            }
        }
        #endregion

        #region Manager����
        #region GlobalManager
        private List<GlobalManager.IGlobalManager> globalManagers;
        /// <summary>
        /// ʵ������ע�����GlobalManager
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T RegisterGlobalManager<T>() where T : class, GlobalManager.IGlobalManager, new()
        {
            T manager = new T();
            this.globalManagers.Add(manager);
            manager.OnRegister();
            return manager;
        }

        /// <summary>
        /// ע���Ѵ��ڵ�GlobalManager
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public bool RegisterGlobalManager(GlobalManager.IGlobalManager manager)
        {
            if (this.globalManagers.Contains(manager))
            {
                return false;
            }
            this.globalManagers.Add(manager);
            manager.OnRegister();
            return true;
        }
        
        /// <summary>
        /// ע��һ��T���͵�GlobalManager
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T UnregisterGlobalManager<T>() where T : class, GlobalManager.IGlobalManager, new()
        {
            T manager = null;
            foreach(var m in this.globalManagers)
            {
                // to-do : �����ж������⣬��ȷ���޸�
                if(m.GetType() == typeof(T))
                {
                    manager = (T)m;
                    break;
                }
            }
            this.globalManagers.Remove(manager);
            if (manager != null)
                manager.OnUnregister();
            return manager;
        }

        /// <summary>
        /// ע������T���͵�GlobalManager
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> UnregisterAllGlobalManager<T>() where T : class, GlobalManager.IGlobalManager, new()
        {
            List<T> managers = null;
            foreach (var m in this.globalManagers)
            {
                // to-do : �����ж������⣬��ȷ���޸�
                if (m.GetType() == typeof(T))
                {
                    if(managers == null)
                    {
                        managers = new List<T>();
                    }
                    managers.Add((T)m);
                    m.OnUnregister();
                }
            }
            this.globalManagers.RemoveAll(manager => managers.Contains((T)manager));
            return managers;
        }
        
        /// <summary>
        /// ��ȡ��һ��ƥ��� GlobalManager
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetGlobalManager<T>() where T : class, GlobalManager.IGlobalManager, new()
        {
            T manager = null;
            foreach (var m in this.globalManagers)
            {
                // to-do : �����ж������⣬��ȷ���޸�
                if (m.GetType() == typeof(T))
                {
                    manager = (T)m;
                    break;
                }
            }
            return manager;
        }

        /// <summary>
        /// ��ȡ����ƥ��� GlobalManager
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetGlobalManagers<T>() where T : class, GlobalManager.IGlobalManager, new()
        {
            List<T> managers = null;
            foreach (var m in this.globalManagers)
            {
                // to-do : �����ж������⣬��ȷ���޸�
                if (m.GetType() == typeof(T))
                {
                    if (managers == null)
                    {
                        managers = new List<T>();
                    }
                    managers.Add((T)m);
                }
            }
            return managers;
        }
        #endregion

        #region LocalManager
        private List<LocalManager.ILocalManager> localManagers;
        /// <summary>
        /// ʵ������ע�����LocalManager
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T RegisterLocalManager<T>() where T : class, LocalManager.ILocalManager, new()
        {
            T manager = new T();
            this.localManagers.Add(manager);
            manager.OnRegister();
            return manager;
        }

        /// <summary>
        /// ע���Ѵ��ڵ�LocalManager
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public bool RegisterLocalManager(LocalManager.ILocalManager manager)
        {
            if (this.localManagers.Contains(manager))
            {
                return false;
            }
            this.localManagers.Add(manager);
            manager.OnRegister();
            return true;
        }

        /// <summary>
        /// ע��һ��T���͵�LocalManager
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T UnregisterLocalManager<T>() where T : class, LocalManager.ILocalManager, new()
        {
            T manager = null;
            foreach (var m in this.localManagers)
            {
                // to-do : �����ж������⣬��ȷ���޸�
                if (m.GetType() == typeof(T))
                {
                    manager = (T)m;
                    break;
                }
            }
            this.localManagers.Remove(manager);
            if (manager != null)
                manager.OnUnregister();
            return manager;
        }

        /// <summary>
        /// ע������T���͵�LocalManager
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> UnregisterAllLocalManager<T>() where T : class, LocalManager.ILocalManager, new()
        {
            List<T> managers = null;
            foreach (var m in this.localManagers)
            {
                // to-do : �����ж������⣬��ȷ���޸�
                if (m.GetType() == typeof(T))
                {
                    if (managers == null)
                    {
                        managers = new List<T>();
                    }
                    managers.Add((T)m);
                    m.OnUnregister();
                }
            }
            this.localManagers.RemoveAll(manager => managers.Contains((T)manager));
            return managers;
        }

        public void UnregisterAllLocalManager()
        {
            this.localManagers.ForEach(m => m.OnUnregister());
            this.localManagers.Clear();
        }

        /// <summary>
        /// ��ȡ��һ��ƥ��� LocalManager
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetLocalManager<T>() where T : class, LocalManager.ILocalManager, new()
        {
            T manager = null;
            foreach (var m in this.localManagers)
            {
                // to-do : �����ж������⣬��ȷ���޸�
                if (m.GetType() == typeof(T))
                {
                    manager = (T)m;
                    break;
                }
            }
            return manager;
        }

        /// <summary>
        /// ��ȡ����ƥ��� LocalManager
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetLocalManagers<T>() where T : class, LocalManager.ILocalManager, new()
        {
            List<T> managers = null;
            foreach (var m in this.localManagers)
            {
                // to-do : �����ж������⣬��ȷ���޸�
                if (m.GetType() == typeof(T))
                {
                    if (managers == null)
                    {
                        managers = new List<T>();
                    }
                    managers.Add((T)m);
                }
            }
            return managers;
        }

        #endregion
        #endregion

        #region Update
        private void Update()
        {
            this.TickManager.UpdateTickComponentList();

            if (IsPause)
            {
            }
            else
            {
                // �ȸ��¼�ʱ��
                this.CounterDownTimerManager.Update(Time.deltaTime);

                this.TickManager.Tick(Time.deltaTime);

            }
        }

        private void FixedUpdate()
        {
            this.TickManager.UpdateFixedTickComponentList();

            if (IsPause)
            {
            }
            else
            {
                // �ȸ��¼�ʱ��
                this.CounterDownTimerManager.FixedUpdate(Time.fixedDeltaTime);

                this.TickManager.FixedTick(Time.fixedDeltaTime);

            }
        }

        private void LateUpdate()
        {
            this.CounterDownTimerManager.LateUpdate(Time.deltaTime);
            this.TickManager.UpdateLateTickComponentList();

            if (IsPause)
            {
            }
            else
            {
                this.TickManager.LateTick(Time.deltaTime);
            }
        }
        #endregion
        
        #region GameTimeRate
        /// <summary>
        /// �Ƿ�ȫ����ͣ
        /// </summary>
        public bool IsPause { get; private set; } = false;

        /// <summary>
        /// ������Ϸʱ��
        /// 0 Ϊ��ͣ
        /// </summary>
        /// <param name="rate"></param>
        public void SetAllGameTimeRate(float rate)
        {
            Time.timeScale = rate;
            IsPause = rate < float.Epsilon;
        }

        /// <summary>
        /// ����Tick��ʱ������
        /// </summary>
        /// <param name="rate"></param>
        public void SetTickTimeRate(float rate)
        {
            this.TickManager.TimeScale = rate;
        }

        public void SetCDTimerRate(float rate)
        {
            this.CounterDownTimerManager.TimeScale = rate;
        }

        #endregion

        #region Destroy
        /// <summary>
        /// SetEnablelTrueΪtrueʱ���Ὣobj���������mono.enabled����Ϊtrue��Ȼ������ʱ�ͻ��Զ�����OnDestroy
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="SetEnablelTrue"></param>
        /// <param name="t"></param>
        public static void DestroyObj(Object obj, bool SetEnablelTrue = true, float t = 0.0f)
        {
            if(SetEnablelTrue)
            {
                if (obj is MonoBehaviour)
                {
                    MonoBehaviour mono = obj as MonoBehaviour;
                    mono.enabled = true;
                }
                else if (obj is GameObject)
                {
                    GameObject go = obj as GameObject;
                    go.SetActive(true);
                    // ��ȡ��Ϸ�����ϵ����� MonoBehaviour
                    MonoBehaviour[] monos = go.GetComponents<MonoBehaviour>();

                    // �������� MonoBehaviour
                    foreach (MonoBehaviour mono in monos)
                    {
                        // ���� MonoBehaviour
                        mono.enabled = true;
                    }
                }

            }

            if (obj is GameObject)
            {
                GameObject go = obj as GameObject;
                Instance.ABResourceManager.ReleaseInstance(go);
            }

             Object.Destroy(obj, t);
        }

        #endregion

        #region Config
        [LabelText("����"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Config.Language language = Config.Language.Chinese;
        [LabelText("ƽ̨"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Config.Platform platform = Config.Platform.Windows;
        [LabelText("�����豸"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Config.InputDevice inputDevice = Config.InputDevice.Keyboard;
        [LabelText("�汾��"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Version version;
        #endregion

        #region Version
        [System.Serializable, LabelText("�汾��")]
        public struct Version
        {
            [LabelText("���汾��")]
            public int Major;
            [LabelText("�ΰ汾��")]
            public int Minor;
            [LabelText("�޶��汾��")]
            public int Patch;

            public Version(string version)
            {
                string[] versions = version.Split('.');
                Major = int.Parse(versions[0]);
                Minor = int.Parse(versions[1]);
                Patch = int.Parse(versions[2]);
            }
            public Version(int major, int minor, int patch)
            {
                Major = major;
                Minor = minor;
                Patch = patch;
            }

            public override bool Equals(object obj)
            {
                if (obj is Version other)
                {
                    return this == other;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return System.HashCode.Combine(Major, Minor, Patch);
            }

            public static bool operator ==(Version A, Version B)
            {
                return A.Major == B.Major && A.Minor == B.Minor && A.Patch == B.Patch;
            }
            public static bool operator !=(Version A, Version B)
            {
                return !(A == B);
            }

            public static bool operator >(Version A, Version B)
            {
                if (A.Major != B.Major)
                {
                    return A.Major > B.Major;
                }
                else if (A.Minor != B.Minor)
                {
                    return A.Minor > B.Minor;
                }
                else
                {
                    return A.Patch > B.Patch;
                }
            }
            public static bool operator >=(Version A, Version B)
            {
                return A.Major >= B.Major || A.Minor >= B.Minor || A.Patch >= B.Patch;
            }

            public static bool operator <(Version A, Version B)
            {
                if (A.Major != B.Major)
                {
                    return A.Major < B.Major;
                }
                else if (A.Minor != B.Minor)
                {
                    return A.Minor < B.Minor;
                }
                else
                {
                    return A.Patch < B.Patch;
                }
            }
            public static bool operator <=(Version A, Version B)
            {
                return A.Major <= B.Major || A.Minor <= B.Minor || A.Patch <= B.Patch;
            }


            public override string ToString()
            {
                return $"{Major}.{Minor}.{Patch}";
            }
        }
        #endregion
    }
}

