/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Switch.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This class will allow the player to interact with an object
//					in the world by input or by a trigger. The script takes a target
//					object and a message can be sent to that target object.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_SupplySpot : vp_Interactable
{

	public bool is_Client = false;


	public AudioSource AudioSource = null;

	
	protected override void Start()
	{
		
		base.Start();
		
		if(AudioSource == null)
			AudioSource = audio == null ? gameObject.AddComponent<AudioSource>() : audio;
		
	}
	
	
	/// <summary>
	/// try to interact with this object
	/// </summary>
	public override bool TryInteract(vp_FPPlayerEventHandler player)
	{
		

		
		if(m_Player == null)
			m_Player = player;
		
	
		
		//Target.SendMessage(TargetMessage, SendMessageOptions.DontRequireReceiver);

		if (!is_Client) 
		{
			int toAdd = 10 - m_Player.GetItemCount.Send("AmmoClip");

			m_Player.AddItem.Try(new object[] { "AmmoClip", toAdd });

			m_Player.Health.Set(100f);

			m_Player.transform.GetComponent<NetworkEvents>().SupplySpotStart();

		}
		else
			Debug.Log ("CLIENT INTERACTED");
		
		return true;
		
	}
	

	
	
	/// <summary>
	/// this is triggered when an object enters the collider and
	/// InteractType is set to trigger
	/// </summary>
	protected override void OnTriggerEnter(Collider col)
	{
		
		// only do something if the trigger is of type Trigger
		if (InteractType != vp_InteractType.Trigger)
			return;
		
		// see if the colliding object was a valid recipient
		foreach(string s in RecipientTags)
		{
			if(col.gameObject.tag == s)
				goto isRecipient;
		}
		return;
	isRecipient:
			
			if (m_Player == null)
				m_Player = GameObject.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
		
		// calls the TryInteract method which is hopefully on the inherited class
		TryInteract(m_Player);
		
	}
	
}
