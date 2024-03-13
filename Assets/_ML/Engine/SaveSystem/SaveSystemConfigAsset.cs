using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        [ReadOnly]
        public SaveConfig Config = new SaveConfig();
        /// <summary>
        /// 是否正在编辑
        /// </summary>
        private bool IsEditing;

#if UNITY_EDITOR

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
            Temp = new SaveConfig();
        }
        /// <summary>
        /// 保存修改
        /// </summary>
        [Button("Save"), ShowIf("IsEditing")]
        protected void SaveButton()
        {
            // 保存时会将存档数据读入，根据新的配置数据重新写入存档文件，并删除旧的存档文件
            SaveSystem oldSystem;
            if (Config.SaveType == SaveType.Binary)
            {
                oldSystem = new BinarySaveSystem();
            }
            else
            {
                oldSystem = new JsonSaveSystem();
            }
            SaveSystem newSystem;
            if (Temp.SaveType == SaveType.Binary)
            {
                newSystem = new BinarySaveSystem();
            }
            else
            {
                newSystem = new JsonSaveSystem();
            }

            string root = Path.Combine(Application.persistentDataPath, "Save");
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            List<SaveDataFolder> saveDataFolders = new List<SaveDataFolder>();
            foreach (var dir in Directory.GetDirectories(root))
            {
                DirectoryInfo info = new DirectoryInfo(dir);
                SaveDataFolder saveDataFolder = oldSystem.LoadData<SaveDataFolder>(Path.Combine(info.Name, "SaveConfig"), Config.UseEncrption);
                saveDataFolder.SaveName = "SaveConfig";
                saveDataFolder.Path = info.Name;

                saveDataFolders.Add(saveDataFolder);
            }

            foreach (SaveDataFolder saveDataFolder in saveDataFolders)
            {
                saveDataFolder.IsDirty = true;
                foreach (var kv in saveDataFolder.FileMap)
                {
                    ISaveData data = oldSystem.LoadData<ISaveData>(Path.Combine(saveDataFolder.Path, kv.Key), Config.UseEncrption);
                    data.IsDirty = true;
                    data.SaveName = kv.Key;
                    data.Path = saveDataFolder.Path;
                    newSystem.SaveData(data, Temp.UseEncrption);
                }

                newSystem.SaveData<SaveDataFolder>(saveDataFolder, Temp.UseEncrption);
                if (Config.SaveType != Temp.SaveType)
                {
                    foreach (var kv in saveDataFolder.FileMap)
                    {
                        if (Config.SaveType == SaveType.Binary)
                        {
                            File.Delete(kv.Value + ".bytes");
                        }
                        else
                        {
                            File.Delete(kv.Value + ".json");
                        }
                    }
                    string path = Path.Combine(root, saveDataFolder.Path, saveDataFolder.SaveName);
                    if (Config.SaveType == SaveType.Binary)
                    {
                        File.Delete(path + ".bytes");
                    }
                    else
                    {
                        File.Delete(path + ".json");
                    }
                }
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