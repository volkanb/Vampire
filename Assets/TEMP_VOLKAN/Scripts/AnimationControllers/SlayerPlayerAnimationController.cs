using UnityEngine;
using System.Collections;

public class SlayerPlayerAnimationController : MonoBehaviour {


	public GameObject model;
	public GameObject pistol1;
	public GameObject tomp2;
	public GameObject mace3;

	//----------------------------------------------------------------------------------
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
	//----------------------------------------------------------------------------------


	void OnStart_SetWeapon()
	{
		int weaponID = (int)m_Player.SetWeapon.Argument;

		SetWeapon(weaponID);
	}

	public void SetWeapon( int weaponID )
	{
		
		if ( weaponID == 1 )
		{
			pistol1.SetActive(true);
			tomp2.SetActive(false);
			mace3.SetActive(false);		
		}
		else if ( weaponID == 2 )
		{
			tomp2.SetActive(true);
			pistol1.SetActive(false);
			mace3.SetActive(false);
		}
		else if ( weaponID == 3 )
		{
			mace3.SetActive(true);
			pistol1.SetActive(false);
			tomp2.SetActive(false);
		}
	}
}
