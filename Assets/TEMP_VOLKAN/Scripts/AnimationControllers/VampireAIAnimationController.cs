using UnityEngine;
using System.Collections;

public class VampireAIAnimationController : MonoBehaviour {

	public GameObject model;



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

}
