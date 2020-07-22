using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public enum State
{
    Menu,
    EnterMap,
    Play,
    FinishLevel,
    FinishMap
}

public enum GameMode
{
    Single,
    Double
}

public enum Map
{
    Forest,
    Building,
    UnderSea
}

public enum Level
{
    Begin,
    Penetrate,
    End
}

public class GameState : MonoBehaviour
{
    public State gameState;
    
    [HideInInspector]
    public string playerName;

    [HideInInspector]
    public Player[] players;
    [HideInInspector]
    public int maxMapNumber = 3;
    [HideInInspector]
    public PlayerInfo playerInfo;

    public bool isSceneLoadFinish = false;
    public bool isGameStart = false;

    public GameObject playerRobotPrefab;

    public RobotType playerRobot;
    
    [HideInInspector]
    public Map map;
    [HideInInspector]
    public Level level;
    [HideInInspector]
    public GameMode gameMode;


    [HideInInspector]
    public static GameState singleton;
        
    public GameObject BlackGun;
    public GameObject Ghost;
    public GameObject Monster;

    void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);
    }

    public void ChangeState(State newState)
    {
        gameState = newState;
        switch(gameState)
        {
            case State.Menu:
                isGameStart = false;
                isSceneLoadFinish = false;
                SceneManager.LoadScene(0);
                break;
            case State.EnterMap:
                Network.singleton.StartHost();
                break;
            case State.Play:
                //Debug.Log("playerInfo:" + playerInfo);
                isGameStart = false;
                LoadLevel();
                NetworkServer.SpawnObjects();
                StartCoroutine(AddPlayerPrefab());
                break;
            case State.FinishLevel:
                level++;
                if (level > Level.End)
                {
                    ChangeState(State.FinishMap);
                    break;
                }
                ChangeState(State.Play);
                break;
            case State.FinishMap:
                HUDUI.singleton.OnGameWin();
                // TODO: show win screen
                break;
        }
    }

    public void SetPlayers()
    {
        players = FindObjectsOfType<Player>();
        Debug.Log("number of players = " + players.Length);
        GameState.singleton.GameFailCheck();
    }

    public void GameFinishCheck()
    {
        playerInfo.CmdGameFinishCheck();
    }

    public void GameFailCheck()
    {
        playerInfo.CmdGameFailCheck();
    }

    private void LoadLevel()
    {
        int levelIndex = (int)GameState.singleton.map * 3 + (int)GameState.singleton.level + 1;
        
        if(gameMode == GameMode.Double)
            playerInfo.CmdLoadLevel(levelIndex);
        else
        {
            Network.singleton.ServerChangeScene(SceneUtility.GetScenePathByBuildIndex(levelIndex));
        }

    }


    IEnumerator AddPlayerPrefab()
    {
        yield return new WaitUntil(() => isSceneLoadFinish == true);
        isSceneLoadFinish = false;
        playerInfo.CmdCreatePlayerPrefab(playerRobot);
        isGameStart = true;
    }

    // Use this for initialization
    void Start () {
        gameState = State.Menu;
    }
	
	// Update is called once per frame
	void Update () {
	}

}
