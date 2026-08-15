using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Pages")]
    public GameObject startPage;
    public GameObject selectModePage;
    public GameObject victoryPage;
    public GameObject defeatPage;

    [Header("Start Page")]
    public Button startButton;

    [Header("Select Mode Page")]
    public Button singlePlayerButton;
    public Button twoPlayerButton;

    [Header("End Pages")]
    public Button victoryRetryButton;
    public Button victoryReturnButton;
    public Button defeatRetryButton;
    public Button defeatReturnButton;

    [Header("Victory Page Dynamic")]
    public Text victoryTitleText;

    private void Start()
    {
        HideAllPages();

        if (startButton != null && GameManager.Instance != null)
            startButton.onClick.AddListener(GameManager.Instance.GoToSelectMode);

        if (singlePlayerButton != null && GameManager.Instance != null)
            singlePlayerButton.onClick.AddListener(() => GameManager.Instance.SelectMode(GameMode.SinglePlayer));
        if (twoPlayerButton != null && GameManager.Instance != null)
            twoPlayerButton.onClick.AddListener(() => GameManager.Instance.SelectMode(GameMode.TwoPlayer));

        if (victoryRetryButton != null && GameManager.Instance != null)
            victoryRetryButton.onClick.AddListener(GameManager.Instance.RestartGame);
        if (victoryReturnButton != null && GameManager.Instance != null)
            victoryReturnButton.onClick.AddListener(GameManager.Instance.ReturnToMenu);

        if (defeatRetryButton != null && GameManager.Instance != null)
            defeatRetryButton.onClick.AddListener(GameManager.Instance.RestartGame);
        if (defeatReturnButton != null && GameManager.Instance != null)
            defeatReturnButton.onClick.AddListener(GameManager.Instance.ReturnToMenu);

        if (GameManager.PendingAutoStart)
        {
            GameManager.PendingAutoStart = false;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SelectMode(GameManager.PendingMode);
            }
        }
        else
        {
            ShowStartPage();
        }
    }

    public void ShowStartPage()
    {
        HideAllPages();
        if (startPage != null) startPage.SetActive(true);
    }

    public void ShowSelectModePage()
    {
        HideAllPages();
        if (selectModePage != null) selectModePage.SetActive(true);
    }

    public void ShowVictoryPage(bool player1Won)
    {
        HideAllPages();
        if (victoryPage != null) victoryPage.SetActive(true);
        if (victoryTitleText != null)
        {
            if (GameManager.Instance.CurrentMode == GameMode.TwoPlayer)
            {
                victoryTitleText.text = player1Won ? "玩家1 胜利" : "玩家2 胜利";
                victoryTitleText.color = player1Won ? new Color(0.3f, 0.9f, 1f) : new Color(1f, 0.4f, 0.3f);
            }
            else
            {
                victoryTitleText.text = "胜  利";
                victoryTitleText.color = new Color(0.15f, 0.7f, 0.2f);
            }
        }
    }

    public void ShowDefeatPage()
    {
        HideAllPages();
        if (defeatPage != null) defeatPage.SetActive(true);
    }

    public void HideAllPages()
    {
        if (startPage != null) startPage.SetActive(false);
        if (selectModePage != null) selectModePage.SetActive(false);
        if (victoryPage != null) victoryPage.SetActive(false);
        if (defeatPage != null) defeatPage.SetActive(false);
    }
}
