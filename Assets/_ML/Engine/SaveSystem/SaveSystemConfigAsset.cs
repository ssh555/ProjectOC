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
    /// �ʲ������ļ�
    /// </summary>
    [CreateAssetMenu(fileName = "SaveSystemConfig", menuName = "ML/SaveSystem/SaveSystemConfig", order = 1)]
    public class SaveSystemConfigAsset : SerializedScriptableObject
    {
        /// <summary>
        /// ʵ���������ݣ������ʾֻ��
        /// </summary>
        [ReadOnly, HideIf("IsEditing")]
        public SaveConfig Config = new SaveConfig();
#if UNITY_EDITOR
#pragma warning disable 0414
        /// <summary>
        /// �Ƿ����ڱ༭
        /// </summary>
        private bool IsEditing;
#pragma warning restore 0414
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
        /// �����޸�
        /// ����ʱ�Ὣ�浵���ݶ��룬�����µ�������������д��浵�ļ�����ɾ���ɵĴ浵�ļ�
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