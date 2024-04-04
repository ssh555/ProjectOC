using ML.Engine.Manager;
using Sirenix.OdinInspector;
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
        public Utility.Version Version { get; set; }
        #endregion

        /// <summary>
        /// ´æµµÃû³Æ
        /// </summary>
        public List<string> SaveDataFolders = new List<string>();
        public GlobalSaveDataFolder(){}
        public GlobalSaveDataFolder(List<string> saveDataFolders, Utility.Version version)
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