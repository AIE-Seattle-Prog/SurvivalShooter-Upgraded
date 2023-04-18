using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioSourceExtensions
{
    public static void PlayOneShot(this AudioSource audioSource, AudioClipMetadata clipMetadata, float volumeLevel=1.0f)
    {
        audioSource.PlayOneShot(clipMetadata.Clip, volumeLevel);
        SubtitlePanel.Instance.AddSubtitle(clipMetadata);
    }
    
    public static void Play(this AudioSource audioSource, AudioClipMetadata clipMetadata)
    {
        audioSource.clip = clipMetadata.Clip;
        audioSource.Play();
        SubtitlePanel.Instance.AddSubtitle(clipMetadata);
    }
}
