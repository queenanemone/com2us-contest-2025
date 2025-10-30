using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 상점 시스템 매니저
/// </summary>
public class ShopManager : MonoBehaviour
{
    private static ShopManager instance;
    public static ShopManager Instance {
        get {
            if (instance == null)
            {
                instance = FindObjectOfType<ShopManager>();
            }
            return instance;
        }
    }

    [Header("Shop Items")]
    [SerializeField] private List<ShopUpgrade> availableUpgrades;

    [Header("Merchant")]
    [SerializeField] private string merchantName = "암시장 상인";
    [SerializeField] private List<string> merchantDialogues;

    private Dictionary<UpgradeType, int> purchasedUpgrades = new Dictionary<UpgradeType, int>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        InitializeUpgrades();
    }

    private void InitializeUpgrades()
    {
        // 모든 업그레이드 타입을 0으로 초기화
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            purchasedUpgrades[type] = 0;
        }
    }

    public bool PurchaseUpgrade(ShopUpgrade upgrade)
    {
        int currentLevel = GetUpgradeLevel(upgrade.upgradeType);

        // 최대 레벨 체크
        if (currentLevel >= upgrade.maxLevel)
        {
            Debug.Log($"{upgrade.upgradeName} is already at max level!");
            return false;
        }

        int cost = upgrade.GetCostForLevel(currentLevel);

        // 골드 체크
        if (GameManager.Instance.SpendGold(cost))
        {
            purchasedUpgrades[upgrade.upgradeType]++;
            Debug.Log($"Purchased {upgrade.upgradeName} Level {currentLevel + 1}!");

            ApplyUpgrade(upgrade, currentLevel + 1);
            return true;
        }
        else
        {
            Debug.Log("Not enough gold!");
            return false;
        }
    }

    private void ApplyUpgrade(ShopUpgrade upgrade, int level)
    {
        // 업그레이드 효과 적용
        // 실제 게임에서는 PlayerController나 다른 시스템에 적용
        Debug.Log($"Applied {upgrade.upgradeName} Level {level}: {upgrade.GetDescription(level)}");
    }

    public int GetUpgradeLevel(UpgradeType type)
    {
        if (purchasedUpgrades.ContainsKey(type))
        {
            return purchasedUpgrades[type];
        }
        return 0;
    }

    public List<ShopUpgrade> GetAvailableUpgrades()
    {
        return new List<ShopUpgrade>(availableUpgrades);
    }

    public int SellItems(InventorySystem inventory)
    {
        int totalValue = inventory.CalculateTotalValue();
        inventory.Clear();

        GameManager.Instance.AddGold(totalValue);
        Debug.Log($"Sold items for {totalValue} gold!");

        return totalValue;
    }

    public string GetRandomMerchantDialogue()
    {
        if (merchantDialogues == null || merchantDialogues.Count == 0)
        {
            return "어서오게, 고블린 친구.";
        }
        return merchantDialogues[Random.Range(0, merchantDialogues.Count)];
    }
}

/// <summary>
/// 상점 업그레이드 데이터
/// </summary>
[System.Serializable]
public class ShopUpgrade
{
    public string upgradeName;
    public string description;
    public UpgradeType upgradeType;
    public Sprite icon;

    [Header("Cost")]
    public int baseCost;
    public float costMultiplier = 1.5f;
    public int maxLevel = 5;

    [Header("Effect")]
    public float baseValue;
    public float valuePerLevel;

    public int GetCostForLevel(int currentLevel)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }

    public float GetValueForLevel(int level)
    {
        return baseValue + (valuePerLevel * level);
    }

    public string GetDescription(int level)
    {
        float value = GetValueForLevel(level);

        switch (upgradeType)
        {
            case UpgradeType.MaxHealth:
                return $"최대 체력 +{value}";
            case UpgradeType.MoveSpeed:
                return $"이동 속도 +{value}%";
            case UpgradeType.StaminaRegen:
                return $"스태미나 회복 +{value}/초";
            case UpgradeType.InventorySize:
                return $"인벤토리 슬롯 +{(int)value}";
            case UpgradeType.StealthBonus:
                return $"은신 효과 +{value}%";
            case UpgradeType.NoiseReduction:
                return $"소음 감소 -{value}%";
            case UpgradeType.LuckBonus:
                return $"아이템 가치 +{value}%";
            default:
                return description;
        }
    }
}

public enum UpgradeType
{
    MaxHealth,        // 최대 체력 증가
    MoveSpeed,        // 이동 속도 증가
    StaminaRegen,     // 스태미나 회복 속도
    InventorySize,    // 인벤토리 크기
    StealthBonus,     // 은신 효과
    NoiseReduction,   // 소음 감소
    LuckBonus,        // 아이템 가치 증가
    DamageReduction   // 받는 데미지 감소
}

/// <summary>
/// 업그레이드 효과 적용 헬퍼
/// </summary>
public static class UpgradeEffects
{
    public static float GetHealthMultiplier()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.MaxHealth);
        return 1f + (level * 0.1f); // 레벨당 10% 증가
    }

    public static float GetSpeedMultiplier()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.MoveSpeed);
        return 1f + (level * 0.05f); // 레벨당 5% 증가
    }

    public static float GetStaminaRegenBonus()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.StaminaRegen);
        return level * 2f; // 레벨당 +2
    }

    public static int GetInventorySizeBonus()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.InventorySize);
        return level * 2; // 레벨당 +2 슬롯
    }

    public static float GetStealthMultiplier()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.StealthBonus);
        return 1f + (level * 0.1f); // 레벨당 10% 증가
    }

    public static float GetNoiseReduction()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.NoiseReduction);
        return level * 0.15f; // 레벨당 15% 감소
    }

    public static float GetLuckMultiplier()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.LuckBonus);
        return 1f + (level * 0.1f); // 레벨당 10% 증가
    }

    public static float GetDamageReduction()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.DamageReduction);
        return level * 0.05f; // 레벨당 5% 감소
    }
}

/// <summary>
/// 상점 UI 컨트롤러
/// </summary>
public class ShopUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform upgradeListContainer;
    [SerializeField] private GameObject upgradeButtonPrefab;

    [Header("Player Info")]
    [SerializeField] private TMPro.TextMeshProUGUI goldText;
    [SerializeField] private TMPro.TextMeshProUGUI dayText;

    private void Start()
    {
        RefreshShop();
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        RefreshShop();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    private void RefreshShop()
    {
        // UI 업데이트
        if (goldText != null)
        {
            goldText.text = $"Gold: {GameManager.Instance.gold}";
        }

        if (dayText != null)
        {
            dayText.text = $"Day {GameManager.Instance.currentDay}";
        }

        // 업그레이드 목록 갱신
        RefreshUpgradeList();
    }

    private void RefreshUpgradeList()
    {
        // 기존 버튼 제거
        foreach (Transform child in upgradeListContainer)
        {
            Destroy(child.gameObject);
        }

        // 업그레이드 버튼 생성
        List<ShopUpgrade> upgrades = ShopManager.Instance.GetAvailableUpgrades();
        foreach (var upgrade in upgrades)
        {
            CreateUpgradeButton(upgrade);
        }
    }

    private void CreateUpgradeButton(ShopUpgrade upgrade)
    {
        GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeListContainer);

        // TODO: 버튼 UI 설정
        // - 업그레이드 이름
        // - 현재 레벨 / 최대 레벨
        // - 비용
        // - 설명
        // - 구매 버튼
    }

    public void OnStartDayButtonClicked()
    {
        CloseShop();
        GameManager.Instance.StartDay();
    }

    public void OnSellAllItemsButtonClicked()
    {
        // TODO: 플레이어 인벤토리 접근
        // ShopManager.Instance.SellItems(playerInventory);
        RefreshShop();
    }
}