using UnityEngine;
using uLink;

public class NetworkTraffic : uLink.MonoBehaviour
{
	public float maxSpeed = 4.6f;
	public float maxDistance = 10f;
	public float arrivalDistance = 1;
	
	[HideInInspector]
	public Vector3 velocity;
	
	public bool moveCharacter = true;
	
	[HideInInspector]
	public float arrivalSpeed;
	
	private Vector3 targetDir;
	private float targetDistance;
	
	private bool firstState = true;
	
	public float rotationDamping = 0.85f;
	
	private Quaternion curRot;
	private Quaternion targetRot;
	
	private CharacterController character;
	
	private double serverLastTimestamp = 0;
	
	private bool isInitiaized = false;

	// EVENT HANDLER
	vp_FPPlayerEventHandler m_Player;
	

	void Awake()
	{
		// GETTING THE PLAYER'S EVENT HANDLER
		m_Player = transform.GetComponent<vp_FPPlayerEventHandler>();

		arrivalSpeed = maxSpeed / arrivalDistance;
		
		curRot = transform.rotation;
		
		character = GetComponent<CharacterController>();
	}
	
	void Start()
	{
		if (networkView.viewID == uLink.NetworkViewID.unassigned) return;
		
		isInitiaized = true;
		
		if (!networkView.isMine) return;
		
		enabled = false;
		
		if (uLink.Network.isAuthoritativeServer && uLink.Network.isClient)
		{
			InvokeRepeating("SendToServer", 0, 1.0f / uLink.Network.sendRate);
		}

		if (!networkView.isOwner)
						m_Player.AllowGameplayInput.Set (false);

	}
	
	void uLink_OnSerializeNetworkView(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.isWriting)
		{
			// Send information to all proxies (opponent player's computers)
			// This code is executed on the creator (server) when server is auth, or on the owner (client) when the server is non-auth.
			stream.Write(m_Player.Position.Get());
			stream.Write(m_Player.Velocity.Get());
			stream.Write(m_Player.Rotation.Get());
		}
		else
		{
			// Update the proxy state when statesync arrives.
			Vector3 pos = stream.Read<Vector3>();
			Vector3 vel = stream.Read<Vector3>();
			Vector2 rot = stream.Read<Vector2>();
			
			UpdateState(pos, vel, rot, info.timestamp);
		}
	}
	
	private void UpdateState(Vector3 pos, Vector3 vel, Vector2 rot, double timestamp)
	{
		/*
		float deltaTime = (float)(uLink.Network.time - timestamp);
		Vector3 target = pos + vel * deltaTime;
		
		if (firstState)
		{
			firstState = false;
			targetDistance = 0;
			transform.position = target;
			return;
		}
		
		targetRot = rot;
		Vector3 offset = target - transform.position;
		targetDistance = offset.magnitude;
		
		if (targetDistance > maxDistance)
		{
			// Detected a too big distance error! Snap to correct position!
			targetDistance = 0;
			transform.position = target;
		}
		else if (targetDistance > 0)
		{
			targetDir = offset / targetDistance;
		}
		*/

		m_Player.Position.Set (pos);
		m_Player.Velocity.Set (vel);
		m_Player.Rotation.Set (rot);


	}
	
	void SendToServer()
	{
		// This code is only executed on the client which is the owner of this game object
		// Sends Movement RPC to server. The nice part is that this code works when using 
		// an auth server or non-auth server. Both can handle this RPC!
		networkView.UnreliableRPC("Move", uLink.NetworkPlayer.server, m_Player.Position.Get() , m_Player.Velocity.Get(), m_Player.Rotation.Get());
	}
	
	void Update()
	{
		/*
		if (!isInitiaized && networkView.viewID != uLink.NetworkViewID.unassigned)
		{
			Start();
			return;
		}
		
		// Executes the smooth movement of character controller for proxies and the game object in the server.
		curRot = Quaternion.Lerp(targetRot, curRot, rotationDamping);
		transform.rotation = curRot;
		
		if (targetDistance == 0)
		{
			return;
		}
		
		float speed = (targetDistance > arrivalDistance) ? maxSpeed : arrivalSpeed * targetDistance;
		
		velocity = speed * targetDir; 
		
		if (moveCharacter)
		{
			character.SimpleMove(velocity);
			
			targetDistance -= speed * Time.deltaTime;
		}
		*/


	}

	//------------------------------------------------------------------------------------------------------
	// OnStart_ function is not working.
	// RPC is not working.

	void OnStart_Crouch(){
		Debug.Log("CROUCH STARTED!");

		if (networkView.isOwner) 
		{
			Debug.Log("CROUCH STARTED ON OWNER!");
			networkView.RPC("TryActivity", uLink.NetworkPlayer.server, "Crouch");
		}
	}

	[RPC]
	void TryActivity(string activityType, uLink.NetworkMessageInfo info) {

		Debug.Log ("TRYACTIVITY RPC IS WORKING!");

		if (activityType == "Crouch") {
			if (m_Player.Crouch.TryStart() == false) {
				//STOP OWNER'S ACTIVITY
			}
			else {
				Debug.Log("CROUCH STARTED!");
				//START PROXIES ACTIVITY
			}
		}
	}

	//------------------------------------------------------------------------------------------------------


	[RPC]
	void Move(Vector3 pos, Vector3 vel, Vector2 rot, uLink.NetworkMessageInfo info)
	{
		// This code is only executed in the auth server
		if (info.sender != networkView.owner || info.timestamp <= serverLastTimestamp)
		{
			// Make sure we throw away late and duplicate RPC messages. And trow away messages 
			// from the wrong client (they could trying to cheat this way) 
			return;
		}
		
		serverLastTimestamp = info.timestamp;
		
		// Add some more code right here if the server is authoritave and you want to do more security checks
		// The server state is updated with incoming data from the client beeing the "owner" of this game object
		UpdateState(pos, vel, rot, info.timestamp);
	}
}
