using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Game controller.
/// </summary>
public class GameController : MonoBehaviour {

    private string BEST_SCORE = "bestscore";

	/// <summary>
	/// Reference to player.
	/// </summary>
	public GameObject player;
	private PlayerController playerController;
		
	public GameObject PanelGameOver;
	public Text TextTapToPlay;
	public Text TextScore;
    public Text TextBestScore;
	public Text TextBestScoreResult;

    private int _Score;
    private AudioSource TapAudio;
    private AudioSource GameOverAudio;

	// Use this for initialization
	void Start () {
		this.playerController = player.GetComponent<PlayerController> ();
        Debug.Log(GetComponents<AudioSource>());
        this.TapAudio = GetComponents<AudioSource>()[0];
        this.GameOverAudio = GetComponents<AudioSource>()[1];
        this._Score = 0;
        DisplayScore();
        DisplayBestScore();
        this.PanelGameOver.SetActive(false);
	}

	public void Replay() {
        PlayTapSound();
		Application.LoadLevel (Application.loadedLevel);
	}

	public void RateGame(){
		#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
		Application.OpenURL("http://www.nguyenkim.com");
		#elif UNITY_ANDROID
		//Application.OpenURL("market://details?id=com.nguyenkim.zigzag");
		Application.OpenURL("https://play.google.com/store/apps/details?id=com.nguyenkim.zigzag");
		#elif UNITY_IPHONE
		Application.OpenURL("itms-apps://itunes.apple.com/us/app/zigzag-and-the-ball/id1023106262?ls=1&mt=8");
		#endif
	}

    private void PlayTapSound()
    {
        if (TapAudio != null)
        {
            TapAudio.Play();
        }
    }

    private float nextFire;
	// Update is called once per frame
	void Update () {

        if (this.playerController.IsWating())
        {
            // An hien text tap to play
            this.TextTapToPlay.enabled = true;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
            if (Input.GetButton("Fire1") && Time.time > nextFire)
            {
                nextFire = Time.time + 0.2f;
                PlayTapSound();
                this.playerController.Run();
            }   
#endif

#if UNITY_IOS || UNITY_ANDROID ||UNITY_WP8 || UNITY_WP8_1 || UNITY_BLACKBERRY
            if (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Began)
            {
                PlayTapSound();
                this.playerController.Run();
            }            
#endif
        }
        else
        {
            // An hien text tap to play
            this.TextTapToPlay.enabled = false;
        }

        // Dieu khien
        if (this.playerController.IsRunning())
        {
            // Cap nhat diem
            int score = Mathf.RoundToInt(this.player.transform.position.x + this.player.transform.position.z);
            if (score != this._Score)
            {
                this._Score = score;
                DisplayScore();
            }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
            if (Input.GetButton("Fire1") && Time.time > nextFire)
            {
                nextFire = Time.time + 0.2f;
                this.playerController.TurnDirection();
            }
#endif

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_WP8_1 || UNITY_BLACKBERRY
            if (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Began)
            {
                this.playerController.TurnDirection();
            }
#endif
        }

		if (this.playerController.IsFalling()) {
            this.playerController.Stop();
			this.GameOver();
        }


#if UNITY_ANDROID || UNITY_WP8 || UNITY_WP8_1
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit(); 
        }
#endif
	}

	void DisplayScore () {
		TextScore.text = "Score: " + this._Score.ToString();
	}

    void DisplayBestScore()
    {
        int Bestscore = PlayerPrefs.GetInt(BEST_SCORE, 0);
        TextBestScore.text = "Best: " + Bestscore.ToString();
    }
	void UpdateBestScore(){
		int Bestscore = PlayerPrefs.GetInt(BEST_SCORE, 0);
		if (this._Score > Bestscore)
		{
			PlayerPrefs.SetInt(BEST_SCORE, this._Score);
			DisplayBestScore();
		}
	}

	void DisplayScoreResult(){
		int Bestscore = PlayerPrefs.GetInt(BEST_SCORE, 0);
		if (this._Score > Bestscore) {
			TextBestScoreResult.text = "New Best Score: " + this._Score;
		} else {
			TextBestScoreResult.text = "Score: " + this._Score;
		}
	}

	void GameOver(){
        PlayGameOverSound();
		DisplayScoreResult ();
		UpdateBestScore ();
		this.PanelGameOver.SetActive (true);
	}

    private void PlayGameOverSound()
    {
        Debug.Log("Play GameOverSound");
        if (this.GameOverAudio != null)
        {
            Debug.Log("Playing GameOverSound");
            this.GameOverAudio.Play();
        }
    }
}
