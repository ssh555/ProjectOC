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
    /// �浵���ݿ�����
    /// </summary>
    public class SaveController
    {
        [LabelText("��ǰѡ������Ĵ浵Index"), ShowInInspector, ReadOnly]
        public int CurIndex { get; private set; }
        [LabelText("��ȡ��ǰѡ�д浵��SaveConfig"), ShowInInspector, ReadOnly]
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
        [LabelText("�Ƿ������浵����"), ShowInInspector, ReadOnly]
        public bool IsLoadedSaveData { get; private set; }
        /// <summary>
        /// �浵ʱ����
        /// </summary>
        [HideInInspector]
        public Action<List<ISaveData>> OnSaveAction;


        [LabelText("���ر���浵��·��"), ShowInInspector, ReadOnly]
        private string SavePath = Path.Combine(Application.persistentDataPath, "Save");
        [LabelText("�浵λ�������"), ShowInInspector, ReadOnly]
        private const int MAXSAVEDATACOUNT = 3;
        [LabelText("ʵ�ʴ浵����"), ShowInInspector, ReadOnly]
        private List<ISaveData> Datas = new List<ISaveData>();
        [LabelText("�浵���ݵ�SaveConfig"), ShowInInspector, ReadOnly]
        private SaveDataFolder[] SaveDataFolders = new SaveDataFolder[MAXSAVEDATACOUNT];
        [LabelText("SaveConfig����"), ShowInInspector, ReadOnly]
        private string[] ConfigPaths = new string[MAXSAVEDATACOUNT];
        [LabelText("�浵Config"), ShowInInspector, ReadOnly]
        private SaveConfig SaveConfig;
        private SaveSystem SaveSystem;
        /// <summary>
        /// �浵��ʱ�����浵ʱ��Ҫ��1s�ٴ浵
        /// </summary>
        private CounterDownTimer SaveTimer;
        /// <summary>
        /// �ȴ��浵�����ݣ���ʱ�������󽫸����ݴ浵
        /// </summary>
        private List<ISaveData> TempDatas = new List<ISaveData>();
        /// <summary>
        /// �ȴ��浵ʱ��SaveDataFolder
        /// </summary>
        private SaveDataFolder TempSaveDataFolder;
        /// <summary>
        /// ��������
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
        /// ���°汾
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
        /// ��ȡ��Ӧ���͵Ĵ浵����
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
        /// �����ݼ���浵������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">����</param>
        /// <param name="isSave">�Ƿ�浵</param>
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
        /// ���ش浵����
        /// </summary>
        /// <param name="relativePath">���·����û�к�׺</param>
        public T LoadData<T>(string relativePath) where T : ISaveData
        {
            T data = (T)this.SaveSystem.LoadData<T>(relativePath, this.SaveConfig.UseEncrption);
            return data;
        }
        /// <summary>
        /// ��������
        /// </summary>
        public void SaveData<T>(T data) where T : ISaveData
        {
            this.SaveSystem.SaveData<T>(data, this.SaveConfig.UseEncrption);
        }

        /// <summary>
        /// �޸�CurIndex�浵����
        /// </summary>
        public async Task ChangeSaveDataFolderNameAsync(string name, Action callback)
        {
            await ChangeSaveDataFolderNameAsync(CurIndex, name, callback);
        }
        /// <summary>
        /// �޸Ĵ浵����
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
        /// �����浵
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
        /// ѡ���Ӧ�浵
        /// </summary>
        /// <param name="callback">������ɺ�Ļص�</param>
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
        /// ����CurIndex����
        /// </summary>
        public async Task LoadSaveDataAsync(Action<List<ISaveData>> callback)
        {
            await LoadSaveDataAsync(CurIndex, callback);
        }
        /// <summary>
        /// ��������
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
        /// ɾ��CurIndex��Ӧ�浵
        /// </summary>
        public async Task DeleteSaveDataFolderAsync(Action callback)
        {
            await DeleteSaveDataFolderAsync(CurIndex, callback);
        }
        /// <summary>
        /// ɾ����Ӧ�浵
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
        /// ����ǰ�����ݱ����ڵ�ǰ��CurIndex��Ӧ�Ĵ浵λ����
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
        /// ����ǰ�����ݱ�����TempCurIndex��Ӧ�Ĵ浵λ����
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
        /// ����SaveDataFolder����ܵ���
        /// </summary>
        public void SaveGlobalSaveDataFolder()
        {
            GlobalSaveDataFolder global = new GlobalSaveDataFolder(ConfigPaths.ToList(), GameManager.Instance.version);
            global.IsDirty = true;
            SaveData(global);
        }
    }
}