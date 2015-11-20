using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Notification : MonoBehaviour {

	public Text alertMessage;
	public float DelayTimeToHide = 2f;
	public float SpeedHide = 0.1f;

	private ShowToastStatus showsToastStatus;

	// Use this for initialization
	void Start () {
		alertMessage.text = string.Empty;
		showsToastStatus = ShowToastStatus.Hide;
	}
	
	// Update is called once per frame
	void Update () {
		if (showsToastStatus == ShowToastStatus.Show) {
			if (time + DelayTimeToHide < Time.time && autoHide) {
				time = Time.time;
				showsToastStatus = ShowToastStatus.Hidding;
			}
		} else if (showsToastStatus == ShowToastStatus.Hidding) {
			if (alertMessage.text.Length == 0){
				showsToastStatus = ShowToastStatus.Hide;
			} else {
				if (time + SpeedHide < Time.time) {
					alertMessage.text = alertMessage.text.Remove(alertMessage.text.Length-1);
					time = Time.time;
				}
			}
		}
	}

	float time;
	bool autoHide;

	public void showToast(string message, bool autoHide = true) {
		alertMessage.text = message;
		time = Time.time;
		this.autoHide = autoHide;
		showsToastStatus = ShowToastStatus.Show;
	}

	public void hideToast() {
		time = Time.time;
		showsToastStatus = ShowToastStatus.Hidding;
	}

	private enum ShowToastStatus {
		Hide, Hidding, Showwing, Show
	}
}
