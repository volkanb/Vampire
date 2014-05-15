// (c)2011 MuchDifferent. All Rights Reserved.

using UnityEngine;
using uLink;

[AddComponentMenu("uLink Utilities/Client GUI")]
public class uLinkClientGUI : uLink.MonoBehaviour
{
	public bool inputName = true;

	public string quickText = "Play on Localhost";
	public string quickHost = "127.0.0.1";
	public int quickPort = 7100;

	public bool hasAdvancedMode = true;
	public string gameType = "MyUniqueGameType";
	public bool showGameLevel = false;

	public Texture2D iconFavorite;
	public Texture2D iconNonfavorite;

	public UnityEngine.MonoBehaviour[] enableWhenGUI;
	public UnityEngine.MonoBehaviour[] disableWhenGUI;

	public bool reloadOnDisconnect = false;

	public int targetFrameRate = 60;

	public GUISkin guiSkin = null;
	public int guiDepth = 0;

	private string playerName;

	//private bool isQuickMode = true;
	private bool isQuickMode = false;

	private const float QUICK_WIDTH = 220;
	private const float ADVANCED_WIDTH = 620;
	private const float BUSY_WIDTH = 220;

	private Vector2 scrollPosition = Vector2.zero;
	private int selectedGrid = 0;

	private bool isRedirected = false;

	public bool dontDestroyOnLoad = false;

	public bool lockCursor = true;
	public bool hideCursor = true;

	// ------------------------------------
	// TEAM SELECTORS
	private bool isSlayer = false;
	private bool isVampire = false;

	public uLink.BitStream vars;
	// ------------------------------------

	// ------------------------------------
	// SHOWS CREDITS PAGE
	private bool credits = false;
	private const string creditsText = 
		"Developers:\n" +
		"Volkan Benli\t\t\tvolkanbenli92gmail.com\n" +
		"Mehmet Burucu\t\tmehmetburucuhotmail.com\n" +
		"Onur Keklik\t\t\tonurkeklik90gmail.com\n" +
		"Ersel Erbay\t\t\terselerbaylive.com\n" +
		"Hande Akin\t\t\thande.akin123gmail.com\n" +
		"\n" +
		"\"Vampire Slayer\" online fps game is a dissertation project of the five software engineers\n" +
		"who are mentioned above. Developers would be happy to receive questions about the game or any\n" +
		"other concern.\n" +
		"\n" +
		"All Rights Reserved.\n" +
		"\n" +
		"Izmir - Turkey\n" +
		"2014";
	// ------------------------------------

	// ------------------------------------
	// SHOWS INSTRUCTIONS PAGE
	private bool howTo = false;
	private const string howToText = 
		"Objective:\n" +
		"The objective of the game if you are a vampire is to turn as many humans as you can into vampires\n" +
		"and if you are a slayer take as many humans as you can to the safe spot, which is the golden ball\n" +
		"where slayers respawn, within the allocated time limit.A rescued human means 1 point for the slayer\n" +
		"team. A bitten and turned human means 1 point for the vampire team. First team to reach the winning\n" +
		"score of 10 wins the game!\n" +
		"\n" +
		"Controls:\n" +
		"\n" +
		"For Slayers\n" +
		"Move - WASD\n" +
		"Jump - Space\n" +
		"Sprint - Left Shift\n" +
		"Shoot - Left Mouse Button\n" +
		"Zoom - Right Mouse Button\n" +
		"Make a human follow you - E\n" +
		"Changing weapons - 123\n" +
		"Crouch - Left Ctrl\n" +
		"Replenish Health&Ammo - F\n" +
		"\n" +
		"For vampires:\n" +
		"Move - WASD\n" +
		"Jump - Space\n" +
		"Sprint - Left Shift\n" +
		"Melee Attack - Left Mouse Button\n" +
		"Turn a human into a vampire - E\n" +
		"\n" +
		"Game Mechanics:\n" +
		"Vampires go down when their health reaches zero but in order to completely kill them you need\n" +
		"to go over their bodies and attack them using the mace in button 3.If you are a vampire, in\n" +
		"order to turn a human into a vampire you need to stand still for a few seconds while holding\n" +
		"down button E in front of a human. If you are a slayer it is much shorter to give a human a\n" +
		"pistol and making them follow you. Just tap E in front of a human You can prevent slayers\n" +
		"from gaining points if you kill the humans while they are being taken to the safe house.\n" +
		"Slayers can replenish their health + ammo from a box at the gate of their base by pressing F.\n" +
		"Do not be afraid of dying, dead players instantly respawns from the base of their team";
	// ------------------------------------

	void Awake()
	{
#if !UNITY_2_6 && !UNITY_2_6_1
		if (Application.webSecurityEnabled)
		{
			Security.PrefetchSocketPolicy(uLink.NetworkUtility.ResolveAddress(quickHost).ToString(), 843);
			Security.PrefetchSocketPolicy(uLink.MasterServer.ipAddress, 843);
		}
#endif

		Application.targetFrameRate = targetFrameRate;

		if (dontDestroyOnLoad) DontDestroyOnLoad(this);

		playerName = PlayerPrefs.GetString("playerName", "Guest" + Random.Range(1, 100));
	}

	void OnDisable()
	{
		PlayerPrefs.SetString("playerName", playerName);
	}

	void OnGUI()
	{
		if (uLink.Network.lastError == uLink.NetworkConnectionError.NoError && uLink.Network.status == uLink.NetworkStatus.Connected && uLink.NetworkView.FindByOwner(uLink.Network.player).Length != 0 && (!lockCursor || Screen.lockCursor))
		{
			EnableGUI(false);
			return;
		}

		EnableGUI(true);

		var oldSkin = GUI.skin;
		var oldDepth = GUI.depth;

		GUI.skin = guiSkin;
		GUI.depth = guiDepth;

		GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();

		if (uLink.Network.lastError != uLink.NetworkConnectionError.NoError || uLink.Network.status != uLink.NetworkStatus.Disconnected)
		{
			GUILayout.BeginVertical("Box", GUILayout.Width(BUSY_WIDTH));
			BusyGUI();
			GUILayout.EndVertical();
		}
		else if (credits)
		{
			GUILayout.BeginVertical("Box", GUILayout.Width(620));
			CreditsPage();
			GUILayout.EndVertical();
		}
		else if (howTo)
		{
			GUILayout.BeginVertical("Box", GUILayout.Width(620));
			HowToPage();
			GUILayout.EndVertical();
		}
		else if (isQuickMode)
		{
			GUILayout.BeginVertical("Box", GUILayout.Width(QUICK_WIDTH));
			QuickGUI();
			GUILayout.EndVertical();
		}
		else
		{
			GUILayout.BeginVertical("Box", GUILayout.Width(ADVANCED_WIDTH));
			AdvancedGUI();
			GUILayout.EndVertical();
		}

		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		GUI.skin = oldSkin;
		GUI.depth = oldDepth;
	}

	void BusyGUI()
	{
		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		string busyDoingWhat = "Busy...";

		if (uLink.Network.lastError != uLink.NetworkConnectionError.NoError)
		{
			busyDoingWhat = "Error: " + uLink.NetworkUtility.GetErrorString(uLink.Network.lastError);
		}
		else if (uLink.Network.status == uLink.NetworkStatus.Connected)
		{
			if (uLink.NetworkView.FindByOwner(uLink.Network.player).Length != 0)
			{
				if (lockCursor)
				{
					busyDoingWhat = "Click to start playing";

					if (Input.GetMouseButton(0)) Screen.lockCursor = true;
				}
			}
			else
			{
				busyDoingWhat = "Instantiating...";
			}
		}
		else if (uLink.Network.status == uLink.NetworkStatus.Connecting)
		{
			string prefix = isRedirected ? "Redirecting to " : "Connecting to ";
			busyDoingWhat = prefix + uLink.NetworkPlayer.server.endpoint;
		}
		else if (uLink.Network.status == uLink.NetworkStatus.Disconnecting)
		{
			busyDoingWhat = "Disconnecting";
		}

		GUILayout.Label(busyDoingWhat);

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		if (uLink.Network.status == uLink.NetworkStatus.Connecting && !isRedirected)
		{
			GUILayout.BeginVertical();
			GUILayout.Space(5);
			GUILayout.EndVertical();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Cancel", GUILayout.Width(80), GUILayout.Height(25)))
			{
				uLink.Network.DisconnectImmediate();
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();
	}

	void QuickGUI()
	{
		if (inputName)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Please enter your name:");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginVertical();
			GUILayout.Space(5);
			GUILayout.EndVertical();

			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			playerName = GUILayout.TextField(playerName, GUILayout.MinWidth(80));
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.BeginVertical();
			GUILayout.Space(5);
			GUILayout.EndVertical();

			//------------------------------------------------------
			// FOLLOWING CODES SHOWS THE TEAM SELECTOR CHECKBOXES

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Please select a team:");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Space(10);

			if ( isVampire == false )
				isSlayer = true;
			else if ( isVampire == true )
				isSlayer = false;

			isSlayer = GUILayout.Toggle( isSlayer , "Slayer");
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Space(10);

			if ( isSlayer == false )
				isVampire = true;
			else if ( isSlayer == true )
				isVampire = false;

			isVampire = GUILayout.Toggle( isVampire , "Vampire");
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.BeginVertical();
			GUILayout.Space(5);
			GUILayout.EndVertical();

			//------------------------------------------------------
		}

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (GUILayout.Button(quickText, GUILayout.Width(120), GUILayout.Height(25)))
		{
			Connect(quickHost, quickPort);
		}

		if (hasAdvancedMode)
		{
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Advanced", GUILayout.Width(80), GUILayout.Height(25)))
			{
				isQuickMode = false;
			}
		}

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginVertical();
		GUILayout.Space(2);
		GUILayout.EndVertical();
	}

	void AdvancedGUI()
	{
		GUILayout.BeginHorizontal();
		selectedGrid = GUILayout.SelectionGrid(selectedGrid, new string[] { "Internet", "LAN", "Favorites" }, 3, GUILayout.Height(22));
		GUILayout.EndHorizontal();

		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();

		GUILayout.BeginHorizontal("Box");
		GUILayout.Space(5);

		GUILayout.Label("Name", GUILayout.Width(80));
		if (showGameLevel) GUILayout.Label("Level", GUILayout.Width(80));
		GUILayout.Label("Players", GUILayout.Width(80));
		GUILayout.Label("Host", GUILayout.Width(140));
		GUILayout.Label("Ping", GUILayout.Width(80));

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal(GUILayout.Height(260));
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, "Box");

		uLink.HostData[] hosts = null;
		switch (selectedGrid)
		{
			case 0: hosts = uLink.MasterServer.PollAndRequestHostList(gameType, 2); break;
			case 1: hosts = uLink.MasterServer.PollAndDiscoverLocalHosts(gameType, quickPort, 2); break;
			case 2: hosts = uLink.MasterServer.PollAndRequestKnownHosts(2); break;
		}

		if (hosts != null && hosts.Length > 0)
		{
			System.Array.Sort(hosts, delegate(uLink.HostData x, uLink.HostData y) { return x.gameName.CompareTo(y.gameName); });

			foreach (uLink.HostData data in hosts)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(5);

				GUILayout.Label(data.gameName, GUILayout.Width(80));
				if (showGameLevel) GUILayout.Label(data.gameLevel, GUILayout.Width(80));
				GUILayout.Label(data.connectedPlayers + "/" + data.playerLimit, GUILayout.Width(80));
				GUILayout.Label(data.ipAddress + ":" + data.port, GUILayout.Width(140));
				GUILayout.Label(data.ping.ToString(), GUILayout.Width(80));

				GUILayout.FlexibleSpace();

				if (uLink.MasterServer.PollKnownHostData(data.externalEndpoint) != null)
				{
					if (iconFavorite != null ? GUILayout.Button(iconFavorite, "Label") : GUILayout.Button("Unlove"))
					{
						uLink.MasterServer.RemoveKnownHostData(data.externalEndpoint);
					}
				}
				else
				{
					if (iconNonfavorite != null ? GUILayout.Button(iconNonfavorite, "Label") : GUILayout.Button("Love"))
					{
						uLink.MasterServer.AddKnownHostData(data);
					}
				}

				GUILayout.Space(5);

				if (uLink.Network.status == uLink.NetworkStatus.Disconnected)
				{
					if (GUILayout.Button("Join"))
					{
						Connect(data);
					}
				}

				GUILayout.Space(10);
				GUILayout.EndHorizontal();
			}
		}
		else if (selectedGrid == 2)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("No hosts have been marked as favorite");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		else if (selectedGrid == 1 && (Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer))
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("No LAN hosts have been discovered yet");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("this may be due to security restrictions on the webplayer");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		else
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Please wait for the list to begin populating...");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		GUILayout.EndScrollView();
		GUILayout.EndHorizontal();

		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();


		//------------------------------------------------------------------------------------
		// FOLLOWING CODES SHOWS THE TEAM SELECTOR CHECKBOXES

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Please enter your name:");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		playerName = GUILayout.TextField(playerName, GUILayout.MinWidth(80));
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();
	
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Please select a team:");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		
		if ( isVampire == false )
			isSlayer = true;
		else if ( isVampire == true )
			isSlayer = false;
		
		isSlayer = GUILayout.Toggle( isSlayer , "Slayer");
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		
		if ( isSlayer == false )
			isVampire = true;
		else if ( isSlayer == true )
			isVampire = false;
		
		isVampire = GUILayout.Toggle( isVampire , "Vampire");
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();


		//------------------------------------------------------------------------------------

		GUILayout.BeginHorizontal();
		GUILayout.Space(2);

		//if (GUILayout.Button("Back", GUILayout.Width(80), GUILayout.Height(25)))
		if (GUILayout.Button("Quit", GUILayout.Width(80), GUILayout.Height(25)))
		{
			//isQuickMode = true;
			//isQuickMode = false;

			Application.Quit();
			Application.Quit();
			Application.Quit();
			Application.Quit();
			Application.Quit();
		}

		GUILayout.Space(2);

		if (GUILayout.Button("Credits", GUILayout.Width(80), GUILayout.Height(25)))
		{
			credits = true;
		}

		GUILayout.FlexibleSpace();


		if (GUILayout.Button("HOW TO PLAY (PLEASE READ BEFORE PLAYING!)", GUILayout.Width(350), GUILayout.Height(25)))
		{
			howTo = true;
		}


		GUILayout.Space(2);
		GUILayout.EndHorizontal();

		GUILayout.BeginVertical();
		GUILayout.Space(2);
		GUILayout.EndVertical();
	}

	// SHOWS CReDITS PAGE
	void CreditsPage()
	{
		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();
		
		GUI.skin.box.alignment = TextAnchor.UpperLeft;
		
		GUILayout.Box (creditsText, GUILayout.Width(600));
		
		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		
		if (GUILayout.Button("Back", GUILayout.Width(80), GUILayout.Height(25)))
		{
			credits = false;
		}
		
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();

	}

	// SHOWS INSTRUCTIONS PAGE
	void HowToPage()
	{

		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();

		GUI.skin.box.alignment = TextAnchor.UpperLeft;

		GUILayout.Box (howToText, GUILayout.Width(600));

		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();

		GUILayout.BeginHorizontal();
		GUILayout.Space(10);

		if (GUILayout.Button("Back", GUILayout.Width(80), GUILayout.Height(25)))
		{
			howTo = false;
		}

		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();

		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();

	}

	void EnableGUI(bool enabled)
	{
		if (lockCursor) Screen.lockCursor = !enabled;
		if (hideCursor) Screen.showCursor = enabled;

		foreach (UnityEngine.MonoBehaviour component in enableWhenGUI)
		{
			component.enabled = enabled;
		}

		foreach (UnityEngine.MonoBehaviour component in disableWhenGUI)
		{
			component.enabled = !enabled;
		}
	}

	void Connect(string host, int port)
	{
		isRedirected = false;

		if (inputName)
		{

			//uLink.Network.Connect(host, port, "", playerName);

			uLink.Network.Connect(host, port, "", playerName, isSlayer, isVampire);
		}
		else
		{
			uLink.Network.Connect(host, port);
		}
	}

	void Connect(uLink.HostData host)
	{
		isRedirected = false;

		if (inputName)
		{	
			uLink.Network.Connect(host, "", playerName, isSlayer, isVampire);
		}
		else
		{
			uLink.Network.Connect(host);
		}
	}

	void uLink_OnRedirectingToServer()
	{
		isRedirected = true;
		EnableGUI(true);
	}

	void uLink_OnDisconnectedFromServer(uLink.NetworkDisconnection mode)
	{
		//isQuickMode = true;
		isQuickMode = false;

		if (reloadOnDisconnect && mode != uLink.NetworkDisconnection.Redirecting && Application.loadedLevel != -1)
		{
			Application.LoadLevel(Application.loadedLevel);
		}
	}
}

