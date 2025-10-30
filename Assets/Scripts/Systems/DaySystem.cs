using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ��¥ �ý��� �� ���̵� ����
/// </summary>
public class DayManager : MonoBehaviour
{
    private static DayManager instance;
    public static DayManager Instance {
        get {
            if (instance == null)
            {
                instance = FindObjectOfType<DayManager>();
            }
            return instance;
        }
    }

    [Header("Day Settings")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private float dayDuration = 300f; // 5��
    private float currentDayTime = 0f;

    [Header("Hero Spawning")]
    [SerializeField] private List<GameObject> heroPrefabs;
    [SerializeField] private Transform[] heroSpawnPoints;
    [SerializeField] private AnimationCurve heroCountCurve; // ��¥�� ���� ��
    [SerializeField] private AnimationCurve heroStrengthCurve; // ��¥�� ���� ����

    [Header("Difficulty Scaling")]
    [SerializeField] private DifficultyConfig difficultyConfig;

    private List<GameObject> spawnedHeroes = new List<GameObject>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void StartNewDay(int day)
    {
        currentDay = day;
        currentDayTime = 0f;

        Debug.Log($"=== Day {currentDay} Started ===");

        SpawnHeroesForDay();
    }

    public void UpdateDay(float deltaTime)
    {
        currentDayTime += deltaTime;

        // �߰� ���� ���� (Ư�� �ð�����)
        if (currentDayTime > dayDuration * 0.5f && spawnedHeroes.Count < GetMaxHeroesForDay())
        {
            SpawnAdditionalHero();
        }
    }

    public bool IsDayOver()
    {
        return currentDayTime >= dayDuration;
    }

    public float GetDayProgress()
    {
        return Mathf.Clamp01(currentDayTime / dayDuration);
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0, dayDuration - currentDayTime);
    }

    private void SpawnHeroesForDay()
    {
        ClearExistingHeroes();

        int heroCount = GetHeroCountForDay();

        for (int i = 0; i < heroCount; i++)
        {
            SpawnHero(i);
        }

        Debug.Log($"Spawned {heroCount} heroes for Day {currentDay}");
    }

    private void SpawnHero(int index)
    {
        if (heroPrefabs == null || heroPrefabs.Count == 0)
        {
            Debug.LogError("No hero prefabs assigned!");
            return;
        }

        if (heroSpawnPoints == null || heroSpawnPoints.Length == 0)
        {
            Debug.LogError("No hero spawn points assigned!");
            return;
        }

        // ���� ���� ���� (���̵��� ���� ����ġ ���� ����)
        GameObject heroPrefab = GetRandomHeroPrefab();

        // ���� ���� ����Ʈ
        Transform spawnPoint = heroSpawnPoints[Random.Range(0, heroSpawnPoints.Length)];

        // ���� ����
        GameObject hero = Instantiate(heroPrefab, spawnPoint.position, Quaternion.identity);

        // ���̵��� ���� ���� ����
        ApplyDifficultyScaling(hero);

        spawnedHeroes.Add(hero);
    }

    private void SpawnAdditionalHero()
    {
        // �߰��� �߰� ���� ����
        SpawnHero(spawnedHeroes.Count);
    }

    private GameObject GetRandomHeroPrefab()
    {
        float difficulty = heroStrengthCurve.Evaluate(currentDay / 30f);

        // �ʹݿ��� ���� ����, �Ĺݿ��� ���� ������ �� ���� ����
        if (difficulty < 0.3f)
        {
            // ����/���� ����
            return heroPrefabs[Random.Range(0, Mathf.Min(2, heroPrefabs.Count))];
        }
        else if (difficulty < 0.7f)
        {
            // ��� ����
            return heroPrefabs[Random.Range(0, heroPrefabs.Count)];
        }
        else
        {
            // ���� ���� ���� (�����, ������)
            return heroPrefabs[Random.Range(Mathf.Max(0, heroPrefabs.Count - 2), heroPrefabs.Count)];
        }
    }

    private void ApplyDifficultyScaling(GameObject hero)
    {
        HeroAI heroAI = hero.GetComponent<HeroAI>();
        if (heroAI == null) return;

        float strengthMultiplier = heroStrengthCurve.Evaluate(currentDay / 30f);

        // ���� ���� (���÷����̳� ���� ��������)
        // heroAI.maxHealth *= (1f + strengthMultiplier);
        // heroAI.attackDamage *= (1f + strengthMultiplier * 0.5f);
        // heroAI.moveSpeed *= (1f + strengthMultiplier * 0.2f);

        Debug.Log($"Hero scaled with multiplier: {strengthMultiplier:F2}");
    }

    private int GetHeroCountForDay()
    {
        float normalizedDay = currentDay / 30f;
        int baseCount = Mathf.RoundToInt(heroCountCurve.Evaluate(normalizedDay));
        return Mathf.Clamp(baseCount, difficultyConfig.minHeroes, difficultyConfig.maxHeroes);
    }

    private int GetMaxHeroesForDay()
    {
        return GetHeroCountForDay() + 2; // �߰� ���� ����
    }

    private void ClearExistingHeroes()
    {
        foreach (var hero in spawnedHeroes)
        {
            if (hero != null)
            {
                Destroy(hero);
            }
        }
        spawnedHeroes.Clear();
    }

    public DifficultyStats GetCurrentDifficultyStats()
    {
        DifficultyStats stats = new DifficultyStats
        {
            day = currentDay,
            heroCount = GetHeroCountForDay(),
            heroStrength = heroStrengthCurve.Evaluate(currentDay / 30f),
            itemQuality = difficultyConfig.itemQualityCurve.Evaluate(currentDay / 30f),
            timeLimit = dayDuration
        };
        return stats;
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }
}

/// <summary>
/// ���̵� ���� ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Monster's Heist/Difficulty Config")]
public class DifficultyConfig : ScriptableObject
{
    [Header("Hero Spawning")]
    public int minHeroes = 1;
    public int maxHeroes = 10;
    public AnimationCurve heroCountByDay;
    public AnimationCurve heroStrengthByDay;

    [Header("Item Spawning")]
    public int minItems = 5;
    public int maxItems = 20;
    public AnimationCurve itemQualityCurve;

    [Header("Time Limits")]
    public float baseDayDuration = 300f; // 5��
    public bool decreaseTimeOverDays = false;

    [Header("Rewards")]
    public AnimationCurve goldMultiplierByDay;

    public float GetGoldMultiplier(int day)
    {
        return goldMultiplierByDay.Evaluate(day / 30f);
    }
}

[System.Serializable]
public struct DifficultyStats
{
    public int day;
    public int heroCount;
    public float heroStrength;
    public float itemQuality;
    public float timeLimit;
}

/// <summary>
/// ���� ��Ƽ ������ - Ư�� ������ �������� ����
/// </summary>
public class HeroPartySpawner : MonoBehaviour
{
    [System.Serializable]
    public class HeroParty
    {
        public string partyName;
        public List<HeroClass> members;
        public int minDay; // �� ��Ƽ�� �����ϱ� �����ϴ� ��
        public float spawnWeight = 1f;
    }

    [SerializeField] private List<HeroParty> predefinedParties;

    public HeroParty GetRandomPartyForDay(int day)
    {
        List<HeroParty> availableParties = new List<HeroParty>();

        foreach (var party in predefinedParties)
        {
            if (day >= party.minDay)
            {
                availableParties.Add(party);
            }
        }

        if (availableParties.Count == 0)
        {
            return null;
        }

        // ����ġ ��� ���� ����
        float totalWeight = 0f;
        foreach (var party in availableParties)
        {
            totalWeight += party.spawnWeight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var party in availableParties)
        {
            currentWeight += party.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return party;
            }
        }

        return availableParties[availableParties.Count - 1];
    }
}

/// <summary>
/// �̺�Ʈ �ý��� - Ư���� �� �̺�Ʈ
/// </summary>
public class DayEventSystem : MonoBehaviour
{
    [System.Serializable]
    public class DayEvent
    {
        public string eventName;
        public string description;
        public int triggerDay;
        public bool isRepeating = false;
        public int repeatInterval = 7; // 7�ϸ��� �ݺ�

        public EventType eventType;
        public float modifier = 1.0f;
    }

    public enum EventType
    {
        DoubleLoot,      // ������ ��ġ 2��
        ExtraHeroes,     // ���� �� ����
        BossDay,         // ���� ���� ����
        LuckyDay,        // ���� �����۸� ����
        FastDay          // �ð� ���� ����
    }

    [SerializeField] private List<DayEvent> scheduledEvents;

    public DayEvent GetEventForDay(int day)
    {
        foreach (var evt in scheduledEvents)
        {
            if (evt.isRepeating)
            {
                if ((day - evt.triggerDay) % evt.repeatInterval == 0 && day >= evt.triggerDay)
                {
                    return evt;
                }
            }
            else
            {
                if (day == evt.triggerDay)
                {
                    return evt;
                }
            }
        }
        return null;
    }

    public bool HasEventOnDay(int day)
    {
        return GetEventForDay(day) != null;
    }
}