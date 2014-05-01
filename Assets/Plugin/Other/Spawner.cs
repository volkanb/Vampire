using UnityEngine;
using System.Collections;

public class Spawner : uLink.MonoBehaviour {

	private NetworkRoundController roundController;

	public int HumanNumberTreshold = 0;
	public float HumanInstantiateDelay = 0;


	public GameObject HumanProxy = null;
	public GameObject HumanCreator = null;

	private Transform zone = null;

	float delay;
	//public GameObject human;
	bool isDelay=false;
	// Use this for initialization
	void Start () 
	{
		delay=Time.time;

		// GETTING THE ZONE TRANSFORM
		zone = this.gameObject.transform;

		// GETTING THE ROUND CONTROLLER
		roundController = GameObject.Find("NetworkControlUnits").transform.gameObject.GetComponentInChildren<NetworkRoundController>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( roundController.roundStarted )
		{
			GameObject[] humans;
			humans=GameObject.FindGameObjectsWithTag("Human");

			if(humans.Length>= HumanNumberTreshold )
			{
				isDelay=false;
			}
			else if(humans.Length< HumanNumberTreshold )
			{
				if(isDelay==false)
				{
					delay=Time.time;
					isDelay=true;
				}
				if(Time.time-delay> HumanInstantiateDelay )
				{
					//Instantiate(human, transform.position, Quaternion.identity);	
					uLink.Network.Instantiate (HumanProxy, HumanCreator, zone.position, Quaternion.identity, 0, "Human");
					delay=Time.time;
					
				}
				
			}
		}
	
	}
}
