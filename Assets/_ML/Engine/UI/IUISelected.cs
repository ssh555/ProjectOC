                                                                                                                                                                               using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.UI
{
    public interface IUISelected
    {
        public IUISelected LeftUI { get; set; }
        public IUISelected RightUI { get; set; }
        public IUISelected UpUI { get; set; }
        public IUISelected DownUI { get; set; }
        
        public abstract void SelectedEnter();
        public abstract void SelectedExit();

        public abstract void Interact();
    }

}
