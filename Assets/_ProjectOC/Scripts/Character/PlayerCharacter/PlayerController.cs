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
    public class PlayerController : RoleController
    {
        public ICharacter currentCharacter = null;

        private List<string> ICharacterABResourcePath = new List<string>();

        public PlayerController()
        {
            SpawnedCharacters = new List<ICharacter>();
            //ABResource Path ����
            ICharacterABResourcePath.Add("OC/Character/Player/Prefabs/PlayerCharacter.prefab");
        }
        
        public override ICharacter SpawnCharacter(int _index = 0, IStartPoint _startPoint = null)
        {
            IPlayerCharacter playerCharacter = null;
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.
                InstantiateAsync(ICharacterABResourcePath[_index],isGlobal:true).Completed += (handle) =>
            {
                // ʵ����
                var playerCharacter = handle.Result.GetComponent<PlayerCharacter>();
                SetCharacterTransform(playerCharacter.transform, _startPoint);
                currentCharacter = playerCharacter;
                SpawnedCharacters.Add(playerCharacter); 
                playerCharacter.OnSpawn(this);
            };
            return playerCharacter as ICharacter;
        }


        public override void ReSpawn(ICharacter _character, IStartPoint _startPoint = null)
        {
            base.ReSpawn(_character,_startPoint);
        }
        public override void Dispose(ICharacter _character)
        {
            base.Dispose(_character);
        }
        public override void DisposeAll()
        {
            base.DisposeAll();
        }
    }
}