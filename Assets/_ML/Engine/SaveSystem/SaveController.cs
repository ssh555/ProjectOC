using ML.Engine.Manager;
using ML.Engine.Timer;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// 存档数据控制器
    /// </summary>
    public class SaveController
    {
        [LabelText("当前选择游玩的存档Index"), ShowInInspector, ReadOnly]
        public int CurIndex { get; private set; }
        [LabelText("获取当前选中存档的SaveConfig"), ShowInInspector, ReadOnly]
        public SaveDataFolder CurrentSaveDataFolder
        {
            get
            {
                if (CurIndex >= 0 && CurIndex < MAXSAVEDATACOUNT)
                {
                    return SaveDataFolders[CurIndex];
                }
                return null;
            }
        }
        [LabelText("是否加载完存档数据"), ShowInInspector, ReadOnly]
        public bool IsLoadedSaveData { get; private set; }
        /// <summary>
        /// 存档时调用
        /// </summary>
        [HideInInspector]
        public Action<List<ISaveData>> OnSaveAction;


        [LabelText("本地保存存档的路径"), ShowInInspector, ReadOnly]
        private string SavePath = Path.Combine(Application.persistentDataPath, "Save");
        [LabelText("存档位最大数量"), ShowInInspector, ReadOnly]
        private const int MAXSAVEDATACOUNT = 3;
        [LabelText("实际存档数据"), ShowInInspector, ReadOnly]
        private List<ISaveData> Datas = new List<ISaveData>();
        [LabelText("存档数据的SaveConfig"), ShowInInspector, ReadOnly]
        private SaveDataFolder[] SaveDataFolders = new SaveDataFolder[MAXSAVEDATACOUNT];
        [LabelText("SaveConfig名称"), ShowInInspector, ReadOnly]
        private string[] ConfigPaths = new string[MAXSAVEDATACOUNT];
        [LabelText("存档Config"), ShowInInspector, ReadOnly]
        private SaveConfig SaveConfig;
        private SaveSystem SaveSystem;
        /// <summary>
        /// 存档计时器，存档时需要过1s再存档
        /// </summary>
        private CounterDownTimer SaveTimer;
        /// <summary>
        /// 等待存档的数据，计时器结束后将该数据存档
        /// </summary>
        private List<ISaveData> TempDatas = new List<ISaveData>();
        /// <summary>
        /// 等待存档时的SaveDataFolder
        /// </summary>
        private SaveDataFolder TempSaveDataFolder;
        /// <summary>
        /// 用来反射
        /// </summary>
        private Dictionary<string, MethodInfo> MethodDict = new Dictionary<string, MethodInfo>();

        public SaveController(SaveConfig saveConfig, SaveSystem saveSystem)
        {
            SaveConfig = saveConfig;
            SaveSystem = saveSystem;
            SaveTimer = new CounterDownTimer(1f, false, false);
            SaveTimer.OnEndEvent += SaveTimerEndAction;
            if (Directory.Exists(SavePath))
            {
                GlobalSaveDataFolder globalSaveDataFolder = LoadData<GlobalSaveDataFolder>("GlobalSaveConfig");
                if (globalSaveDataFolder != null)
                {
                    List<string> configs = globalSaveDataFolder.SaveDataFolders;
                    if (configs != null)
                    {
                        for (int i = 0; i < MAXSAVEDATACOUNT; i++)
                        {
                            if (configs[i] != null && File.Exists(Path.Combine(SavePath, configs[i], "SaveConfig")))
                            {
                                ConfigPaths[i] = configs[i];
                                SaveDataFolders[i] = LoadData<SaveDataFolder>(Path.Combine(ConfigPaths[i], "SaveConfig"));
                                SaveDataFolders[i].SavePath = ConfigPaths[i];
                            }
                        }
                    }
                    UpdateVersion();
                }
            }
        }

        /// <summary>
        /// 更新版本
        /// </summary>
        private void UpdateVersion()
        {
            Manager.GameManager.Version version = GameManager.Instance.version;
            GlobalSaveDataFolder globalSaveDataFolder = LoadData<GlobalSaveDataFolder>("GlobalSaveConfig");
            if (globalSaveDataFolder != null && globalSaveDataFolder.Version < GameManager.Instance.version)
            {
                SaveGlobalSaveDataFolder();
            }

            foreach (SaveDataFolder saveDataFolder in this.SaveDataFolders)
            {
                if (saveDataFolder != null)
                {
                    foreach (var kv in saveDataFolder.FileMap)
                    {
                        if (!MethodDict.ContainsKey(kv.Value))
                        {
                            Type type = Type.GetType(kv.Value);
                            MethodInfo method = typeof(SaveController).GetMethod("LoadData").MakeGenericMethod(type);
                            MethodDict.Add(kv.Value, method);
                        }
                        string fileName = Path.GetFileName(kv.Key);
                        object loadData = MethodDict[kv.Value].Invoke(this, new object[] { Path.Combine(saveDataFolder.Name, fileName) });
                        if (loadData != null)
                        {
                            ISaveData data = (ISaveData)loadData;
                            if (data.Version < version)
                            {
                                data.SaveName = fileName;
                                data.SavePath = saveDataFolder.SavePath;
                                data.Version = version;
                                data.IsDirty = true;
                                SaveData(data);
                            }
                        }
                    }

                    if (saveDataFolder.Version < version)
                    {
                        saveDataFolder.Version = version;
                        saveDataFolder.IsDirty = true;
                        SaveData(saveDataFolder);
                    }
                }
            }
        }

        public SaveDataFolder GetSaveDataFolder(int index)
        {
            if (index >= 0 && index < MAXSAVEDATACOUNT)
            {
                return SaveDataFolders[index];
            }
            return null;
        }

        /// <summary>
        /// 获取对应类型的存档数据
        /// </summary>
        public T GetSaveData<T>() where T : ISaveData
        {
            if (Datas != null)
            {
                foreach (ISaveData data in Datas)
                {
                    if (data is T t)
                    {
                        return t;
                    }
                }
            }
            return default(T);
        }
        /// <summary>
        /// 将数据加入存档数据中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据</param>
        /// <param name="isSave">是否存档</param>
        public void AddSaveData<T>(T data, bool isSave=true) where T : ISaveData
        {
            if (this.GetSaveData<T>() == null)
            {
                SaveDataFolder saveDataFolder = CurrentSaveDataFolder;
                if (saveDataFolder != null)
                {
                    this.Datas.Add(data);
                    data.SavePath = saveDataFolder.Name;
                    data.SaveName = data.GetType().Name;
                    data.Version = GameManager.Instance.version;
                    data.IsDirty = true;
                    saveDataFolder.FileMap[Path.Combine(SavePath, data.SavePath, data.SaveName)] = data.GetType().ToString();
                    saveDataFolder.IsDirty = true;
                    if (isSave)
                    {
                        SaveData(saveDataFolder);
                    }
                }
            }
        }
        /// <summary>
        /// 加载存档数据
        /// </summary>
        /// <param name="relativePath">相对路径，没有后缀</param>
        public T LoadData<T>(string relativePath) where T : ISaveData
        {
            T data = (T)this.SaveSystem.LoadData<T>(relativePath, this.SaveConfig.UseEncrption);
            return data;
        }
        /// <summary>
        /// 保存数据
        /// </summary>
        public void SaveData<T>(T data) where T : ISaveData
        {
            this.SaveSystem.SaveData<T>(data, this.SaveConfig.UseEncrption);
        }

        /// <summary>
        /// 修改CurIndex存档名字
        /// </summary>
        public async Task ChangeSaveDataFolderNameAsync(string name, Action callback)
        {
            await ChangeSaveDataFolderNameAsync(CurIndex, name, callback);
        }
        /// <summary>
        /// 修改存档名字
        /// </summary>
        public async Task ChangeSaveDataFolderNameAsync(int index, string name, Action callback)
        {
            if (!string.IsNullOrEmpty(name) 
                && index >= 0 && index < MAXSAVEDATACOUNT 
                && !string.IsNullOrEmpty(ConfigPaths[index]) 
                && !ConfigPaths.Contains(name))
            {
                if (name != ConfigPaths[index])
                {
                    string oldPath = Path.Combine(SavePath, ConfigPaths[index]);
                    string path = Path.Combine(SavePath, name);
                    Task task1 = Task.Run(() => Directory.CreateDirectory(path));
                    await task1;
                    Task task2 = Task.Run(() =>
                    {
                        foreach (string file in Directory.GetFiles(oldPath))
                        {
                            File.Move(file, Path.Combine(path, Path.GetFileName(file)));
                        }
                    });
                    await task2;
                    Task task3 = Task.Run(() => Directory.Delete(oldPath, true));
                    await task3;

                    SaveDataFolders[index].ChangeName(name);
                    if (CurIndex == index)
                    {
                        foreach (ISaveData data in Datas)
                        {
                            data.SavePath = name;
                        }
                    }
                    ConfigPaths[index] = name;
                    await Task.Run(() =>
                    {
                        SaveData(SaveDataFolders[index]);
                        SaveGlobalSaveDataFolder();
                    });
                }
                callback?.Invoke();
            }
        }
        /// <summary>
        /// 创建存档
        /// </summary>
        public async Task<SaveDataFolder> CreateSaveDataFolderAsync(int index, string name, Action callback)
        {
            if (!string.IsNullOrEmpty(name) && index >= 0 && index < MAXSAVEDATACOUNT && string.IsNullOrEmpty(ConfigPaths[index]) && !ConfigPaths.Contains(name))
            {
                await Task.Run(() => Directory.CreateDirectory(Path.Combine(SavePath, name)));
                SaveDataFolder saveDataFolder = new SaveDataFolder(name, GameManager.Instance.version);
                saveDataFolder.IsDirty = true;
                ConfigPaths[index] = name;
                SaveDataFolders[index] = saveDataFolder;
                await Task.Run(() =>
                {
                    SaveData(saveDataFolder);
                    SaveGlobalSaveDataFolder();
                });
                callback?.Invoke();
                return saveDataFolder;
            }
            return null;
        }
        /// <summary>
        /// 选择对应存档
        /// </summary>
        /// <param name="callback">载入完成后的回调</param>
        public async Task SelectSaveDataFolderAsync(int index, Action callback)
        {
            if (index >= 0 && index < MAXSAVEDATACOUNT)
            {
                CurIndex = index;
                if (SaveDataFolders[index] == null || string.IsNullOrEmpty(ConfigPaths[index]))
                {
                    SaveDataFolders[index] = await CreateSaveDataFolderAsync(index, index.ToString(), null);
                }
                this.Datas.Clear();
                this.IsLoadedSaveData = false;
                callback?.Invoke();
            }
        }
        /// <summary>
        /// 加载CurIndex数据
        /// </summary>
        public async Task LoadSaveDataAsync(Action<List<ISaveData>> callback)
        {
            await LoadSaveDataAsync(CurIndex, callback);
        }
        /// <summary>
        /// 加载数据
        /// </summary>
        private async Task LoadSaveDataAsync(int index, Action<List<ISaveData>> callback)
        {
            if (index >= 0 && index < MAXSAVEDATACOUNT && !this.IsLoadedSaveData)
            {
                SaveDataFolder saveDataFolder = SaveDataFolders[index];
                await Task.Run(() =>
                {
                    foreach (var kv in saveDataFolder.FileMap)
                    {
                        if (!MethodDict.ContainsKey(kv.Value))
                        {
                            Type type = Type.GetType(kv.Value);
                            MethodInfo method = typeof(SaveController).GetMethod("LoadData").MakeGenericMethod(type);
                            MethodDict.Add(kv.Value, method);
                        }
                        string fileName = Path.GetFileName(kv.Key);
                        object loadData = MethodDict[kv.Value].Invoke(this, new object[] { Path.Combine(saveDataFolder.Name, fileName) });
                        if (loadData != null) 
                        {
                            ISaveData data = (ISaveData)loadData;
                            data.SavePath = saveDataFolder.Name;
                            data.SaveName = fileName;
                            data.IsDirty = false;
                            this.Datas.Add(data);
                        }
                    }
                });
                this.IsLoadedSaveData = true;
                callback?.Invoke(this.Datas);
            }
        }
        /// <summary>
        /// 删除CurIndex对应存档
        /// </summary>
        public async Task DeleteSaveDataFolderAsync(Action callback)
        {
            await DeleteSaveDataFolderAsync(CurIndex, callback);
        }
        /// <summary>
        /// 删除对应存档
        /// </summary>
        public async Task DeleteSaveDataFolderAsync(int index, Action callback)
        {
            if (index >= 0 && index < MAXSAVEDATACOUNT)
            {
                if (!string.IsNullOrEmpty(ConfigPaths[index]))
                {
                    string oldPath = Path.Combine(SavePath, ConfigPaths[index]);
                    if (Directory.Exists(oldPath))
                    {
                        await Task.Run(() => Directory.Delete(oldPath, true));
                    }
                    ConfigPaths[index] = null;
                    SaveDataFolders[index] = null;
                    if (index == CurIndex)
                    {
                        this.Datas.Clear();
                        this.IsLoadedSaveData = false;
                    }
                    SaveGlobalSaveDataFolder();
                }
                callback?.Invoke();
            }
        }
        /// <summary>
        /// 将当前的数据保存在当前的CurIndex对应的存档位置上
        /// </summary>
        public void SaveSaveDataFolder()
        {
            if (CurrentSaveDataFolder != null)
            {
                this.TempSaveDataFolder = (SaveDataFolder)CurrentSaveDataFolder.Clone();
                CurrentSaveDataFolder.IsDirty = false;
                this.TempDatas.Clear();
                foreach (ISaveData data in Datas)
                {
                    this.TempDatas.Add((ISaveData)data.Clone());
                    data.IsDirty = false;
                }
                SaveTimer.Reset(1f);
            }
        }
        private void SaveTimerEndAction()
        {
#pragma warning disable CS4014
            SaveSaveDataFolderAsync();
#pragma warning restore CS4014
        }
        /// <summary>
        /// 将当前的数据保存在TempCurIndex对应的存档位置上
        /// </summary>
        private async Task SaveSaveDataFolderAsync()
        {
            await Task.Run(() =>
            {
                foreach (ISaveData data in TempDatas)
                {
                    SaveData(data);
                }
                TempSaveDataFolder.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                TempSaveDataFolder.IsDirty = true;
                SaveData(TempSaveDataFolder);
            });
            OnSaveAction?.Invoke(TempDatas);
        }
        /// <summary>
        /// 更新SaveDataFolder后才能调用
        /// </summary>
        public void SaveGlobalSaveDataFolder()
        {
            GlobalSaveDataFolder global = new GlobalSaveDataFolder(ConfigPaths.ToList(), GameManager.Instance.version);
            global.IsDirty = true;
            SaveData(global);
        }
    }
}