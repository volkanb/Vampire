//#define ASTARDEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

/** AI for following paths.
 * This AI is the default movement script which comes with the A* Pathfinding Project.
 * It is in no way required by the rest of the system, so feel free to write your own. But I hope this script will make it easier
 * to set up movement for the characters in your game. This script is not written for high performance, so I do not recommend using it for large groups of units.
 * \n
 * \n
 * This script will try to follow a target transform, in regular intervals, the path to that target will be recalculated.
 * It will on FixedUpdate try to move towards the next point in the path.
 * However it will only move in the forward direction, but it will rotate around it's Y-axis
 * to make it reach the target.
 * 
 * \section variables Quick overview of the variables
 * In the inspector in Unity, you will see a bunch of variables. You can view detailed information further down, but here's a quick overview.\n
 * The #repathRate determines how often it will search for new paths, if you have fast moving targets, you might want to set it to a lower value.\n
 * The #target variable is where the AI will try to move, it can be a point on the ground where the player has clicked in an RTS for example.
 * Or it can be the player object in a zombie game.\n
 * The speed is self-explanatory, so is turningSpeed, however #slowdownDistance might require some explanation.
 * It is the approximate distance from the target where the AI will start to slow down. Note that this doesn't only affect the end point of the path
 * but also any intermediate points, so be sure to set #forwardLook and #pickNextWaypointDist to a higher value than this.\n
 * #pickNextWaypointDist is simply determines within what range it will switch to target the next waypoint in the path.\n
 * #forwardLook will try to calculate an interpolated target point on the current segment in the path so that it has a distance of #forwardLook from the AI\n
 * Below is an image illustrating several variables as well as some internal ones, but which are relevant for understanding how it works.
 * Note that the #forwardLook range will not match up exactly with the target point practically, even though that's the goal.
 * \shadowimage{aipath_variables.png}
 * This script has many movement fallbacks.
 * If it finds a NavmeshController, it will use that, otherwise it will look for a character controller, then for a rigidbody and if it hasn't been able to find any
 * it will use Transform.Translate which is guaranteed to always work.
 */
[RequireComponent(typeof(Seeker))]
public class AIPathSlayer : MonoBehaviour {
	
	/** Determines how often it will search for new paths. 
	 * If you have fast moving targets or AIs, you might want to set it to a lower value.
	 * The value is in seconds between path requests.
	 */
	public float repathRate = 0.5F;
	
	/** Target to move towards.
	 * The AI will try to follow/move towards this target.
	 * It can be a point on the ground where the player has clicked in an RTS for example, or it can be the player object in a zombie game.
	 */
	public Transform target;
	
	/** Enables or disables searching for paths.
	 * Setting this to false does not stop any active path requests from being calculated or stop it from continuing to follow the current path.
	 * \see #canMove
	 */
	public bool canSearch = true;
	
	/** Enables or disables movement.
	  * \see #canSearch */
	public bool canMove = true;
	
	/** Maximum velocity.
	 * This is the maximum speed in world units per second.
	 */
	public float speed = 3;
	
	/** Rotation speed.
	 * Rotation is calculated using Quaternion.SLerp. This variable represents the damping, the higher, the faster it will be able to rotate.
	 */
	public float turningSpeed = 5;
	
	/** Distance from the target point where the AI will start to slow down.
	 * Note that this doesn't only affect the end point of the path
 	 * but also any intermediate points, so be sure to set #forwardLook and #pickNextWaypointDist to a higher value than this
 	 */
	public float slowdownDistance = 0.6F;
	
	/** Determines within what range it will switch to target the next waypoint in the path */
	public float pickNextWaypointDist = 2;
	
	/** Target point is Interpolated on the current segment in the path so that it has a distance of #forwardLook from the AI.
	  * See the detailed description of AIPath for an illustrative image */
	public float forwardLook = 1;
	
	/** Distance to the end point to consider the end of path to be reached.
	 * When this has been reached, the AI will not move anymore until the target changes and OnTargetReached will be called.
	 */
	public float endReachedDistance = 0.2F;
	
	/** Do a closest point on path check when receiving path callback.
	 * Usually the AI has moved a bit between requesting the path, and getting it back, and there is usually a small gap between the AI
	 * and the closest node.
	 * If this option is enabled, it will simulate, when the path callback is received, movement between the closest node and the current
	 * AI position. This helps to reduce the moments when the AI just get a new path back, and thinks it ought to move backwards to the start of the new path
	 * even though it really should just proceed forward.
	 */
	public bool closestOnPathCheck = true;
	
	protected float minMoveScale = 0.05F;
	
	/** Cached Seeker component */
	protected Seeker seeker;
	
	/** Cached Transform component */
	protected Transform tr;
	
	/** Time when the last path request was sent */
	private float lastRepath = -9999;
	
	/** Current path which is followed */
	protected Path path;
	
	/** Cached CharacterController component */
	protected CharacterController controller;
	
	/** Cached NavmeshController component */
	protected NavmeshController navController;
	
	
	/** Cached Rigidbody component */
	protected Rigidbody rigid;
	
	/** Current index in the path which is current target */
	protected int currentWaypointIndex = 0;
	
	/** Holds if the end-of-path is reached
	 * \see TargetReached */
	protected bool targetReached = false;
	
	/** Only when the previous path has been returned should be search for a new path */
	protected bool canSearchAgain = true;
	
	/** Returns if the end-of-path has been reached
	 * \see targetReached */
	public bool TargetReached {
		get {
			return targetReached;
		}
	}
	
	/** Holds if the Start function has been run.
	 * Used to test if coroutines should be started in OnEnable to prevent calculating paths
	 * in the awake stage (or rather before start on frame 0).
	 */
	private bool startHasRun = false;
	
	/** Initializes reference variables.
	 * If you override this function you should in most cases call base.Awake () at the start of it.
	  * */
	
	public vp_FPPlayerEventHandler Player = null;
	public GameObject currentlyEscorting;
	bool isEscorting=false;
	float radius=3f;
	float stakeRadius=1.1f;
	public bool isAttacking=false;
	GameObject random;
	GameObject closest;
	public GameObject cam;

	//Align
	float targetDegree;
	//float aSpeed;
	//int maxAngularAcceleration = 1;
	int maxRotationSpeed = 270;
	float targetRadius = 0.1f;
	//float slowRadius = 2.0f;
	//float timeToTarget = 0.1f;
	void alignMe()
	{
		
		Vector3 dir =  target.transform.position - transform.position;
		
		
		float degree= Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
		targetDegree=degree;
		
		float angle = transform.eulerAngles.y;
		float rotation = Mathf.DeltaAngle(angle, targetDegree);
		float rotationSize = Mathf.Abs(rotation);
		float targetRotation = 0.0f;
		if (rotationSize > targetRadius)
		{
			targetRotation = maxRotationSpeed;
			
			/*if (rotationSize < slowRadius)
				targetRotation = maxRotationSpeed * rotationSize / slowRadius;*/
			targetRotation *= rotation / rotationSize;
			/*float angular = targetRotation - aSpeed;
			angular /= timeToTarget;
			float angularAcceleration = Mathf.Abs(angular);
			if (angularAcceleration > maxAngularAcceleration)
			{
				angular /= angularAcceleration;
				angular *= maxAngularAcceleration;
			}
			
			aSpeed += angular;*/
			transform.Rotate(Vector3.up, targetRotation * Time.deltaTime);
		}
	}

	GameObject getRandomHuman()
	{
		ArrayList myList = new ArrayList();
		GameObject[] targets;
		targets=GameObject.FindGameObjectsWithTag("Human");
		random=null;
		//float distance=Mathf.Infinity;
		foreach (GameObject go in targets)
		{
			Vector3 diff=go.transform.position-transform.position;
			if((go.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0) && (go.GetComponent<AIPathHuman>().isPossessed==false) 
			&&(go.GetComponent<AIPathHuman>().isFollowing==false) && (go.GetComponent<AIPathHuman>().isRescued==false))
			{
				myList.Add(go);
				/*random=go;
				break;
				distance=diff.magnitude;*/
			}
		}	
		if(myList.Count>0)
		{
			int randomNumber=Random.Range(0,myList.Count);
			for(int i=0; i<myList.Count; i++)
			{
				random=(GameObject)myList[randomNumber];
			}
		}

		if(random==null) {return gameObject;}
		return random;
	}

	GameObject getRandomZone()
	{
		ArrayList myList = new ArrayList();
		GameObject[] targets;
		targets=GameObject.FindGameObjectsWithTag("zone");
		random=null;

		foreach (GameObject go in targets)
		{
			myList.Add(go);
		}	
		if(myList.Count>0)
		{
			int randomNumber=Random.Range(0,myList.Count);
			for(int i=0; i<myList.Count; i++)
			{
				random=(GameObject)myList[randomNumber];
			}
		}
		if(random==null) {return gameObject;}
		return random;
	}
	
	/*GameObject getClosestVampire()
	{
		GameObject[] targets;
		targets=GameObject.FindGameObjectsWithTag("Vampire");
		closest=null;
		float distance=Mathf.Infinity;
		foreach (GameObject go in targets)
		{
			Vector3 diff=go.transform.position-transform.position;
			if((diff.magnitude<30))
			{
				closest=go;
				distance=diff.magnitude;	
			}
		}
		return closest;
	}*/

	enum State {SEARCH, DEFEND, ESCORT, GOTOHUMAN, DEAD, STAKE};
	State state;

	protected virtual void Awake () {
		Player = (vp_FPPlayerEventHandler)transform.root.GetComponentInChildren(typeof(vp_FPPlayerEventHandler));
		currentlyEscorting=null;
		target=getRandomZone().transform;
		seeker = GetComponent<Seeker>();
		
		//This is a simple optimization, cache the transform component lookup
		tr = transform;
		
		//Make sure we receive callbacks when paths complete
		seeker.pathCallback += OnPathComplete;
		
		//Cache some other components (not all are necessarily there)
		controller = GetComponent<CharacterController>();
		navController = GetComponent<NavmeshController>();
		rigid = rigidbody;
	}
	
	/** Starts searching for paths.
	 * If you override this function you should in most cases call base.Start () at the start of it.
	 * \see OnEnable
	 * \see RepeatTrySearchPath
	 */
	protected virtual void Start () {
		startHasRun = true;
		OnEnable ();
		GetComponent<log>().EnterLog("Search");
	}
	
	/** Run at start and when reenabled.
	 * Starts RepeatTrySearchPath.
	 * 
	 * \see Start
	 */
	protected virtual void OnEnable () {
		if (startHasRun) StartCoroutine (RepeatTrySearchPath ());
	}
	
	/** Tries to search for a path every #repathRate seconds.
	  * \see TrySearchPath
	  */
	public IEnumerator RepeatTrySearchPath () {
		while (true) {
			TrySearchPath ();
			yield return new WaitForSeconds (repathRate);
		}
	}
	
	/** Tries to search for a path.
	 * Will search for a new path if there was a sufficient time since the last repath and both
	 * #canSearchAgain and #canSearch are true.
	 * Otherwise will start WaitForPath function.
	 */
	public void TrySearchPath () {
		if (Time.time - lastRepath >= repathRate && canSearchAgain && canSearch) {
			SearchPath ();
		} else {
			StartCoroutine (WaitForRepath ());
		}
	}
	
	/** Is WaitForRepath running */
	private bool waitingForRepath = false;
	
	/** Wait a short time til Time.time-lastRepath >= repathRate.
	  * Then call TrySearchPath
	  * 
	  * \see TrySearchPath
	  */
	protected IEnumerator WaitForRepath () {
		if (waitingForRepath) yield break; //A coroutine is already running
		
		waitingForRepath = true;
		//Wait until it is predicted that the AI should search for a path again
		yield return new WaitForSeconds (repathRate - (Time.time-lastRepath));
		
		waitingForRepath = false;
		//Try to search for a path again
		TrySearchPath ();
	}
	
	/** Requests a path to the target */
	public virtual void SearchPath () {

		if (target == null) { Debug.LogError ("Target is null, aborting all search"); canSearch = false; return; }
		
		lastRepath = Time.time;
		//This is where we should search to
		Vector3 targetPosition = target.position;
		
		canSearchAgain = false;
		
		//Alternative way of requesting the path
		//Path p = PathPool<Path>.GetPath().Setup(GetFeetPosition(),targetPoint,null);
		//seeker.StartPath (p);
		
		//We should search from the current position
		seeker.StartPath (GetFeetPosition(), targetPosition);
	}
	
	public virtual void OnTargetReached () {
		//End of path has been reached
		//If you want custom logic for when the AI has reached it's destination
		//add it here
		//You can also create a new script which inherits from this one
		//and override the function in that script
	}
	
	public void OnDestroy () {
		if (path != null) path.Release (this);
	}
	
	/** Called when a requested path has finished calculation.
	  * A path is first requested by #SearchPath, it is then calculated, probably in the same or the next frame.
	  * Finally it is returned to the seeker which forwards it to this function.\n
	  */
	public virtual void OnPathComplete (Path _p) {
		ABPath p = _p as ABPath;
		if (p == null) throw new System.Exception ("This function only handles ABPaths, do not use special path types");
		
		//Release the previous path
		if (path != null) path.Release (this);
		
		//Claim the new path
		p.Claim (this);
		
		//Replace the old path
		path = p;
		
		//Reset some variables
		currentWaypointIndex = 0;
		targetReached = false;
		canSearchAgain = true;
		
		//The next row can be used to find out if the path could be found or not
		//If it couldn't (error == true), then a message has probably been logged to the console
		//however it can also be got using p.errorLog
		//if (p.error)
		
		if (closestOnPathCheck) {
			Vector3 p1 = p.startPoint;
			Vector3 p2 = GetFeetPosition ();
			float magn = Vector3.Distance (p1,p2);
			Vector3 dir = p2-p1;
			dir /= magn;
			int steps = (int)(magn/pickNextWaypointDist);
			for (int i=0;i<steps;i++) {
				CalculateVelocity (p1);
				p1 += dir;
			}
		}
	}
	
	public virtual Vector3 GetFeetPosition () {
		if (controller != null) {
			return tr.position - Vector3.up*controller.height*0.5F;
		}

		return tr.position;
	}
	
	public virtual void Update () 
	{
		switch(state)
		{
			case State.SEARCH:
			Vector3 zonePos=target.position-transform.position;	
			if(GetComponent<vp_DamageHandler2>().m_CurrentHealth<1)
			{
				state=State.DEAD;
				GetComponent<log>().EnterLog("Dead");
			}
			else if((zonePos.magnitude>6f) && (target.tag=="zone"))
			{
				Player.SetWeapon.TryStart(2);
				if(Player.CurrentWeaponAmmoCount.Get()<2)
				{
					Player.Reload.TryStart();
				}
				if (!canMove) { return; }
				
				else if(zonePos.magnitude>=radius) 
				{
					Vector3 dir = CalculateVelocity (GetFeetPosition());
					
					//Rotate towards targetDirection (filled in by CalculateVelocity)
					if (targetDirection != Vector3.zero) 
					{
						RotateTowards (targetDirection);
					}
					
					if (navController != null) 
					{
						navController.SimpleMove (GetFeetPosition(),dir);
					}
					else if (controller != null) 
					{
						controller.SimpleMove (dir);
					} 
					else if (rigid != null) 
					{
						rigid.AddForce (dir);
					} 
					else 
					{
						transform.Translate (dir*Time.deltaTime, Space.World);
					}
				}

			}
			else
			{
				target=getRandomZone().transform;
				if(Player.CurrentWeaponAmmoCount.Get()<2)
				{
					Player.Reload.TryStart();
				}
			}
			break;

			case State.GOTOHUMAN:
			if(GetComponent<vp_DamageHandler2>().m_CurrentHealth<1)
			{
				state=State.DEAD;
				GetComponent<log>().EnterLog("Dead");
			}
			else if((target.GetComponent<AIPathHuman>().isFollowing==true) || (target.GetComponent<vp_DamageHandler2>().m_CurrentHealth<1) || (target.GetComponent<AIPathHuman>().isPossessed==true) )
			{
				state=State.SEARCH;
				GetComponent<log>().EnterLog("Search");
			}
			else
			{
				Player.SetWeapon.TryStart(2);
				if(Player.CurrentWeaponAmmoCount.Get()<2)
				{
					Player.Reload.TryStart();
				}
				Vector3 pos=target.position-transform.position;	
				
				if (!canMove) { return; }
				
				else if(pos.magnitude>=radius) 
				{
					Vector3 dir = CalculateVelocity (GetFeetPosition());
					
					//Rotate towards targetDirection (filled in by CalculateVelocity)
					if (targetDirection != Vector3.zero) 
					{
						RotateTowards (targetDirection);
					}
					
					if (navController != null) 
					{
						navController.SimpleMove (GetFeetPosition(),dir);
					}
					else if (controller != null) 
					{
						controller.SimpleMove (dir);
					} 
					else if (rigid != null) 
					{
						rigid.AddForce (dir);
					} 
					else 
					{
						transform.Translate (dir*Time.deltaTime, Space.World);
					}
				}
				else if(pos.magnitude<radius)
				{
					vp_FPPlayerEventHandler Human =(vp_FPPlayerEventHandler)target.transform.root.GetComponentInChildren(typeof(vp_FPPlayerEventHandler));
					Human.SetWeapon.TryStart(1);
					isEscorting=true;
					target.GetComponent<AIPathHuman>().isArmed=true;
					target.tag="ArmedHuman";
					target.GetComponent<vp_DamageHandler2>().capsule.renderer.material.color=Color.cyan;
					target.GetComponent<AIPathHuman>().isFollowing=true;
					target.GetComponent<AIPathHuman>().target=gameObject.transform;
					currentlyEscorting=target.gameObject;	
					target=GameObject.Find("SlayerBase").transform;
					state=State.ESCORT;
					GetComponent<log>().EnterLog("Escort");
					//target.GetComponent<AIPathHuman>().follow();

				}
			}
			break;

			case State.ESCORT:

			if(GetComponent<vp_DamageHandler2>().m_CurrentHealth<1)
			{
				isEscorting=false;
				state=State.DEAD;
				GetComponent<log>().EnterLog("Dead");
			}
			else if((currentlyEscorting.GetComponent<vp_DamageHandler2>().m_CurrentHealth<1) || (currentlyEscorting.GetComponent<AIPathHuman>().isRescued==true))
			{
				isEscorting=false;
				state=State.SEARCH;
				GetComponent<log>().EnterLog("Search");
			}
			else
			{
				Player.SetWeapon.TryStart(2);
				if(Player.CurrentWeaponAmmoCount.Get()<2)
				{
					Player.Reload.TryStart();
				}
				Vector3 posBase=target.position-transform.position;	
				
				if (!canMove) { return; }
				
				else if(posBase.magnitude>=radius) 
				{
					Vector3 dir = CalculateVelocity (GetFeetPosition());
					
					//Rotate towards targetDirection (filled in by CalculateVelocity)
					if (targetDirection != Vector3.zero) 
					{
						RotateTowards (targetDirection);
					}
					
					if (navController != null) 
					{
						navController.SimpleMove (GetFeetPosition(),dir);
					}
					else if (controller != null) 
					{
						controller.SimpleMove (dir);
					} 
					else if (rigid != null) 
					{
						rigid.AddForce (dir);
					} 
					else 
					{
						transform.Translate (dir*Time.deltaTime, Space.World);
					}
				}
			}

			break;

			case  State.DEFEND:
			Vector3 runPos=target.position-transform.position;
			if(GetComponent<vp_DamageHandler2>().m_CurrentHealth<1)
			{
				state=State.DEAD;
				GetComponent<log>().EnterLog("Dead");
			}
			else if((target.tag=="Vampire") && (target.GetComponent<AIPathVampire>()!=null) && (target.GetComponent<AIPathVampire>().isDown==true) && (target.GetComponent<AIPathVampire>().isDead==false))
			{
				Player.Attack.TryStop(); 
				state=State.STAKE;
				GetComponent<log>().EnterLog("Stake");
			}
			else if((target.tag=="VampirePlayer") && (target.GetComponent<vp_PlayerDamageHandler2>()!=null) && (target.GetComponent<vp_PlayerDamageHandler2>().isDown==true) && (target.GetComponent<vp_PlayerDamageHandler2>().isDead==false))
			{
				Player.Attack.TryStop(); 
				state=State.STAKE;
				GetComponent<log>().EnterLog("Stake");
			}
			else if((target.tag=="VampirePlayer") && (target.GetComponent<vp_PlayerDamageHandler2>().isDown==false) && (target.GetComponent<vp_PlayerDamageHandler2>().isDead==false) && (runPos.magnitude>=15f))
			{
				if((currentlyEscorting!=null) && (currentlyEscorting.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0) && (isEscorting==true))
				{
					target=GameObject.Find("SlayerBase").transform;
					isAttacking=false;
					Player.Attack.TryStop(); 
					state=State.ESCORT;
					GetComponent<log>().EnterLog("Escort");
				}
				else {Player.Attack.TryStop(); isAttacking=false; state=State.SEARCH; GetComponent<log>().EnterLog("Search");}
			}
			else if((target.tag!="Vampire") && (target.tag!="VampirePlayer") && ((target.GetComponent<vp_DamageHandler2>().m_CurrentHealth<1) || (runPos.magnitude>=20f)))
			{
				if((currentlyEscorting!=null) && (currentlyEscorting.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0) && (isEscorting==true))
				{
					target=GameObject.Find("SlayerBase").transform;
					isAttacking=false;
					Player.Attack.TryStop(); 
					state=State.ESCORT;
					GetComponent<log>().EnterLog("Escort");
				}
				else {Player.Attack.TryStop(); isAttacking=false; state=State.SEARCH; GetComponent<log>().EnterLog("Search");}
			}
			else
			{
				if(runPos.magnitude<9f)
				{
					Vector3 pos1=transform.position-target.position;
					pos1.Normalize();
					GetComponent<CharacterController>().SimpleMove(pos1*2.5f);
					//transform.LookAt(target.transform.position);
					alignMe();
					isAttacking=true;
					
					RaycastHit objectHit;
					Vector3 fwd = cam.transform.TransformDirection(Vector3.forward);
					Debug.DrawRay(cam.transform.position, fwd * 50, Color.red);

					if((Player.CurrentWeaponAmmoCount.Get()>0) &&(Physics.Raycast(cam.transform.position, fwd, out objectHit, 100)))
					{
						//Debug.Log(objectHit.collider.name);
						if((objectHit.transform.tag=="VampirePlayer") || (objectHit.transform.tag=="Vampire") || (objectHit.transform.tag=="PossessedHuman") )
						{
							Player.Attack.TryStart();	
						}
						/*if(objectHit.transform.tag=="VampirePlayer")
						{
							Player.Attack.TryStart();	
						}
						else if(objectHit.transform.parent)
						{
							if(objectHit.transform.parent.parent)
							{
								if( (objectHit.transform.parent.parent.tag=="Vampire") || (objectHit.transform.parent.parent.tag=="PossessedHuman"))
								{
									Player.Attack.TryStart();	
								}

							}

							
						}*/

					}
					if(Player.CurrentWeaponAmmoCount.Get()<1)
					{
						Player.SetWeapon.TryStart(1);
						if(Player.CurrentWeaponAmmoCount.Get()<1)
						{
							Player.Reload.TryStart();
						}
					}
				}

			}

			break;

			case State.STAKE:
			isAttacking=false;
			Vector3 runPos1=target.position-transform.position;
			if(GetComponent<vp_DamageHandler2>().m_CurrentHealth<1)
			{
				state=State.DEAD;
				GetComponent<log>().EnterLog("Dead");
			}
			else if((target.tag=="Vampire") && (target.GetComponent<AIPathVampire>()) && (target.GetComponent<AIPathVampire>().isDown==false) && (target.GetComponent<AIPathVampire>().isDead==false))
			{
				state=State.DEFEND;
				GetComponent<log>().EnterLog("Defend");
			}
			else if((target.tag=="VampirePlayer") && (target.GetComponent<vp_PlayerDamageHandler2>()) && (target.GetComponent<vp_PlayerDamageHandler2>().isDown==false) && (target.GetComponent<vp_PlayerDamageHandler2>().isDead==false))
			{
				state=State.DEFEND;
				GetComponent<log>().EnterLog("Defend");
			}
			else if( (target.GetComponent<vp_DamageHandler2>()) && ((target.GetComponent<vp_DamageHandler2>().m_CurrentHealth<1) || (runPos1.magnitude>=20f)) && (target.GetComponent<AIPathVampire>().isDead==true) )
			{
				if((currentlyEscorting!=null) && (currentlyEscorting.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0) && (isEscorting==true))
				{
					target=GameObject.Find("SlayerBase").transform;
					Player.Attack.TryStop(); 
					state=State.ESCORT;
					GetComponent<log>().EnterLog("Escort");
				}
				else {Player.Attack.TryStop(); isAttacking=false; state=State.SEARCH; GetComponent<log>().EnterLog("Search");}
			}
			else if( (target.GetComponent<vp_DamageHandler2>()) && ((target.GetComponent<vp_DamageHandler2>().m_CurrentHealth<1) || (runPos1.magnitude>=20f)) && (target.tag!="Vampire") )
			{
				if((currentlyEscorting!=null) && (currentlyEscorting.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0) && (isEscorting==true))
				{
					target=GameObject.Find("SlayerBase").transform;
					Player.Attack.TryStop(); 
					state=State.ESCORT;
					GetComponent<log>().EnterLog("Escort");
				}
				else {Player.Attack.TryStop(); isAttacking=false; state=State.SEARCH; GetComponent<log>().EnterLog("Search");}
			}
			else if( (target.GetComponent<vp_PlayerDamageHandler2>()) && ((target.GetComponent<vp_PlayerDamageHandler2>().m_CurrentHealth<1) || (runPos1.magnitude>=20f)) && (target.GetComponent<vp_PlayerDamageHandler2>().isDead==true) )
			{
				if((currentlyEscorting!=null) && (currentlyEscorting.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0) && (isEscorting==true))
				{
					target=GameObject.Find("SlayerBase").transform;
					Player.Attack.TryStop(); 
					state=State.ESCORT;
					GetComponent<log>().EnterLog("Escort");
				}
				else {Player.Attack.TryStop(); isAttacking=false; state=State.SEARCH; GetComponent<log>().EnterLog("Search");}
			}
			else if( (target.GetComponent<vp_PlayerDamageHandler2>()) && ((target.GetComponent<vp_PlayerDamageHandler2>().m_CurrentHealth<1) || (runPos1.magnitude>=20f)) && (target.tag!="VampirePlayer") )
			{
				if((currentlyEscorting!=null) && (currentlyEscorting.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0) && (isEscorting==true))
				{
					target=GameObject.Find("SlayerBase").transform;
					Player.Attack.TryStop(); 
					state=State.ESCORT;
					GetComponent<log>().EnterLog("Escort");
				}
				else {Player.Attack.TryStop(); isAttacking=false; state=State.SEARCH; GetComponent<log>().EnterLog("Search");}
			}
			else
			{
				if (!canMove) { return; }
				Vector3 pos=target.position-transform.position;	
				if(pos.magnitude>=stakeRadius)
				{
					Vector3 dir = CalculateVelocity (GetFeetPosition());
					
					//Rotate towards targetDirection (filled in by CalculateVelocity)
					if (targetDirection != Vector3.zero) 
					{
						RotateTowards (targetDirection);
					}
					
					if (navController != null)
					{
						navController.SimpleMove (GetFeetPosition(),dir);
					} 
					else if (controller != null) 
					{
						controller.SimpleMove (dir);
					}
					else if (rigid != null) 
					{
						rigid.AddForce (dir);
					}
					else
					{
						transform.Translate (dir*Time.deltaTime, Space.World);
					}
				}
				else if(pos.magnitude<stakeRadius)
				{
					Player.SetWeapon.TryStart(3);
					//transform.LookAt(target.transform.position);
					alignMe();
					Player.Attack.TryStart();
				}
			}
			break;

			case State.DEAD:
			isAttacking=false;
			Player.Attack.TryStop();
			Destroy(gameObject,10);
			break;
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		if( (other.tag=="Vampire") && (other.GetComponent<AIPathVampire>().isDead==false)) {target=other.gameObject.transform; Player.Attack.TryStop(); state=State.DEFEND; GetComponent<log>().EnterLog("Defend");}
		else if( (other.tag=="PossessedHuman") && (other.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0)) {target=other.gameObject.transform; Player.Attack.TryStop(); state=State.DEFEND; GetComponent<log>().EnterLog("Defend");}
		else if((other.tag=="VampirePlayer") && (other.GetComponent<vp_PlayerDamageHandler2>().isDead==false)) {target=other.gameObject.transform; Player.Attack.TryStop(); state=State.DEFEND; GetComponent<log>().EnterLog("Defend");}
		else if((other.tag=="Human") && (state==State.SEARCH) && (other.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0)) {target=other.gameObject.transform; state=State.GOTOHUMAN; GetComponent<log>().EnterLog("GotoHuman");} 
		else if((other.tag=="ArmedHuman") && (other.GetComponent<AIPathHuman>().isFollowing==false)&& (state==State.SEARCH) && (other.GetComponent<vp_DamageHandler2>().m_CurrentHealth>0)) {target=other.gameObject.transform; state=State.GOTOHUMAN; GetComponent<log>().EnterLog("GotoHuman");} 
	}
	
	/** Point to where the AI is heading.
	  * Filled in by #CalculateVelocity */
	protected Vector3 targetPoint;
	/** Relative direction to where the AI is heading.
	 * Filled in by #CalculateVelocity */
	protected Vector3 targetDirection;
	
	protected float XZSqrMagnitude (Vector3 a, Vector3 b) {
		float dx = b.x-a.x;
		float dz = b.z-a.z;
		return dx*dx + dz*dz;
	}
	
	/** Calculates desired velocity.
	 * Finds the target path segment and returns the forward direction, scaled with speed.
	 * A whole bunch of restrictions on the velocity is applied to make sure it doesn't overshoot, does not look too far ahead,
	 * and slows down when close to the target.
	 * /see speed
	 * /see endReachedDistance
	 * /see slowdownDistance
	 * /see CalculateTargetPoint
	 * /see targetPoint
	 * /see targetDirection
	 * /see currentWaypointIndex
	 */
	protected Vector3 CalculateVelocity (Vector3 currentPosition) {
		if (path == null || path.vectorPath == null || path.vectorPath.Count == 0) return Vector3.zero; 
		
		List<Vector3> vPath = path.vectorPath;
		//Vector3 currentPosition = GetFeetPosition();
		
		if (vPath.Count == 1) {
			vPath.Insert (0,currentPosition);
		}
		
		if (currentWaypointIndex >= vPath.Count) { currentWaypointIndex = vPath.Count-1; }
		
		if (currentWaypointIndex <= 1) currentWaypointIndex = 1;
		
		while (true) {
			if (currentWaypointIndex < vPath.Count-1) {
				//There is a "next path segment"
				float dist = XZSqrMagnitude (vPath[currentWaypointIndex], currentPosition);
					//Mathfx.DistancePointSegmentStrict (vPath[currentWaypointIndex+1],vPath[currentWaypointIndex+2],currentPosition);
				if (dist < pickNextWaypointDist*pickNextWaypointDist) {
					currentWaypointIndex++;
				} else {
					break;
				}
			} else {
				break;
			}
		}
		
		Vector3 dir = vPath[currentWaypointIndex] - vPath[currentWaypointIndex-1];
		Vector3 targetPosition = CalculateTargetPoint (currentPosition,vPath[currentWaypointIndex-1] , vPath[currentWaypointIndex]);
			//vPath[currentWaypointIndex] + Vector3.ClampMagnitude (dir,forwardLook);
		
		
		
		dir = targetPosition-currentPosition;
		dir.y = 0;
		float targetDist = dir.magnitude;
		
		float slowdown = Mathf.Clamp01 (targetDist / slowdownDistance);
		
		this.targetDirection = dir;
		this.targetPoint = targetPosition;
		
		if (currentWaypointIndex == vPath.Count-1 && targetDist <= endReachedDistance) {
			if (!targetReached) { targetReached = true; OnTargetReached (); }
			
			//Send a move request, this ensures gravity is applied
			return Vector3.zero;
		}
		
		Vector3 forward = tr.forward;
		float dot = Vector3.Dot (dir.normalized,forward);
		float sp = speed * Mathf.Max (dot,minMoveScale) * slowdown;
		
		
		if (Time.deltaTime	> 0) {
			sp = Mathf.Clamp (sp,0,targetDist/(Time.deltaTime*2));
		}
		return forward*sp;
	}
	
	/** Rotates in the specified direction.
	 * Rotates around the Y-axis.
	 * \see turningSpeed
	 */
	protected virtual void RotateTowards (Vector3 dir) {
		Quaternion rot = tr.rotation;
		Quaternion toTarget = Quaternion.LookRotation (dir);
		
		rot = Quaternion.Slerp (rot,toTarget,turningSpeed*Time.fixedDeltaTime);
		Vector3 euler = rot.eulerAngles;
		euler.z = 0;
		euler.x = 0;
		rot = Quaternion.Euler (euler);
		
		tr.rotation = rot;
	}
	
	/** Calculates target point from the current line segment.
	 * \param p Current position
	 * \param a Line segment start
	 * \param b Line segment end
	 * The returned point will lie somewhere on the line segment.
	 * \see #forwardLook
	 * \todo This function uses .magnitude quite a lot, can it be optimized?
	 */
	protected Vector3 CalculateTargetPoint (Vector3 p, Vector3 a, Vector3 b) {
		a.y = p.y;
		b.y = p.y;
		
		float magn = (a-b).magnitude;
		if (magn == 0) return a;
		
		float closest = Mathfx.Clamp01 (Mathfx.NearestPointFactor (a, b, p));
		Vector3 point = (b-a)*closest + a;
		float distance = (point-p).magnitude;
		
		float lookAhead = Mathf.Clamp (forwardLook - distance, 0.0F, forwardLook);
		
		float offset = lookAhead / magn;
		offset = Mathf.Clamp (offset+closest,0.0F,1.0F);
		return (b-a)*offset + a;
	}
}
