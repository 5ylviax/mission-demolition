using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectPlayer : MonoBehaviour
{
    public AudioSource src;
    public AudioClip sfx1;

    public void Button1()
    {
        src.clip = sfx1;
        src.Play();
    }
}
