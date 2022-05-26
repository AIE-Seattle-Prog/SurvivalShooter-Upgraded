using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class MixLevels : MonoBehaviour {

	public AudioMixer masterMixer;
	[Space]
	public int minAttenuation = -80;
	public int maxAttenuation = -10;

	public void SetSfxLvl(float sfxLvl)
	{
		float normLvl = sfxLvl / 100.0f;
		float atten = Mathf.Lerp(minAttenuation, maxAttenuation, normLvl);

		masterMixer.SetFloat("sfxVol", atten);
	}

	public void SetMusicLvl (float musicLvl)
	{
		float normLvl = musicLvl / 100.0f;
		float atten = Mathf.Lerp(minAttenuation, maxAttenuation, normLvl);

		masterMixer.SetFloat ("musicVol", atten);
	}
}
