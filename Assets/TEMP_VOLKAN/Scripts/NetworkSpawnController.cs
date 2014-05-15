using uLink;
using UnityEngine;

public class NetworkSpawnController : uLink.MonoBehaviour {

	public NetworkRoundController roundController;


	public GameObject SlayerProxyPref = null;
	public GameObject SlayerOwnerPref = null;
	public GameObject SlayerCreatorPref = null;

	public GameObject VampireProxyPref = null;
	public GameObject VampireOwnerPref = null;
	public GameObject VampireCreatorPref = null;

	public GameObject VampireAIProxyPref = null;
	public GameObject VampireAICreatorPref = null;

	public GameObject SlayerAIProxyPref = null;
	public GameObject SlayerAICreatorPref = null;

	public Transform SlayerSpawnLocation = null;
	public Transform VampireSpawnLocation = null;


	private Vector3 createPos;
	private int createNumSlayer = 0;
	private int createNumVampire = 0;


	void uLink_OnPlayerConnected(uLink.NetworkPlayer player)
	{

		string loginName = player.loginData.Read<string> ();
		bool isSlayer = player.loginData.Read<bool> ();
		bool isVampire = player.loginData.Read<bool> ();


		if ( isSlayer == true ) 
		{
			uLink.Network.Instantiate (player, SlayerProxyPref, SlayerOwnerPref, SlayerCreatorPref, GetPosition('s'), SlayerSpawnLocation.rotation, 0, "SlayerPlayer");
			roundController.CurrentSlayerPlayerNumber++;

			GA.API.Design.NewEvent("New Slayer Player Connected");
		}
		else if ( isVampire == true )
		{

			uLink.Network.Instantiate (player, VampireProxyPref, VampireOwnerPref, VampireCreatorPref, GetPosition('v'), VampireSpawnLocation.rotation, 0, "VampirePlayer");
			roundController.CurrentVampirePlayerNumber++;
	
			GA.API.Design.NewEvent("New Vampire Player Connected");
		}

		if (!roundController.anybodyInTheGame)
			roundController.anybodyInTheGame = true;

		if( !roundController.roundStarted )
			if( ( roundController.CurrentSlayerPlayerNumber > 0 ) && ( roundController.CurrentVampirePlayerNumber > 0 ) )
			{
				roundController.roundStarted = true;
				roundController.StartTheRound();
			}

	}

	void uLink_OnPlayerDisconnected(uLink.NetworkPlayer player)
	{
		uLink.Network.DestroyPlayerObjects (player);
		uLink.Network.RemoveRPCs (player);
		uLink.Network.RemoveInstantiates (player);

		GA.API.Design.NewEvent("Player Disconnected");
	}

	public void InstantiateBots( int SlayerBotNumber, int VampireBotNumber )
	{
		while (SlayerBotNumber != 0) 
		{
			uLink.Network.Instantiate (SlayerAIProxyPref, SlayerAICreatorPref, GetPosition('s'), SlayerSpawnLocation.rotation, 0, "SlayerBot");
			SlayerBotNumber--;
		}

		while (VampireBotNumber != 0) 
		{
			uLink.Network.Instantiate (VampireAIProxyPref, VampireAICreatorPref, GetPosition('v'), VampireSpawnLocation.rotation, 0, "VampireBot");
			VampireBotNumber--;
		}

	}

	private Vector3 GetPosition( char c )
	{
		if (c == 's') 
		{
			createPos = SlayerSpawnLocation.position;
			createPos.x = createPos.x + (float)createNumSlayer;
			createPos.z = createPos.z + (float)createNumSlayer;

			createNumSlayer++;

			createNumSlayer = (createNumSlayer%10);
		}
		else if ( c == 'v' )
		{
			createPos = VampireSpawnLocation.position;
			createPos.x = createPos.x + (float)createNumVampire;
			createPos.z = createPos.z + (float)createNumVampire;

			createNumVampire++;

			createNumVampire = (createNumVampire%10);
		}

		return createPos;

	}
	
}
