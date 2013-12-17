/// <summary>
/// gui_EventHandler.cs
/// 
/// TODO: 	NPCNumbers and RoundTimer are not functional !!!
/// 		Crosshair and Damage textures are manually adjusted !!!
/// 		GUI ELEMENTS are manully adjusted !!!
/// 		Weapon symbol does not change on weapon cycle !!!
/// 		
/// 		
/// 
/// description:	a very primitive GUI HUD displaying health, clips and ammo,
/// 				drawing classic FPS crosshair center screen, along
///					with a soft red full screen flash for when taking damage.
/// </summary>


using UnityEngine;


public class gui_EventHandler : MonoBehaviour {

	// Percentage of player health
	public int PlayerHealthPercentage = 200;


	// GUI elements
	public UILabel ClipAmmoText;
	public UILabel HealthText;
	public UILabel RoundTimer;
	public UILabel NPCNumbers;
	
	// crosshair texture
	public Texture m_ImageCrosshair = null;

	public Texture DamageFlashTexture = null;
	public bool ShowDamageOrMessage = true;
	Color m_MessageColor = new Color(2, 2, 0, 2);
	Color m_InvisibleColor = new Color(1, 1, 0, 0);
	Color m_DamageFlashColor = new Color(0.8f, 0, 0, 0);
	Color m_DamageFlashInvisibleColor = new Color(1, 0, 0, 0);
	string m_PickupMessage = "";
	protected static GUIStyle m_MessageStyle = null;
	public static GUIStyle MessageStyle
	{
		get
		{
			if (m_MessageStyle == null)
			{
				m_MessageStyle = new GUIStyle("Label");
				m_MessageStyle.alignment = TextAnchor.MiddleCenter;
			}
			return m_MessageStyle;
		}
	}
	
	private vp_FPPlayerEventHandler m_Player = null;

	void Awake()
	{
		m_Player = this.transform.GetComponent<vp_FPPlayerEventHandler>();
	}

	/// <summary>
	/// loads GUI labels	
	/// </summary>
	void Start()
	{
		 
		Component[] Labels = GameObject.Find("UI Root (2D)").GetComponentsInChildren<UILabel>();
		foreach( UILabel label in Labels )
		{
			if( label.name == "ClipAmmoLabel" ) ClipAmmoText = label;
			if( label.name == "HealthLabel" ) HealthText = label;
			if( label.name == "RoundTimerLabel" ) RoundTimer = label;
			if( label.name == "NPCNumbersLabel" ) NPCNumbers = label;
		}

		if ( ClipAmmoText == null || HealthText == null || RoundTimer == null || NPCNumbers == null ) 
		{
			Debug.Log("GUI Labels can not be load!");
			Debug.Break();
		}

	}

	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected virtual void OnEnable()
	{
		
		if (m_Player != null)
			m_Player.Register(this);
		
	}

	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected virtual void OnDisable()
	{
		
		
		if (m_Player != null)
			m_Player.Unregister(this);
		
	}

	/// <summary>
	/// resets GUI objects every frame
	/// </summary>
	protected virtual void Update()
	{

		// adjust every GUI object
		ClipAmmoText.text = ( m_Player.GetItemCount.Send("AmmoClip").ToString() +  "/" +  m_Player.CurrentWeaponAmmoCount.Get() );
		if (((int)(m_Player.Health.Get() * (PlayerHealthPercentage))) < 0) 
			HealthText.text = "0";
		else
			HealthText.text = ((int)(m_Player.Health.Get() * (PlayerHealthPercentage))).ToString();
		RoundTimer.text = "**:**";
		NPCNumbers.text = "**/**";

	}

	/// <summary>
	/// renders the current message, fading out in the middle of the screen
	/// </summary>
	protected virtual void OnGUI()
	{

		if (!ShowDamageOrMessage)
			return;
		
		// show a message in the middle of the screen and fade it out
		if (!string.IsNullOrEmpty(m_PickupMessage) && m_MessageColor.a > 0.01f)
		{
			m_MessageColor = Color.Lerp(m_MessageColor, m_InvisibleColor, Time.deltaTime * 0.4f);
			GUI.color = m_MessageColor;
			GUI.Box(new Rect(200, 150, Screen.width - 400, Screen.height - 400), m_PickupMessage, MessageStyle);
			GUI.color = Color.white;
		}
		
		// show a red glow along the screen edges when damaged
		if (DamageFlashTexture != null && m_DamageFlashColor.a > 0.01f)
		{
			m_DamageFlashColor = Color.Lerp(m_DamageFlashColor, m_DamageFlashInvisibleColor, Time.deltaTime * 0.4f);
			GUI.color = m_DamageFlashColor;
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), DamageFlashTexture);
			GUI.color = Color.white;
		}


		// draw the crosshair
		if (m_ImageCrosshair != null
		    //&& !m_Player.Zoom.Active	// <-- uncomment this line to make crosshair disappear when aiming down sights
		    )
		{
			GUI.color = new Color(1, 1, 1, 0.8f);
			GUI.DrawTexture(new Rect((Screen.width * 0.5f) - (m_ImageCrosshair.width * 0.5f),
			                         (Screen.height * 0.5f) - (m_ImageCrosshair.height * 0.5f), m_ImageCrosshair.width,
			                         m_ImageCrosshair.height), m_ImageCrosshair);
			GUI.color = Color.white;
		}
		
	}

	/// <summary>
	/// updates the HUD message text and makes it fully visible
	/// </summary>
	protected virtual void OnMessage_HUDText(string message)
	{
		
		m_MessageColor = Color.white;
		m_PickupMessage = (string)message;
		
	}
	
	
	/// <summary>
	/// shows or hides the HUD full screen flash 
	/// </summary>
	protected virtual void OnMessage_HUDDamageFlash(float intensity)
	{
		
		if (DamageFlashTexture == null)
			return;
		
		if (intensity == 0.0f)
			m_DamageFlashColor.a = 0.0f;
		else
			m_DamageFlashColor.a += intensity;
		
	}
}
