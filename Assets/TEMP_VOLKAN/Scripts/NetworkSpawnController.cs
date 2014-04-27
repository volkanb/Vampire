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

	public Transform SlayerSpawnLocation = null;
	public Transform VampireSpawnLocation = null;





	void uLink_OnPlayerConnected(uLink.NetworkPlayer player)
	{

		string loginName = player.loginData.Read<string> ();


		if ( loginName == "s" ) 
		{

			uLink.Network.Instantiate (player, SlayerProxyPref, SlayerOwnerPref, SlayerCreatorPref, SlayerSpawnLocation.position, SlayerSpawnLocation.rotation, 0, "SlayerPlayer");
			roundController.CurrentSlayerPlayerNumber++;


		}
		else if ( loginName == "v" )
		{

			uLink.Network.Instantiate (player, VampireProxyPref, VampireOwnerPref, VampireCreatorPref, VampireSpawnLocation.position, VampireSpawnLocation.rotation, 0, "VampirePlayer");
			roundController.CurrentVampirePlayerNumber++;

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
	
}
