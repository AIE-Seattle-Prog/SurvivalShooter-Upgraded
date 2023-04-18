using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceDecorator : MonoBehaviour
{
    public AudioClipMetadata clip;
    [SerializeField]
    private AudioSource audioSource;
    
    private void OnEnable()
    {
        if (audioSource.playOnAwake)
        {
            audioSource.Play(clip);
        }
    }
}
