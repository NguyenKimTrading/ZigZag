using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public GameObject Player;
	private PlayerController PlayerController;

	private Vector3 Offset;

	public float Smoothing = 5f;

	// Use this for initialization
	void Start () {
		Offset = transform.position - Player.transform.position;
		this.PlayerController = Player.GetComponent<PlayerController> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate () {
		if (this.PlayerController.IsRunning()) {
			Vector3 targetCameraPosition = Player.transform.position + Offset;
			transform.position = Vector3.Lerp(transform.position, targetCameraPosition, Time.deltaTime * Smoothing);
		}
	}
}
