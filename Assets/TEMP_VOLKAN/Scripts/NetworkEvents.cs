using UnityEngine;
using uLink;

public class NetworkEvents: uLink.MonoBehaviour 
{
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

	//----------------------------------------------------------------------------------------------------------
	// Following codes syncs owner's attacks with server and other proxies
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




}
