using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Input
{
    public class InputManager : Engine.Utility.NoMonoSingletonClass<InputManager>, Manager.GlobalManager.IGlobalManager
    {
        /// <summary>
        /// ¹«¹²²Ù×÷
        /// </summary>
        public CommomInput Common = new CommomInput();


        public InputManager()
        {
            this.Common.Enable();
        }
    
        ~InputManager()
        {
            Common.Disable();
        }
    }

}
