using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _TestWWise : MonoBehaviour
{
    public AK.Wwise.Event BGM;

    // Start is called before the first frame update
    void Start()
    {
        BGM.Post(this.gameObject);

    }

}
