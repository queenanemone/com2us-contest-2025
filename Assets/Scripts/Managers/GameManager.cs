using UnityEngine;

/// <summary>
/// 게임 전체 흐름를 관리하는 싱글톤 매니저
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance {
        get {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    [Header("Game State")]
    public GameState currentState = GameState.MainMenu;

    [Header("Day System")]
    public int currentDay = 1;
    public float dayDuration = 300f; // 5분
    private float dayTimer = 0f;

    [Header("Player Data")]
    public int gold = 0;
    public int totalItemsStolen = 0;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeGame();
    }

    private void Update()
    {
        switch (currentState)
        {
            case GameState.InDungeon:
                UpdateDungeonTimer();
                break;
            case GameState.InShop:
                // 상점 로직
                break;
        }
    }

    private void InitializeGame()
    {
        currentDay = 1;
        gold = 0;
        ChangeState(GameState.MainMenu);
    }

    private void UpdateDungeonTimer()
    {
        dayTimer += Time.deltaTime;
        if (dayTimer >= dayDuration)
        {
            EndDay();
        }
    }

    public void StartDay()
    {
        dayTimer = 0f;
        ChangeState(GameState.InDungeon);
        Debug.Log($"Day {currentDay} started!");
    }

    public void EndDay()
    {
        ChangeState(GameState.DayEnd);
        Debug.Log($"Day {currentDay} ended!");
        // 결과 화면 표시
    }

    public void GoToShop()
    {
        currentDay++;
        ChangeState(GameState.InShop);
        Debug.Log("Entering shop...");
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        OnStateChanged(newState);
    }

    private void OnStateChanged(GameState newState)
    {
        // 상태 변경 시 이벤트 처리
        switch (newState)
        {
            case GameState.MainMenu:
                // 메인 메뉴 UI 표시
                break;
            case GameState.InShop:
                // 상점 UI 표시
                break;
            case GameState.InDungeon:
                // 던전 씬 로드
                break;
            case GameState.DayEnd:
                // 결과 화면 표시
                break;
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"Gold added: {amount}. Total: {gold}");
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            Debug.Log($"Gold spent: {amount}. Remaining: {gold}");
            return true;
        }
        Debug.Log("Not enough gold!");
        return false;
    }
}

public enum GameState
{
    MainMenu,
    InShop,
    InDungeon,
    DayEnd,
    GameOver
}