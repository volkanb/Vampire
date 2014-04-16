/////////////////////////////////////////////////////////////////////////////////
//
//	vp_PlayerDamageHandler.cs
//	� VisionPunk. All Rights Reserved.
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

	public bool sendHealth = false;
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
	void Start() 
	{
		netwEvents = gameObject.GetComponent<NetworkEvents> ();
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
		if (!enabled)
			return;

		if (!vp_Utility.IsActive(gameObject))
			return;

		base.Damage(damage);

		m_Player.HUDDamageFlash.Send(damage);


		// FOLLOWING CODES MAKE SURE THAT PROXIES AND OWNERS SYNC HEALTH WITH SERVER
		if (sendHealth && netwEvents != null ) 
		{
			netwEvents.DamageOthers(damage);
		}
	}

	/// <summary>
	/// instantiates the player's death effect, clears the current
	/// weapon, activates the Dead activity and prevents gameplay
	/// input. also, schedules respawning
	/// </summary>
	public override void Die()
	{
//		Debug.LogError("IM DEAD");
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

		m_Player.Position.Set(transform.position);
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

		if ((Respawns) && (isDead==false) && (isDown==true) && (Time.time-downTime>=4.0f))
		{
			Respawn();
		}

	}


}
