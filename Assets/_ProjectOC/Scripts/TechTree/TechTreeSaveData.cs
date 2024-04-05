using ProjectOC.TechTree;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    public class TechTreeSaveData : ISaveData
    {
        #region ISaveData
        public string SavePath { get; set; } = "";
        public string SaveName { get; set; } = "";
        public bool IsDirty { get; set; }
        public ML.Engine.Utility.Version Version { get; set; }
        #endregion

        public List<TechPoint> Datas = new List<TechPoint>();
        public List<TechTreeManager.UnlockingTechPoint> Unlocks = new List<TechTreeManager.UnlockingTechPoint>();

        public TechTreeSaveData(){ }

        public void Reset(List<TechPoint> datas)
        {
            if (datas != null)
            {
                this.Datas = new List<TechPoint>();
                this.Datas.AddRange(datas);
                this.IsDirty = true;
            }
        }
        public void Reset(List<TechTreeManager.UnlockingTechPoint> unlocks)
        {
            if (unlocks != null)
            {
                this.Unlocks = new List<TechTreeManager.UnlockingTechPoint>();
                this.Unlocks.AddRange(unlocks);
                this.IsDirty = true;
            }
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}

