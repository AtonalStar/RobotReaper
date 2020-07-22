 using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class Network : NetworkManager
{

    [HideInInspector]
    public new static Network singleton;

    public PlayerInfo playerInfoPrefab;

    public GameObject playerSpawnPosition;
    //public GameObject hostPlayerPrefab;
    //public GameObject clientPlayerPrefab;

    private int numberOfPlayers;

    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);
    }
    // Use this for initialization
    void Start () {
        numberOfPlayers = 0;
        //playerPrefab = //TODO: set the playerPrefab
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    
    
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
#if false // false for final version, true for demo
        playerSpawnPosition = GameObject.Find("PlayerSpawnPosition");
        Debug.Log("OnServerAddPlayer, conn:" + conn);

        GameObject player = Instantiate(playerPrefab, playerSpawnPosition.transform.position, Quaternion.identity);

        //PlayerInfo newPlayerInfo = Instantiate<PlayerInfo>(playerInfoPrefab);
        //DontDestroyOnLoad(newPlayerInfo);
        NetworkServer.SpawnObjects();
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
#else
        Debug.Log("OnServerAddPlayer, conn:" + conn);
        //GameObject player = Instantiate(playerPrefab, playerSpawnPosition.transform.position, Quaternion.identity);
        PlayerInfo newPlayerInfo = Instantiate<PlayerInfo>(playerInfoPrefab);
        DontDestroyOnLoad(newPlayerInfo);
        NetworkServer.SpawnObjects();
        NetworkServer.AddPlayerForConnection(conn, newPlayerInfo.gameObject, playerControllerId);
        //newPlayerInfo.CmdCreatePlayerPrefab(RobotType.BlackGun);
#endif

    }
    

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        Debug.Log("OnClientSceneChanged");
        if(!ClientScene.ready)
            base.OnClientSceneChanged(conn);
        GameState.singleton.isSceneLoadFinish = true;
    }
    /*
    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("OnServerConnect, conn:" + conn);
        if (numberOfPlayers < 2)
        {

        }
        numberOfPlayers++;
        base.OnServerConnect(conn);
    }
    */
    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("OnClientConnect, conn:" + conn);
        ClientScene.Ready(conn);
        ClientScene.AddPlayer(0);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        NetworkServer.DestroyPlayersForConnection(conn);
        NetworkServer.Reset();
        Shutdown();
        GameState.singleton.ChangeState(State.Menu);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        NetworkServer.DestroyPlayersForConnection(conn);
        NetworkServer.Reset();
        Shutdown();
        GameState.singleton.ChangeState(State.Menu);
    }

    public void CreateGameRoom()
    {
        StartMatchMaker();
        matchMaker.CreateMatch(matchName, matchSize, true, "", "", "", 0, 0, OnGameRoomCreate);
    }

    public void JoinGameRoom(string id)
    {
        StartMatchMaker();
        ulong netId = ulong.Parse(id);
        matchMaker.JoinMatch((NetworkID)netId, "", "", "", 0, 0, OnGameRoomJoin);
    }

    private void OnGameRoomCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        if(!success)
        {
            Debug.LogError(extendedInfo);
        }
        Debug.Log("roomID: " + matchInfo.networkId);

        MenuUI.singleton.OnGameRoomCreate(success, matchInfo.networkId);

        /*
        if (success)
        {
            //Debug.Log("Create match succeeded");

            MatchInfo hostInfo = responseData;
            NetworkServer.Listen(hostInfo, 9000);

            NetworkManager.singleton.StartHost(hostInfo);
        }
        else
        {
            Debug.LogError("Create match failed");
        }
        */
    }

    private void OnGameRoomJoin(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);
        if (!success)
        {
            Debug.LogError(extendedInfo);

        }
        MenuUI.singleton.OnGameRoomJoin(success);
    }
    public void OnMatchDestroy(bool success, string extendedInfo)
    {
        StopMatchMaker();
        //Destroy(GameState.singleton.playerInfo.gameObject);
        StopHost();
        //NetworkServer.Shutdown();
    }
}
