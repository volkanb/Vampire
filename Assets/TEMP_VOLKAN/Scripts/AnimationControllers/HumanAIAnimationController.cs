using UnityEngine;
using System.Collections;

public class HumanAIAnimationController : MonoBehaviour {

	public GameObject model;
	public GameObject possessedModel;
	public GameObject armedModel;
	
	public void StartAnimWithName( string animName )
	{
		GameObject mod = null;
		
		if (model.activeSelf)
			mod = model;
		else if ( possessedModel.activeSelf )
			mod = possessedModel;
		else if ( armedModel.activeSelf )
			mod = armedModel;


		mod.animation.Play(animName);
		
	}
	
	public void StartAnimWithNameLayer( string animName, int layer )
	{
		GameObject mod = null;

		if (model.activeSelf)
			mod = model;
		else if ( possessedModel.activeSelf )
			mod = possessedModel;
		else if ( armedModel.activeSelf )
			mod = armedModel;


		if ( layer == -1 )
		{
			mod.animation.PlayQueued(animName);
		}
		else 
		{
			mod.animation [animName].layer = layer;
			mod.animation.Play(animName);
		}

	}
	
	public void StartAnimWithNameWeight( string animName, float weight )
	{
		GameObject mod = null;
		
		if (model.activeSelf)
			mod = model;
		else if ( possessedModel.activeSelf )
			mod = possessedModel;
		else if ( armedModel.activeSelf )
			mod = armedModel;

		if (weight == 100f) 
		{
			mod.animation[animName].speed=1;
			mod.animation.Play(animName);
		}
		else if ( weight == -100f )
		{
			mod.animation[animName].speed=-1;
			mod.animation[animName].time = model.animation[animName].length;
			mod.animation.Play(animName);
		}
		else
		{
			mod.animation[animName].weight = weight;
			mod.animation.Play(animName);
		}
	}
	
	public void StartAnimWithNameLayerWeight( string animName, int layer, float weight )
	{
		GameObject mod = null;
		
		if (model.activeSelf)
			mod = model;
		else if ( possessedModel.activeSelf )
			mod = possessedModel;
		else if ( armedModel.activeSelf )
			mod = armedModel;

		mod.animation[animName].layer = layer;
		mod.animation[animName].weight = weight;
		mod.animation.Play(animName);		
	}

	public void SetModelWithName( string modelName )
	{
		if ( modelName == "armed" )
		{
			armedModel.SetActive(true);
			model.SetActive(false);
			possessedModel.SetActive(false);
		}
		else if ( modelName == "possessed" )
		{
			possessedModel.SetActive(true);
			armedModel.SetActive(false);
			model.SetActive(false);
		}
	}

}
