using System.Collections;
using System.Collections.Generic;
using ML.PlayerCharacterNS;
using UnityEngine;

namespace ProjectOC.Player
{
    public class OCPlayerController : PlayerController
    {
        public PlayerCharacter currentCharacter = null;

        private List<string> ICharacterABResourcePath = new List<string>();
        public OCPlayerControllerState OCState =>State as OCPlayerControllerState;
        public OCPlayerController()
        {
            State = new OCPlayerControllerState(this);
            //ABResource Path InitÊ±¼ÓÈë
            ICharacterABResourcePath.Add("OC/Character/Player/Prefabs/PlayerCharacter.prefab");
        }
        
        
        public override ICharacter SpawnCharacter(int _index = 0, IStartPoint _startPoint = null)
        {
            IPlayerCharacter playerCharacter = null;
            ML.Engine.Manager.GameManager.Instance.ABResourceManager
                .InstantiateAsync(ICharacterABResourcePath[_index], isGlobal: true).Completed += (handle) =>
            {
                var playerCharacter = handle.Result.GetComponent<PlayerCharacter>();
                SetCharacterTransform(playerCharacter.transform, _startPoint);
                currentCharacter = playerCharacter;
                SpawnedCharacters.Add(playerCharacter);
                playerCharacter.OnSpawn(this);
            };
            return playerCharacter as ICharacter;
        }
        
    }
}
