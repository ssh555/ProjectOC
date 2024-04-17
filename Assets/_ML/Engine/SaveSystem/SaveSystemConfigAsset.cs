using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
#pragma warning disable 0414
        /// <summary>
        /// 是否正在编辑
        /// </summary>
        private bool IsEditing;
#pragma warning restore 0414
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
        private SaveSystem GetSystem(SaveConfig config)
        {
            if (config.SaveType == SaveType.XML)
            {
                return new XMLSaveSystem();
            }
            else
            {
                return new JsonSaveSystem();
            }
        }

        /// <summary>
        /// 保存修改
        /// 保存时会将存档数据读入，根据新的配置数据重新写入存档文件，并删除旧的存档文件
        /// </summary>
        [Button("Save"), ShowIf("IsEditing")]
        protected void SaveButton()
        {
            SaveSystem oldSystem = GetSystem(Config);
            SaveSystem newSystem = GetSystem(Temp);

            string root = Path.Combine(Application.persistentDataPath, "Save");
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            if (File.Exists(Path.Combine(root, "GlobalSaveConfig")))
            {
                GlobalSaveDataFolder global = oldSystem.LoadData<GlobalSaveDataFolder>("GlobalSaveConfig", Config.UseEncrption);
                global.IsDirty = true;
                newSystem.SaveData(global, Temp.UseEncrption);
            }

            List<SaveDataFolder> saveDataFolders = new List<SaveDataFolder>();
            foreach (var dir in Directory.GetDirectories(root))
            {
                DirectoryInfo info = new DirectoryInfo(dir);
                SaveDataFolder saveDataFolder = oldSystem.LoadData<SaveDataFolder>(Path.Combine(info.Name, "SaveConfig"), Config.UseEncrption);
                if (saveDataFolder != null)
                {
                    saveDataFolder.SavePath = info.Name;
                    saveDataFolder.IsDirty = true;
                    saveDataFolders.Add(saveDataFolder);
                }
            }

            Dictionary<string, MethodInfo> MethodDict = new Dictionary<string, MethodInfo>();
            foreach (SaveDataFolder saveDataFolder in saveDataFolders)
            {
                saveDataFolder.IsDirty = true;
                foreach (var kv in saveDataFolder.FileMap)
                {
                    if (!MethodDict.ContainsKey(kv.Value))
                    {
                        Type type = Type.GetType(kv.Value);
                        MethodInfo method = oldSystem.GetType().GetMethod("LoadData", new[] { typeof(string), typeof(bool) }).MakeGenericMethod(type);
                        MethodDict.Add(kv.Value, method);
                    }
                    string fileName = Path.GetFileName(kv.Key);
                    object loadData = MethodDict[kv.Value].Invoke(oldSystem, new object[] { Path.Combine(saveDataFolder.SavePath, fileName), Config.UseEncrption });
                    if (loadData != null)
                    {
                        ISaveData data = (ISaveData)loadData;
                        data.IsDirty = true;
                        data.SaveName = fileName;
                        data.SavePath = saveDataFolder.SavePath;
                        newSystem.SaveData(data, Temp.UseEncrption);
                    }
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