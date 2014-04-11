using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class logFile : MonoBehaviour {

	public string fileName="";
	// Use this for initialization
	void Start () 
	{
		int j=0;
		while(File.Exists(fileName) || (fileName=="")) 
		{
			fileName="MyFile"+j+".txt";
			j++;
		}
		var sr = File.CreateText(fileName);
		sr.Close();
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}


}

