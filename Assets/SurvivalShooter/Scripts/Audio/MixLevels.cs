using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MixLevels : MonoBehaviour {

	public AudioMixer masterMixer;
	[Space]
	public int minAttenuation = -80;
	public int maxAttenuation = -10;

	[SerializeField]
	private Slider musicSlider;
	[SerializeField]
	private Slider soundSlider;

	public void SetSfxLvl(float sfxLvl)
	{
		float normLvl = sfxLvl / 100.0f;
		float atten = Mathf.Lerp(minAttenuation, maxAttenuation, normLvl);

		masterMixer.SetFloat("sfxVol", atten);
		UserSettingsSystem.SoundLevel = atten;
	}

	public void SetMusicLvl (float musicLvl)
	{
		float normLvl = musicLvl / 100.0f;
		float atten = Mathf.Lerp(minAttenuation, maxAttenuation, normLvl);

		masterMixer.SetFloat ("musicVol", atten);
		UserSettingsSystem.MusicLevel = atten;
	}

	private void Start()
    {
		float userSound = Mathf.InverseLerp(minAttenuation, maxAttenuation, UserSettingsSystem.SoundLevel) * 100f;
		float userMusic = Mathf.InverseLerp(minAttenuation, maxAttenuation, UserSettingsSystem.MusicLevel) * 100f;

		soundSlider.SetValueWithoutNotify(userSound);
		musicSlider.SetValueWithoutNotify(userMusic);
    }
}
