using UnityEngine;
using System.Collections;

public class colorChanger : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		renderer.material.color=Color.magenta;
	}
}
