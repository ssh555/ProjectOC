using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Input
{
    public class InputManager : Manager.GlobalManager.IGlobalManager
    {
        private static InputManager instance = null;
        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Manager.GameManager.Instance.GetGlobalManager<InputManager>();
                }
                return instance;
            }
        }

        ~InputManager()
        {
            if (instance == this)
            {
                Common.Disable();
                instance = null;
            }

        }


        /// <summary>
        /// ¹«¹²²Ù×÷
        /// </summary>
        public CommomInput Common = new CommomInput();


        public InputManager()
        {
            this.Common.Enable();
        }
    }

}
