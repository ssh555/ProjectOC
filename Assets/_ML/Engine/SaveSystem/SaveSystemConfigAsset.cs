using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// �ʲ������ļ�
    /// </summary>
    [CreateAssetMenu(fileName = "SaveSystemConfig", menuName = "ML/SaveSystem/SaveSystemConfig", order = 1)]
    public class SaveSystemConfigAsset : SerializedScriptableObject
    {
        /// <summary>
        /// ʵ���������ݣ������ʾֻ��
        /// </summary>
        [ReadOnly]
        public SaveConfig Config = new SaveConfig();
        /// <summary>
        /// �Ƿ����ڱ༭
        /// </summary>
        private bool IsEditing;

#if UNITY_EDITOR

        #region ��彻����ť
        /// <summary>
        /// ���ñ༭ʱ��ʾTemp������Config
        /// </summary>
        [ShowIf("IsEditing")]
        public SaveConfig Temp;

        /// <summary>
        /// �༭��ť
        /// </summary>
        [Button("Edit"), HideIf("IsEditing")]
        protected void EditButton()
        {
            IsEditing = true;
            Temp = new SaveConfig();
        }
        /// <summary>
        /// �����޸�
        /// </summary>
        [Button("Save"), ShowIf("IsEditing")]
        protected void SaveButton()
        {
            // ����ʱ�Ὣ�浵���ݶ��룬�����µ�������������д��浵�ļ�����ɾ���ɵĴ浵�ļ�
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
        /// ȡ���޸�
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