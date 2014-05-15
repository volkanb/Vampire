using UnityEngine;
using System.Collections;

public class VampirePlayerAnimationController : MonoBehaviour {

	public GameObject model;

	private bool down = false;

	//----------------------------------------------------------------------------------
	// EVENT HANDLER
	vp_FPPlayerEventHandler m_Player;

	protected virtual void OnEnable()
	{	
		if (m_Player != null) 
			m_Player.Register(this);
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
	//----------------------------------------------------------------------------------

	Vector3 lastPos;


	void Start()
	{
		lastPos = transform.position;
		InvokeRepeating ("Walk", 0, 0.1f);
	}

	void Walk()
	{
		if( !m_Player.Run.Active )
		{
			if ( (transform.position - lastPos).magnitude > 0.2f )
			{
				model.animation.Play("RunForward");

				down = false;
			}
			else
				model.animation.Stop("RunForward");
		}

		lastPos = transform.position;
	}


	void Update()
	{
		if ( !down && !model.animation.isPlaying )
			model.animation.PlayQueued("IdleUsual");

	}

	void OnStart_Attack()
	{
		model.animation["Attack3"].layer=1;
		model.animation["Attack3"].weight=0.5f;
		model.animation ["Attack3"].wrapMode = WrapMode.Loop;
		model.animation.Play("Attack3");

		down = false;
	}
	void OnStop_Attack()
	{
		model.animation.Stop ("Attack3");
	}


	void OnStart_Jump()
	{
		model.animation.Play ("Jump");

		down = false;
	}

	void OnStart_Run()
	{
		model.animation ["RunForward"].wrapMode = WrapMode.Loop;
		model.animation.Play ("RunForward");

		down = false;
	}
	void OnStop_Run()
	{
		model.animation.Stop ("RunForward");
	}

	void OnStart_Dead()
	{
		model.animation["Death1"].speed=1;
		model.animation.Play("Death1");

		down = true;
	}

	public void Revive()
	{
		model.animation["Death1"].speed=-1;
		model.animation["Death1"].time = model.animation["Death1"].length;
		model.animation.Play("Death1");

		down = false;
	}
	


}
