﻿using UnityEngine;
using System.Collections;

public class LayerChanger : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{

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