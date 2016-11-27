using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class roomManager : MonoBehaviour
{

    public string gameTitle = "EZ Fortress 2";
    public string gameVersion = "0.1 Alpha";
    public string roomName = "Room 01";
    public Texture2D profilePic;

    public Texture2D editButton;

    public Texture2D barFront;
    public Texture2D barBack;

    public rankItem[] ranks;

    [HideInInspector]
    public rankItem curRank;

    int curRankIndex = 0;

    public GUISkin skin;

    public float scoreProg = 0f;

    public mapItem[] maps;

    bool createRoom = false;
    bool inSettings = false;

    public GameObject[] classPrefabs;

    public wepItem[] guns;

    float audioLvl = 0.5f;

    public Transform[] spawns;
    [HideInInspector]
    public bool spawned = false;
    public float spawnTimer = 3;
    float time = 0;
    public string killer;
    public float roundOverCD = -1;

    public GameObject[] botAi;

    string leadingPlayer = "Nobody";

    Vector2 scroll = Vector2.zero;
    Vector2 mapScroll = Vector2.zero;

    [HideInInspector]
    public string playerName = "";

    public Texture loading;

    bool changeLoadout = false;

    mapItem mapToLoad;

    public byte[] maxPlayerOptions;
    public int[] maxBotOptions;
    public int[] maxZombieOptions;

    public string profilePicURL = "http://i.imgur.com/0yFl6qI.png";

    public int killLimit = 50;

    Texture killerPfp;

    bool roundOver = false;

    public AudioClip roundEnd;
    public AudioClip spawnSound;
    public AudioClip deathSound;

    public smoothFollow camFollow;

    float sens = 2;

    AudioSource aS;

    void Awake()
    {
        if (!PhotonNetwork.connected && !PhotonNetwork.connecting)
        {
            PhotonNetwork.ConnectUsingSettings(gameVersion);
        }

        if (!PlayerPrefs.HasKey("name"))
        {
            playerName = "Player " + Random.Range(0, 999);
            PlayerPrefs.SetString("name", playerName);
        }
        else
        {
            playerName = PlayerPrefs.GetString("name");
        }

        if (PlayerPrefs.HasKey("vol"))
        {
            audioLvl = PlayerPrefs.GetFloat("vol");
        }

        if (PlayerPrefs.HasKey("mouse"))
        {
            sens = PlayerPrefs.GetFloat("mouse");
        }
        AudioListener.volume = audioLvl;

        aS = GetComponent<AudioSource>();

        updateRank();

        if (PlayerPrefs.HasKey("pfp") && PlayerPrefs.GetString("pfp") != "")
        {
            profilePicURL = PlayerPrefs.GetString("pfp");
            StartCoroutine(updatePFP());
        }

        mapToLoad = maps[0];

        if (!PlayerPrefs.HasKey("class"))
        {
            setDefault();
        }

        PlayerPrefs.SetString("class", classPrefabs[0].name);

        curClass = classPrefabs[0];

        if(PhotonNetwork.inRoom)
        { 
            if (PhotonNetwork.isMasterClient)
            {
                spawnBots();
            }
        }
    }

    void OnJoinedMaster()
    {
        PhotonNetwork.JoinLobby();





    }

    void OnJoinedLobby()
    {

    }

    mapItem curMap;

    void OnJoinedRoom()
    {        
        if (aS != null && spawnSound != null)
        {
            aS.PlayOneShot(spawnSound);
        }
        
        Debug.Log("Joined Room");
        spawned = false;
        PhotonNetwork.player.SetScore(0);
        if (PhotonNetwork.isMasterClient)
        {
            spawnBots();
            char[] splitChar = new char[1];
            splitChar[0] = char.Parse("|");
            string[] roomName = PhotonNetwork.room.name.Split(splitChar[0]);
            Debug.Log(roomName[1]);
            string map = roomName[1];            
            foreach(mapItem mp in maps)
            {
                if(mp.levelToLoad == SceneManager.GetActiveScene().name)
                {
                    if(mp.levelToLoad != map)
                    {
                        PhotonNetwork.room.name = roomName[0] + " |" + mp.levelToLoad;
                    }
                }
            }

        }

        PlayerPrefs.SetInt("bots", 0);
        PlayerPrefs.SetInt("zombies", 0);
    }

    public void Spawn()
    {
        if (aS != null && spawnSound != null)
        {
            aS.PlayOneShot(spawnSound);
        }
        spawned = true;
        Transform spawn = spawns[Random.Range(0, spawns.Length)];
        GameObject pl = PhotonNetwork.Instantiate(PlayerPrefs.GetString("class"), spawn.position, spawn.rotation, 0) as GameObject;
        if (pl != null)
        {
            characterControls cc = pl.GetComponent<characterControls>();
            cc.enabled = true;
            cc.fpsCam.SetActive(true);
            cc.head.gameObject.SetActive(false);
            cc.mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            foreach (MeshRenderer accessory in cc.accessories)
            {
                accessory.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
            cc.pv.RPC("setName", PhotonTargets.AllBuffered, PhotonNetwork.playerName);
            cc.pv.RPC("setPFP", PhotonTargets.AllBuffered, profilePicURL);
        }
    }
    GameObject botToSpawn;
    public void spawnBot(string nm)
    {
        if (GameObject.Find("_Network").GetComponent<PhotonView>().isMine)
        {
            foreach (GameObject bt in botAi)
            {
                if (bt.name == nm)
                {
                    botToSpawn = bt;
                }
            }
            if (botToSpawn != null)
            {
                Transform spawn = spawns[Random.Range(0, spawns.Length)];
                PhotonNetwork.Instantiate(botToSpawn.name, spawn.position, spawn.rotation, 0);
            }
        }
    }

    Vector2 profileScroll = Vector2.zero;

    void OnGUI()
    {

        GUI.skin = skin;
        GUIStyle title = new GUIStyle("Label");
        GUIStyle subTitle = new GUIStyle("Label");
        title.fontSize = 36;
        title.alignment = TextAnchor.LowerLeft;
        subTitle.fontSize = 24;
        subTitle.alignment = TextAnchor.LowerLeft;



        if (PhotonNetwork.inRoom)
        {
            if (!spawned && !roundOver) 
            {
                if (time <= 0)
                {
                    GUILayout.BeginArea(new Rect(0, 0, Screen.width / 2, Screen.height)); //Start the Spawn Menu
                    GUILayout.BeginVertical("Box"); //Main Background
                    GUILayout.Label(PhotonNetwork.room.name);
                    GUILayout.Label("XP: " + PlayerPrefs.GetInt("xp"));
                    if (time <= 0)
                    {
                        if (GUILayout.Button("Spawn", GUILayout.Height(75)))
                        {
                            Spawn();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(time.ToString("F2"), GUILayout.Height(75)))
                        {
                        }
                    }
                    GUILayout.BeginHorizontal(); // Class Selection Toolbar
                    foreach (GameObject cl in classPrefabs)
                    {
                        if (GUILayout.Button(cl.name))
                        {
                            PlayerPrefs.SetString("class", cl.name);
                        }
                    }
                    GUILayout.EndHorizontal();
                    if (PhotonNetwork.isMasterClient && botAi.Length > 0)
                    {
                        GUILayout.Label("Spawn:");
                        GUILayout.BeginHorizontal();
                        foreach (GameObject bt in botAi)
                        {
                            if (GUILayout.Button(bt.name))
                            {
                                spawnBot(bt.name);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Leading Player: " + leadingPlayer, subTitle);
                    GUILayout.Label("Kill Limit: " + killLimit);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical("Window");
                    GUILayout.Label("Players:", subTitle);
                    scroll = GUILayout.BeginScrollView(scroll);

                    foreach (PhotonPlayer pl in PhotonNetwork.playerList)
                    {
                        GUILayout.BeginHorizontal("Box");
                        GUILayout.Label(pl.name);
                        GUILayout.Label(" | " + pl.GetScore());
                        if (PhotonNetwork.isMasterClient)
                        {
                            if (GUILayout.Button("Kick"))
                            {
                                PhotonNetwork.CloseConnection(pl);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndScrollView();
                    GUILayout.Label("Bots: " + GameObject.FindObjectsOfType<botAi>().Length);
                    GUILayout.Label("Zombies: " + GameObject.FindObjectsOfType<zombieAi>().Length);
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical("Window");
                    GUILayout.Label("Loadout:", subTitle);
                    profileScroll = GUILayout.BeginScrollView(profileScroll);
                    foreach (GameObject classes in classPrefabs)
                    {
                        string classWep;
                        foreach (wepItem wi in guns)
                        {
                            if (wi.prefabName == PlayerPrefs.GetString(classes.name + "wep"))
                            {
                                classWep = wi.displayName;
                                GUILayout.BeginVertical();
                                GUILayout.Label(classes.name + ": " + classWep);
                                GUILayout.Label(wi.icon, GUILayout.Height(75), GUILayout.Width(150));
                                GUILayout.EndVertical();
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    if (GUILayout.Button("Disconnect"))
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        GameObject[] plObjs = GameObject.FindGameObjectsWithTag("Player");
                        foreach (GameObject plObj in plObjs)
                        {
                            Destroy(plObj);
                        }
                        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player.ID);
                        PhotonNetwork.LeaveRoom();
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }

                if (time > 0 && killer != null)
                {
                    Color c = Color.white;
                    c.a = time / 3;
                    GUI.color = c;
                    GUILayout.BeginArea(new Rect(Screen.width / 2 - 200, Screen.height - 150, 400, 100)); //Killed By Screen
                    GUILayout.BeginHorizontal("Box");
                    if (killerPfp != null)
                    {
                        GUILayout.Label(killerPfp, GUILayout.Width(64), GUILayout.Height(64));
                    }
                    GUILayout.Label("Killed By: " + killer, title);
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
            }
            else {
                GUI.Box(new Rect(Screen.width / 2 - 100, 10, 200, 30), "Leader: " + leadingPlayer);
                GUI.Box(new Rect(Screen.width / 2 - 100, 40, 200, 10), "");
                if (barBack != null && barFront != null)
                {
                    GUI.DrawTexture(new Rect(Screen.width / 2 - 75, 40, 150, 5), barBack, ScaleMode.StretchToFill);
                    GUI.DrawTexture(new Rect(Screen.width / 2 - 75, 40, scoreProg * 150, 5), barFront, ScaleMode.StretchToFill);
                }
            }
        }
        else
        {

            if (PhotonNetwork.insideLobby)
            {
                GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height)); //Main Menu
                GUILayout.BeginVertical("Box");
                GUILayout.BeginHorizontal();
                GUILayout.Label(gameTitle, title);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(); //Top Horizontal Navigation Bar
                if (GUILayout.Button("Quick Play"))
                {
                    quickPlay();
                }
                if (GUILayout.Button("Servers"))
                {
                    changeLoadout = false;
                    inSettings = false;
                }
                if (GUILayout.Button("Loadout"))
                {
                    changeLoadout = true;
                    inSettings = false;
                }
                if (GUILayout.Button("Settings"))
                {
                    inSettings = true;
                }
                if (GUILayout.Button("Quit"))
                {
                    Application.Quit();
                }
                GUILayout.EndHorizontal();
                if (!inSettings)
                {
                    if (!changeLoadout)
                    {
                        GUILayout.BeginVertical();
                        GUILayout.BeginVertical("Box");
                        GUILayout.BeginHorizontal();
                        GUILayout.BeginVertical("Window", GUILayout.Width(200), GUILayout.Height(Screen.height - 90));
                        GUILayout.Label("Profile:", title);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(playerName, subTitle);
                        if (GUILayout.Button(editButton, "label", GUILayout.Width(32), GUILayout.Height(32)))
                        {
                            inSettings = true; ;
                        }
                        GUILayout.EndHorizontal();
                        if (profilePic != null)
                        {
                            GUILayout.Label(profilePic, GUILayout.Width(150), GUILayout.Height(150));
                        }
                        GUILayout.Label("Statistics:", subTitle);
                        if (curRank != null)
                        {
                            GUILayout.BeginVertical("ScrollView");
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(curRank.icon, GUILayout.Width(64), GUILayout.Height(64));
                            GUILayout.BeginVertical();
                            GUILayout.Label("[" + curRank.abbreviation + "]", subTitle);
                            GUILayout.Label(curRank.rankName);
                            GUILayout.EndVertical();
                            GUILayout.EndHorizontal();
                            rankItem nextRank = ranks[curRankIndex + 1];
                            if (nextRank != null)
                            {
                                GUILayout.Label(PlayerPrefs.GetInt("xp") + " / " + nextRank.xp);
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.Label("XP: " + PlayerPrefs.GetInt("xp"));
                        GUILayout.Label("Kills: " + PlayerPrefs.GetInt("kills") + " / Deaths: " + PlayerPrefs.GetInt("deaths"));
                        GUILayout.Label("Headshots: " + PlayerPrefs.GetInt("headshots") + " / Rounds Won: " + PlayerPrefs.GetInt("won"));

                        if (GUILayout.Button("Reset Stats"))
                        {
                            PlayerPrefs.DeleteAll();
                            PhotonNetwork.Disconnect();
                            SceneManager.LoadScene(0);
                        }
                        GUILayout.EndVertical();
                        GUILayout.BeginVertical("Window");
                        if (!createRoom)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.BeginVertical();
                            scroll = GUILayout.BeginScrollView(scroll);
                            if (PhotonNetwork.GetRoomList().Length > 0)
                            {
                                foreach (RoomInfo room in PhotonNetwork.GetRoomList())
                                {
                                    GUILayout.BeginHorizontal("Box");
                                    GUILayout.Label(room.name + " | " + room.playerCount + "/" + room.maxPlayers.ToString());
                                    if (GUILayout.Button("Join", GUILayout.Width(100)))
                                    {
                                        PhotonNetwork.playerName = "[" + curRank.abbreviation + "] " + playerName;
                                        joinRoom(room.name);
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                            else
                            {
                                GUILayout.Label("No Rooms Found...");
                            }
                            GUILayout.EndScrollView();

                            GUILayout.BeginHorizontal();
                            roomName = GUILayout.TextField(roomName);
                            if (GUILayout.Button("Create", GUILayout.Width(100)))
                            {
                                createRoom = true;
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                            GUILayout.BeginVertical("Box", GUILayout.Width(200));
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Loadout:", subTitle);
                            if (GUILayout.Button(editButton, "label", GUILayout.Width(32), GUILayout.Height(32)))
                            {
                                changeLoadout = true;
                            }
                            GUILayout.EndHorizontal();
                            profileScroll = GUILayout.BeginScrollView(profileScroll);
                            if (PlayerPrefs.GetString(classPrefabs[0].name + "wep") != "")
                            {
                                foreach (GameObject classes in classPrefabs)
                                {
                                    string classWep;
                                    foreach (wepItem wi in guns)
                                    {
                                        if (wi.prefabName == PlayerPrefs.GetString(classes.name + "wep"))
                                        {
                                            classWep = wi.displayName;
                                            GUILayout.BeginVertical();
                                            GUILayout.Label(classes.name + ": " + classWep);
                                            GUILayout.Label(wi.icon, GUILayout.Height(75), GUILayout.Width(150));
                                            GUILayout.EndVertical();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                GUILayout.Label("Set Your Loadout in the Loadout Tab");
                            }
                            GUILayout.EndScrollView();
                            if (GUILayout.Button("Edit"))
                            {
                                changeLoadout = true;
                            }
                            GUILayout.EndVertical();
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.BeginVertical(GUILayout.Height(Screen.height - 90));
                            RoomOptions ro = new RoomOptions();
                            GUILayout.Label("Creating " + roomName);
                            GUILayout.Label("Map Select:");
                            mapScroll = GUILayout.BeginScrollView(mapScroll, GUILayout.Height(Screen.width / 6 + 125));
                            GUILayout.BeginHorizontal();
                            foreach (mapItem map in maps)
                            {
                                GUILayout.BeginVertical("Window");
                                GUILayout.Label(map.levelToLoad);
                                GUILayout.Label(map.icon, GUILayout.Width(Screen.width / 3), GUILayout.Height(Screen.width / 6));
                                GUILayout.Label(map.size + " | Kill Limit: " + map.scoreLimit);
                                if (GUILayout.Button("Choose"))
                                {
                                    mapToLoad = map;
                                }
                                GUILayout.EndVertical();
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.EndScrollView();
                            //max player select
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Max Players:", GUILayout.Width(150));

                            foreach (byte mxpO in maxPlayerOptions)
                            {
                                if (GUILayout.Button(mxpO.ToString()))
                                {
                                    ro.MaxPlayers = mxpO;
                                    Debug.Log(ro.MaxPlayers.ToString());
                                }
                            }
                            GUILayout.EndHorizontal();

                            //max bot select
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Bot Count:", GUILayout.Width(150));
                            foreach (int opt in maxBotOptions)
                            {
                                if (GUILayout.Button(opt.ToString()))
                                {
                                    PlayerPrefs.SetInt("bots", opt);
                                }
                            }
                            GUILayout.EndHorizontal();

                            //max zombie select
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Zombie Count:", GUILayout.Width(150));
                            foreach (int opt in maxZombieOptions)
                            {
                                if (GUILayout.Button(opt.ToString()))
                                {
                                    PlayerPrefs.SetInt("zombies", opt);
                                }
                            }
                            GUILayout.EndHorizontal();


                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button("Start", GUILayout.Height(50)))
                            {
                                if (ro.MaxPlayers == 0)
                                {
                                    ro.MaxPlayers = 8;
                                }
                                PhotonNetwork.JoinOrCreateRoom(roomName + "|" + mapToLoad.levelToLoad, ro, null);
                                PhotonNetwork.playerName = "[" + curRank.abbreviation + "] " + playerName;
                                SceneManager.LoadScene(mapToLoad.levelToLoad);
                            }
                            if (GUILayout.Button("Cancel", GUILayout.Height(50)))
                            {
                                createRoom = false;
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                        }

                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                        GUILayout.EndVertical();

                    }
                    else
                    {
                        GUILayout.BeginVertical("Box");                       
                        GUILayout.BeginHorizontal();
                        foreach (GameObject plClass in classPrefabs)
                        {
                            if (GUILayout.Button(plClass.name, GUILayout.Height(50)))
                            {
                                curClass = plClass;
                            }
                        }
                        GUILayout.EndHorizontal();
                        scroll2 = GUILayout.BeginScrollView(scroll2);

                        foreach (wepItem wepIt in guns)
                        {
                            if (wepIt.classFor == curClass)
                            {
                                GUILayout.BeginHorizontal("Box");
                                GUILayout.BeginVertical();
                                GUILayout.BeginVertical();
                                GUILayout.Label(wepIt.icon, GUILayout.Height(100), GUILayout.Width(200));
                                if (PlayerPrefs.GetInt("xp") >= wepIt.xpRequired)
                                {
                                    if (GUILayout.Button("Equip", GUILayout.Height(50), GUILayout.Width(200)))
                                    {
                                        PlayerPrefs.SetString(wepIt.classFor.name + "wep", wepIt.prefabName);
                                        Debug.Log(PlayerPrefs.GetString(wepIt.classFor.name + "wep"));
                                    }
                                }
                                else
                                {
                                    if (GUILayout.Button("LOCKED", GUILayout.Width(200)))
                                    {
                                    }
                                }
                                GUILayout.EndVertical();
                                GUILayout.EndHorizontal();
                                GUILayout.BeginVertical();
                                GUILayout.Label(wepIt.displayName, title);
                                GUILayout.Label(wepIt.description);
                                GUILayout.Label("XP Required: " + wepIt.xpRequired);
                                if (PlayerPrefs.GetInt("xp") >= wepIt.xpRequired)
                                {
                                    if (wepIt.sightAttachments.Length > 0)
                                    {
                                        GUILayout.Label("Optic: " + PlayerPrefs.GetString(wepIt.prefabName + "sight"));
                                    }
                                    if (wepIt.barrelAttachments.Length > 0)
                                    {
                                        GUILayout.Label("Barrel: " + PlayerPrefs.GetString(wepIt.prefabName + "barrel"));
                                    }
                                    if (wepIt.sightAttachments.Length > 0 || wepIt.barrelAttachments.Length > 0)
                                    {
                                        if (GUILayout.Button("Edit"))
                                        {
                                            if (!wepIt.showAttachments)
                                            {
                                                wepIt.showAttachments = true;
                                            }
                                            else
                                            {
                                                wepIt.showAttachments = false;
                                            }
                                        }
                                    }
                                }
                                GUILayout.EndVertical();
                                GUILayout.EndHorizontal();
                                if (wepIt.showAttachments)
                                {
                                    GUILayout.BeginVertical("Box");

                                    if (GUILayout.Button("Clear Attachments"))
                                    {
                                        PlayerPrefs.SetString(wepIt.prefabName + "sight", "");
                                        PlayerPrefs.SetString(wepIt.prefabName + "barrel", "");
                                    }
                                    GUILayout.BeginHorizontal();
                                    if (wepIt.sightAttachments.Length > 0)
                                    {
                                        GUILayout.BeginVertical("Window");
                                        GUILayout.Label("Optics:", subTitle);
                                        foreach (GameObject att in wepIt.sightAttachments)
                                        {
                                            GUILayout.BeginHorizontal("Box");
                                            GUILayout.Label(att.name);
                                            if (GUILayout.Button("Equip", GUILayout.Width(100)))
                                            {
                                                PlayerPrefs.SetString(wepIt.prefabName + "sight", att.name);
                                                wepIt.showAttachments = false;
                                            }
                                            GUILayout.EndHorizontal();
                                        }
                                        GUILayout.EndVertical();
                                    }
                                    if (wepIt.barrelAttachments.Length > 0)
                                    {
                                        GUILayout.BeginVertical("Window");
                                        GUILayout.Label("Barrels:", subTitle);
                                        foreach (GameObject att in wepIt.barrelAttachments)
                                        {
                                            GUILayout.BeginHorizontal("Box");
                                            GUILayout.Label(att.name);
                                            if (GUILayout.Button("Equip", GUILayout.Width(100)))
                                            {
                                                PlayerPrefs.SetString(wepIt.prefabName + "barrel", att.name);
                                                wepIt.showAttachments = false;
                                            }
                                            GUILayout.EndHorizontal();
                                        }
                                        GUILayout.EndVertical();
                                    }
                                    GUILayout.EndHorizontal();
                                    GUILayout.EndVertical();
                                }

                            }
                        }

                        GUILayout.EndScrollView();
                        GUILayout.EndVertical();
                    }
                }
                else
                {
                    GUILayout.BeginVertical("Box", GUILayout.Height(Screen.height - 80)); //Settings Window
                    GUILayout.Label("Settings:", title);
                    GUILayout.BeginVertical("Window"); //Username edit
                    GUILayout.Label("Username:", subTitle);
                    GUILayout.BeginHorizontal();
                    playerName = GUILayout.TextField(playerName, GUILayout.Width(Screen.width - 120));
                    if (GUILayout.Button("Apply", GUILayout.Width(100)))
                    {
                        PlayerPrefs.SetString("name", playerName);
                        PhotonNetwork.playerName = playerName;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical("Window");
                    GUILayout.Label("Profile Pic:", subTitle);
                    GUILayout.BeginHorizontal(); //Change Profile Picture                      
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();                 
                    GUILayout.BeginHorizontal();
                    profilePicURL = GUILayout.TextField(profilePicURL, GUILayout.Width(Screen.width - 100));
                    if (GUILayout.Button("Upload", GUILayout.Width(100)))
                    {
                        StartCoroutine(updatePFP());
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginVertical("Window");
                    GUILayout.BeginHorizontal(); //Volume 
                    GUILayout.Label("Volume:", subTitle, GUILayout.Width(100));
                    audioLvl = GUILayout.HorizontalSlider(audioLvl, 0, 1);
                    if(GUILayout.Button("Apply", GUILayout.Width(100)))
                    {
                        PlayerPrefs.SetFloat("vol", audioLvl);
                        AudioListener.volume = audioLvl;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(); //Mouse Sensitivity
                    GUILayout.Label("Mouse Sensitivity:", subTitle, GUILayout.Width(100));
                    sens = GUILayout.HorizontalSlider(sens, 0.1f, 4f);
                    if (GUILayout.Button("Apply", GUILayout.Width(100)))
                    {
                        PlayerPrefs.SetFloat("mouse", sens);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical("Window"); //Quality Settings
                    GUILayout.Label("Quality:", subTitle);
                    GUILayout.BeginHorizontal();
                    if(GUILayout.Button("<", GUILayout.Width(50)))
                    {
                        QualitySettings.DecreaseLevel();
                    }
                    GUILayout.Box(QualitySettings.names[QualitySettings.GetQualityLevel()], "Window");
                    if (GUILayout.Button(">", GUILayout.Width(50)))
                    {
                        QualitySettings.IncreaseLevel();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    GUILayout.EndVertical();
                }



                GUILayout.EndVertical();
                GUILayout.EndArea();


            }
            else
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), loading, ScaleMode.StretchToFill);
                GUI.Label(new Rect(0, 0, 200, 60), PhotonNetwork.connectionState.ToString());
            }
        }

        if (roundOver)
        {
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 200)); //Round over screen
            GUILayout.BeginVertical("Box");
            title.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Round Ended!", title);
            if (leadingPlayer == PhotonNetwork.playerName)
            {
                GUILayout.Box("You Reached the Kill Limit!! +100 XP");
            }
            else
            {
                GUILayout.Box(leadingPlayer + " Reached the kill limit!");
            }
            if (GUILayout.Button("Continue?  " + roundOverCD.ToString("F0")))
            {
                if (PhotonNetwork.inRoom)
                {
                    roundOver = false;
                    quickPlay();
                }

            }
            if (GUILayout.Button("Go Back to Lobby"))
            {
                if (PhotonNetwork.inRoom)
                {
                    roundOver = false;
                    SceneManager.LoadScene(0);
                    PhotonNetwork.LeaveRoom();
                }

            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    public void Died(string knm)
    {
        time = spawnTimer;
        killer = knm;
        spawned = false;       
        getKillerPFP();
        if (aS != null && deathSound != null)
        {
            aS.PlayOneShot(deathSound);
        }
    }

    public void quickPlay()
    {
        if (PhotonNetwork.connectedAndReady)
        {
            if (PhotonNetwork.inRoom)
            {
                PhotonNetwork.player.SetScore(0);
            }
            if (PhotonNetwork.GetRoomList().Length > 0)
            {
                RoomInfo[] theRooms = PhotonNetwork.GetRoomList();
                joinRoom(theRooms[Random.Range(0, theRooms.Length)].name);
            }
            else
            {
                if (PhotonNetwork.insideLobby || PhotonNetwork.inRoom)
                {
                    PlayerPrefs.SetInt("bots", Random.Range(0, 8));
                    PlayerPrefs.SetInt("zombies", Random.Range(0, 8));
                    RoomOptions ro = new RoomOptions();
                    ro.MaxPlayers = 8;
                    PhotonNetwork.playerName = "[" + curRank.abbreviation + "] " + playerName;
                    string nm1 = "Room " + Random.Range(0, 999) + "|" + maps[Random.Range(0, maps.Length)].levelToLoad;
                    if (PhotonNetwork.connectedAndReady)
                    {
                        PhotonNetwork.JoinOrCreateRoom(nm1, ro, null);
                    }
                    char[] splitChar = new char[1];
                    splitChar[0] = char.Parse("|");
                    string[] roomName = nm1.Split(splitChar[0]);
                    Debug.Log(roomName[1]);
                    if (PhotonNetwork.inRoom)
                    {
                        PhotonNetwork.player.SetScore(0);
                    }
                    SceneManager.LoadScene(roomName[1]);
                }

            }
        }
    }

    public void joinRoom(string nm)
    {
        PhotonNetwork.playerName = "[" + curRank.abbreviation + "] " + playerName;
        char[] splitChar = new char[1];
        splitChar[0] = char.Parse("|");
        string[] roomName = nm.Split(splitChar[0]);
        Debug.Log(roomName[1]);
        string map = roomName[1];
        Debug.Log(map);
        if (PhotonNetwork.inRoom)
        {
            PhotonNetwork.player.SetScore(0);
        }
        foreach (mapItem mapI in maps)
        {
            if (mapI.levelToLoad == map)
            {
                mapToLoad = mapI;
                Debug.Log(mapI.levelToLoad + " / " + mapI.levelToLoad);
            }
        }
        SceneManager.LoadScene(mapToLoad.levelToLoad);
        PhotonNetwork.JoinRoom(nm);        
    }

    int topScore = 0;
    Vector2 scroll2 = Vector2.zero;
    GameObject curClass;

    void FixedUpdate()
    {

        if (PhotonNetwork.inRoom)
        {
            char[] splitChar = new char[1];
            splitChar[0] = char.Parse("|");
            string[] roomName = PhotonNetwork.room.name.Split(splitChar[0]);
            if (roomName[1] != SceneManager.GetActiveScene().name)
            {
                PhotonNetwork.room.name = roomName[0] + " |" + SceneManager.GetActiveScene().name;
            }

            if (time > 0)
            {
                time += -Time.fixedDeltaTime;

            }
            else
            {
                if (camFollow != null)
                {
                    GameObject kl = GameObject.Find(killer);
                    camFollow.enabled = true;
                    if (kl != null)
                    {
                        camFollow.target = kl.transform;
                    }
                    else
                    {
                        tpEffect[] trgs = GameObject.FindObjectsOfType<tpEffect>();
                        if (camFollow.target == null)
                        {
                            if (trgs.Length > 0)
                            {
                                tpEffect trg = trgs[Random.Range(0, trgs.Length)];
                                camFollow.target = trg.transform;
                            }
                        }
                    }
                }

                time = 0;

            }

            if (roundOverCD > 0)
            {
                roundOverCD += -Time.fixedDeltaTime;
            }
            if (roundOverCD < 1 && roundOverCD > -1)
            {
                roundOver = false;
                roundOverCD = -1;
                quickPlay();
            }


            if (!roundOver)
            {
                foreach (PhotonPlayer pp in PhotonNetwork.playerList)
                {
                    if (pp.GetScore() >= killLimit)
                    {
                        Debug.Log(pp.name);
                        roundOver = true;
                        endGame(pp);
                    }
                }


            }



            foreach (PhotonPlayer pl in PhotonNetwork.playerList)
            {
                if (pl.GetScore() > topScore)
                {
                    topScore = pl.GetScore();
                    leadingPlayer = pl.name;
                    scoreProg = float.Parse(pl.GetScore().ToString()) / killLimit;
                }
            }

            if (!roundOver)
            {
                Cursor.visible = !spawned;
            }
            else
            {
                Cursor.visible = true;
            }

            if (!spawned)
            {
                Cursor.lockState = CursorLockMode.None;
            }

        }

        if (GetComponentInChildren<AudioListener>() != null)
        {
            GetComponentInChildren<AudioListener>().enabled = !spawned;
        }

        if (curRank == null)
        {
            updateRank();
        }

    }

    void OnLeftRoom()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        updateRank();
        SceneManager.LoadScene(0);
    }

    IEnumerator updatePFP()
    {
        while (true)
        {
            WWW www = new WWW(profilePicURL);
            yield return www;
            profilePic = www.texture;
            PlayerPrefs.SetString("pfp", profilePicURL);
        }
    }

    public void getKillerPFP()
    {
        if (GameObject.Find(killer) != null)
        {
            GameObject kilr = GameObject.Find(killer);
            if (kilr.GetComponent<characterControls>() != null)
            {
                if (kilr.GetComponent<characterControls>().pfp != null)
                {
                    killerPfp = kilr.GetComponent<characterControls>().pfp;
                }
            }
            else
            {
                if (kilr.GetComponent<botAi>() != null)
                {
                    if (kilr.GetComponent<botAi>().pfp != null)
                    {
                        killerPfp = kilr.GetComponent<botAi>().pfp;
                    }
                }
            }
        }
    }

    public void setDefault()
    {
        foreach (GameObject clss in classPrefabs)
        {
            characterControls cc = clss.GetComponent<characterControls>();
            wepManager wm = cc.fpsCam.GetComponentInChildren<wepManager>();
            if (wm != null)
            {
                PlayerPrefs.SetString(clss.name + "wep", wm.primaries[0].name);
            }
        }
    }

    public void updateRank()
    {
        foreach (rankItem rank in ranks)
        {
            if (PlayerPrefs.GetInt("xp") >= rank.xp)
            {
                curRank = rank;
                curRankIndex = System.Array.IndexOf(ranks, curRank);
                PhotonNetwork.playerName = "[" + curRank.abbreviation + "] " + playerName;
            }
        }
    }

    public void endGame(PhotonPlayer winn)
    {
        if (winn == PhotonNetwork.player)
        {
            PlayerPrefs.SetInt("xp", PlayerPrefs.GetInt("xp") + 100);
            PlayerPrefs.SetInt("won", PlayerPrefs.GetInt("won") + 1);
        }
        foreach (GameObject pl in GameObject.FindGameObjectsWithTag("Player"))
        {
            botAi ba = pl.GetComponent<botAi>();
            zombieAi za = pl.GetComponent<zombieAi>();
            characterControls cc = pl.GetComponent<characterControls>();
            Rigidbody rb = pl.GetComponent<Rigidbody>();
            NavMeshAgent nma = pl.GetComponent<NavMeshAgent>();
            dummyAi da = pl.GetComponent<dummyAi>();
            mouseLook mL = pl.GetComponentInChildren<mouseLook>();

            if (cc != null)
            {
                cc.enabled = false;
            }

            if (ba != null)
            {
                ba.enabled = false;
            }

            if (za != null)
            {
                za.enabled = false;
            }

            if (rb != null)
            {
                rb.isKinematic = true;
            }

            if (nma != null)
            {
                nma.Stop();
                nma.enabled = false;
            }

            if (da != null)
            {
                da.enabled = false;
            }

            if (mL != null)
            {
                mL.enabled = false;
            }

        }

        foreach (wepManager wM in FindObjectsOfType<wepManager>())
        {
            wM.enabled = false;
        }

        foreach (gunScript gS in FindObjectsOfType<gunScript>())
        {
            gS.enabled = false;
        }

        foreach (projectileLauncher pL in FindObjectsOfType<projectileLauncher>())
        {
            pL.enabled = false;
        }

        foreach (aimScript aS1 in FindObjectsOfType<aimScript>())
        {
            aS1.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (aS != null && roundEnd != null)
        {
            aS.PlayOneShot(roundEnd);
        }

        roundOverCD = 20;
    }

    void spawnBots()
    {
        Debug.Log("spawning bots");
        for (int i = 0; i < PlayerPrefs.GetInt("bots"); i++)
        {
            if (botAi.Length > 0)
            {
                spawnBot(botAi[0].name);
            }
        }
        for (int i = 0; i < PlayerPrefs.GetInt("zombies"); i++)
        {
            if (botAi.Length > 1)
            {
                spawnBot(botAi[1].name);
            }
        }
    }


    


}
