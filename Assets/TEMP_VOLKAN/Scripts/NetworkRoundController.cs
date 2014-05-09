//using System.Diagnostics; 
using UnityEngine;
using System.Collections;

public class NetworkRoundController : uLink.MonoBehaviour {

	public int CurrentSlayerPlayerNumber;
	public int CurrentVampirePlayerNumber;

	public GameObject[] vampirePlayers = new GameObject[20];
	public GameObject[] slayerPlayers = new GameObject[20];

	public GameObject[] vampireBots = new GameObject[20];
	public GameObject[] slayerBots = new GameObject[20];

	public int SlayerBotNumber = 0;
	public int VampireBotNumber = 0;

	public bool roundStarted;
	private float remainingTime;
	private float endingTime;

	public int remainingMinutes;
	public int remainingSeconds;

	public float RoundTime;

	public int WinningScore;

	public int VampireTeamScore;
	public int SlayerTeamScore;

	public bool anybodyInTheGame;

	private NetworkSpawnController netwSpawnController;

	// Use this for initialization
	void Start () {

		netwSpawnController = gameObject.transform.parent.GetComponentInChildren<NetworkSpawnController> ();


		CurrentSlayerPlayerNumber = 0;
		CurrentVampirePlayerNumber = 0;

		roundStarted = false;
		remainingTime = -1f;
		endingTime = -1f;


		VampireTeamScore = 0;
		SlayerTeamScore = 0;

		anybodyInTheGame = false;
	
	}
	
	// Update is called once per frame
	void Update () {
		if (roundStarted) 
		{
			remainingTime = endingTime - Time.time;

			int roundedInt = Mathf.CeilToInt( remainingTime );

			remainingMinutes = roundedInt / 60;
			remainingSeconds = roundedInt % 60;


			if ( remainingTime <= 0f )
			{
				EndTheRound();
			}
			else if( uLink.Network.connections.Length < 1 )
			{
				EndTheRound();
			}


			//----------------------------------------------------------------------------------------------
			// INSTANTIATE BOTS IF NEEDED
			CollectBots();

			if( slayerBots.Length < SlayerBotNumber )
			{
				netwSpawnController.InstantiateBots ( (SlayerBotNumber - slayerBots.Length) , 0 );
			}
			if( vampireBots.Length < VampireBotNumber )
			{
				netwSpawnController.InstantiateBots ( 0 , (VampireBotNumber - vampireBots.Length) );
			}
			//----------------------------------------------------------------------------------------------



		}
	
		if (anybodyInTheGame) 
		{
			if( uLink.Network.connections.Length < 1 )
			{

				anybodyInTheGame = false;
				EndTheRound();

			}
		}


	}


	void OnGUI()
	{
		string text = string.Format("{0:00}:{1:00}", remainingMinutes, remainingSeconds); 
		GUI.Label ( new Rect (400, 25, 100, 30), text);
	}


	public void StartTheRound()
	{

		CollectPlayers ();

		foreach (GameObject slayerGameObject in slayerPlayers) 
		{
			slayerGameObject.GetComponent<NetworkEvents>().StartRoundOnOthers(RoundTime);
		}
		foreach (GameObject vampireGameObject in vampirePlayers) 
		{
			vampireGameObject.GetComponent<NetworkEvents>().StartRoundOnOthers(RoundTime);
		}

		networkView.UnreliableRPC("StartRoundOnOthers", uLink.RPCMode.Others, RoundTime );


		// START THE ROUND TIMER
		endingTime = (Time.time + (RoundTime*60f));

		//TRIGGER THE BOT INSTANTIATIONS
		netwSpawnController.InstantiateBots ( SlayerBotNumber, VampireBotNumber );


	}

	public IEnumerator DisconnectPlayersWithDelay( float delay  )
	{
		yield return new WaitForSeconds(delay);
		
		if (uLink.Network.connections.Length > 0)
			uLink.Network.Disconnect();

		//Application.LoadLevel (Application.loadedLevel);

		/*
		Process myProcess = new Process();
		myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		myProcess.StartInfo.CreateNoWindow = true;
		myProcess.StartInfo.UseShellExecute = false;
		myProcess.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
		string path = "C:\\Users\\Volkan Benli\\Desktop\\TESTBUILDS\\sv-MAP\\RestartServer.bat";
		myProcess.StartInfo.Arguments = "/c" + path;
		myProcess.EnableRaisingEvents = true;
		myProcess.Start();
		myProcess.WaitForExit();
		*/




		System.Diagnostics.Process.Start ("c:\\Users\\Volkan Benli\\Desktop\\TESTBUILDS\\sv-MAP\\RestartServer.bat");

	}

	public void EndTheRound(char winner = 'a')
	{

		CollectPlayers ();
		
		foreach( GameObject player in slayerPlayers )
		{
			player.GetComponentInChildren<NetworkEvents>().WinLose( winner );
		}
		foreach( GameObject player in vampirePlayers )
		{
			player.GetComponentInChildren<NetworkEvents>().WinLose( winner );
		}


		StartCoroutine (DisconnectPlayersWithDelay(5f));



	}

	public void CollectPlayers()
	{
		slayerPlayers = GameObject.FindGameObjectsWithTag("SlayerPlayer");
		vampirePlayers = GameObject.FindGameObjectsWithTag("VampirePlayer");
	}

	public void CollectBots()
	{
		vampireBots = GameObject.FindGameObjectsWithTag ("Vampire");
		slayerBots = GameObject.FindGameObjectsWithTag ("Slayer");
	}

	public void IncreaseSlayerScore()
	{
		SlayerTeamScore++;

		CollectPlayers ();

		foreach( GameObject player in slayerPlayers )
		{
			player.GetComponentInChildren<NetworkEvents>().IncreaseSlayerScore();
		}
		foreach( GameObject player in vampirePlayers )
		{
			player.GetComponentInChildren<NetworkEvents>().IncreaseSlayerScore();
		}


		if( SlayerTeamScore >= WinningScore )
			EndTheRound('s');

	}

	public void IncreaseVampireScore()
	{
		VampireTeamScore++;

		CollectPlayers ();
		
		foreach( GameObject player in vampirePlayers )
		{
			player.GetComponentInChildren<NetworkEvents>().IncreaseVampireScore();
		}
		foreach( GameObject player in slayerPlayers )
		{
			player.GetComponentInChildren<NetworkEvents>().IncreaseVampireScore();
		}
		
		
		if( VampireTeamScore >= WinningScore )
			EndTheRound('v');

	}



}
