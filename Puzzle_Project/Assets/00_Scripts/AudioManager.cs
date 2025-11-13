using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;
    private Dictionary<string, AudioClip> audios = new Dictionary<string, AudioClip>();
    private AudioSource source;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        source = GetComponent<AudioSource>();
    }

    public void PlaySound(string temp)
    {
        AudioClip audio = null;
        if (!audios.ContainsKey(temp))
        {
            audio = Resources.Load<AudioClip>("Sound/" + temp);
        }
        else audio = audios[temp];
        
        source.PlayOneShot(audio);
    }
}
