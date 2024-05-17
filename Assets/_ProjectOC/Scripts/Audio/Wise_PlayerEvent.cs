using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wise_PlayerEvent : MonoBehaviour
{
    public AK.Wwise.Event Act1;
    public AK.Wwise.Event Act2;
    public AK.Wwise.Event Act3;
    public AK.Wwise.Event Act4;

    void AudioPlayRun()
    {
        Act1.Post(gameObject);
    }

    void AudioPlayJump()
    {
        Act2.Post(gameObject);
    }

    void AudioPlaySprint()
    {
        Act3.Post(gameObject);
    }

    void AudioPlayCollect()
    {
        Act4.Post(gameObject);
    }
}
