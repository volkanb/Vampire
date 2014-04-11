using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class log : MonoBehaviour {
	
	string  fileName ="";
	float logDelayTime;
	// Use this for initialization
	void Start () 
	{
		while(fileName=="")
		{
			fileName=GameObject.Find("Control").GetComponent<logFile>().fileName;
		}
		
	}

	void WriteFile(string filepathIncludingFileName,string message)	
	{
		int i=0;
		int size=0;
		//string[] arr = new string[2000];
		string[] arr=null;

		var sr = File.OpenText(filepathIncludingFileName);		
		var line = sr.ReadLine();
		while(line != null)
		{
			//Debug.Log(line); // prints each line of the file
			line = sr.ReadLine();
			size++;
				
		} 
		sr.Close();

		sr = File.OpenText(filepathIncludingFileName);
		line = sr.ReadLine();
		arr=new string[size];
		while(line != null)
		{
			//Debug.Log(line); // prints each line of the file
			arr[i] = line;
			line = sr.ReadLine();
			i++;
		} 
		sr.Close();

		StreamWriter sw = new StreamWriter(filepathIncludingFileName);

		for(int j=0; j<arr.Length; j++)
		{
			sw.WriteLine(arr[j]);
		}

		sw.WriteLine(message);
		
		//sw.WriteLine("Another Line");
		
		sw.Flush();
		
		sw.Close();
		
	}

	void ReadFile(string file)
	{	
		if(File.Exists(file))
		{
			var sr = File.OpenText(file);
			
			var line = sr.ReadLine();
			
			while(line != null)
			{
				
				Debug.Log(line); // prints each line of the file
				
				line = sr.ReadLine();
			}   
			
		} 
		else 
		{
			Debug.Log("Could not Open the file: " + file + " for reading.");
			return;
		}
		
	}


	public void EnterLog(string CurrentState)
	{
		logDelayTime=Time.time;
		while(fileName=="")
		{
			fileName=GameObject.Find("Control").GetComponent<logFile>().fileName;
			if(Time.time-logDelayTime>2.5f){break;}
		}

		if(File.Exists(fileName)&& (fileName!="")) 
		{
			WriteFile(fileName,System.DateTime.Now.Day+"/"+System.DateTime.Now.Month+"/"+System.DateTime.Now.Year+" "+System.DateTime.Now.TimeOfDay+" "+gameObject.name+" "+gameObject.GetInstanceID()+" "+CurrentState);
		}
		else
		{
			Debug.Log("can not write to log file");
		}


	}

	
	// Update is called once per frame
	void Update () 
	{

	}

	/*int j=0;
	while(File.Exists(fileName)) 
	{
		fileName="MyFile"+j+".txt";
		WriteFile(fileName,System.DateTime.Now.Day+"/"+System.DateTime.Now.Month+"/"+System.DateTime.Now.Year+" "+System.DateTime.Now.TimeOfDay+" "+gameObject.name+" "+CurrentState);
	}*/
}

