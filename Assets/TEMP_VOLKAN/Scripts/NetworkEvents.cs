using UnityEngine;
using uLink;

public class NetworkEvents: uLink.MonoBehaviour 
{
	// EVENT HANDLER
	vp_FPPlayerEventHandler m_Player;

	// DAMAGE HANDLER WHiCH HAS THE DAMAGE() FUNC iN iT
	public vp_PlayerDamageHandler2 PlayerDamageHandler = null;
	public vp_DamageHandler2 DamageHandler = null;
	
	
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
		Debug.Log ("DAMAGE OTHERS CALLED WITH DAMAGE : " + damage);
	}

	[RPC]
	public void DamageSync(float damage, uLink.NetworkMessageInfo info)
	{
		if ( DamageHandler != null )
		{
			// AI KULLANANLAR İÇİN DAMAGEHANDLER2.cs'yi KURCALA !!!!!!!!
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
}
