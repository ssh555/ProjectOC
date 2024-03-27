using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// 资产配置文件
    /// </summary>
    [CreateAssetMenu(fileName = "SaveSystemConfig", menuName = "ML/SaveSystem/SaveSystemConfig", order = 1)]
    public class SaveSystemConfigAsset : SerializedScriptableObject
    {
        /// <summary>
        /// 实际配置数据，面板显示只读
        /// </summary>
        [ReadOnly, HideIf("IsEditing")]
        public SaveConfig Config = new SaveConfig();
#if UNITY_EDITOR
        /// <summary>
        /// 是否正在编辑
        /// </summary>
        private bool IsEditing;
        #region 面板交互按钮
        /// <summary>
        /// 启用编辑时显示Temp，隐藏Config
        /// </summary>
        [ShowIf("IsEditing")]
        public SaveConfig Temp;

        /// <summary>
        /// 编辑按钮
        /// </summary>
        [Button("Edit"), HideIf("IsEditing")]
        protected void EditButton()
        {
            IsEditing = true;
            Temp = new SaveConfig(Config);
        }
        /// <summary>
        /// 保存修改
        /// </summary>
        [Button("Save"), ShowIf("IsEditing")]
        protected void SaveButton()
        {
            // 保存时会将存档数据读入，根据新的配置数据重新写入存档文件，并删除旧的存档文件
            SaveSystem oldSystem;
            string oldExt;
            if (Config.SaveType == SaveType.XML)
            {
                oldSystem = new XMLSaveSystem();
                oldExt = "xml";
            }
            else
            {
                oldSystem = new JsonSaveSystem();
                oldExt = "json";
            }
            SaveSystem newSystem;
            string newExt;
            if (Temp.SaveType == SaveType.XML)
            {
                newSystem = new XMLSaveSystem();
                newExt = "xml";
            }
            else
            {
                newSystem = new JsonSaveSystem();
                newExt = "json";
            }

            string root = Path.Combine(Application.persistentDataPath, "Save");
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            if (File.Exists(Path.Combine(root, "GlobalSaveConfig."+oldExt)))
            {
                GlobalSaveDataFolder global = oldSystem.LoadData<GlobalSaveDataFolder>("GlobalSaveConfig", Config.UseEncrption);
                global.IsDirty = true;
                global.SaveName = "GlobalSaveConfig";
                if (Config.SaveType != Temp.SaveType)
                {
                    File.Delete(Path.Combine(root, "GlobalSaveConfig."+oldExt));
                }
                newSystem.SaveData(global, Temp.UseEncrption);
            }

            List<SaveDataFolder> saveDataFolders = new List<SaveDataFolder>();
            foreach (var dir in Directory.GetDirectories(root))
            {
                DirectoryInfo info = new DirectoryInfo(dir);
                SaveDataFolder saveDataFolder = oldSystem.LoadData<SaveDataFolder>(Path.Combine(info.Name, "SaveConfig"), Config.UseEncrption);
                saveDataFolder.IsDirty = true;
                saveDataFolder.SaveName = "SaveConfig";
                saveDataFolder.Path = info.Name;
                saveDataFolders.Add(saveDataFolder);
            }

            Dictionary<string, MethodInfo> MethodDict = new Dictionary<string, MethodInfo>();
            foreach (SaveDataFolder saveDataFolder in saveDataFolders)
            {
                saveDataFolder.IsDirty = true;
                foreach (var kv in saveDataFolder.FileMap)
                {
                    if (!MethodDict.ContainsKey(kv.Key))
                    {
                        Type type = Type.GetType(kv.Key);
                        MethodInfo method = oldSystem.GetType().GetMethod("LoadData", new[] { typeof(string), typeof(bool) }).MakeGenericMethod(type);
                        MethodDict.Add(kv.Key, method);
                    }
                    object loadData = MethodDict[kv.Key].Invoke(oldSystem, new object[] { Path.Combine(saveDataFolder.Path, kv.Key), Config.UseEncrption });
                    ISaveData data = (ISaveData)loadData;
                    data.IsDirty = true;
                    data.SaveName = kv.Key;
                    data.Path = saveDataFolder.Path;
                    newSystem.SaveData(data, Temp.UseEncrption);
                }
                if (Config.SaveType != Temp.SaveType)
                {
                    foreach (var kv in saveDataFolder.FileMap.ToList())
                    {
                        File.Delete(kv.Value);
                        saveDataFolder.FileMap[kv.Key] = kv.Value.Replace(oldExt, newExt);
                    }
                    string path = Path.Combine(root, saveDataFolder.Path, saveDataFolder.SaveName);
                    File.Delete(path + "." + oldExt);
                }
                newSystem.SaveData(saveDataFolder, Temp.UseEncrption);
            }
            Config.SaveType = Temp.SaveType;
            Config.UseEncrption = Temp.UseEncrption;
            IsEditing = false;
        }
        /// <summary>
        /// 取消修改
        /// </summary>
        [Button("Cancel"), ShowIf("IsEditing")]
        protected void CancelButton()
        {
            IsEditing = false;
        }
        #endregion
#endif
    }
}