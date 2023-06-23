using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class SubtitlePanel : MonoBehaviour
{
    public RectTransform content;
    public CanvasGroup canvasGroup;
    
    public GameObject subtitlePrefab;

    private List<SubtitleDisplay> activeSubtitles = new();

    private bool showSubtitles;
    public bool ShowSubtitles
    {
        get => showSubtitles;
        set
        {
            showSubtitles = value;
            UserSettingsSystem.ShowSubtitle = value;
        }
    }
    private bool showClosedCaptioning;
    public bool ShowClosedCaptioning
    {
        get => showClosedCaptioning;
        set
        {
            showClosedCaptioning = value;
            UserSettingsSystem.ShowClosedCaptions = value;
        }
    }
    
    public int maxSubtitleCount = 3;

    private Dictionary<AudioClipMetadata, float> lastSubtitlePlaytime = new();

    public class SubtitleDisplay
    {
        public SubtitleLine subtitle;
        public float duration = 3.0f;
        private float startTime;

        public bool ShouldRemove => DisplayTime > duration;
        public float DisplayTime => Time.realtimeSinceStartup - startTime;
        
        public SubtitleDisplay(SubtitleLine subtitle, float duration)
        {
            this.subtitle = subtitle;
            this.duration = duration;

            startTime = Time.realtimeSinceStartup;
        }
        
    }
    
    public static SubtitlePanel Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void HandleGameReset()
    {
        Instance = null;
    }

    public void AddSubtitle(string subtitle, float displayTime)
    {
        // early exit if subtitle empty
        if(string.IsNullOrWhiteSpace(subtitle))
        {
            return;
        }

        // remove the oldest subtitle if we would have too many
        if (activeSubtitles.Count+1 > maxSubtitleCount)
        {
            // find the oldest subtitle display
            float oldestTime = float.MinValue;
            SubtitleDisplay oldestDisplay = null;
            foreach(var display in activeSubtitles)
            {
                if (display.DisplayTime > oldestTime)
                {
                    oldestTime = display.DisplayTime;
                    oldestDisplay = display;
                }
            }
            
            RemoveSubtitle(oldestDisplay);
        }
        
        // add subtitle
        var subtitleLine = Instantiate(subtitlePrefab, content).GetComponent<SubtitleLine>();
        subtitleLine.subtitleLabel.text = subtitle;
        activeSubtitles.Add(new SubtitleDisplay(subtitleLine, displayTime));
        
        canvasGroup.alpha = activeSubtitles.Count > 0 ? 1.0f : 0.0f;
    }
    
    public void AddSubtitle(AudioClipMetadata clip)
    {
        if (clip.Cooldown > 0.0f)
        {
            // skip clip if on cooldown
            if (lastSubtitlePlaytime.TryGetValue(clip, out float lastPlaytime) &&
                Time.realtimeSinceStartup - lastPlaytime < clip.Cooldown)
            {
                return;
            }
            
            // record latest play
            lastSubtitlePlaytime[clip] = Time.realtimeSinceStartup;
        }
        
        string finalText = "";
        if (ShowSubtitles && clip.HasSpokenText) finalText += clip.SpokenText;
        if (ShowClosedCaptioning  && clip.HasClosedCaptioningText) finalText += $"[{clip.ClosedCaptioningText}]";

        AddSubtitle(finalText, clip.TimeToDisplay);
    }
    
    private void RemoveSubtitle(SubtitleDisplay subtitle)
    {
        activeSubtitles.Remove(subtitle);
        Destroy(subtitle.subtitle.gameObject);
        
        canvasGroup.alpha = activeSubtitles.Count > 0 ? 1.0f : 0.0f;
    }

    private void Awake()
    {
        Instance = this;

        canvasGroup.alpha = activeSubtitles.Count > 0 ? 1.0f : 0.0f;
    }

    private void OnEnable()
    {
        ShowSubtitles = UserSettingsSystem.ShowSubtitle;
        ShowClosedCaptioning = UserSettingsSystem.ShowClosedCaptions;
    }

    private void FixedUpdate()
    {
        // check if any subtitles should be removed
        for(int i = 0; i < activeSubtitles.Count; i++)
        {
            if (activeSubtitles[i].ShouldRemove)
            {
                RemoveSubtitle(activeSubtitles[i]);

                // offset the index since we removed an item
                --i;
                // immediately continue to avoid double-evaluation
                continue;
            }
        }
    }
}
