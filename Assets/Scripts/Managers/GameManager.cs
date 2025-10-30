using UnityEngine;

/// <summary>
/// ���� ��ü �帧�� �����ϴ� �̱��� �Ŵ���
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
    public float dayDuration = 300f; // 5��
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
                // ���� ����
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
        // ��� ȭ�� ǥ��
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
        // ���� ���� �� �̺�Ʈ ó��
        switch (newState)
        {
            case GameState.MainMenu:
                // ���� �޴� UI ǥ��
                break;
            case GameState.InShop:
                // ���� UI ǥ��
                break;
            case GameState.InDungeon:
                // ���� �� �ε�
                break;
            case GameState.DayEnd:
                // ��� ȭ�� ǥ��
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