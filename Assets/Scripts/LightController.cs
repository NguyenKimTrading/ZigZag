using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightController : MonoBehaviour {

	string LIGHT_INDICATOR_VALUE = "LIGHT_INDICATOR_VALUE";

	public Slider lightIndicator;

	// Use this for initialization
	void Start () {
		// Khoi tao gia tri cho LightIndicator
		float value = PlayerPrefs.HasKey (LIGHT_INDICATOR_VALUE) ? PlayerPrefs.GetFloat(LIGHT_INDICATOR_VALUE) : 0.5f;
		this.lightIndicator.value = value;
		this.updateLightComponent ();
	}

	public void OnLightIndicatorChange(){
		this.updateLightComponent ();
		PlayerPrefs.SetFloat(LIGHT_INDICATOR_VALUE, this.lightIndicator.value);
	}

	private void updateLightComponent(){
		this.GetComponent<Light>().intensity = 0.5f + lightIndicator.value * 1.5f;
	}

}
