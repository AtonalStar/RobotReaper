using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PlayerInfo : NetworkBehaviour
{
    [HideInInspector]
    public string playerName;
    [HideInInspector]
    public RobotType robotType;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Use this for initialization
    void Start () {
		if(hasAuthority)
        {
            Debug.Log("Local PlayerInfo Start()");
            GameState.singleton.playerInfo = this;
            //Debug.Log("Start(): playerInfo:" + GameState.singleton.playerInfo);
            if (GameState.singleton.gameMode == GameMode.Single)
                GameState.singleton.ChangeState(State.Play);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    [Command]
    public void CmdLoadLevel(int levelIndex)
    {
        //int levelIndex = (int)GameState.singleton.map * 3 + (int)GameState.singleton.level + 1;
        //Debug.Log("scene " + levelIndex + " : " + SceneUtility.GetScenePathByBuildIndex(levelIndex));
        //Network.singleton.ServerChangeScene(SceneUtility.GetScenePathByBuildIndex(10));
        GameState.singleton.playerInfo.CmdServerChangeScene(levelIndex);
        //Network.singleton.ServerChangeScene(SceneUtility.GetScenePathByBuildIndex(levelIndex));
        GameState.singleton.isSceneLoadFinish = false;
        //SceneManager.LoadScene(levelIndex);
        //SceneManager.LoadScene(10); // only for demo1 testing
        //Network.singleton.OnServerAddPlayer();
    }

    [Command]
    public void CmdServerChangeScene(int levelIndex)
    {
        Network.singleton.ServerChangeScene(SceneUtility.GetScenePathByBuildIndex(levelIndex));
    }

    [Command]
    public void CmdCreatePlayerPrefab(RobotType playerRobotType)
    {
        GameObject playerSpawnPosition = GameObject.Find("PlayerSpawnPosition");
        GameObject playerPrefab = GetRobot(playerRobotType);
        Debug.Log("CmdCreatePlayerRobotPrefab()");
        GameObject player = Instantiate(playerPrefab, playerSpawnPosition.transform.position, Quaternion.identity);

        NetworkServer.SpawnWithClientAuthority(player, connectionToClient);
        player.GetComponent<Player>().RpcSetFollowCamera();
    }

    private GameObject GetRobot(RobotType type)
    {
        switch(type)
        {
            case RobotType.BlackGun : return GameState.singleton.BlackGun;
            case RobotType.Ghost: return GameState.singleton.Ghost;
            case RobotType.Monster: return GameState.singleton.Monster;
        }
        return GameState.singleton.BlackGun;
    }

    [Command]
    public void CmdGameFinishCheck()
    {
        if (!GameState.singleton.isGameStart)
            return;
        foreach (Player player in GameState.singleton.players)
        {
            if (player.isFinishLevel == false)
                return;
        }
        foreach (Player player in GameState.singleton.players)
        {
            Destroy(player.gameObject);
        }
        GameState.singleton.ChangeState(State.FinishLevel);

    }
    [Command]
    public void CmdGameFailCheck()
    {
        if (!GameState.singleton.isGameStart)
            return;
        if (GameState.singleton.players.Length <= 0)
        {
            Debug.Log("players <= 0");
            HUDUI.singleton.OnGameFail();
            return;
        }
        foreach (Player player in GameState.singleton.players)
        {
            if (player == null)
            {
                Debug.Log("player == null");
                HUDUI.singleton.OnGameFail();
                return;
            }
        }
    }

    [Command]
    public void CmdDoublePlayerStart()
    {
        RpcDoublePlayerStart();
    }

    [ClientRpc]
    public void RpcDoublePlayerStart()
    {
        GameState.singleton.ChangeState(State.Play);
    }
}
