using System.Collections;
using System.Collections.Generic;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using UnityEngine;

namespace ProjectOC.PlayerCharacterNS
{
    public class CharacterManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        List<PlayerController> playerControllers;
        List<IPlayerCharacter> playerCharacters;
        List<IAICharacter> AICharacters;
        int CurrentControllerIndex;

        private string CharacterPrefabABRPath = "OC/Character/Player/Prefabs/PlayerCharacter.prefab";

        public PlayerCharacter SpawnPlayerCharacter()
        {
            PlayerCharacter player = null;
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(CharacterPrefabABRPath).Completed += (handle) =>
            {
                // สตภýปฏ
                player = handle.Result.GetComponent<PlayerCharacter>();
                player.transform.position = GameObject.Find("PlayerSpawnPoint").transform.position;
            };
            return player;
        }
        
        
        public PlayerController GetCurrentController(int index)
        {
            return playerControllers[index];
        }

        public void AddPlayerController(PlayerController controller)
        {
            playerControllers.Add(controller);
        }
        public void AddPlayerCharacter(IPlayerCharacter character)
        {
            playerCharacters.Add(character);
        }
        public void AddAICharacter(IAICharacter character)
        {
            AICharacters.Add(character);
        }
        public void RemovePlayerCharacter(IPlayerCharacter character)
        {
            playerCharacters.Remove(character);
        }
        public void RemoveAICharacter(IAICharacter character)
        {
            AICharacters.Remove(character);
        }
    }
}