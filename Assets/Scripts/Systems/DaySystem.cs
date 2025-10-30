using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 날짜 시스템 및 난이도 관리
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
    [SerializeField] private float dayDuration = 300f; // 5분
    private float currentDayTime = 0f;

    [Header("Hero Spawning")]
    [SerializeField] private List<GameObject> heroPrefabs;
    [SerializeField] private Transform[] heroSpawnPoints;
    [SerializeField] private AnimationCurve heroCountCurve; // 날짜별 영웅 수
    [SerializeField] private AnimationCurve heroStrengthCurve; // 날짜별 영웅 강도

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

        // 추가 영웅 스폰 (특정 시간마다)
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

        // 랜덤 영웅 선택 (난이도에 따라 가중치 적용 가능)
        GameObject heroPrefab = GetRandomHeroPrefab();

        // 랜덤 스폰 포인트
        Transform spawnPoint = heroSpawnPoints[Random.Range(0, heroSpawnPoints.Length)];

        // 영웅 생성
        GameObject hero = Instantiate(heroPrefab, spawnPoint.position, Quaternion.identity);

        // 난이도에 따른 스탯 조정
        ApplyDifficultyScaling(hero);

        spawnedHeroes.Add(hero);
    }

    private void SpawnAdditionalHero()
    {
        // 중간에 추가 영웅 등장
        SpawnHero(spawnedHeroes.Count);
    }

    private GameObject GetRandomHeroPrefab()
    {
        float difficulty = heroStrengthCurve.Evaluate(currentDay / 30f);

        // 초반에는 약한 영웅, 후반에는 강한 영웅이 더 많이 등장
        if (difficulty < 0.3f)
        {
            // 전사/도적 위주
            return heroPrefabs[Random.Range(0, Mathf.Min(2, heroPrefabs.Count))];
        }
        else if (difficulty < 0.7f)
        {
            // 모든 직업
            return heroPrefabs[Random.Range(0, heroPrefabs.Count)];
        }
        else
        {
            // 강한 직업 위주 (성기사, 마법사)
            return heroPrefabs[Random.Range(Mathf.Max(0, heroPrefabs.Count - 2), heroPrefabs.Count)];
        }
    }

    private void ApplyDifficultyScaling(GameObject hero)
    {
        HeroAI heroAI = hero.GetComponent<HeroAI>();
        if (heroAI == null) return;

        float strengthMultiplier = heroStrengthCurve.Evaluate(currentDay / 30f);

        // 스탯 증가 (리플렉션이나 직접 접근으로)
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
        return GetHeroCountForDay() + 2; // 추가 스폰 여유
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
/// 난이도 설정 ScriptableObject
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
    public float baseDayDuration = 300f; // 5분
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
/// 영웅 파티 생성기 - 특정 조합의 영웅들을 생성
/// </summary>
public class HeroPartySpawner : MonoBehaviour
{
    [System.Serializable]
    public class HeroParty
    {
        public string partyName;
        public List<HeroClass> members;
        public int minDay; // 이 파티가 등장하기 시작하는 날
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

        // 가중치 기반 랜덤 선택
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
/// 이벤트 시스템 - 특별한 날 이벤트
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
        public int repeatInterval = 7; // 7일마다 반복

        public EventType eventType;
        public float modifier = 1.0f;
    }

    public enum EventType
    {
        DoubleLoot,      // 아이템 가치 2배
        ExtraHeroes,     // 영웅 수 증가
        BossDay,         // 보스 영웅 등장
        LuckyDay,        // 좋은 아이템만 등장
        FastDay          // 시간 제한 감소
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