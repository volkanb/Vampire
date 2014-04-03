using UnityEngine;
using uLink;

public class TEST : uLink.MonoBehaviour {

	// EVENT HANDLER
	vp_FPPlayerEventHandler m_Player;


	protected virtual void OnEnable()
	{
		
		if (m_Player != null) 
		{
			m_Player.Register(this);
		}
		
	}
	protected virtual void OnDisable()
	{
		if (m_Player != null)
			m_Player.Unregister(this);
	}

	void Awake() 
	{
		// GETTING THE PLAYER'S EVENT HANDLER
		m_Player = transform.GetComponent<vp_FPPlayerEventHandler>();
	}

	void OnStart_Jump(){
		Debug.Log("ONSTART_JUMP STARTED IN TEST!");
		

	}


	void Update ()
	{

		if (Input.GetKeyUp(KeyCode.J)) 
		{
			m_Player.Unregister(this);

		}

	}




}
