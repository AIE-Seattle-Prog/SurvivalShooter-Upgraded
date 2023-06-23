using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseManager : MonoBehaviour
{	
	public static PauseManager Instance { get; private set; }

	public AudioMixerSnapshot paused;
	public AudioMixerSnapshot unpaused;

	public bool IsPaused => Time.timeScale == 0;

	public GameObject pausePanel;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void HandleGameReset()
    {
		Instance = null;
    }

	private void Awake()
    {
		Instance = this;
    }

	public void SetPause(bool shouldPause)
	{
		pausePanel.SetActive(shouldPause);
		PauseTime(shouldPause);
	}

	private void PauseTime(bool shouldPause)
	{
		Time.timeScale = shouldPause ? 0 : 1;

		if (shouldPause)
		{
			paused.TransitionTo(.01f);
		}
		else
		{
			unpaused.TransitionTo(.01f);
		}
	}

	public void Quit()
	{
		#if UNITY_EDITOR 
		EditorApplication.isPlaying = false;
		#else 
		Application.Quit();
		#endif
	}
}
