/////////////////////////////////////////////////////////////////////////////////
//
//	vp_PlayerDamageHandler.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a version of the vp_DamageHandler class extended for use with
//					the player event handler, via which it talks to the player HUD,
//					weapon handler, controller and camera
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(vp_FPPlayerEventHandler))]

public class vp_PlayerDamageHandler2 : vp_DamageHandler
{

	private float prevDamage;

	public bool is_Client = false;
	public bool RPCDamageEnable = false;

	// GETTING THE COMPONENTS OF STATE SYNC AND EVENTS
	private NetworkStateSync netwStateSync;
	private NetworkEvents netwEvents;

	private vp_FPPlayerEventHandler m_Player = null;
	protected float m_RespawnOffset = 0.0f;

	// falling damage
	public bool AllowFallDamage = true;
	public float FallImpactThreshold = .15f;
	public bool DeathOnFallImpactThreshold = false;
	public Vector2 FallImpactPitch = new Vector2(1.0f, 1.5f);	// random pitch range for fall impact sounds
	public List<AudioClip> FallImpactSounds = new List<AudioClip>();
	protected float m_FallImpactMultiplier = 2;
	public bool isDown=false; 
	public bool isDead=false;
	float downTime;
	bool isSet=false;
	public GameObject capsule;
	bool isKeyHolding;
	float channelingTime;
	public GameObject target;

	GameObject getClosestHuman()
	{
		ArrayList myList = new ArrayList();
		GameObject[] targets;
		targets=GameObject.FindGameObjectsWithTag("Human");
		//add armed human
		target=null;
		//float distance=Mathf.Infinity;
		foreach (GameObject go in targets)
		{
			Vector3 diff=go.transform.position-transform.position;
			if((diff.magnitude<=1.9f) && (go.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0) 
			 && (go.GetComponent<AIPathHuman>().isRescued==false) && (go.GetComponent<AIPathHuman>().isPossessed==false))
			{
				target=go;
				break;
			}
		}	
		return target;
	}

	GameObject getClosestHuman1()
	{
		ArrayList myList = new ArrayList();
		GameObject[] targets;
		GameObject[] targets2;
		targets=GameObject.FindGameObjectsWithTag("Human");
		targets2=GameObject.FindGameObjectsWithTag("ArmedHuman");
		//add armed human
		target=null;
		//float distance=Mathf.Infinity;
		foreach (GameObject go in targets)
		{
			Vector3 diff=go.transform.position-transform.position;
			if((diff.magnitude<=1.9f) && (go.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0) 
			&& (go.GetComponent<AIPathHuman>().isRescued==false) && (go.GetComponent<AIPathHuman>().isPossessed==false) && (go.GetComponent<AIPathHuman>().isFollowing==false))
			{
				target=go;
				break;
			}
		}	
		foreach (GameObject go in targets2)
		{
			Vector3 diff=go.transform.position-transform.position;
			if((diff.magnitude<=1.9f) && (go.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0) 
			&& (go.GetComponent<AIPathHuman>().isRescued==false) && (go.GetComponent<AIPathHuman>().isPossessed==false) && (go.GetComponent<AIPathHuman>().isFollowing==false))
			{
				target=go;
				break;
			}
		}
		return target;
	}

	void Start() 
	{
		target=null;
		netwEvents = gameObject.GetComponent<NetworkEvents> ();
		netwStateSync = gameObject.GetComponent<NetworkStateSync> ();
		prevDamage = 0f;
	}


	/// <summary>
	/// 
	/// </summary>
	protected override void Awake()
	{

		base.Awake();

		m_Player = transform.GetComponent<vp_FPPlayerEventHandler>();

	}


	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected virtual void OnEnable()
	{

		if (m_Player != null)
			m_Player.Register(this);

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected virtual void OnDisable()
	{

		if (m_Player != null)
			m_Player.Unregister(this);

	}


	/// <summary>
	/// applies damage to the player and sends a message to the
	/// HUD that a damage flash should be played
	/// </summary>
	public override void Damage(float damage)
	{
		if (RPCDamageEnable) 
		{

			if( ( damage - (float)Mathf.FloorToInt(damage) ) > 0 )
			{
				Debug.Log("DAMAGE FAIL. REVERSING DAMAGE. FAILED DAMAGE QUATITY IS : " + damage);
				damage = prevDamage;
			}

			
			if (!enabled)
				return;
			
			if (!vp_Utility.IsActive(gameObject))
				return;




			if( is_Client ) 
				RPCDamageEnable = false;
			else
			{
				// FOLLOWING CODE INVOKES DAMAGE TO OTHERS BY PASSING IT AT THE SEND RATE
				// Debug.Log("DAMAGE IS : " + damage);
				netwStateSync.damageToBeDone += damage;
				//netwEvents.DamageOthers(damage);
				
				prevDamage = damage;
				
			}


			base.Damage(damage);
			
			m_Player.HUDDamageFlash.Send(damage);

		}

	}

	/// <summary>
	/// instantiates the player's death effect, clears the current
	/// weapon, activates the Dead activity and prevents gameplay
	/// input. also, schedules respawning
	/// </summary>
	public override void Die()
	{

		if(gameObject.tag=="VampirePlayer")
		{
			if(isSet==false)
			{
				downTime=Time.time;
				isSet=true;
				capsule.animation.Play("death");
			}
			isDown=true;

			if (!enabled || !vp_Utility.IsActive(gameObject))
				return;

			if (DeathEffect != null)
				Object.Instantiate(DeathEffect, transform.position, transform.rotation);
			
			m_Player.SetWeapon.Argument = 0;
			m_Player.SetWeapon.Start();
			m_Player.Dead.Start();
			m_Player.AllowGameplayInput.Set(false);

		}

		else
		{
			capsule.animation.Play("death");

			if (!enabled || !vp_Utility.IsActive(gameObject))
				return;
			
			if (DeathEffect != null)
				Object.Instantiate(DeathEffect, transform.position, transform.rotation);
			
			m_Player.SetWeapon.Argument = 0;
			m_Player.SetWeapon.Start();
			m_Player.Dead.Start();
			m_Player.AllowGameplayInput.Set(false);

			// RESPAWN SLAYER PLAYER AT BASE WHEN HE DIES
			netwEvents.RespawnAtBase();

			/*
			if (Respawns)
				vp_Timer.In(Random.Range(MinRespawnTime, MaxRespawnTime), Respawn);
			*/

		}

	}


	/// <summary>
	/// handles respawning the player in such a way that it doesn't
	/// intersect with other objects. then reactivates the player
	/// </summary>
	protected override void Respawn()
	{

		// return if the object has been destroyed (for example
		// as a result of loading a new level while it was gone)
		if (this == null)
			return;

		// adjust respawn position upwards if respawn position contains any obstacles
		// TIP: this can be expanded upon to check for alternative object layers
		if (Physics.CheckSphere(transform.position + (Vector3.up * m_RespawnOffset), RespawnCheckRadius, vp_Layer.Mask.PhysicsBlockers))
		{
			m_RespawnOffset += 1.0f;
			Respawn();
			return;
		}

		m_RespawnOffset = 0.0f;

		Reactivate();

		Reset();
		
	}


	/// <summary>
	/// resets health, position, angle and motion + camera angle
	/// </summary>
	protected override void Reset()
	{

		m_CurrentHealth = MaxHealth;

		m_Player.Stop.Send();

		m_Player.Rotation.Set(m_StartRotation.eulerAngles);

		m_Player.SetWeapon.TryStart(1);
		isDown=false;
		isSet=false;
		capsule.animation.Play("reanimate");
	}


	/// <summary>
	/// reactivates object and plays spawn sound + disables 'Dead' state
	/// </summary>
	protected override void Reactivate()
	{

		m_Player.Dead.Stop();
		m_Player.AllowGameplayInput.Set(true);

		m_Player.HUDDamageFlash.Send(0.0f);

		if (m_Audio != null)
		{
			m_Audio.pitch = Time.timeScale;
			m_Audio.PlayOneShot(RespawnSound);
		}

	}


	/// <summary>
	/// gets or sets the current player 
	/// </summary>
	protected virtual float OnValue_Health
	{
		get
		{
			return m_CurrentHealth;
		}
		set
		{
			m_CurrentHealth = Mathf.Min(value, MaxHealth);	// health is not allowed to go above max, but negative health is allowed (for gibbing)
		}
	}
	
	
	/// <summary>
	/// applies falling damage to the player
	/// </summary>
	protected virtual void OnMessage_FallImpact(float impact)
	{
		
		if(m_Player.Dead.Active || !AllowFallDamage || impact <= FallImpactThreshold)
			return;
		
		vp_Utility.PlayRandomSound(m_Audio, FallImpactSounds, FallImpactPitch);

		float damage = (float)Mathf.Abs((float)(DeathOnFallImpactThreshold ? MaxHealth : MaxHealth * impact));
		
		Damage(damage);

	}


	/// <summary>
	/// 
	/// </summary>
	void Update()
	{
		gameObject.layer=0;
		// restore time if dying during slomo
		if (m_Player.Dead.Active && Time.timeScale < 1.0f)
			vp_TimeUtility.FadeTimeScale(1.0f, 0.05f);

		if(gameObject.tag=="VampirePlayer")
		{
			if ((Respawns) && (isDead==false) && (isDown==true) && (Time.time-downTime>=4.0f))
			{
				Respawn();
			}

			else if( ( netwEvents.actionStatus == 1 && is_Client == false ) && (isKeyHolding==false) && (getClosestHuman()!=null))
			{
				target=getClosestHuman();
				isKeyHolding=true;
				channelingTime=Time.time;
			}
			else if( ( netwEvents.actionStatus == 1 && is_Client == false ) &&  (isKeyHolding==true) && ((target.transform.position-transform.position).magnitude>1.9f) )
			{
				target.GetComponent<AIPathHuman>().isChanneling=false;
				isKeyHolding=false;
				target=null;
			}
			else if( ( netwEvents.actionStatus == 1 && is_Client == false ) && (Time.time-channelingTime>4f) &&  (isKeyHolding==true) && ((target.transform.position-transform.position).magnitude<=1.9f) )
			{
				target.GetComponent<AIPathHuman>().isPossessed=true;
				target.GetComponent<vp_DamageHandler2>().capsule.renderer.material.color=Color.magenta;
			}
			else if( ( netwEvents.actionStatus == 1 && is_Client == false ) && (isKeyHolding==true) && ((target.transform.position-transform.position).magnitude<=1.9f))
			{
				target.GetComponent<AIPathHuman>().isChanneling=true;
				if(GetComponent<vp_PlayerDamageHandler2>().m_CurrentHealth<GetComponent<vp_PlayerDamageHandler2>().MaxHealth)
				{
					GetComponent<vp_PlayerDamageHandler2>().m_CurrentHealth+=0.015f;
				}
			}

			else if ( ( netwEvents.actionStatus == 2  && is_Client == false ) && (isKeyHolding==true))
			{
				target.GetComponent<AIPathHuman>().isChanneling=false;
				isKeyHolding=false;
				target=null;
			}

		}
		else if(gameObject.tag=="SlayerPlayer")
		{
			if( ( netwEvents.actionStatus == 1  && is_Client == false ) &&(target!=null) && ((target.transform.position-transform.position).magnitude<=1.9f) )
			{
				target.GetComponent<AIPathHuman>().isFollowing=false;
				target.GetComponent<AIPathHuman>().target=gameObject.transform;
				target=null;
			}

			else if( ( netwEvents.actionStatus == 1  && is_Client == false ) && (getClosestHuman1()!=null) )
			{
				target=getClosestHuman1();
				if(((target.transform.position-transform.position).magnitude<=1.9f) && (target.GetComponent<AIPathHuman>().isFollowing==false))
				{
					vp_FPPlayerEventHandler Human =(vp_FPPlayerEventHandler)target.transform.root.GetComponentInChildren(typeof(vp_FPPlayerEventHandler));
					Human.SetWeapon.TryStart(1);
					target.GetComponent<AIPathHuman>().isArmed=true;
					target.tag="ArmedHuman";
					target.GetComponent<vp_DamageHandler2>().capsule.renderer.material.color=Color.cyan;
					target.GetComponent<AIPathHuman>().isFollowing=true;
					target.GetComponent<AIPathHuman>().target=gameObject.transform;
				}

			}



		}



	}

	public void CallRespawn()
	{
		Respawn();
	}

}