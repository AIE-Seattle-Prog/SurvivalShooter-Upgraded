using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
using IniParser.Configuration;
using IniParser.Model;
using IniParser.Parser;
using IniParser;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class UserSettingsSystem
{
    private static float soundLevel = -10;
    public static float SoundLevel
    {
        get => soundLevel;
        set => soundLevel = value;
    }
    
    private static float musicLevel = -10;
    public static float MusicLevel
    {
        get => musicLevel;
        set => musicLevel = value;
    }

    private static bool showSubtitle = true;
    public static bool ShowSubtitle
    {
        get => showSubtitle;
        set => showSubtitle = value;
    }
    private static bool showClosedCaptions = true;
    public static bool ShowClosedCaptions
    {
        get => showClosedCaptions;
        set => showClosedCaptions = value;
    }

    private static int graphicsPreset = -1;
    public static int GraphicsPreset
    {
        get => graphicsPreset;
        set => graphicsPreset = value;
    }

    public static string CONFIG_NAME = "config.ini";
    public static string CONFIG_PATH = Application.persistentDataPath + "/" + CONFIG_NAME;

    private static IniData data;
    private static AsyncOperationHandle<AudioMixer> loadMixerOperation;

    public static void SaveSettings()
    {
        FileIniDataParser parser = new();

        data["audio"]["sound"] = SoundLevel.ToString();
        data["audio"]["music"] = MusicLevel.ToString();
        data["audio"]["showSubtitles"] = ShowSubtitle.ToString();
        data["audio"]["showClosedCaptions"] = ShowClosedCaptions.ToString();
        data["graphics"]["preset"] = GraphicsPreset.ToString();

        parser.WriteFile(CONFIG_PATH, data);
    }

    static void LoadSettings()
    {
        FileIniDataParser parser = new();
        data = parser.ReadFile(CONFIG_PATH);

        if (data.TryGetKey("audio.sound", out var soundStr)) { SoundLevel = float.Parse(soundStr); }
        if (data.TryGetKey("audio.music", out var musicStr)) { MusicLevel = float.Parse(musicStr); }
        if (data.TryGetKey("audio.showSubtitles", out var subtitleStr)) { ShowSubtitle = bool.Parse(subtitleStr); }
        if (data.TryGetKey("audio.showClosedCaptions", out var closedCapStr)) { ShowClosedCaptions = bool.Parse(closedCapStr); }
        if (data.TryGetKey("graphics.preset", out var graphicsStr)) { GraphicsPreset = int.Parse(graphicsStr); }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void OnApplicationInit()
    {
        if(File.Exists(CONFIG_PATH))
        {
            LoadSettings();
        }
        else
        {
            data = new IniData();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnApplicationFirstSceneLoad()
    {
        // setup end of game hook for saving
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += OnEditorApplicationPlayModeChanged;
#else
        Application.quitting += OnApplicationQuit;
#endif
        //
        // apply settings on startup
        //

        loadMixerOperation = Addressables.LoadAssetAsync<AudioMixer>("MasterMixer");
        loadMixerOperation.Completed += Handle_MasterMixerLoaded;

        // if not -1, override platform default w/ user preference
        if(GraphicsPreset != -1)
        {
            QualitySettings.SetQualityLevel(GraphicsPreset);
        }
    }

#if UNITY_EDITOR
    private static void OnEditorApplicationPlayModeChanged(UnityEditor.PlayModeStateChange obj)
    {
        if (obj == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnEditorApplicationPlayModeChanged;
            OnApplicationQuit();
        }
    }
#endif

    private static void Handle_MasterMixerLoaded(AsyncOperationHandle<AudioMixer> operation)
    {
        if(operation.Status == AsyncOperationStatus.Succeeded)
        {
            operation.Result.SetFloat("musicVol", MusicLevel);
            operation.Result.SetFloat("sfxVol", SoundLevel);
        }
        else
        {
            Debug.LogError("Failed to load Master Mixer; cannot apply sound settings.");
        }
    }

    static void OnApplicationQuit()
    {
        Debug.Log("Game shutting down, saving settings!");

        SaveSettings();
    }
}
