using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Newtonsoft.Json;

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
		#endif

		#if UNITY_ANDROID
		//Application.OpenURL("market://details?id=com.nguyenkim.zigzag");
		Application.OpenURL("https://play.google.com/store/apps/details?id=com.nguyenkim.zigzag");
		#endif

		#if UNITY_IOS
		Application.OpenURL("itms-apps://itunes.apple.com/us/app/zigzag-and-the-ball/id1023106262?ls=1&mt=8");
		#endif

		#if UNITY_WP8 || UNITY_WP8_1
		Application.OpenURL("itms-apps://itunes.apple.com/us/app/zigzag-and-the-ball/id1023106262?ls=1&mt=8");
		#endif
	}

    private void PlayTapSound()
    {
        if (TapAudio != null) {
			TapAudio.Play();
		}
    }

    private float nextFire;
	// Update is called once per frame
	void Update () {
		if (this.playerController.IsWating()) // Player dang trong trang thai chuan bi bat dau
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
            if (Input.GetButton("Fire1") && Time.time > nextFire)
            {
                nextFire = Time.time + 0.2f;
                this.PlayTapSound();
                this.playerController.Run(); // Cho player chay
				this.TextTapToPlay.enabled = false;
            }   
#endif

#if UNITY_IOS || UNITY_ANDROID ||UNITY_WP8 || UNITY_WP8_1 || UNITY_BLACKBERRY
            if (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Began)
            {
                this.PlayTapSound();
				this.playerController.Run(); // Cho player chay
				this.TextTapToPlay.enabled = false;
            }            
#endif
        }
        
		else if (this.playerController.IsRunning()) // Player dang chay. Thuc hien dieu khien player
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
            if (Input.GetButton("Fire1") && Time.time > nextFire)
            {
                nextFire = Time.time + 0.2f;
                this.playerController.TurnDirection(); // Chuyen huong
				this.PlayTapSound();
            }
#endif

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_WP8_1 || UNITY_BLACKBERRY
            if (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Began)
            {
                this.playerController.TurnDirection();
				this.PlayTapSound();
            }
#endif
			// Cap nhat diem
			int score = Mathf.RoundToInt(this.player.transform.position.x + this.player.transform.position.z);
			if (score != this._Score)
			{
				this._Score = score;
				DisplayScore(); // Hien thi diem
			}
        }

		else if (this.playerController.IsFalling()) {
            this.playerController.Stop();
			this.GameOver();
        }

		// Xu ly nut back tren Android va WP8
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
			// Submit score to server
		} else {
			TextBestScoreResult.text = "Score: " + this._Score;
		}
		StartCoroutine(SubmitScore (2, this._Score));
	}

	IEnumerator SubmitScore(int userId, int score) {
		string highscore_url = "http://event.nguyenkim.com/game/submit_point.json";
		// Create a form object for sending high score data to the server
		WWWForm form = new WWWForm();

		// Assuming the perl script manages high scores for different games
		form.AddField ("CpMobileGamePoint[game_id]", 4);
		// The name of the player submitting the scores
		form.AddField ("CpMobileGamePoint[user_id]", "1f52111e34a629d5ed1d68fdcd725498");
		// The score
		form.AddField ("CpMobileGamePoint[point]", score);
		// The log
		form.AddField ("CpMobileGamePoint[log]", "This is a log");
		// The status
		form.AddField ("CpMobileGamePoint[status]", 1);
		// Create a download object
		WWW download = new WWW( highscore_url, form );
		// Wait until the download is done
		yield return download;
		if(!string.IsNullOrEmpty(download.error)) {
			Debug.Log( "Error: " + download.error );
		} else {
			var JsonString = download.text.Substring(download.text.IndexOf("{"));
			var result = JsonConvert.DeserializeObject<APIResult<Newtonsoft.Json.Linq.JContainer>>(JsonString);
			if (result.status=="error") {
				Debug.Log("Error: " + result.message);
			} else if (result.status=="success") {
				Debug.Log("Success: " + result.message);
			}
		}
	}

    void GameOver()
    {
        PlayGameOverSound();
		DisplayScoreResult ();
		UpdateBestScore ();
		this.PanelGameOver.SetActive (true);
		this.PanelGameOver.GetComponent<Animator>().Play("Open");
	}

    private void PlayGameOverSound()
    {
        if (this.GameOverAudio != null)
        {
            this.GameOverAudio.Play();
        }
    }
}

public class APIResult<T> {
	public string status;
	public string code;
	public string message;
	public T data;
}
