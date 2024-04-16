using System;
using System.Collections.Generic;

namespace ML.Engine.SaveSystem
{
    public class GlobalSaveDataFolder : ISaveData
    {
        #region ISaveData
        public string SavePath { get; set; } = "";
        public string SaveName { get; set; } = "GlobalSaveConfig";
        public bool IsDirty { get; set; }
        public Manager.GameManager.Version Version { get; set; }
        #endregion

        /// <summary>
        /// �浵����
        /// </summary>
        public List<string> SaveDataFolders = new List<string>();
        public GlobalSaveDataFolder(){}
        public GlobalSaveDataFolder(List<string> saveDataFolders, Manager.GameManager.Version version)
        {
            this.SaveDataFolders = new List<string>(saveDataFolders);
            this.Version = version;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}