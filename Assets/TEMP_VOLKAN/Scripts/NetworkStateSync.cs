using UnityEngine;
using uLink;

public class NetworkStateSync : uLink.MonoBehaviour
{
	// FOLLOWING VARIABLES ARE USED FOR INTERPOLATION (LERP)
	//---------------------------------------------------------------------------------
	private double serverLastTimestamp = 0;
	private bool isInitiaized = false;
	
	private Vector3 lastPos;
	private Vector3 lastVel;
	private Vector2 lastRot;
	
	private Vector3 targetPos;
	private Vector3 targetVel;
	private Vector2 targetRot;
	
	private double lastTimestamp;
	private double targetTimestamp;
	
	private float timeToBeCovered;
	private float timeCovered;
	
	private float fraction;
	//---------------------------------------------------------------------------------
	
	public bool isBot = false;

	// GETTING THE COMPONENT OF NETW EVENTS
	private NetworkEvents netwEvents;
	
	// DAMAGE BUFFER
	public float damageToBeDone;

	public float failRadius = 0.2f;
	
	// EVENT HANDLER
	vp_FPPlayerEventHandler m_Player;
	
	// CHARACTER CONTROLLER
	private CharacterController character;
	
	
	
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
		
		// GETTING THE PLAYER'S CHARACTER CONTROLLER
		character = GetComponent<CharacterController>();
		
	}
	
	
	
	void Start()
	{
		// Initialization of last/target pos, rot, vel, timestamp values
		lastPos = m_Player.Position.Get();
		lastVel = m_Player.Velocity.Get();
		lastRot = m_Player.Rotation.Get();
		
		targetPos = lastPos;
		targetRot = lastRot;
		targetVel = lastVel;
		
		lastTimestamp = 0;
		targetTimestamp = 0;
		
		
		
		if (networkView.viewID == uLink.NetworkViewID.unassigned) return;
		
		isInitiaized = true;
		
		// Following codes closes input and cameras for CREATOR and PROXY
		if( !networkView.isMine )
		{
			m_Player.AllowGameplayInput.Set (false);
			
			foreach (Transform child in gameObject.transform) 
			{
				if(child.name == "FPSCamera")
				{
					foreach (Transform child2 in child.gameObject.transform) 
					{
						if(child2.name == "WeaponCamera")
						{
							child2.transform.gameObject.GetComponent<Camera>().enabled = false;
							break;
						}
					}
					
					child.transform.gameObject.GetComponent<Camera>().enabled = false;
					break;
				}
			}
		}
		

		// INITIALIZING DAMAGE BUFFER
		damageToBeDone = 0f;

		// INITIALIZING NETW EVENTS COMPONENT
		netwEvents = gameObject.GetComponent<NetworkEvents> ();

		if (!networkView.isMine) return;
		
		enabled = false;
		
		if (uLink.Network.isAuthoritativeServer && uLink.Network.isClient)
		{
			InvokeRepeating("SendToServer", 0, 1.0f / uLink.Network.sendRate);
		}
		
	}
	
	
	
	void uLink_OnSerializeNetworkView(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.isWriting)
		{
			if (isBot) 
			{
				// Send information to all proxies (opponent player's computers)
				// This code is executed on the creator (server) when server is auth, or on the owner (client) when the server is non-auth.
				stream.Write(m_Player.Position.Get());
				stream.Write(m_Player.gameObject.transform.rotation.eulerAngles.y);
			}
			else
			{

				// Send information to all proxies (opponent player's computers)
				// This code is executed on the creator (server) when server is auth, or on the owner (client) when the server is non-auth.
				stream.Write(m_Player.Position.Get());
				stream.Write(m_Player.Velocity.Get());
				stream.Write(m_Player.Rotation.Get());
				stream.Write(m_Player.InputMoveVector.Get());
				
			}


			if (damageToBeDone > 0f) 
			{
				netwEvents.DamageOthers(damageToBeDone);
				damageToBeDone = 0f;
			}
		}
		else
		{
			if (isBot) 
			{
				// Update the proxy state when statesync arrives.
				Vector3 pos = stream.Read<Vector3>();
				float rotationY = stream.Read<float>();


				//-----------------------------------
				// BOTLAR iÇiN LERP iŞiNDE BU KıSıM DEĞiŞECEK!!!!!!!!!!!!!!!
				m_Player.Position.Set(pos);
				gameObject.transform.rotation = Quaternion.Euler( new Vector3(0f,rotationY,0f) );
				//-----------------------------------

			}
			else
			{
				// Update the proxy state when statesync arrives.
				Vector3 pos = stream.Read<Vector3>();
				Vector3 vel = stream.Read<Vector3>();
				Vector2 rot = stream.Read<Vector2>();
				Vector2 inpMov = stream.Read<Vector2>();
				
				UpdateState(pos, vel, rot, inpMov, info.timestamp);
			}

		}
	}
	
	
	
	private void UpdateState(Vector3 pos, Vector3 vel, Vector2 rot, Vector2 inpMov, double timestamp)
	{
		
		/*
		 * 
		 * // set edilen değerleri lerp yapma işi
		lastPos = targetPos;
		lastRot = targetRot;
		lastVel = targetVel;
		lastTimestamp = targetTimestamp;


		targetPos = pos;
		targetRot = rot;
		targetVel = vel;
		targetTimestamp = timestamp;

		timeToBeCovered = (float)(targetTimestamp - lastTimestamp);
		timeCovered = 0f;
		*/
		
		// rotation lerp için 
		lastRot = targetRot;
		lastPos = targetPos;
		
		lastTimestamp = targetTimestamp;
		
		targetRot = rot;
		targetPos = pos;
		
		targetTimestamp = timestamp;
		
		timeToBeCovered = (float)(targetTimestamp - lastTimestamp);
		
		timeCovered = 0f;
		
		
		m_Player.InputMoveVector.Set (inpMov);
		
		
	}
	
	
	
	void SendToServer()
	{
		// This code is only executed on the client which is the owner of this game object
		// Sends Movement RPC to server. The nice part is that this code works when using 
		// an auth server or non-auth server. Both can handle this RPC!
		networkView.UnreliableRPC("Move", uLink.NetworkPlayer.server, m_Player.Position.Get() , m_Player.Velocity.Get(), m_Player.Rotation.Get(), m_Player.InputMoveVector.Get() );
	}
	
	
	void Update()
	{
		/*
		if (!networkView.isOwner) 
		{
			// FOLLOWING CODES HANDLE THE SMOOTH MOVEMENT OF SERVER AND PROXIES
			timeCovered += Time.deltaTime;
			fraction = (timeCovered / timeToBeCovered);

			m_Player.Position.Set ( Vector3.Lerp( lastPos, targetPos, fraction ) );
			m_Player.Velocity.Set ( Vector3.Lerp( lastVel, targetVel, fraction ) );
			m_Player.Rotation.Set ( Vector2.Lerp( lastRot, targetRot, fraction ) );

		}
		*/


		// THIS SECTION HANDLES PLAYER MOVE LERPING
		if (!networkView.isOwner && !isBot) 
		{
			// FOLLOWING CODES HANDLE THE SMOOTH ROTATION OF SERVER AND PROXIES
			timeCovered += Time.deltaTime;
			fraction = (timeCovered / timeToBeCovered);
			
			m_Player.Rotation.Set (Vector2.Lerp (lastRot, targetRot, fraction));
			
			
			// IF PLAYER DIVERGES FROM SERVER POSITION, SNAP THE POSITION
			if (Vector3.Distance(m_Player.Position.Get(), targetPos) >= failRadius) 
			{
				m_Player.Position.Set( targetPos );
				Debug.Log("SNAPPING");
			}
			
			
		}
		
		
	}
	
	
	[RPC]
	void Move(Vector3 pos, Vector3 vel, Vector2 rot, Vector2 inpMov, uLink.NetworkMessageInfo info)
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
		
		UpdateState(pos, vel, rot, inpMov, info.timestamp);
	}
	
}
