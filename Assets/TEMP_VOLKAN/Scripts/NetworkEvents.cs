using UnityEngine;
using uLink;

public class NetworkEvents: uLink.MonoBehaviour 
{

	// GETTING THE COMPONENT OF NETW STATESYNC
	private NetworkStateSync netwStateSync;

	// VAMPIRE AND SLAYER BASE POSITIONS
	private Vector3 SlayerBasePos;
	private Vector3 VampireBasePos;

	// EVENT HANDLER
	vp_FPPlayerEventHandler m_Player;

	// DAMAGE HANDLER WHiCH HAS THE DAMAGE() FUNC iN iT
	public vp_PlayerDamageHandler2 PlayerDamageHandler = null;
	public vp_DamageHandler2 DamageHandler = null;


	// COUNTDOWN TIMER VARIABLES
	//---------------------------------
	public bool roundStarted;
	private float remainingTime;
	private float endingTime;
	public int remainingMinutes;
	public int remainingSeconds;
	public float RoundTime;
	//---------------------------------

	// ROUND CONTROLLER VARIABLES
	//---------------------------------
	public int VampireTeamScore;
	public int SlayerTeamScore;
	//---------------------------------


	// WIN LOSE MESSAGE
	private string WinLoseText = null;


	public bool isDown = false;


	// ANIMATION CONTROLLERS
	//---------------------------------
	public VampireAIAnimationController VampAIAnimController;
	public SlayerAIAnimationController SlayAIAnimController;
	public HumanAIAnimationController HumAIAnimController;

	public SlayerPlayerAnimationController SlayPlayerAnimController;
	public VampirePlayerAnimationController VampPlayerAnimController;
	//---------------------------------
	
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

		// GETTING THE PLAYER'S DAMAGE HANDLER
		DamageHandler = transform.GetComponent<vp_DamageHandler2>();

		if( DamageHandler == null )
			PlayerDamageHandler = transform.GetComponent<vp_PlayerDamageHandler2>();

	}

	void Start()
	{
		// INITIALIZING NETW STATESYNC COMPONENT
		netwStateSync = gameObject.GetComponent<NetworkStateSync>();


		// VAMPIRE AND SLAYER BASE POSITIONS INITIALIZE
		VampireBasePos = GameObject.Find ("VampireBase").transform.position;
		SlayerBasePos = GameObject.Find ("SlayerBase").transform.position;
	}


	public int actionStatus = 0;

	void Update() 
	{

		if (roundStarted && !netwStateSync.isBot) 
		{
			remainingTime = endingTime - Time.time;
			
			int roundedInt = Mathf.CeilToInt( remainingTime );
			
			remainingMinutes = roundedInt / 60;
			remainingSeconds = roundedInt % 60;
		}


		// CORRECTS THE ACTION VARIABLE AT SERVER
		if (uLink.Network.isServer) 
		{
			if ( actionStatus == 2 )
				actionStatus = 0;
		}

		// SENDS ACTION VARIABLE STATUS TO SERVER
		if ( networkView.isOwner )
		{
			if ( actionStatus == 2 ) 
			{
				actionStatus = 0;
			}
			else if (Input.GetButton ("Action")) 
			{
				if ( actionStatus == 0 )
				{
					actionStatus = 1;
					SendAction(actionStatus);
				}
			}
			else if ( Input.GetButtonUp("Action") )
			{
				if ( actionStatus == 1 )
				{
					actionStatus = 2;
					SendAction(actionStatus);
				}
			}
		}

	}


	// SHOWS COUNTDOWN TIMER
	void OnGUI()
	{
		if (networkView.isOwner && roundStarted) 
		{
			string text = string.Format("{0:00}:{1:00}", remainingMinutes, remainingSeconds); 
			GUI.Label ( new Rect (400, 25, 100, 30), text);
		}

		if ( WinLoseText != null )
			GUI.Label ( new Rect (400, 400, 500, 30), WinLoseText);

	}

	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs server's round scores with others
	
	public void IncreaseSlayerScore()
	{
		networkView.UnreliableRPC("IncreaseSlayerScoreOnOthers", uLink.RPCMode.Others);	
		Debug.Log("SLAYER TEAM SCORE INCREASED !!!");
	}

	public void IncreaseVampireScore()
	{
		networkView.UnreliableRPC("IncreaseVampireScoreOnOthers", uLink.RPCMode.Others);
	}
	
	[RPC]
	void IncreaseSlayerScoreOnOthers()
	{
		SlayerTeamScore++;
	}

	[RPC]
	void IncreaseVampireScoreOnOthers()
	{
		VampireTeamScore++;
	}
	
	//----------------------------------------------------------------------------------------------------------



	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs server's round start and end with others

	public void StartRoundOnOthers (float RT)
	{
		networkView.UnreliableRPC("StartRound", uLink.RPCMode.Owner, RT);

	}

	[RPC]
	void StartRound( float RT )
	{
		Debug.Log ("ROUND STARTED");
		RoundTime = RT;
		endingTime = (Time.time + (RoundTime*60f));
		roundStarted = true;

	}

	//----------------------------------------------------------------------------------------------------------


	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs owner's attacks with server and other proxies, also sets the ammo count on owner and proxies
	void OnStart_Attack()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("AttackStartServer", uLink.NetworkPlayer.server);
	}
	void OnStop_Attack()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("AttackStopServer", uLink.NetworkPlayer.server);
	}
	[RPC]
	void AttackStartServer(uLink.NetworkMessageInfo info)
	{
		m_Player.Attack.TryStart();
		networkView.UnreliableRPC("AttackStartProxies", uLink.RPCMode.OthersExceptOwner);
	}
	[RPC]
	void AttackStopServer(uLink.NetworkMessageInfo info)
	{	
		m_Player.Attack.TryStop();
		networkView.UnreliableRPC("AttackStopProxies", uLink.RPCMode.OthersExceptOwner);
		networkView.UnreliableRPC("SetAmmo", uLink.RPCMode.Others, m_Player.CurrentWeaponAmmoCount.Get());
	}
	[RPC]
	void AttackStartProxies(uLink.NetworkMessageInfo info)
	{
		m_Player.Attack.TryStart();
	}
	[RPC]
	void AttackStopProxies(uLink.NetworkMessageInfo info)
	{		
		m_Player.Attack.TryStop();
	}

	[RPC]
	void SetAmmo( int ammoCount, uLink.NetworkMessageInfo info )
	{
		m_Player.CurrentWeaponAmmoCount.Set(ammoCount);
	}
	//----------------------------------------------------------------------------------------------------------
	
	
	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs owner's weapon change with server and other proxies
	void OnStart_SetWeapon()
	{	
		if (networkView.isOwner) 
			networkView.UnreliableRPC("SetWeaponStartServer", uLink.NetworkPlayer.server, m_Player.SetWeapon.Argument);
	}
	[RPC]
	void SetWeaponStartServer(int WeaponID, uLink.NetworkMessageInfo info)
	{
		m_Player.SetWeapon.TryStart(WeaponID);
		networkView.UnreliableRPC("SetWeaponStartProxies", uLink.RPCMode.OthersExceptOwner, WeaponID);
	}
	[RPC]
	void SetWeaponStartProxies(int WeaponID, uLink.NetworkMessageInfo info)
	{
		m_Player.SetWeapon.TryStart(WeaponID);
	}
	//----------------------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs owner's reload attempts with server and other proxies
	void OnStart_Reload()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("ReloadStartServer", uLink.NetworkPlayer.server);
	}
	[RPC]
	void ReloadStartServer(uLink.NetworkMessageInfo info)
	{
		m_Player.Reload.TryStart();
		networkView.UnreliableRPC("ReloadStartProxies", uLink.RPCMode.OthersExceptOwner);
	}
	[RPC]
	void ReloadStartProxies(uLink.NetworkMessageInfo info)
	{
		m_Player.Reload.TryStart();
	}
	//----------------------------------------------------------------------------------------------------------


	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs owner's crouches with server and other proxies
	void OnStart_Crouch()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("CrouchStartServer", uLink.NetworkPlayer.server);
	}
	void OnStop_Crouch()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("CrouchStopServer", uLink.NetworkPlayer.server);
	}
	[RPC]
	void CrouchStartServer(uLink.NetworkMessageInfo info)
	{
		m_Player.Crouch.TryStart();
		networkView.UnreliableRPC("CrouchStartProxies", uLink.RPCMode.OthersExceptOwner);
	}
	[RPC]
	void CrouchStopServer(uLink.NetworkMessageInfo info)
	{		
		m_Player.Crouch.TryStop();
		networkView.UnreliableRPC("CrouchStopProxies", uLink.RPCMode.OthersExceptOwner);
	}
	[RPC]
	void CrouchStartProxies(uLink.NetworkMessageInfo info)
	{
		m_Player.Crouch.TryStart();
	}
	[RPC]
	void CrouchStopProxies(uLink.NetworkMessageInfo info)
	{		
		m_Player.Crouch.TryStop();
	}
	//----------------------------------------------------------------------------------------------------------

	
	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs owner's zooms with server and other proxies
	void OnStart_Zoom()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("ZoomStartServer", uLink.NetworkPlayer.server);
	}
	void OnStop_Zoom()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("ZoomStopServer", uLink.NetworkPlayer.server);
	}
	[RPC]
	void ZoomStartServer(uLink.NetworkMessageInfo info)
	{
		m_Player.Zoom.TryStart();
		networkView.UnreliableRPC("ZoomStartProxies", uLink.RPCMode.OthersExceptOwner);
	}
	[RPC]
	void ZoomStopServer(uLink.NetworkMessageInfo info)
	{		
		m_Player.Zoom.TryStop();
		networkView.UnreliableRPC("ZoomStopProxies", uLink.RPCMode.OthersExceptOwner);
	}
	[RPC]
	void ZoomStartProxies(uLink.NetworkMessageInfo info)
	{
		m_Player.Zoom.TryStart();
	}
	[RPC]
	void ZoomStopProxies(uLink.NetworkMessageInfo info)
	{		
		m_Player.Zoom.TryStop();
	}
	//----------------------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs owner's jumps with server and other proxies
	void OnStart_Jump()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("JumpStartServer", uLink.NetworkPlayer.server);
	}
	void OnStop_Jump()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("JumpStopServer", uLink.NetworkPlayer.server);
	}
	[RPC]
	void JumpStartServer(uLink.NetworkMessageInfo info)
	{
		m_Player.Jump.TryStart();
		networkView.UnreliableRPC("JumpStartProxies", uLink.RPCMode.OthersExceptOwner);
	}
	[RPC]
	void JumpStopServer(uLink.NetworkMessageInfo info)
	{		
		m_Player.Jump.TryStop();
		networkView.UnreliableRPC("JumpStopProxies", uLink.RPCMode.OthersExceptOwner);
	}
	[RPC]
	void JumpStartProxies(uLink.NetworkMessageInfo info)
	{
		m_Player.Jump.TryStart();
	}
	[RPC]
	void JumpStopProxies(uLink.NetworkMessageInfo info)
	{		
		m_Player.Jump.TryStop();
	}
	//----------------------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs owner's run with server and other proxies
	void OnStart_Run()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("RunStartServer", uLink.NetworkPlayer.server);
	}
	void OnStop_Run()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("RunStopServer", uLink.NetworkPlayer.server);
	}
	[RPC]
	void RunStartServer(uLink.NetworkMessageInfo info)
	{
		m_Player.Run.TryStart();
		networkView.UnreliableRPC("RunStartProxies", uLink.RPCMode.OthersExceptOwner);
	}
	[RPC]
	void RunStopServer(uLink.NetworkMessageInfo info)
	{		
		m_Player.Run.TryStop();
		networkView.UnreliableRPC("RunStopProxies", uLink.RPCMode.OthersExceptOwner);
	}
	[RPC]
	void RunStartProxies(uLink.NetworkMessageInfo info)
	{
		m_Player.Run.TryStart();
	}
	[RPC]
	void RunStopProxies(uLink.NetworkMessageInfo info)
	{		
		m_Player.Run.TryStop();
	}
	//----------------------------------------------------------------------------------------------------------


	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs owner's interact with server
	void OnStart_Interact()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("InteractStartServer", uLink.NetworkPlayer.server);
	}
	void OnStop_Interact()
	{
		if (networkView.isOwner) 
			networkView.UnreliableRPC("InteractStopServer", uLink.NetworkPlayer.server);
	}
	[RPC]
	void InteractStartServer(uLink.NetworkMessageInfo info)
	{
		m_Player.Interact.TryStart();
	}
	[RPC]
	void InteractStopServer(uLink.NetworkMessageInfo info)
	{		
		m_Player.Interact.TryStop();
	}
	//----------------------------------------------------------------------------------------------------------



	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs server's damage with others
	public void DamageOthers(float damage)
	{ 
		networkView.UnreliableRPC("DamageSync", uLink.RPCMode.Others, damage);
		// Debug.Log ("DAMAGE OTHERS CALLED WITH DAMAGE : " + damage);
	}

	[RPC]
	public void DamageSync(float damage, uLink.NetworkMessageInfo info)
	{
		if ( DamageHandler != null )
		{
			DamageHandler.RPCDamageEnable = true;
			DamageHandler.Damage ( damage );
		}
		else
		{
			PlayerDamageHandler.RPCDamageEnable = true;
			PlayerDamageHandler.Damage( damage );
		}

	}

	//----------------------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs server's clip count with others
	public void SupplySpotStart()
	{ 
		networkView.UnreliableRPC("SupplySpotStartOthers", uLink.RPCMode.Others);
	}
	
	[RPC]
	void SupplySpotStartOthers(uLink.NetworkMessageInfo info)
	{
		int toAdd = 10 - m_Player.GetItemCount.Send("AmmoClip");
		
		m_Player.AddItem.Try(new object[] { "AmmoClip", toAdd });

		m_Player.Health.Set(100f);

	}
	
	//----------------------------------------------------------------------------------------------------------


	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs human NPC'S state change with proxies
	public void ChangeStateNPC(string s)
	{ 
		networkView.UnreliableRPC("ChangeStateOtherNPCs", uLink.RPCMode.Others, s);
		Debug.Log ("HUMAN STATE IS : " + s);
	}

	[RPC]
	void ChangeStateOtherNPCs(string state, uLink.NetworkMessageInfo info)
	{
		switch (state) 
		{
		case "WANDER":

			break;

		case "RUN":
			
			break;

		case "FOLLOW":
			m_Player.SetWeapon.TryStart(1);
			GetComponent<vp_DamageHandler2>().capsule.renderer.material.color = Color.cyan;

			HumAIAnimController.SetModelWithName("armed");

			break;

		case "DEFEND":
			
			break;

		case "POSSESSED":
			m_Player.SetWeapon.TryStart(2);
			GetComponent<vp_DamageHandler2>().capsule.renderer.material.color = Color.magenta;

			HumAIAnimController.SetModelWithName("possessed");

			break;

		case "CHANNELING":
			
			break;

		case "DEAD":

			break;

		case "RESCUED":

			break;

		case "IDLE":
			
			break;

		
		
		default:
			break;
		}

		
	}
	
	//----------------------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs slayer NPC'S state change with proxies
	public void ChangeStateSlayerNPC(string s)
	{ 
		networkView.UnreliableRPC("ChangeStateOtherSlayerNPCs", uLink.RPCMode.Others, s);
		Debug.Log ("SLAYER BOT STATE IS : " + s);
		
		if (s == "DEAD") 
		{
			// DESTROYS BOT WHEN DEAD
			uLink.Network.Destroy(gameObject.GetComponent<uLink.NetworkView>());
		}	
		
	}

	[RPC]
	void ChangeStateOtherSlayerNPCs(string state, uLink.NetworkMessageInfo info)
	{
				switch (state) {
	
				case "DEAD":
						break;
			
				case "GOTOHUMAN":
						break;
			
				case "DEFEND":
						break;
			
				case "SEARCH":
						break;
			
				case "STAKE":
						break;

				case "ESCORT":
						break;
			
				default:
						break;
				}

	}


	//----------------------------------------------------------------------------------------------------------


	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs vampire NPC'S state change with proxies
	public void ChangeStateVampireNPC(string s)
	{ 
		networkView.UnreliableRPC("ChangeStateOtherVampireNPCs", uLink.RPCMode.Others, s);
		Debug.Log ("VAMPIRE STATE IS : " + s);

		if (s == "DEAD") 
		{
			// DESTROYS BOT WHEN DEAD
			uLink.Network.Destroy(gameObject.GetComponent<uLink.NetworkView>());
		}	

	}
	
	[RPC]
	void ChangeStateOtherVampireNPCs(string state, uLink.NetworkMessageInfo info)
	{
		switch (state) 
		{
		
		case "CHANNELING":
			isDown = false;
			
			break;
			
		case "DEAD":
			isDown = false;

			// KILLS THE UFPS OBJECT
			//m_Player.GetComponent<vp_DamageHandler2>().KillVampireAI();

			
			break;
	
		case "ATTACKHUMAN":
			isDown = false;

			break;
			
		case "DOWN":
			isDown = true;
			
			break;
			
		case "SEARCH":
			isDown = false;

			break;
			
		case "ATTACK":
			isDown = false;
			
			break;
			
		default:
			break;
		}
		
		
	}
	//----------------------------------------------------------------------------------------------------------


	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs vampire player's bite attemtp with server



	public void SendAction( int i )
	{ 
		if ( networkView.isOwner )
			networkView.UnreliableRPC("SendActionToServer", uLink.RPCMode.Server, i);
	}

	
	[RPC]
	void SendActionToServer(int i, uLink.NetworkMessageInfo info)
	{
		actionStatus = i;
	}

	
	//----------------------------------------------------------------------------------------------------------


	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs servers respawn at base attempts with others
		
	
	public void RespawnAtBase()
	{ 
		vp_PlayerDamageHandler2 PDH2 = null;
		PDH2 = gameObject.GetComponent<vp_PlayerDamageHandler2>();

		vp_DamageHandler2 DH2 = null;
		DH2 = gameObject.GetComponent<vp_DamageHandler2>();

		if (DH2 != null) 
		{
			// IF THIS INSTANCE IS A AI OBJECT
			if ( gameObject.tag == "Vampire" )
			{
				m_Player.Position.Set(VampireBasePos);
			}
			else if ( gameObject.tag == "Slayer" )
			{
				m_Player.Position.Set(SlayerBasePos);
			}

			DH2.CallRespawn();

		}
		else if ( PDH2 != null )
		{
			// IF THIS INSTANCE IS A PLAYER

			if ( gameObject.tag == "VampirePlayer" )
			{
				m_Player.Position.Set(VampireBasePos);
			}
			else if ( gameObject.tag == "SlayerPlayer" )
			{
				m_Player.Position.Set(SlayerBasePos);
			}

			PDH2.CallRespawn();

		}
	}

	//----------------------------------------------------------------------------------------------------------


	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs winning/losing messages with clients

	public void WinLose( char c )
	{ 
		networkView.UnreliableRPC("WinLoseOthers", uLink.RPCMode.Owner, c);
	}
	
	
	[RPC]
	void WinLoseOthers(char c, uLink.NetworkMessageInfo info)
	{
		m_Player.AllowGameplayInput.Set(false);

		if (c == 's' )
			WinLoseText = "Slayer Team Won !!!";
		else if ( c == 'v' )
			WinLoseText = "Vampire Team Won !!!";
		else if ( c == 'a' )
			WinLoseText = "Round Ended !!!";
	}
	

	//----------------------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs AI vampire animations with proxies
	
	public void VampireAIAnimationSync( string animName, int layer = 99, float weight = 99f )
	{ 
		if ( layer == 99 && weight == 99f )
			networkView.UnreliableRPC("VampireAIAnimationSyncOthersName", uLink.RPCMode.AllExceptOwner, animName);
		else if ( layer == 99 && weight != 99f )
			networkView.UnreliableRPC("VampireAIAnimationSyncOthersWeight", uLink.RPCMode.AllExceptOwner, animName, weight);
		else if ( layer != 99 && weight == 99f )
			networkView.UnreliableRPC("VampireAIAnimationSyncOthersLayer", uLink.RPCMode.AllExceptOwner, animName, layer);
		else if ( layer != 99 && weight != 99f )
			networkView.UnreliableRPC("VampireAIAnimationSyncOthersLayerWeight", uLink.RPCMode.AllExceptOwner, animName, layer, weight);
	}

	[RPC]
	void VampireAIAnimationSyncOthersName(string animName, uLink.NetworkMessageInfo info)
	{
		VampAIAnimController.StartAnimWithName ( animName );
	}
	[RPC]
	void VampireAIAnimationSyncOthersWeight(string animName, float weight, uLink.NetworkMessageInfo info)
	{
		VampAIAnimController.StartAnimWithNameWeight ( animName, weight );
	}
	[RPC]
	void VampireAIAnimationSyncOthersLayer(string animName, int layer, uLink.NetworkMessageInfo info)
	{
		VampAIAnimController.StartAnimWithNameLayer ( animName, layer );
	}
	[RPC]
	void VampireAIAnimationSyncOthersLayerWeight(string animName, int layer, float weight, uLink.NetworkMessageInfo info)
	{
		VampAIAnimController.StartAnimWithNameLayerWeight ( animName, layer, weight );
	}
	
	
	//----------------------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs AI slayer animations with proxies
	
	public void SlayerAIAnimationSync( string animName, int layer = 99, float weight = 99f )
	{ 
		if ( layer == 99 && weight == 99f )
			networkView.UnreliableRPC("SlayerAIAnimationSyncOthersName", uLink.RPCMode.AllExceptOwner, animName);
		else if ( layer == 99 && weight != 99f )
			networkView.UnreliableRPC("SlayerAIAnimationSyncOthersWeight", uLink.RPCMode.AllExceptOwner, animName, weight);
		else if ( layer != 99 && weight == 99f )
			networkView.UnreliableRPC("SlayerAIAnimationSyncOthersLayer", uLink.RPCMode.AllExceptOwner, animName, layer);
		else if ( layer != 99 && weight != 99f )
			networkView.UnreliableRPC("SlayerAIAnimationSyncOthersLayerWeight", uLink.RPCMode.AllExceptOwner, animName, layer, weight);
	}

	[RPC]
	void SlayerAIAnimationSyncOthersName(string animName, uLink.NetworkMessageInfo info)
	{
		SlayAIAnimController.StartAnimWithName ( animName );
	}
	[RPC]
	void SlayerAIAnimationSyncOthersWeight(string animName, float weight, uLink.NetworkMessageInfo info)
	{
		SlayAIAnimController.StartAnimWithNameWeight ( animName, weight );
	}
	[RPC]
	void SlayerAIAnimationSyncOthersLayer(string animName, int layer, uLink.NetworkMessageInfo info)
	{
		SlayAIAnimController.StartAnimWithNameLayer ( animName, layer );
	}
	[RPC]
	void SlayerAIAnimationSyncOthersLayerWeight(string animName, int layer, float weight, uLink.NetworkMessageInfo info)
	{
		SlayAIAnimController.StartAnimWithNameLayerWeight ( animName, layer, weight );
	}
	
	
	//----------------------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs slayer's weapon change animations with others
	
	public void SlayerAISetWeaponAnimSync ( int weaponID )
	{ 

		networkView.UnreliableRPC("SlayerAISetWeaponAnimSyncOthers", uLink.RPCMode.Others, weaponID);

	}
	
	[RPC]
	void SlayerAISetWeaponAnimSyncOthers(int weaponID, uLink.NetworkMessageInfo info)
	{
		SlayAIAnimController.SetWeapon (weaponID);
	}
	
	//----------------------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs human animations with proxies
	
	public void HumanAIAnimationSync( string animName, int layer = 99, float weight = 99f )
	{ 
		if ( layer == 99 && weight == 99f )
			networkView.UnreliableRPC("HumanAIAnimationSyncOthersName", uLink.RPCMode.AllExceptOwner, animName);
		else if ( layer == 99 && weight != 99f )
			networkView.UnreliableRPC("HumanAIAnimationSyncOthersWeight", uLink.RPCMode.AllExceptOwner, animName, weight);
		else if ( layer != 99 && weight == 99f )
			networkView.UnreliableRPC("HumanAIAnimationSyncOthersLayer", uLink.RPCMode.AllExceptOwner, animName, layer);
		else if ( layer != 99 && weight != 99f )
			networkView.UnreliableRPC("HumanAIAnimationSyncOthersLayerWeight", uLink.RPCMode.AllExceptOwner, animName, layer, weight);
	}
	
	[RPC]
	void HumanAIAnimationSyncOthersName(string animName, uLink.NetworkMessageInfo info)
	{
		HumAIAnimController.StartAnimWithName ( animName );
	}
	[RPC]
	void HumanAIAnimationSyncOthersWeight(string animName, float weight, uLink.NetworkMessageInfo info)
	{
		HumAIAnimController.StartAnimWithNameWeight ( animName, weight );
	}
	[RPC]
	void HumanAIAnimationSyncOthersLayer(string animName, int layer, uLink.NetworkMessageInfo info)
	{
		HumAIAnimController.StartAnimWithNameLayer ( animName, layer );
	}
	[RPC]
	void HumanAIAnimationSyncOthersLayerWeight(string animName, int layer, float weight, uLink.NetworkMessageInfo info)
	{
		HumAIAnimController.StartAnimWithNameLayerWeight ( animName, layer, weight );
	}
	
	
	//----------------------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs human models with proxies
	
	public void HumanAIModelSync( string modelName )
	{ 

		networkView.UnreliableRPC("HumanAIModelSyncOthers", uLink.RPCMode.AllExceptOwner, modelName);

	}
	
	[RPC]
	void HumanAIModelSyncOthers(string modelName, uLink.NetworkMessageInfo info)
	{
		HumAIAnimController.SetModelWithName( modelName );
	}
	
	
	//----------------------------------------------------------------------------------------------------------


}
