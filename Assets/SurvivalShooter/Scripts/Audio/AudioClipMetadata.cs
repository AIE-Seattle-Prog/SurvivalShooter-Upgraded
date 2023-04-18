using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AudioClip Metadata", menuName = "Audio/AudioClip Metadata")]
public class AudioClipMetadata : ScriptableObject
{
    [field: SerializeField]
    public AudioClip Clip { get; private set; }
    [field: SerializeField]
    public float TimeToDisplay { get; private set; } = 3.0f;
    [field: SerializeField]
    public float Cooldown { get; private set; } = 0.0f;

    [field: SerializeField, Header("Captioning")]
    public string SpokenText { get; private set; } = "";
    public bool HasSpokenText => !string.IsNullOrEmpty(SpokenText);
    [field: SerializeField]
    public string ClosedCaptioningText { get; private set; } = "";
    public bool HasClosedCaptioningText => !string.IsNullOrEmpty(ClosedCaptioningText);
}