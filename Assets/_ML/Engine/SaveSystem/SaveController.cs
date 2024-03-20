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
        /// <summary>
        /// ���ر���浵��·��
        /// </summary>
        private string SavePath = Path.Combine(Application.persistentDataPath, "Save");
        /// <summary>
        /// �浵λ�������
        /// </summary>
        private const int MAXSAVEDATACOUNT = 3;
        /// <summary>
        /// �浵���ݵ�SaveConfig������
        /// string��ӦSaveDataFolder.name
        /// </summary>
        private string[] ConfigPaths = new string[MAXSAVEDATACOUNT];
        /// <summary>
        /// �浵����SaveConfig��Ϊnull��ʾû�ж�Ӧ�Ĵ浵
        /// ������configpaths���ݺ�ͼ��ش�������
        /// </summary>
        private SaveDataFolder[] SaveDataFolders = new SaveDataFolder[MAXSAVEDATACOUNT];
        /// <summary>
        /// ��ǰѡ������Ĵ浵index
        /// </summary>
        public int CurIndex { get; private set; }
        /// <summary>
        /// ��ȡ��ǰѡ�еĴ浵��SaveConfig
        /// </summary>
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
        /// <summary>
        /// ʵ�ʴ浵���ݣ�Ĭ��Ϊnull
        /// ֻ��ѡ���˶�Ӧ�浵֮�󣬲Ż�����ʵ������
        /// </summary>
        private List<ISaveData> Datas = new List<ISaveData>();
        private SaveConfig SaveConfig;
        private SaveSystem SaveSystem;

        public bool IsLoadedSaveData { get; private set; } = false;
        /// <summary>
        /// ��������
        /// </summary>
        private Dictionary<string, MethodInfo> MethodDict = new Dictionary<string, MethodInfo>();

        public SaveController(SaveConfig saveConfig, SaveSystem saveSystem)
        {
            SaveConfig = saveConfig;
            SaveSystem = saveSystem;
            if (Directory.Exists(SavePath))
            {
                GlobalSaveDataFolder globalSaveDataFolder = LoadData<GlobalSaveDataFolder>("GlobalSaveConfig");
                List<string> configs = globalSaveDataFolder?.SaveDataFolders;
                if (configs != null)
                {
                    for (int i=0; i< MAXSAVEDATACOUNT; i++)
                    {
                        ConfigPaths[i] = configs[i];
                        if (ConfigPaths[i] != null)
                        {
                            SaveDataFolders[i] = LoadData<SaveDataFolder>(Path.Combine(ConfigPaths[i], "SaveConfig"));
                        }
                    }
                }
            }
        }

        public string GetFileExtension()
        {
            if (this.SaveConfig.SaveType == SaveType.Json)
            {
                return ".json";
            }
            else if (this.SaveConfig.SaveType == SaveType.XML)
            {
                return ".xml";
            }
            return "";
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
            return null;
        }
        public void AddSaveData<T>(T data) where T : ISaveData
        {
            if (this.GetSaveData<T>() == null)
            {
                SaveDataFolder saveDataFolder = CurrentSaveDataFolder;
                this.Datas.Add(data);
                data.Path = saveDataFolder.Name;
                data.SaveName = data.GetType().ToString();
                string extension = GetFileExtension();
                saveDataFolder.FileMap[data.SaveName] = Path.Combine(SavePath, saveDataFolder.Name, data.SaveName + extension);
                saveDataFolder.IsDirty = true;
                SaveData(saveDataFolder);
            }
        }
        /// <summary>
        /// ���ش浵����
        /// </summary>
        /// <param name="relativePathWithoutSuffix">���·����û�к�׺</param>
        public T LoadData<T>(string relativePathWithoutSuffix) where T : ISaveData
        {
            T data = (T)this.SaveSystem.LoadData<T>(relativePathWithoutSuffix, this.SaveConfig.UseEncrption);
            return data;
        }

        /// <summary>
        /// ��������
        /// </summary>
        public void SaveData<T>(T data, bool isAdd = false) where T : ISaveData
        {
            if (isAdd)
            {
                AddSaveData<T>(data);
            }
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
                    Task task2 = Task.Run(() =>
                    {
                        foreach (string file in Directory.GetFiles(oldPath))
                        {
                            File.Move(file, Path.Combine(path, Path.GetFileName(file)));
                        }
                    });
                    Task task3 = Task.Run(() => Directory.Delete(oldPath, true));
                    await task1;
                    await task2;
                    await task3;

                    SaveDataFolders[index].ChangeName(name);
                    if (CurIndex == index)
                    {
                        foreach (ISaveData data in Datas)
                        {
                            data.Path = name;
                        }
                    }
                    ConfigPaths[index] = name;
                    await Task.Run(() => SaveData(SaveDataFolders[index]));
                    await SaveGlobalSaveDataFolderAsync();
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
                SaveDataFolder saveDataFolder = new SaveDataFolder(name);
                ConfigPaths[index] = name;
                SaveDataFolders[index] = saveDataFolder;
                await Task.Run(() => SaveData(saveDataFolder));
                await SaveGlobalSaveDataFolderAsync();
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
        public async Task LoadSaveDataAsync(int index, Action<List<ISaveData>> callback)
        {
            if (index >= 0 && index < MAXSAVEDATACOUNT)
            {
                this.IsLoadedSaveData = false;
                SaveDataFolder saveDataFolder = SaveDataFolders[index];
                await Task.Run(() =>
                {
                    foreach (var kv in saveDataFolder.FileMap)
                    {
                        if (!MethodDict.ContainsKey(kv.Key))
                        {
                            Type type = Type.GetType(kv.Key);
                            MethodInfo method = typeof(SaveController).GetMethod("LoadData").MakeGenericMethod(type);
                            MethodDict.Add(kv.Key, method);
                        }
                        object loadData = MethodDict[kv.Key].Invoke(this, new object[] { Path.Combine(saveDataFolder.Name, kv.Key) });
                        ISaveData data = (ISaveData) loadData;
                        data.Path = saveDataFolder.Name;
                        data.SaveName = kv.Key;
                        data.IsDirty = false;
                        this.Datas.Add(data);
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
                    }
                    await SaveGlobalSaveDataFolderAsync();
                }
                callback?.Invoke();
            }
        }
        /// <summary>
        /// ����ǰ�����ݱ����ڵ�ǰ��CurIndex��Ӧ�Ĵ浵λ����
        /// </summary>
        public async Task SaveSaveDataFolderAsync(Action callback)
        {
            await SaveSaveDataFolderAsync(CurIndex, callback);
        }
        /// <summary>
        /// ����ǰ�����ݱ����ڵ�ǰ��index��Ӧ�Ĵ浵λ����
        /// </summary>
        public async Task SaveSaveDataFolderAsync(int index, Action callback)
        {
            if (index >= 0 && index < MAXSAVEDATACOUNT)
            {
                // ������ʱ����
                List<ISaveData> tempDatas = new List<ISaveData>();
                if (Datas != null)
                {
                    foreach (ISaveData data in Datas)
                    {
                        tempDatas.Add((ISaveData)data.Clone());
                        data.IsDirty = false;
                    }
                }
                if (SaveDataFolders[index] == null || string.IsNullOrEmpty(ConfigPaths[index]))
                {
                    SaveDataFolders[index] = await CreateSaveDataFolderAsync(index, index.ToString(), null);
                }
                SaveDataFolder tempSaveDataFolder = (SaveDataFolder)SaveDataFolders[index].Clone();
                SaveDataFolders[index].IsDirty = false;

                await Task.Run(() =>
                {
                    foreach (ISaveData data in tempDatas)
                    {
                        data.Path = ConfigPaths[index];
                        SaveData(data);
                        string extension = GetFileExtension();
                        tempSaveDataFolder.FileMap[data.SaveName] = Path.Combine(SavePath, data.Path, data.SaveName + extension);
                    }
                });
                await Task.Run(() =>
                {
                    tempSaveDataFolder.IsDirty = true;
                    SaveData(tempSaveDataFolder);
                });
                callback?.Invoke();
            }
        }
        public async Task SaveGlobalSaveDataFolderAsync()
        {
            GlobalSaveDataFolder global = new GlobalSaveDataFolder(ConfigPaths.ToList());
            global.IsDirty = true;
            await Task.Run(() => SaveData(global));
        }

        #region Test
#if UNITY_EDITOR
        public void ChangeSaveDataFolderName(int index, string name, Action callback)
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
                    Directory.CreateDirectory(path);
                    foreach (string file in Directory.GetFiles(oldPath))
                    {
                        File.Move(file, Path.Combine(path, Path.GetFileName(file)));
                    }
                    Directory.Delete(oldPath, true);
                    SaveDataFolders[index].ChangeName(name);
                    if (CurIndex == index)
                    {
                        foreach (ISaveData data in Datas)
                        {
                            data.Path = name;
                        }
                    }
                    ConfigPaths[index] = name;
                    SaveData(SaveDataFolders[index]);
                    SaveGlobalSaveDataFolder();
                }
                callback?.Invoke();
            }
        }
        public SaveDataFolder CreateSaveDataFolder(int index, string name, Action callback)
        {
            if (!string.IsNullOrEmpty(name) && index >= 0 && index < MAXSAVEDATACOUNT && string.IsNullOrEmpty(ConfigPaths[index]) && !ConfigPaths.Contains(name))
            {
                Directory.CreateDirectory(Path.Combine(SavePath, name));
                SaveDataFolder saveDataFolder = new SaveDataFolder(name);
                ConfigPaths[index] = name;
                SaveDataFolders[index] = saveDataFolder;
                SaveData(saveDataFolder);
                SaveGlobalSaveDataFolder();
                callback?.Invoke();
                return saveDataFolder;
            }
            return null;
        }
        public void SelectSaveDataFolder(int index, Action callback)
        {
            if (index >= 0 && index < MAXSAVEDATACOUNT)
            {
                CurIndex = index;
                if (SaveDataFolders[index] == null || string.IsNullOrEmpty(ConfigPaths[index]))
                {
                    SaveDataFolders[index] = CreateSaveDataFolder(index, index.ToString(), null);
                }
                this.Datas.Clear();
                callback?.Invoke();
            }
        }
        public void LoadSaveData(int index, Action<List<ISaveData>> callback)
        {
            if (index >= 0 && index < MAXSAVEDATACOUNT)
            {
                this.IsLoadedSaveData = false;
                SaveDataFolder saveDataFolder = SaveDataFolders[index];
                foreach (var kv in saveDataFolder.FileMap)
                {
                    if (!MethodDict.ContainsKey(kv.Key))
                    {
                        Type type = Type.GetType(kv.Key);
                        MethodInfo method = typeof(SaveController).GetMethod("LoadData").MakeGenericMethod(type);
                        MethodDict.Add(kv.Key, method);
                    }
                    object loadData = MethodDict[kv.Key].Invoke(this, new object[] { Path.Combine(saveDataFolder.Name, kv.Key) });
                    ISaveData data = (ISaveData)loadData;
                    data.Path = saveDataFolder.Name;
                    data.SaveName = kv.Key;
                    data.IsDirty = false;
                    this.Datas.Add(data);
                }
                this.IsLoadedSaveData = true;
                callback?.Invoke(this.Datas);
            }
        }
        public void DeleteSaveDataFolder(int index, Action callback)
        {
            if (index >= 0 && index < MAXSAVEDATACOUNT)
            {
                if (!string.IsNullOrEmpty(ConfigPaths[index]))
                {
                    string oldPath = Path.Combine(SavePath, ConfigPaths[index]);
                    if (Directory.Exists(oldPath))
                    {
                        Directory.Delete(oldPath, true);
                    }
                    ConfigPaths[index] = null;
                    SaveDataFolders[index] = null;
                    if (index == CurIndex)
                    {
                        this.Datas.Clear();
                    }
                    SaveGlobalSaveDataFolder();
                }
                callback?.Invoke();
            }
        }
        public void SaveSaveDataFolder(int index, Action callback)
        {
            if (index >= 0 && index < MAXSAVEDATACOUNT)
            {
                // ������ʱ����
                List<ISaveData> tempDatas = new List<ISaveData>();
                if (Datas != null)
                {
                    foreach (ISaveData data in Datas)
                    {
                        tempDatas.Add((ISaveData)data.Clone());
                        data.IsDirty = false;
                    }
                }
                if (SaveDataFolders[index] == null || string.IsNullOrEmpty(ConfigPaths[index]))
                {
                    SaveDataFolders[index] = CreateSaveDataFolder(index, index.ToString(), null);
                }

                SaveDataFolder tempSaveDataFolder = (SaveDataFolder)SaveDataFolders[index].Clone();
                SaveDataFolders[index].IsDirty = false;

                foreach (ISaveData data in tempDatas)
                {
                    data.Path = ConfigPaths[index];
                    SaveData(data);
                    string extension = GetFileExtension();
                    tempSaveDataFolder.FileMap[data.SaveName] = Path.Combine(SavePath, data.Path, data.SaveName + extension);
                }
                tempSaveDataFolder.IsDirty = true;
                SaveData(tempSaveDataFolder);
                callback?.Invoke();
            }
        }
        public void SaveGlobalSaveDataFolder()
        {
            GlobalSaveDataFolder global = new GlobalSaveDataFolder(ConfigPaths.ToList());
            global.IsDirty = true;
            SaveData(global);
        }
#endif
#endregion
    }
}