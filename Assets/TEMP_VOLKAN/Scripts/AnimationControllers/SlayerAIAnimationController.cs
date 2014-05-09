using UnityEngine;
using System.Collections;

public class SlayerAIAnimationController : MonoBehaviour {

	public GameObject model;
	public GameObject pistol1;
	public GameObject tomp2;
	public GameObject mace3;
	
	
	
	public void StartAnimWithName( string animName )
	{
		model.animation.Play(animName);
		
	}
	
	public void StartAnimWithNameLayer( string animName, int layer )
	{
		model.animation [animName].layer = layer;
		model.animation.Play(animName);
	}
	
	public void StartAnimWithNameWeight( string animName, float weight )
	{
		if (weight == 100f) 
		{
			model.animation[animName].speed=1;
			model.animation.Play(animName);
		}
		else if ( weight == -100f )
		{
			model.animation[animName].speed=-1;
			model.animation[animName].time = model.animation[animName].length;
			model.animation.Play(animName);
		}
		else
		{
			model.animation[animName].weight = weight;
			model.animation.Play(animName);
		}
	}
	
	public void StartAnimWithNameLayerWeight( string animName, int layer, float weight )
	{
		model.animation[animName].layer = layer;
		model.animation[animName].weight = weight;
		model.animation.Play(animName);		
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
