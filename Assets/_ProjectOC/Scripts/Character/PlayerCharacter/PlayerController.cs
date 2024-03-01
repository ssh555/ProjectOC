using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.PlayerCharacterNS
{
    public class PlayerController : IController
    {
        public List<ICharacter> SpawnedCharacters { get; set; }
        public IControllerState State { get; set; }
        public IStartPoint startPoint { get; set; }
        public ICharacter SpawnCharacter()
        {
            ICharacter character = null;
            return character;
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
    }
}