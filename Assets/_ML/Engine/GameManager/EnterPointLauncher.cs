using ML.Engine.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterPointLauncher : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.EnterPoint.EnterGame();
    }

}
