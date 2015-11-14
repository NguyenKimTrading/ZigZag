using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using Facebook.MiniJSON;

/// <summary>
/// Game controller.
/// </summary>
public class GameController : MonoBehaviour {

    private string BEST_SCORE = "bestscore";

	private GameStatus status;

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

	public GameObject pnlMenu;
	public Button btnLogin;
	public Button btnPlay;

	public Image userPictureProfile;
	public Sprite userPictureProfileDefault;
	public Text txtPlayerName;
	public Text txtPlayerEmail;

	public GameObject pnlUserForm;
	public InputField infName;
	public InputField infPhone;
	public InputField infIdentifier;


    private int _Score;
    private AudioSource TapAudio;
    private AudioSource GameOverAudio;

	void Awake ()
	{
		if (!FB.IsInitialized) {
			// Initialize the Facebook SDK
			FB.Init(InitCallback, OnHideUnity);
		} else {
			// Already initialized, signal an app activation App Event
			if (!FB.IsLoggedIn) {
				FB.ActivateApp();
			} else {
				DisplayUserInfo();
			}
		}
	}

	private void InitCallback ()
	{
		if (FB.IsInitialized) {
			// Signal an app activation App Event
			FB.ActivateApp();
		} else {
			Debug.Log("Failed to Initialize the Facebook SDK");
		}
	}
	
	private void OnHideUnity (bool isGameShown)
	{
		if (!isGameShown) {
			// Pause the game - we will need to hide
			Time.timeScale = 0;
		} else {
			// Resume the game - we're getting focus again
			Time.timeScale = 1;
		}
	}

	public void Login() {
		if (FB.IsInitialized) {
			if (!FB.IsLoggedIn) {
				var perms = new List<string> (){"public_profile", "email", "user_friends"};
				FB.LogInWithReadPermissions (perms, AuthCallback);
			} else {
				Logout ();
			}
		} 
	}

	private void AuthCallback (ILoginResult result) {
		if (FB.IsLoggedIn) {
			// AccessToken class will have session details
			var aToken = AccessToken.CurrentAccessToken;
			// Print current access token's User ID
			Debug.Log(aToken.UserId);
			Debug.Log(aToken.TokenString);
			FB.API ("/me?fields=id,name,email,picture", Facebook.Unity.HttpMethod.GET, (graphResult)=>{
				if (graphResult != null){
					var abc = graphResult.ResultDictionary;
					string fbid = abc["id"].ToString();
					string name = abc["name"].ToString();
					string email = abc["email"].ToString();
					var picture = abc["picture"] as IDictionary<string, object>;
					var data = picture["data"] as IDictionary<string, object>;
					var pictureURL = data["url"].ToString();
					StartCoroutine(LoginToNK(fbid,name,email,pictureURL,aToken.TokenString));
				}
				else {
					Alert("Cannot retrieve user info");
				}
			});
		} else {
			Alert("User cancelled login");
		}
	}

	IEnumerator LoginToNK(string fbid, string name, string email, string pictureURL, string accessToken) {
		if (name != null && email != null && accessToken != null) {
			string loginURL = "http://event.nguyenkim.com/game/login.json";
			// Create a form object for sending high score data to the server
			WWWForm form = new WWWForm();
			// Assuming the perl script manages high scores for different games
			form.AddField ("CpMobileUser[game_id]", 4);
			form.AddField ("CpMobileUser[email]", email);
			form.AddField ("CpMobileUser[fullname]", name);
			form.AddField ("CpMobileUser[fb_id]", fbid);
			form.AddField ("access_token", accessToken);
			// Create a download object
			WWW login = new WWW( loginURL, form );
			// Wait until the download is done
			yield return login;
			if(!string.IsNullOrEmpty(login.error)) {
				Alert(login.error);
			} else {
				string resultJsonString = login.text.Substring(login.text.IndexOf("{"));
				Debug.Log("resultJsonString: " + resultJsonString);
				var result = Json.Deserialize(resultJsonString) as IDictionary<string,object>;
				string resultStatus = result.GetValueOrDefault<string>("status");
				if (resultStatus == "error") 
				{
					Debug.Log("Error: " + result["message"]);
				} 
				else if (resultStatus == "success") 
				{
					var data = result.GetValueOrDefault<IDictionary<string,object>>("data");
					var user = data.GetValueOrDefault<IDictionary<string,object>>("user");
					CurrentUser.id = user.GetValueOrDefault<string>("id");
					CurrentUser.email = user.GetValueOrDefault<string>("email");
					CurrentUser.name = user.GetValueOrDefault<string>("fullname");
					CurrentUser.phone = user.GetValueOrDefault<string>("phone");
					CurrentUser.cardId = user.GetValueOrDefault<string>("id_card");
					CurrentUser.fbId = user.GetValueOrDefault<string>("fb_id");
					// Get User's Picture Profile URL
					StartCoroutine(LoadPicture(pictureURL,(pic) => {
						Vector2 v = new Vector2 (0, 0);
						Rect r = new Rect (0f,0f,pic.width,pic.height);
						userPictureProfile.sprite = CurrentUser.pictureProfile = Sprite.Create(pic, r, v);
						DisplayUserInfo();
					}));
				}
			}
		}
	}



	IEnumerator LoadPicture(string url, FacebookDelegate<Texture2D> callback)    
	{
		WWW www = new WWW(url);
		yield return www;
		callback(www.texture);
	}

	void DisplayUserInfo(){
		txtPlayerName.text = CurrentUser.name; // Hien thi ten
		txtPlayerEmail.text = CurrentUser.email; // Hien thi email
		infName.text = CurrentUser.name; // Gan ten cho textbox
		infPhone.text = CurrentUser.phone; // Gan phone cho text box
		infIdentifier.text = CurrentUser.cardId; // Gan cardid cho text box
		userPictureProfile.sprite = CurrentUser.pictureProfile; // Hien thi hinh nguoi dung
		if (FB.IsLoggedIn) {
			var txt  = btnLogin.GetComponentInChildren(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
			if (txt!= null) txt.text = "Log out"; // Thay doi text cua button
		} else {
			var txt  = btnLogin.GetComponentInChildren(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
			if (txt!= null) txt.text = "Log in"; // Thay doi text cua button
		}
	}

	public void Edit(){
		Debug.Log ("Editing User's Profile");
		// An pannel menu
		pnlMenu.SetActive (false);
		// Show user form
		pnlUserForm.SetActive (true);
	}

	public void Save(){
		if (AccessToken.CurrentAccessToken == null) {
			Alert("You must log in to edit your info");
			return;
		}
		// Get info from input
		string email = CurrentUser.email;
		string name = infName.text;
		string phone = infPhone.text;
		string cardId = infIdentifier.text;
		string tokenAccess = AccessToken.CurrentAccessToken.TokenString;
		StartCoroutine(UpdateUserInfo(email, name, phone, cardId, tokenAccess));
	}

	private IEnumerator UpdateUserInfo(string email, string name, string phone, string cardId, string tokenAccess){
		// Send info to server

		// Create a form object for sending high score data to the server
		WWWForm form = new WWWForm();
		// Assuming the perl script manages high scores for different games
		form.AddField ("CpMobileUser[game_id]", 4);
		form.AddField ("CpMobileUser[email]", email);
		form.AddField ("CpMobileUser[fullname]", name);
		form.AddField ("CpMobileUser[phone]", phone);
		form.AddField ("CpMobileUser[id_card]", cardId);
		form.AddField ("access_token", tokenAccess);
		// Create a download object
		string submitURL = "http://event.nguyenkim.com/game/update_user.json";
		WWW response = new WWW( submitURL, form );
		// Wait until the download is done
		yield return response;
		if(!string.IsNullOrEmpty(response.error)) {
			Alert(response.error);
		} else {
			string responseJsonString = response.text.Substring(response.text.IndexOf("{"));
			Debug.Log("resultJsonString: " + responseJsonString);
			var result = Json.Deserialize(responseJsonString) as IDictionary<string,object>;
			string resultStatus = result.GetValueOrDefault<string>("status");
			if (resultStatus == "error") 
			{
				Alert(result.GetValueOrDefault<string>("message"));
			} 
			else if (resultStatus == "success") 
			{
				var data = result.GetValueOrDefault<IDictionary<string,object>>("data");
				var user = data.GetValueOrDefault<IDictionary<string,object>>("user");
				CurrentUser.id = user.GetValueOrDefault<string>("id");
				CurrentUser.email = user.GetValueOrDefault<string>("email");
				CurrentUser.name = user.GetValueOrDefault<string>("fullname");
				CurrentUser.phone = user.GetValueOrDefault<string>("phone");
				CurrentUser.cardId = user.GetValueOrDefault<string>("id_card");
				CurrentUser.fbId = user.GetValueOrDefault<string>("fb_id");
				DisplayUserInfo();
				Alert(result.GetValueOrDefault<string>("message"));
				// Hien pannel menu
				pnlMenu.SetActive (true);
				// An form user
				pnlUserForm.SetActive (false);
			} 
			else {
				Alert(result.GetValueOrDefault<string>("message"));
			}
		}
	}

	public void Cancel(){
		// Hien pannel menu
		pnlMenu.SetActive (true);
		// An form user
		pnlUserForm.SetActive (false);
	}

	void Logout() {
		FB.LogOut();
		CurrentUser.id = null;
		CurrentUser.email = null;
		CurrentUser.name = null;
		CurrentUser.phone = null;
		CurrentUser.cardId = null;
		CurrentUser.fbId = null;
		CurrentUser.pictureProfile = userPictureProfileDefault;
		DisplayUserInfo ();
	}
	
	// Use this for initialization
	void Start () {
		this.status = GameStatus.MainMenu;
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

	public void Play(){
		this.status = GameStatus.Game;
		this.TextTapToPlay.text = "Tap to play";
		this.pnlMenu.SetActive (false);
	}

    private float nextFire;
	// Update is called once per frame
	void Update () {
		if (this.status != GameStatus.Game)
			return;

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
		TextScore.text = this._Score.ToString();
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
		if (AccessToken.CurrentAccessToken != null) {
			string highscore_url = "http://event.nguyenkim.com/game/submit_point.json";
			// Create a form object for sending high score data to the server
			WWWForm form = new WWWForm();
			
			// Assuming the perl script manages high scores for different games
			form.AddField ("CpMobileGamePoint[game_id]", 4);
			// The name of the player submitting the scores
			form.AddField ("CpMobileGamePoint[user_id]", AccessToken.CurrentAccessToken.UserId);
			// The score
			form.AddField ("CpMobileGamePoint[point]", score);
			// Access Token
			form.AddField ("access_token", AccessToken.CurrentAccessToken.TokenString);
			// The log
			var log = string.Format("This is a log. UserId: '{0}'. Access Token: '{1}'", 
			                        AccessToken.CurrentAccessToken.UserId,
			                        AccessToken.CurrentAccessToken.TokenString);
			form.AddField ("CpMobileGamePoint[log]", log);
			// The status
			form.AddField ("CpMobileGamePoint[status]", 1);
			// Create a download object
			WWW download = new WWW( highscore_url, form );
			// Wait until the download is done
			yield return download;
			if(!string.IsNullOrEmpty(download.error)) {
				Debug.Log( "Error: " + download.error );
			} else {
				var resultJsonString = download.text.Substring(download.text.IndexOf("{"));
				var result = Json.Deserialize(resultJsonString) as IDictionary<string, object>;
				var resultStatus = (string)result["status"];
				if (resultStatus == "error") 
				{
					Debug.Log("Error: " + result["message"]);
				} else if (resultStatus == "success") 
				{
					Debug.Log("Success: " + result["message"]);
				}
			}
		}
	}

    void GameOver()
    {
		this.status = GameStatus.GameOver;
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

	private void Alert(string message) {
		Debug.Log (message);
	}

	enum GameStatus {
		MainMenu, Game, GameOver, ScoreBoard
	}

	static class CurrentUser{
		public static string id = null;
		public static string name = null;
		public static string email = null;
		public static string phone = null;
		public static string cardId = null;
		public static string fbId = null;
		public static Sprite pictureProfile = null;
	}
}