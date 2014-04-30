using UnityEngine;
using System.Collections;

public class VampireAnim : MonoBehaviour {

	// Use this for initialization
	void Start () {
        animation["Death1"].speed = 0.5f;
	}
	
	// Update is called once per frame
	void Update () {
      //  animation.PlayQueued("IdleBattel");
        animation.PlayQueued("IdleUsual");
        if (Input.GetKey(KeyCode.A))
        { animation.Play("walk"); }
        if (Input.GetKey(KeyCode.LeftShift)&&Input.GetKey(KeyCode.W))
        { animation.Play("RunForward"); } 
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.S))
        { animation.Play("RunBackward"); }
        if (Input.GetKey(KeyCode.Space))
        { animation.Play("Jump"); }
        if (Input.GetKey(KeyCode.D))
        { animation.Play("Death1"); }
        if (Input.GetKey(KeyCode.F))
        { animation.Play("Death2"); }
        if (Input.GetKey(KeyCode.Q))
        { animation.Play("DamageReceive1"); }
        if (Input.GetMouseButtonDown(0))
        { animation.Play("Attack3"); }
	}
}
