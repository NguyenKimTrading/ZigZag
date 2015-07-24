using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	/// <summary>
	/// The speed of player.
	/// </summary>
	public float speed = 8f;

	public float acceleration = 0.01f;

	/// <summary>
	/// The status of player.
	/// </summary>
	private Status _status;

	/// Rigidbody component
	private Rigidbody rb;
	// Velocity unit vector
	private Vector3 verlocityUnitVector;

	private Vector3 startPosition;


	// Use this for initialization
	void Start () {
		this._status = Status.Wating;
		// Get reference to rigidbody component
		this.rb = GetComponent<Rigidbody> ();
		// Store start position
		this.startPosition = this.transform.position;
		// Init movement direction
		this.verlocityUnitVector = Vector3.zero;
        // Sou
	}


    public bool IsWating()
    {
        return this._status == Status.Wating;
    }

    public bool IsRunning()
    {
        return this._status == Status.Running;
    }

    public bool IsFalling()
    {
        return this._status == Status.Falling;
    }

    public void Run()
    {
        this._status = Status.Running;
        this.verlocityUnitVector = Vector3.right;
    }

    public void Stop()
    {
        this._status = Status.Stoped;
    }
	
	// Update is called once per frame
	void Update () {
	}
    
    private float nextFire;
	void FixedUpdate() {

        // Chuyen doi trang thai
        if (this._status == Status.Running)
        {
            if (this.transform.position.y < this.startPosition.y)
            {
                this._status = Status.Falling;
            }
        }

        // Cap nhat trang thai
		if (this._status == Status.Running) {
			this.rb.velocity = this.speed * this.verlocityUnitVector;
			this.speed += this.acceleration * Time.deltaTime;
		}

		if (this._status == Status.Falling) {
			if (transform.position.y < -100f) {
				this.gameObject.SetActive(false);
			}
		}
	}

	public void TurnDirection() {
		if (this.verlocityUnitVector == Vector3.right) {
            PlaySound();
			this.verlocityUnitVector = Vector3.forward;
		}
		else if (this.verlocityUnitVector == Vector3.forward) {
            PlaySound();
			this.verlocityUnitVector = Vector3.right;
		}
	}

    private void PlaySound()
    {
        GetComponent<AudioSource>().Play();
    }

	private enum Status {
		Wating,
		/// <summary>
		/// Player is running.
		/// </summary>
		Running, 
		/// <summary>
		/// Player is falling.
		/// </summary>
		Falling,
        /// <summary>
        /// Player stoped
        /// </summary>
        Stoped
	}
}
