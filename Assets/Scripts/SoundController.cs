using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SoundController : MonoBehaviour {

	string SOUND_INDICATOR_ISON = "SOUND_INDICATOR_ISON";

	public Toggle  soundIndicator;

	// Use this for initialization
	void Start () {
		// Khoi tao gia tri cho LightIndicator
		bool isOn = PlayerPrefs.HasKey (SOUND_INDICATOR_ISON) ? PlayerPrefs.GetInt(SOUND_INDICATOR_ISON) == 1 : true;
		this.soundIndicator.isOn = isOn;
		this.updateAudioSource ();
	}

	public void OnSoundIndicatorChange(){
		this.updateAudioSource ();
		PlayerPrefs.SetInt(SOUND_INDICATOR_ISON, this.soundIndicator.isOn ? 1 : 0);
	}

	private void updateAudioSource(){
		this.GetComponents<AudioSource>()[0].mute = !soundIndicator.isOn;
		this.GetComponents<AudioSource>()[1].mute = !soundIndicator.isOn;
	}
	
}
