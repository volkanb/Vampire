using UnityEngine;
using System.Collections;

public class CivAnim : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        animation.PlayQueued("idle");
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        { animation.Play("run"); }
	}
}
