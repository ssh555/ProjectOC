using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// 存档数据控制器
    /// </summary>
    public class SaveController
    {
        /// <summary>
        /// 本地保存存档的路径
        /// </summary>
        public string SavePath = Path.Combine(Application.persistentDataPath, "Save");
        /// <summary>
        /// 存档位最大数量
        /// </summary>
        public const int MAXSAVEDATACOUNT = 3;
        /// <summary>
        /// 存档数据的SaveConfig的名称
        /// string对应SaveDataFolder.name
        /// </summary>
        public string[] ConfigPaths = new string[MAXSAVEDATACOUNT];
        /// <summary>
        /// 存档数据SaveConfig，为null表示没有对应的存档
        /// 加载完configpaths数据后就加载此项数据
        /// </summary>
        public SaveDataFolder[] SaveDataFolders = new SaveDataFolder[MAXSAVEDATACOUNT];
        /// <summary>
        /// 当前选择游玩的存档index
        /// </summary>
        [ShowInInspector]
        protected int CurIndex = -1;
        /// <summary>
        /// 获取当前选中的存档的SaveConfig
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
        /// 实际存档数据，默认为null
        /// 只有选择了对应存档之后，才会载入实际数据
        /// </summary>
        public List<ISaveData> datas = new List<ISaveData>();
        public bool IsLoadedSaveData = false;
        
        public SaveController()
        {
            if (Directory.Exists(SavePath))
            {
                string[] dirs = Directory.GetDirectories(SavePath).OrderBy(d => d).ToArray(); ;
                for (int i = 0; i < dirs.Length; i++)
                {
                    DirectoryInfo info = new DirectoryInfo(dirs[i]);
                    SaveDataFolder saveDataFolder = ML.Engine.Manager.GameManager.Instance.SaveManager.LoadData<SaveDataFolder>(Path.Combine(info.Name, "SaveConfig"));
                    Debug.Log(saveDataFolder);
                    saveDataFolder.SaveName = "SaveConfig";
                    saveDataFolder.IsDirty = false;
                    saveDataFolder.Path = info.Name;
                    ConfigPaths[i] = info.Name;
                    SaveDataFolders[i] = saveDataFolder;
                    if (i >= MAXSAVEDATACOUNT)
                    {
                        break;
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
            if (datas != null)
            {
                foreach (ISaveData data in datas)
                {
                    if (data is T t)
                    {
                        return t;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 选择对应存档
        /// </summary>
        /// <param name="callback">载入完成后的回调</param>
        public async Task SelectSaveDataFolderAsync(int index, Action<List<ISaveData>> callback)
        {
            this.IsLoadedSaveData = false;
            if (index < 0 || index >= MAXSAVEDATACOUNT)
            {
                bool flag = true;
                for (int i=0; i<MAXSAVEDATACOUNT; i++) 
                {
                    if (SaveDataFolders[i] != null)
                    {
                        index = i;
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    index = 0;
                }
            }
            CurIndex = index;
            if (SaveDataFolders[index] == null)
            {
                await CreateSaveDataFolderAsync(index, index.ToString(), null, this.datas);
            }
            callback?.Invoke(this.datas);
            this.IsLoadedSaveData = true;
        }
        /// <summary>
        /// 删除对应存档
        /// </summary>
        public async Task DeleteSaveDataFolderAsync(int index)
        {
            if (index >= 0 || index < MAXSAVEDATACOUNT)
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
                }
            }
        }
        /// <summary>
        /// 将当前的数据保存在当前的SaveDataFolder对应的存档位置上,若CurIndex==-1，则需要先Create再调用此项
        /// </summary>
        public async Task SaveSaveDataFolderAsync(Action callback)
        {
            if (CurIndex < 0 || CurIndex >= MAXSAVEDATACOUNT)
            {
                for (int i=0; i<MAXSAVEDATACOUNT; i++)
                {
                    if (SaveDataFolders[i] == null)
                    {
                        SaveDataFolders[i] = await CreateSaveDataFolderAsync(i, i.ToString(), callback, this.datas);
                        CurIndex = i;
                        return;
                    }
                }
            }
            else if (SaveDataFolders[CurIndex] == null || string.IsNullOrEmpty(ConfigPaths[CurIndex]))
            {
                SaveDataFolders[CurIndex] = await CreateSaveDataFolderAsync(CurIndex, CurIndex.ToString(), callback, this.datas);
            }
            else
            {
                SaveDataFolder saveDataFolder = SaveDataFolders[CurIndex];
                if (datas != null)
                {
                    foreach (ISaveData data in datas)
                    {
                        data.Path = ConfigPaths[CurIndex];
                        await Task.Run(() => ML.Engine.Manager.GameManager.Instance.SaveManager.SaveData(data));
                        SaveType saveType = ML.Engine.Manager.GameManager.Instance.SaveManager.Config.SaveType;
                        saveDataFolder.FileMap[data.SaveName] = Path.Combine(SavePath, data.Path, data.SaveName);
                    }
                }
                saveDataFolder.IsDirty = true;
                saveDataFolder.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await Task.Run(() => ML.Engine.Manager.GameManager.Instance.SaveManager.SaveData(saveDataFolder));
                callback?.Invoke();
            }
        }
        /// <summary>
        /// 根据传入的参数，异步创建对应的存档数据
        /// </summary>
        public async Task<SaveDataFolder> CreateSaveDataFolderAsync(int index, string name, Action callback, List<ISaveData> datalist = null)
        {
            if (!string.IsNullOrEmpty(name) && index>=0 && index<MAXSAVEDATACOUNT)
            {
                if (!string.IsNullOrEmpty(ConfigPaths[index]))
                {
                    string oldPath = Path.Combine(SavePath, ConfigPaths[index]);
                    if (Directory.Exists(oldPath))
                    {
                        await Task.Run(() => Directory.Delete(oldPath, true));
                    }
                }

                SaveDataFolder saveDataFolder = new SaveDataFolder();
                string path = Path.Combine(SavePath, name);
                if (Directory.Exists(path))
                {
                    await Task.Run(() => Directory.Delete(path, true));
                }
                await Task.Run(() => Directory.CreateDirectory(path));
                DirectoryInfo info = new DirectoryInfo(path);
                saveDataFolder.Name = info.Name;
                saveDataFolder.CreateTime = info.CreationTime.ToString();
                saveDataFolder.IsDirty = true;
                saveDataFolder.SaveName = "SaveConfig";
                saveDataFolder.Path = name;
                ConfigPaths[index] = saveDataFolder.Name;
                SaveDataFolders[index] = saveDataFolder;
                if (datalist != null)
                {
                    foreach (ISaveData data in datalist)
                    {
                        data.IsDirty = true;
                        data.Path = name;
                        await Task.Run(() => ML.Engine.Manager.GameManager.Instance.SaveManager.SaveData(data));
                        SaveType saveType = ML.Engine.Manager.GameManager.Instance.SaveManager.Config.SaveType;
                        saveDataFolder.FileMap[data.SaveName] = Path.Combine(SavePath, data.Path, data.SaveName);
                    }
                }
                saveDataFolder.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await Task.Run(() => ML.Engine.Manager.GameManager.Instance.SaveManager.SaveData(saveDataFolder));
                callback?.Invoke();
                return saveDataFolder;
            }
            return null;
        }

        #region Test
        public void SaveSaveDataFolder(Action callback)
        {
            if (CurIndex < 0 || CurIndex >= MAXSAVEDATACOUNT)
            {
                for (int i = 0; i < MAXSAVEDATACOUNT; i++)
                {
                    if (SaveDataFolders[i] == null)
                    {
                        SaveDataFolders[i] = CreateSaveDataFolder(i, i.ToString(), callback, this.datas);
                        CurIndex = i;
                        return;
                    }
                }
            }
            else if (SaveDataFolders[CurIndex] == null || string.IsNullOrEmpty(ConfigPaths[CurIndex]))
            {
                SaveDataFolders[CurIndex] = CreateSaveDataFolder(CurIndex, CurIndex.ToString(), callback, this.datas);
            }
            else
            {
                SaveDataFolder saveDataFolder = SaveDataFolders[CurIndex];
                if (datas != null)
                {
                    foreach (ISaveData data in datas)
                    {
                        data.Path = ConfigPaths[CurIndex];
                        ML.Engine.Manager.GameManager.Instance.SaveManager.SaveData(data);
                        SaveType saveType = ML.Engine.Manager.GameManager.Instance.SaveManager.Config.SaveType;
                        saveDataFolder.FileMap[data.SaveName] = Path.Combine(SavePath, data.Path, data.SaveName);
                    }
                }
                saveDataFolder.IsDirty = true;
                saveDataFolder.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ML.Engine.Manager.GameManager.Instance.SaveManager.SaveData(saveDataFolder);
                callback?.Invoke();
            }
        }
        public SaveDataFolder CreateSaveDataFolder(int index, string name, Action callback, List<ISaveData> datalist = null)
        {
            if (!string.IsNullOrEmpty(name) && index >= 0 && index < MAXSAVEDATACOUNT)
            {
                if (!string.IsNullOrEmpty(ConfigPaths[index]))
                {
                    string oldPath = Path.Combine(SavePath, ConfigPaths[index]);
                    if (Directory.Exists(oldPath))
                    {
                        Directory.Delete(oldPath, true);
                    }
                }

                SaveDataFolder saveDataFolder = new SaveDataFolder();
                string path = Path.Combine(SavePath, name);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                Directory.CreateDirectory(path);
                DirectoryInfo info = new DirectoryInfo(path);
                saveDataFolder.Name = info.Name;
                saveDataFolder.CreateTime = info.CreationTime.ToString();
                saveDataFolder.IsDirty = true;
                saveDataFolder.SaveName = "SaveConfig";
                saveDataFolder.Path = name;
                ConfigPaths[index] = saveDataFolder.Name;
                SaveDataFolders[index] = saveDataFolder;
                if (datalist != null)
                {
                    foreach (ISaveData data in datalist)
                    {
                        data.IsDirty = true;
                        data.Path = name;
                        ML.Engine.Manager.GameManager.Instance.SaveManager.SaveData(data);
                        SaveType saveType = ML.Engine.Manager.GameManager.Instance.SaveManager.Config.SaveType;
                        saveDataFolder.FileMap[data.SaveName] = Path.Combine(SavePath, data.Path, data.SaveName);
                    }
                }
                saveDataFolder.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ML.Engine.Manager.GameManager.Instance.SaveManager.SaveData(saveDataFolder);
                callback?.Invoke();
                return saveDataFolder;
            }
            return null;
        }
        #endregion
    }
}