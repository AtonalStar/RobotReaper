using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDUI : MonoBehaviour {

    [SerializeField]
    private CanvasGroup HUDPanel;
    [SerializeField]
    private CanvasGroup pausePanel;
    [SerializeField]
    private Image healthImage;
    [SerializeField]
    private Image skillImage;
    [SerializeField]
    private Image remainImage;

    [SerializeField]
    private Text healthText;
    [SerializeField]
    private Text skillText;
    [SerializeField]
    private Text remainText;

    [SerializeField]
    private Text pauseText;
    [SerializeField]
    private Button continueButton;
    [SerializeField]
    private Button retryButton;


    [HideInInspector]
    public static HUDUI singleton;

    // Use this for initialization
    void Start () {
        singleton = this;
        DisableAllPanel();
        HUDPanel.gameObject.SetActive(true);
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DisableAllPanel();
            pausePanel.gameObject.SetActive(true);
        }
    }

    public  void UpdateHealth(int health, int maxHealth)
    {
        healthText.text = ""+health;
        healthImage.rectTransform.sizeDelta = new Vector2(200 * ((float)health / maxHealth) ,healthImage.rectTransform.sizeDelta.y);
    }

    public  void UpdateSkill(float skill, float maxSkill)
    {
        if(skill > maxSkill)
        {
            skillText.text = "";
        }
        else
        skillText.text = "" + (int)(maxSkill-skill);
        skillImage.rectTransform.sizeDelta = new Vector2(skillImage.rectTransform.sizeDelta.x, 30 * (skill / maxSkill));
    }

    public void DisableAllPanel()
    {
        HUDPanel.gameObject.SetActive(false);
        pausePanel.gameObject.SetActive(false);
    }

    public void OnPauseButtonClicked()
    {
        DisableAllPanel();
        pausePanel.gameObject.SetActive(true);
    }

    public void OnContinueButtonClicked()
    {
        DisableAllPanel();
        HUDPanel.gameObject.SetActive(true);
    }

    public void OnRetryButtonClicked()
    {
        DisableAllPanel();
        HUDPanel.gameObject.SetActive(true);
        GameState.singleton.ChangeState(State.Play);
        
    }

    public void OnBackButtonClicked()
    {
        Network.singleton.StopClient();
        Network.singleton.StopHost();
        GameState.singleton.ChangeState(State.Menu);
    }

    public void OnGameWin()
    {
        Debug.Log("HUDUI::You Win");
        pauseText.text = "You Win";
        GameState.singleton.isGameStart = false;
        continueButton.interactable = false;
        retryButton.interactable = false;
        OnPauseButtonClicked();
    }

    public void OnGameFail()
    {
        pauseText.text = "You Lose";
        continueButton.interactable = false;
        OnPauseButtonClicked();
    }

}
