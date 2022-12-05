using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseManager : MonoBehaviour
{	
	public static PauseManager Instance { get; private set; }

	public AudioMixerSnapshot paused;
	public AudioMixerSnapshot unpaused;
	
	Canvas canvas;

	public bool IsPaused => Time.timeScale == 0;


	private void Awake()
    {
		Instance = this;
    }

    private void Start()
	{
		canvas = GetComponent<Canvas>();
	}

	public void SetPause(bool shouldPause)
	{
		canvas.enabled = shouldPause;
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
