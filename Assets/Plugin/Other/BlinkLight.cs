using UnityEngine;
using System.Collections;

public class BlinkLight : MonoBehaviour {
	
	public string waveFunction = "sin"; // olası değerler sin, tri(angle), sqr(square), saw(tooth), inv(erted sawtooth), noise(random)
	public float start = 0.0f;
	public float amplitude = 1.0f; // dalganın genlikği
	public float phase = 0.0f; // dalga döngüsünün içinde ne zaman başlanacağı
	public float frequency = 0.5f; // saniyedeki döngü frekansı
	
	private Color originalColor; //lambanın orjinal rengini tutar
	
	// Use this for initialization
	void Start () {
		originalColor = GetComponent<Light>().color;
	}
	
	// Update is called once per frame
	void Update () {
		Light light = GetComponent<Light>();
		light.color = originalColor * (Evalwave());
	}
	
	float Evalwave(){
		float x = (Time.time + phase)*frequency;
		float y;
		
		x = x - Mathf.Floor(x);
		
		if (waveFunction=="sin") {
			y = Mathf.Sin(x*2*Mathf.PI);
		}
		else if (waveFunction=="tri") {
			if (x < 0.5f)
				y = 4.0f * x - 1.0f;
			else
				y = -4.0f * x + 3.0f;  
		}    
		else if (waveFunction=="sqr") {
			if (x < 0.5f)
				y = 1.0f;
			else
				y = -1.0f;  
		}    
		else if (waveFunction=="saw") {
			y = x;
		}    
		else if (waveFunction=="inv") {
			y = 1.0f - x;
		}    
		else if (waveFunction=="noise") {
			y = 1f - (Random.value*2f);
		}
		else {
			y = 1.0f;
		}        
		return (y*amplitude)+start;
	}
}
