using UnityEngine;
using System.Collections;

public class LayerChanger : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		/*if(GameObject.Find("Arms"))
		{
			if(GameObject.Find("Arms").transform.parent.parent.parent.parent.GetComponent<Camera>().enabled==false)
			{
				GameObject.Find("Arms").layer=1;
			}
		}
		if(GameObject.Find("Mesh"))
		{
			if(GameObject.Find("Mesh").transform.parent.parent.parent.parent.GetComponent<Camera>().enabled==false)
			{
				GameObject.Find("Mesh").layer=1;
			}
		}
		if(GameObject.Find("Mesh_____"))
		{
			if(GameObject.Find("Mesh_____").transform.parent.parent.parent.parent.GetComponent<Camera>().enabled==false)
			{
				GameObject.Find("Mesh_____").layer=1;
			}
		}*/
		if(GameObject.Find("Arms"))
		{
			if((GameObject.Find("Arms").transform.parent.parent.parent.parent.parent.name != "VampirePlayerOwner1(Clone)" )&&(GameObject.Find("Arms").transform.parent.parent.parent.parent.parent.name!="SlayerPlayerOwner1(Clone)"))
			{
				GameObject.Find("Arms").SetActive(false);
			}
		}
		if(GameObject.Find("Mesh"))
		{
			if((GameObject.Find("Mesh").transform.parent.parent.parent.parent.parent.name != "VampirePlayerOwner1(Clone)")&&(GameObject.Find("Mesh").transform.parent.parent.parent.parent.parent.name!="SlayerPlayerOwner1(Clone)"))
			{
				GameObject.Find("Mesh").SetActive(false);;
			}
		}
		if(GameObject.Find("Mesh_____"))
		{
			if((GameObject.Find("Mesh_____").transform.parent.parent.parent.parent.parent.name != "VampirePlayerOwner1(Clone)")&&(GameObject.Find("Mesh_____").transform.parent.parent.parent.parent.parent.name!="SlayerPlayerOwner1(Clone)"))
			{
				GameObject.Find("Mesh_____").SetActive(false);
			}
		}

	}
}
