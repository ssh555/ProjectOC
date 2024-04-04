using ML.Engine.Timer;
using System.Collections;
using System.Collections.Generic;
using ProjectOC.Player;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ML.PlayerCharacterNS
{
    public class PlayerController : IController,ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        public ICharacter currentCharacter;
        public List<ICharacter> SpawnedCharacters { get; set; }
        public IControllerState State { get; set; }
        public IStartPoint startPoint { get; set; }
        private List<string> ICharacterABResourcePath = new List<string>();

        public PlayerController()
        {
            SpawnedCharacters = new List<ICharacter>();
            //ABResource Path 加入
            ICharacterABResourcePath.Add("OC/Character/Player/Prefabs/PlayerCharacter.prefab");
        }
        
        public ICharacter SpawnCharacter(int _index = 0,Transform _transf = null)
        {
            IPlayerCharacter playerCharacter = null;
            if (_transf == null)
            {
                _transf = GameObject.Find("PlayerSpawnPoint").transform;
            }
            
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.
                InstantiateAsync(ICharacterABResourcePath[_index],isGlobal:true).Completed += (handle) =>
            {
                // 实例化
                var playerCharacter = handle.Result.GetComponent<PlayerCharacter>();
                playerCharacter.transform.position = _transf.position;
                
            };

            currentCharacter = playerCharacter;
            SpawnedCharacters.Add(playerCharacter); 
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