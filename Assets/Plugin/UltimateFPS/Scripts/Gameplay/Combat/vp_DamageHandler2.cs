/////////////////////////////////////////////////////////////////////////////////
//
//	vp_DamageHandler.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	class for having a gameobject take damage, die and respawn.
//					any other object can do damage on this monobehaviour like so:
//					    hitObject.SendMessage(Damage, 1.0f, SendMessageOptions.DontRequireReceiver);
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;


public class vp_DamageHandler2 : MonoBehaviour
{
	
	
	private float prevDamage;
	
	public bool is_Client = false;
	public bool RPCDamageEnable = false;
	
	// GETTING THE COMPONENTS OF STATE SYNC AND EVENTS
	private NetworkStateSync netwStateSync;
	private NetworkEvents netwEvents;
	
	
	
	// health and death
	public float MaxHealth = 1.0f;			// initial health of the object instance, to be reset on respawn
	public GameObject [] DeathSpawnObjects = null;	// gameobjects to spawn when object dies.
	// TIP: could be fx, could also be rigidbody rubble
	public float MinDeathDelay = 0.0f;		// random timespan in seconds to delay death. good for cool serial explosions
	public float MaxDeathDelay = 0.0f;
	public float m_CurrentHealth = 0.0f;	// current health of the object instance
	
	// respawn
	public bool Respawns = true;			// whether to respawn object or just delete it
	public float MinRespawnTime = 3.0f;		// random timespan in seconds to delay respawn
	public float MaxRespawnTime = 3.0f;
	public float RespawnCheckRadius = 1.0f;	// area around object which must be clear of other objects before respawn
	protected AudioSource m_Audio = null;
	public AudioClip DeathSound = null;		// sound to play upon death
	public AudioClip RespawnSound = null;	// sound to play upon respawn
	protected Vector3 m_StartPosition;		// initial position detected and used for respawn
	protected Quaternion m_StartRotation;	// initial rotation detected and used for respawn
	
	// impact damage
	public float ImpactDamageThreshold = 10;
	public float ImpactDamageMultiplier = 0.0f;
	public float reanimTime;
	public bool reanimisPlayed=false;
	private vp_FPPlayerEventHandler m_Player = null;
	//public bool isVampire=false;
	bool isPlayed=false;
	public GameObject capsule;
	
	/// <summary>
	/// 
	/// </summary>
	/// 
	/// 
	public void playReanim()
	{
		capsule.animation.Play("reanimate");
	}
	public bool isReanimPlaying()
	{
		
		/*if(capsule.animation["reanimate"].enabled==true)
		{
			return true;
		}
		else {return false;}*/
		if(GetComponent<AIPathVampire>().model.animation["Death1"].enabled==true)
		{
			return true;
		}
		else {return false;}
	}
	protected virtual void Awake()
	{
		m_Player = transform.GetComponent<vp_FPPlayerEventHandler>();
		m_Audio = audio;
		
		m_CurrentHealth = MaxHealth;
		m_StartPosition = transform.position;
		m_StartRotation = transform.rotation;
		
		if (DeathEffect != null)
			Debug.LogWarning("'vp_DamageHandler.DeathEffect' is obsolete and will be removed soon. Please use the new 'DeathSpawnObjects' array instead.");
		
	}
	
	
	void Start() 
	{
		netwEvents = gameObject.GetComponent<NetworkEvents> ();
		netwStateSync = gameObject.GetComponent<NetworkStateSync> ();
		prevDamage = 0f;
	}
	
	
	/// <summary>
	/// reduces current health by 'damage' points and kills the
	/// object if health runs out
	/// </summary>
	public virtual void Damage(float damage)
	{
		if( RPCDamageEnable )
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
			
			if (m_CurrentHealth <= 0.0f)
				return;
			
			m_CurrentHealth = Mathf.Min(m_CurrentHealth - damage, MaxHealth);
			
			
			if( is_Client ) 
				RPCDamageEnable = false;
			else
			{
				// FOLLOWING CODE INVOKES DAMAGE TO OTHERS BY PASSING IT AT THE SEND RATE
				netwStateSync.damageToBeDone += damage;
				//netwEvents.DamageOthers(damage);
				
				prevDamage = damage;
				
			}
			
			
			if (m_CurrentHealth <= 0.0f)
			{
				
				if (m_Audio != null)
				{
					m_Audio.pitch = Time.timeScale;
					m_Audio.PlayOneShot(DeathSound);
				}
				
				vp_Timer.In(Random.Range(MinDeathDelay, MaxDeathDelay), Die);
				return;
			}
			//isVampire=false;
			// TIP: if you want to do things like play a special impact
			// sound upon every hit (but only if the object survives)
			// this is the place
			
			
			
			
		}
		
		
		
		
		
	}
	
	
	/// <summary>
	/// removes the object, plays the death effect and schedules
	/// a respawn if enabled, otherwise destroys the object
	/// </summary>
	public virtual void Die()
	{
		if(transform.tag=="Vampire")
		{
			if ( !is_Client )
			{
				if(transform.GetComponent<AIPathVampire>().isDown==true)
				{
					
					if (!enabled || !vp_Utility.IsActive(gameObject))
						return;
					
					
					//vp_Utility.Activate(gameObject, false);
					/*vp_Utility.Activate(gameObject.transform.parent.parent.gameObject, false);
					Destroy(gameObject.transform.parent.parent.gameObject);*/
					//animation.Play("death");
					
					if (DeathEffect != null)
						Object.Instantiate(DeathEffect, transform.position, transform.rotation);
					
					m_Player.SetWeapon.Argument = 0;
					m_Player.SetWeapon.Start();
					//m_Player.Dead.Start();
					m_Player.AllowGameplayInput.Set(false);
					
					if (Respawns)
						vp_Timer.In(Random.Range(MinRespawnTime, MaxRespawnTime), Respawn);
					
					if(isPlayed==false)
					{
						isPlayed=true;
						reanimTime=Time.time;
						//capsule.animation.Play("death");
						GetComponent<AIPathVampire>().model.animation["Death1"].speed=1;
						GetComponent<AIPathVampire>().model.animation.Play("Death1");
						
						
					}
				}
			}
		}
		else
		{
			if (!enabled || !vp_Utility.IsActive(gameObject))
				return;
			
			//vp_Utility.Activate(gameObject, false);
			/*vp_Utility.Activate(gameObject.transform.parent.parent.gameObject, false);
				Destroy(gameObject.transform.parent.parent.gameObject);*/
			if(gameObject.tag=="Slayer")
			{
				GetComponent<AIPathSlayer>().model.GetComponent<soldierAnimation>().health=0;
				
			}
			/*else
			{
				GetComponent<AIPathSlayer>().model.animation.Play("death");
			}*/
			
			if (DeathEffect != null)
				Object.Instantiate(DeathEffect, transform.position, transform.rotation);
			
			m_Player.SetWeapon.Argument = 0;
			m_Player.SetWeapon.Start();
			//m_Player.Dead.Start();
			m_Player.AllowGameplayInput.Set(false);
			
			// DESTROYS THE SLAYER BOT
			netwEvents.ChangeStateSlayerNPC("DEAD");
			
			/*
			if (Respawns)
				vp_Timer.In(Random.Range(MinRespawnTime, MaxRespawnTime), Respawn);
			*/
			
		}
		
	}
	
	
	/// <summary>
	/// respawns the object if no other object is occupying the
	/// respawn area. otherwise reschedules respawning
	/// </summary>
	protected virtual void Respawn()
	{
		
		// return if the object has been destroyed (for example
		// as a result of loading a new level while it was gone)
		if (this == null)
			return;
		
		// don't respawn if checkradius contains the local player or props
		// TIP: this can be expanded upon to check for alternative object layers
		if (Physics.CheckSphere(m_StartPosition, RespawnCheckRadius, vp_Layer.Mask.PhysicsBlockers))
		{
			// attempt to respawn again until the checkradius is clear
			vp_Timer.In(Random.Range(MinRespawnTime, MaxRespawnTime), Respawn);
			return;
		}
		
		Reset();
		
		Reactivate();
		
	}
	
	
	/// <summary>
	/// resets health, position, angle and motion
	/// </summary>
	protected virtual void Reset()
	{
		
		m_CurrentHealth = MaxHealth;
		
		m_Player.Stop.Send();
		
		m_Player.Rotation.Set(m_StartRotation.eulerAngles);
		
		//m_Player.SetWeapon.TryStart(1);
		m_Player.SetWeapon.Argument = 1;
		m_Player.SetWeapon.Start ();
		
		//capsule.animation.Play("reanimate");
		
		if (rigidbody != null && !rigidbody.isKinematic)
		{
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.velocity = Vector3.zero;
		}
		
	}
	
	
	/// <summary>
	/// reactivates object and plays spawn sound
	/// </summary>
	protected virtual void Reactivate()
	{
		
		vp_Utility.Activate(gameObject);
		
		if (m_Audio != null)
		{
			m_Audio.pitch = Time.timeScale;
			m_Audio.PlayOneShot(RespawnSound);
		}
		
	}
	
	/// <summary>
	/// removes any bullet decals currently childed to this object
	/// </summary>
	protected virtual void RemoveBulletHoles()
	{
		
		foreach (Transform t in transform)
		{
			Component[] c;
			c = t.GetComponents<vp_HitscanBullet>();
			if (c.Length != 0)
				Object.Destroy(t.gameObject);
		}
		
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	/*void OnCollisionEnter(Collision collision)
	{

		float force = collision.impactForceSum.sqrMagnitude * 0.1f;

		float damage = (force > ImpactDamageThreshold) ? (force * ImpactDamageMultiplier) : 0.0f;

		if (damage > 0.0f)
		{
			if (m_CurrentHealth - damage <= 0.0f)
				MaxDeathDelay = MinDeathDelay = 0.0f;
			Damage(damage);
		}

	}*/
	
	/// <summary>
	/// [DEPRECATED] Please use the 'DeathSpawnObjects' array instead.
	/// </summary>
	public GameObject DeathEffect = null;
	void Update()
	{
		gameObject.layer=0;
		capsule.layer=0;
		if(transform.tag=="Vampire" && !is_Client)
		{
			if(transform.GetComponent<AIPathVampire>().isDown==false)
			{
				isPlayed=false;
			}
		}
	}
	
	public void CallRespawn()
	{
		Respawn();
		
		if( !is_Client )
			m_Player.GetComponent<AIPathVampire>().ResetVampireState();
		
	}
	
}

