using ML.Engine.Timer;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using ProjectOC.Player;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

namespace ML.PlayerCharacterNS
{
    public class PlayerController : IController,ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        public ICharacter currentCharacter = null;
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
                Debug.Log($"Instantiate over: {playerCharacter}");
                currentCharacter = playerCharacter;
                SpawnedCharacters.Add(playerCharacter); 
            };
            Debug.Log($"Instantiate outerrr: {playerCharacter}");
            return playerCharacter as ICharacter;
        }


        public void ReSpawn(ICharacter _character,Transform _transf = null)
        {
            int _index = _character.prefabIndex;
            Dispose(_character);
            SpawnCharacter(_index,_transf);
        }
        public void Dispose(ICharacter character,bool _destoryModel = true)
        {
            if (_destoryModel)
            {
                //销毁物体
                ML.Engine.Manager.GameManager.DestroyObj(character.transform.gameObject);
            }
            this.SpawnedCharacters.Remove(character);
        }
        public void DisposeAll()
        {
            foreach (var _character in SpawnedCharacters)
            {
                Dispose(_character);
            }
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