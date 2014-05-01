using UnityEngine;
using System.Collections;

public class NetworkRoundController : uLink.MonoBehaviour {

	public int CurrentSlayerPlayerNumber;
	public int CurrentVampirePlayerNumber;

	public GameObject[] vampirePlayers = new GameObject[20];
	public GameObject[] slayerPlayers = new GameObject[20];

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

	public void EndTheRound(string winner = "")
	{
		if (winner == "V") 
		{
			// SEND VAMPIRES TEAM A WINNING MESSAGE
		}
		else if ( winner == "S" )
		{
			// SEND SLAYER TEAM A WINNING MESSAGE
		}


		if (uLink.Network.connections.Length > 0)
			uLink.Network.Disconnect();


		Application.LoadLevel (Application.loadedLevel);

	}

	public void CollectPlayers()
	{
		slayerPlayers = GameObject.FindGameObjectsWithTag("SlayerPlayer");
		vampirePlayers = GameObject.FindGameObjectsWithTag("VampirePlayer");
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
			EndTheRound("S");

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
			EndTheRound("V");

	}



}
