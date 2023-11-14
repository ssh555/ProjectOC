using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectOC.Terrain;

namespace ProjectOC.Player.Terrain
{
    public class PlayerTerrainDetect : MonoBehaviour
    {
        protected PlayerNormalMove.PlayerMoveSetting moveSetting;

        public void SetMoveSetting(PlayerNormalMove.PlayerMoveSetting setting)
        {
            this.moveSetting = setting;
        }

        private void OnTriggerStay(Collider other)
        {
            MonoTerrainMoveDrag comp = other.GetComponent<MonoTerrainMoveDrag>();
            if (comp != null && comp != this.moveSetting.TerrainDrag){
                if(this.moveSetting.TerrainDrag == null || comp.TerrainDrag <= this.moveSetting.TerrainDrag.TerrainDrag)
                {
                    this.moveSetting.TerrainDrag = comp;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (this.moveSetting != null && other.GetComponent<MonoTerrainMoveDrag>() == this.moveSetting.TerrainDrag)
            {
                this.moveSetting.TerrainDrag = null;
            }
        }
    }
}


