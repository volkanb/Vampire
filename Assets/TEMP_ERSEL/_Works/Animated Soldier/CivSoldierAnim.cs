using UnityEngine;
using System.Collections;

public class CivSoldierAnim : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        animation.PlayQueued("idle");
        if (Input.GetKey(KeyCode.A))
        { animation.Play("walk"); } 
        if (Input.GetKey(KeyCode.Q))
        { animation.Play("hit"); }
        if (Input.GetKey(KeyCode.D))
        { animation.Play("death"); }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        { animation.Play("run"); }
        if (Input.GetMouseButtonDown(0))
        { animation.Play("stand_Fshot"); }
	}
}
