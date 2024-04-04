using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public class AIController : IController
    {
        public List<ICharacter> SpawnedCharacters { get; set; }
        public IControllerState State { get; set; }
        public IStartPoint startPoint { get; set; }
        public ICharacter SpawnCharacter()
        {
            ICharacter character = null;
            return character;
        }

        public ICharacter SpawnCharacter(int _index = 0,Transform _transf = null)
        {
            return null;
        }

        public void ReSpawn()
        {
            //to-do
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