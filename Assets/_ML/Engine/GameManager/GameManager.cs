using ML.Engine.TextContent;
using ML.Engine.Utility;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public Timer.CounterDownTimerManager CounterDownTimerManager { get; private set; }

        /// <summary>
        /// ���õ� TickManager(��Ӧ��unity�е�Update
        /// </summary>
        public Timer.TickManager TickManager { get; private set; }

        /// <summary>
        /// ���õ�ȫ��UIManager
        /// </summary>
        public UI.UIManager UIManager { get; private set; }

        /// <summary>
        /// ���õ�ȫ��InputManager
        /// </summary>
        public Input.InputManager InputManager { get; private set; }
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
                }
            }
            this.localManagers.RemoveAll(manager => managers.Contains((T)manager));
            return managers;
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
            if (IsPause)
            {
                this.TickManager.UpdateTickComponentList();
            }
            else
            {
                // �ȸ��¼�ʱ��
                this.CounterDownTimerManager.Update(Time.deltaTime);

                this.TickManager.Tick(Time.deltaTime);

                this.TickManager.UpdateTickComponentList();
            }
        }

        private void FixedUpdate()
        {
            if (IsPause)
            {
                this.TickManager.UpdateFixedTickComponentList();
            }
            else
            {
                // �ȸ��¼�ʱ��
                this.CounterDownTimerManager.FixedUpdate(Time.fixedDeltaTime);

                this.TickManager.FixedTick(Time.fixedDeltaTime);

                this.TickManager.UpdateFixedTickComponentList();
            }
        }

        private void LateUpdate()
        {
            this.CounterDownTimerManager.LateUpdate(Time.deltaTime);

            if (IsPause)
            {
                this.TickManager.UpdateLateTickComponentList();
            }
            else
            {
                this.TickManager.LateTick(Time.deltaTime);

                this.TickManager.UpdateLateTickComponentList();
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



        #region Config
        [LabelText("����"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Config.Language language = Config.Language.Chinese;
        [LabelText("ƽ̨"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Config.Platform platform = Config.Platform.Windows;
        [LabelText("�����豸"), ShowInInspector, FoldoutGroup("Config"), PropertyOrder(-1)]
        public Config.InputDevice inputDevice = Config.InputDevice.Keyboard;
        #endregion
    }
}

