using uLink;
using UnityEngine;

public class NetworkSpawnController : uLink.MonoBehaviour {

	public GameObject SlayerProxyPref = null;
	public GameObject SlayerOwnerPref = null;
	public GameObject SlayerCreatorPref = null;

	public Transform SlayerSpawnLocation = null;
	

	
	void uLink_OnPlayerConnected(uLink.NetworkPlayer player)
	{
		uLink.Network.Instantiate (player, SlayerProxyPref, SlayerOwnerPref, SlayerCreatorPref, SlayerSpawnLocation.position, SlayerSpawnLocation.rotation, 0, "SlayerPlayer");

	}
	
}
