using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	/// <summary>
	/// The speed of player.
	/// </summary>
	public float speed = 10f;

	public float acceleration = 0.05f;

	/// <summary>
	/// The status of player.
	/// </summary>
	private Status _status;

	/// Rigidbody component
	private Rigidbody rb;
	// Velocity unit vector
	private Vector3 verlocityUnitVector;

	private Vector3 startPosition;

	private IDictionary logger = null;
	private System.Collections.Generic.List<float> turnat = new System.Collections.Generic.List<float>();

	public void SetLogger(IDictionary logger){
		this.logger = logger;
	}

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
		if (logger!= null) logger["beginat"] = Time.time;
        this._status = Status.Running;
        this.verlocityUnitVector = Vector3.right;
    }

    public void Stop()
    {
		if (logger!= null) logger["endat"] = Time.time;
		if (logger!= null) logger["turnat"] = turnat;
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
			if (transform.position.y < - 50f) {
				this.gameObject.SetActive(false);
			}
		}
	}

	public void TurnDirection() {
		turnat.Add (Time.time);
		if (this.verlocityUnitVector == Vector3.right) {
			this.verlocityUnitVector = Vector3.forward;
		}
		else if (this.verlocityUnitVector == Vector3.forward) {
			this.verlocityUnitVector = Vector3.right;
		}
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

public class GameLog{
	public string beginat;
	public string fallat;
	public string endat;
	public System.Collections.Generic.LinkedList<string> turnat = new System.Collections.Generic.LinkedList<string>();
}
