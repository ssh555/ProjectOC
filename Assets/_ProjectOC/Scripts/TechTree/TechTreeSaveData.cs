using ProjectOC.TechTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    public class TechTreeSaveData : ISaveData
    {
        public List<TechPoint> Datas;
        public List<TechTreeManager.UnlockingTechPoint> Unlocks;

        public TechTreeSaveData() : base("TechTreeSaveData", "TechTreeSaveData")
        {
            this.Datas = new List<TechPoint>();
            this.Unlocks = new List<TechTreeManager.UnlockingTechPoint>();
        }
        public TechTreeSaveData(List<TechPoint> datas, List<TechTreeManager.UnlockingTechPoint> unlocks) : base("TechTreeSaveData", "TechTreeSaveData")
        {
            Reset(datas);
            Reset(unlocks);
        }
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
            if(unlocks != null)
            {
                this.Unlocks = new List<TechTreeManager.UnlockingTechPoint>();
                this.Unlocks.AddRange(unlocks);
                this.IsDirty = true;
            }
        }
    }
}

