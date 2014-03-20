using UnityEngine;
using System.Collections;

public class switchCam : MonoBehaviour 
{
	GameObject spect=null;
	GameObject lastSpect=null;
	GameObject FindChildByName(string childName, Transform parentObject) 
	{
		foreach (Transform child in parentObject) 
		{
			if(child.name == childName)
			{
				return child.gameObject;
			}
		}
		return null;
	}

	GameObject getRandomTarget()
	{
		ArrayList myList = new ArrayList();
		GameObject[] targets;
		GameObject random=null;
		targets=GameObject.FindGameObjectsWithTag("Slayer");

		
		foreach (GameObject go in targets)
		{
			myList.Add(go);
		}	

		targets=GameObject.FindGameObjectsWithTag("Vampire");

		foreach (GameObject go in targets)
		{
			myList.Add(go);
		}	

		if(myList.Count>0)
		{
			int randomNumber=Random.Range(0,myList.Count);
			for(int i=0; i<myList.Count; i++)
			{
				random=(GameObject)myList[randomNumber];
			}
		}
		if(random==null) {return null;}
		return random;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyUp(KeyCode.Mouse0))
		{
			spect=getRandomTarget();
			while(lastSpect==spect) 
			{
				spect=getRandomTarget();
			}

			if(lastSpect!=null)
			{
				GameObject cam1=FindChildByName("FPSCamera",lastSpect.transform);
				GameObject weaponCam1=FindChildByName("WeaponCamera",cam1.transform);
				cam1.GetComponent<Camera>().enabled=false;
				weaponCam1.GetComponent<Camera>().enabled=false;
			}
			lastSpect=spect;
			GameObject cam=FindChildByName("FPSCamera",spect.transform);
			GameObject weaponCam=FindChildByName("WeaponCamera",cam.transform);
			cam.GetComponent<Camera>().enabled=true;
			weaponCam.GetComponent<Camera>().enabled=true;

		}
	
	}
}
