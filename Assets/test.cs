using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {
	//public vp_FPPlayerEventHandler Player = null;
	RaycastHit objectHit;
	// Use this for initialization
	void Start () 
	{
//		GetComponent<CharacterController>().center=new Vector3(0,-0.9f,0);
	}
	
	// Update is called once per frame
	void Update () {
		if(GameObject.Find("Arms"))
		{
			GameObject.Find("Arms").layer=1;
		}
		if(GameObject.Find("Mesh"))
		{
			GameObject.Find("Mesh").layer=1;
		}
		if(GameObject.Find("Mesh_____"))
		{
			GameObject.Find("Mesh_____").layer=1;
		}

	}
}
