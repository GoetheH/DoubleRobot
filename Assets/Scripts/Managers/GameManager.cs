using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Menu,
    SelectMode,
    Playing,
    Victory,
    Defeat
}

public enum GameMode
{
    None,
    SinglePlayer,
    TwoPlayer
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // 静态变量跨场景传递"再来一次"状态，避免 PlayerPrefs 时序问题
    public static bool PendingAutoStart = false;
    public static GameMode PendingMode = GameMode.None;

    [Header("References")]
    public RobotController player;
    public RobotController enemy;
    public UIManager uiManager;

    public GameState CurrentState { get; private set; } = GameState.Menu;
    public GameMode CurrentMode { get; private set; } = GameMode.None;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 由 UIManager.Start() 检查 PendingAutoStart 并调用 SelectMode
    }

    public void RegisterPlayer(RobotController playerRobot)
    {
        player = playerRobot;
    }

    public void RegisterEnemy(RobotController enemyRobot)
    {
        enemy = enemyRobot;
    }

    public void GoToSelectMode()
    {
        CurrentState = GameState.SelectMode;
        if (uiManager != null) uiManager.ShowSelectModePage();
    }

    public void SelectMode(GameMode mode)
    {
        CurrentMode = mode;
        CurrentState = GameState.Playing;
        if (uiManager != null) uiManager.HideAllPages();
    }

    public void OnRobotDeath(RobotController robot)
    {
        if (CurrentState != GameState.Playing) return;

        // 双人模式：robot==enemy表示玩家1赢，robot==player表示玩家2赢
        // 单人模式：robot==enemy表示玩家赢，robot==player表示玩家输
        bool player1Won = (robot == enemy);

        if (CurrentMode == GameMode.TwoPlayer)
        {
            CurrentState = GameState.Victory;
            if (uiManager != null) uiManager.ShowVictoryPage(player1Won);
        }
        else
        {
            if (player1Won)
            {
                CurrentState = GameState.Victory;
                if (uiManager != null) uiManager.ShowVictoryPage(true);
            }
            else
            {
                CurrentState = GameState.Defeat;
                if (uiManager != null) uiManager.ShowDefeatPage();
            }
        }
    }

    public void ReturnToMenu()
    {
        PendingAutoStart = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RestartGame()
    {
        PendingAutoStart = true;
        PendingMode = CurrentMode;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
