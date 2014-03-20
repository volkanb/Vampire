using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {
	float delay;
	public GameObject human;
	bool isDelay=false;
	// Use this for initialization
	void Start () 
	{
		delay=Time.time;
	}
	
	// Update is called once per frame
	void Update () 
	{
		GameObject[] humans;
		humans=GameObject.FindGameObjectsWithTag("Human");

		if(humans.Length>=4)
		{
			isDelay=false;
		}
		else if(humans.Length<4)
		{
			if(isDelay==false)
			{
				delay=Time.time;
				isDelay=true;
			}
			if(Time.time-delay>12)
			{
				Instantiate(human, transform.position, Quaternion.identity);	
				delay=Time.time;
				
			}
			
		}
		
	}
}
