using System.Collections;
using System.Collections.Generic;

namespace ML.Engine.LandMassExpand
{
    public class IslandManager : Manager.LocalManager.ILocalManager
    {
        private static IslandManager instance = null;

        public static IslandManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new IslandManager();
                    Manager.GameManager.Instance.RegisterLocalManager(instance);
                }

                return instance;
            }
        }

        ~IslandManager()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public int MapSize = 100;
        private List<IslandMain> islandMains;

        bool UnlockIsland(int island_Index)
        {
            return true;
        }

        //初始地图生产
        public void IslandRandomGeneration()
        {
            //to-do
        }
    }
}