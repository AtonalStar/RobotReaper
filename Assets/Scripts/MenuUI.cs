using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class MenuUI : MonoBehaviour
{
    

    [HideInInspector]
    public static MenuUI singleton;

    

    [SerializeField]
    private CanvasGroup mainMenuPanel;
    [SerializeField]
    private CanvasGroup settingPanel;
    [SerializeField]
    private CanvasGroup creditPanel;
    [SerializeField]
    private CanvasGroup selectRobotPanel;
    [SerializeField]
    private CanvasGroup selectMapPanel;
    [SerializeField]
    private CanvasGroup doublePlayerPanel;
    [SerializeField]
    private CanvasGroup errorPanel;
    [SerializeField]
    private CanvasGroup createGameWaitingPanel;
    [SerializeField]
    private GameObject[] mapButton;  


    [SerializeField]
    private Text playerName;
    [SerializeField]
    private Text roomID;
    //[SerializeField]
    //private PlayerInfo playerInfo;

    private CanvasGroup currentPanel;



    // Use this for initialization
    void Start () {
        
        singleton = this;
        mainMenuPanel.gameObject.SetActive(true);
        settingPanel.gameObject.SetActive(false);
        creditPanel.gameObject.SetActive(false);
        selectRobotPanel.gameObject.SetActive(false);
        selectMapPanel.gameObject.SetActive(false);
        doublePlayerPanel.gameObject.SetActive(false);
        errorPanel.gameObject.SetActive(false);
        createGameWaitingPanel.gameObject.SetActive(false);
        GameState.singleton.gameState = State.Menu;
    }
	
	// Update is called once per frame
	void Update () {
        
	}

    public void ShowPanel(CanvasGroup nextPanel)
    {
        if (currentPanel != null)
        {
            currentPanel.gameObject.SetActive(false);
        }

        currentPanel = nextPanel;
        if (currentPanel != null)
        {
            currentPanel.gameObject.SetActive(true);
        }
    }

    public void OnSinglePlayerClicked()
    {
        GameState.singleton.gameMode = GameMode.Single;
        ShowPanel(selectRobotPanel);
    }

    public void OnDoublePlayerClicked()
    {
        GameState.singleton.gameMode = GameMode.Double;
        // TODO: load the name.text from the disk
        doublePlayerPanel.gameObject.SetActive(true);
    }

    public void OnCreateGameClicked()
    {
        // TODO: save the name.text to the disk
        
        GameState.singleton.playerName = playerName.text;
        if (GameState.singleton.playerName == string.Empty)
        {
            errorPanel.transform.GetChild(1).GetComponent<Text>().text = "Name is Empty, please enter your name.";
            errorPanel.gameObject.SetActive(true);
            return;
        }
        
        Network.singleton.CreateGameRoom();
        // TODO: jump to lobby panel (need to create) (Apr/5: may not needed for a lobby)
    }

    public void OnJoinGameClicked(Text text)
    {
        GameState.singleton.playerName = playerName.text;
        if (GameState.singleton.playerName == string.Empty)
        {
            errorPanel.transform.GetChild(1).GetComponent<Text>().text = "Name is Empty, please enter your name.";
            errorPanel.gameObject.SetActive(true);
            return;
        }
        Network.singleton.JoinGameRoom(text.text);
        Debug.Log("Join:" + text.text);
    }

    public void OnSettingClicked()
    {
        settingPanel.gameObject.SetActive(true);
    }

    public void OnCreditClicked()
    {
        creditPanel.gameObject.SetActive(true);
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }

    public void OnCloseButtonClicked(CanvasGroup panel)
    {
        panel.gameObject.SetActive(false);
    }

    public void OnDoublePlayerCloseButtonClicked()
    {
        try
        {
            Network.singleton.matchMaker.DestroyMatch(Network.singleton.matchInfo.networkId, 0, Network.singleton.OnMatchDestroy);
        }
        catch (System.Exception e) { Debug.Log(e); }
        try
        {
            Network.singleton.StopMatchMaker();
        }
        catch (System.Exception e) { Debug.Log(e); }
    }

    
    public void OnSoundEffectValueChanged(Slider s)
    {
        SoundManager.instance.SEVolume = s.value;
    }

    public void OnBackgroundMusicValueChanged(Slider s)
    {
        SoundManager.instance.BGVolume = s.value;
    }

    public void OnRobotBackClicked()
    {
        switch(GameState.singleton.gameMode)
        {
            case GameMode.Single:
                ShowPanel(mainMenuPanel);
                break;
            case GameMode.Double:
                ShowPanel(mainMenuPanel);
                doublePlayerPanel.gameObject.SetActive(true);
                // TODO: Return to Lobby? Disconnected?
                break;
        }
    }

    public void OnMapBackClicked()
    {
        ShowPanel(selectRobotPanel);
        /*
        switch (GameState.singleton.gameMode)
        {
            case GameMode.Single:
                ShowPanel(selectRobotPanel);
                break;
            case GameMode.Double:
                // TODO: Return to ???
                break;
        }
        */
    }

    public void OnBlackGunClicked()
    {
        GameState.singleton.playerRobot = RobotType.BlackGun;
    }

    public void OnGhostClicked()
    {
        GameState.singleton.playerRobot = RobotType.Ghost;
    }

    public void OnMonsterClicked()
    {
        GameState.singleton.playerRobot = RobotType.Monster;
    }

    public void OnRobotButtonClicked()
    {
        if (GameState.singleton.gameMode == GameMode.Single || GameState.singleton.playerInfo.isServer)
            ShowPanel(selectMapPanel);
        else
        {
            // TODO: Start Game (RPC)
            GameState.singleton.playerInfo.CmdDoublePlayerStart();
            
        }

    }

    public void OnRightClicked()
    {
        GameState.singleton.map++;
        if((int)GameState.singleton.map > (GameState.singleton.maxMapNumber-1))
        {
            GameState.singleton.map = 0;
        }
        ChangeMapButton(GameState.singleton.map);
    }

    public void OnLeftClicked()
    {
        GameState.singleton.map--;
        if ((int)GameState.singleton.map < 0)
        {
            GameState.singleton.map = (Map)(GameState.singleton.maxMapNumber - 1);
        }
        ChangeMapButton(GameState.singleton.map);
    }

    private void ChangeMapButton(Map map)
    {
        foreach(GameObject button in mapButton)
        {
            button.SetActive(false);
        }
        mapButton[(int)map].SetActive(true);
    }

    public void OnMapClicked()
    {
        // GameState.singleton.map = Map.Forest;
        if(GameState.singleton.gameMode == GameMode.Single)
        {
            GameState.singleton.ChangeState(State.EnterMap);
        }
        else
        {
            createGameWaitingPanel.gameObject.SetActive(true);
        }

    }

    public void OnGameRoomCreate(bool success, NetworkID id)
    {
        if (success)
        {
            roomID.transform.parent.GetComponent<InputField>().readOnly = false;
            roomID.transform.parent.GetComponent<InputField>().text = id.ToString();
            roomID.transform.parent.GetComponent<InputField>().readOnly = true;
            doublePlayerPanel.gameObject.SetActive(false);
            ShowPanel(selectRobotPanel);
        }
        else 
        {
            errorPanel.transform.GetChild(1).GetComponent<Text>().text = "Fail to create game room, \nplease check your network connection.";
            errorPanel.gameObject.SetActive(true);
        }
    }

    public void OnGameRoomJoin(bool success)
    {
        if (success)
        {
            doublePlayerPanel.gameObject.SetActive(false);
            ShowPanel(selectRobotPanel);
        }
        else
        {
            errorPanel.transform.GetChild(1).GetComponent<Text>().text = "Fail to join game room, \nplease check your room ID and network connection.";
            errorPanel.gameObject.SetActive(true);
        }


    }
}
