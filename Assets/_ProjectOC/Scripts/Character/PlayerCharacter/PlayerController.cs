using ML.Engine.Timer;
using System.Collections;
using System.Collections.Generic;
using ProjectOC.Player;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectOC.PlayerCharacterNS
{
    public class PlayerController : IController,ITickComponent
    {
        public List<ICharacter> SpawnedCharacters { get; set; }
        public IControllerState State { get; set; }
        public IStartPoint startPoint { get; set; }
        
        public ICharacter SpawnCharacter()
        {
            PlayerCharacter playerCharacter = null;
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/Character/Player/Prefabs/PlayerCharacter.prefab").Completed += (handle) =>
            {
                // สตภýปฏ
                playerCharacter = handle.Result.GetComponent<PlayerCharacter>();
                playerCharacter.transform.position = GameObject.Find("PlayerSpawnPoint").transform.position;
                
            };
            
            return playerCharacter as ICharacter;
        }
        public void ReSpawn()
        {
            
        }
        public void Dispose(ICharacter character)
        {
            this.SpawnedCharacters.Remove(character);
        }
        public void DisposeAll()
        {
            this.SpawnedCharacters.Clear();
        }

        #region ITickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        public virtual void Tick(float deltatime)
        {

        }
        public virtual void FixedTick(float deltatime)
        {

        }
        public virtual void LateTick(float deltatime)
        {

        }
        #endregion
    }
}