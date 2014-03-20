using UnityEngine;
using System.Collections;

public class lookat : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void lookTO(Vector3 target)
	{
		transform.LookAt(target);
	}
}
