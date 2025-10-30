using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ���� �ý��� �Ŵ���
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
    [SerializeField] private string merchantName = "�Ͻ��� ����";
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
        // ��� ���׷��̵� Ÿ���� 0���� �ʱ�ȭ
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            purchasedUpgrades[type] = 0;
        }
    }

    public bool PurchaseUpgrade(ShopUpgrade upgrade)
    {
        int currentLevel = GetUpgradeLevel(upgrade.upgradeType);

        // �ִ� ���� üũ
        if (currentLevel >= upgrade.maxLevel)
        {
            Debug.Log($"{upgrade.upgradeName} is already at max level!");
            return false;
        }

        int cost = upgrade.GetCostForLevel(currentLevel);

        // ��� üũ
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
        // ���׷��̵� ȿ�� ����
        // ���� ���ӿ����� PlayerController�� �ٸ� �ý��ۿ� ����
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
            return "�����, ��� ģ��.";
        }
        return merchantDialogues[Random.Range(0, merchantDialogues.Count)];
    }
}

/// <summary>
/// ���� ���׷��̵� ������
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
                return $"�ִ� ü�� +{value}";
            case UpgradeType.MoveSpeed:
                return $"�̵� �ӵ� +{value}%";
            case UpgradeType.StaminaRegen:
                return $"���¹̳� ȸ�� +{value}/��";
            case UpgradeType.InventorySize:
                return $"�κ��丮 ���� +{(int)value}";
            case UpgradeType.StealthBonus:
                return $"���� ȿ�� +{value}%";
            case UpgradeType.NoiseReduction:
                return $"���� ���� -{value}%";
            case UpgradeType.LuckBonus:
                return $"������ ��ġ +{value}%";
            default:
                return description;
        }
    }
}

public enum UpgradeType
{
    MaxHealth,        // �ִ� ü�� ����
    MoveSpeed,        // �̵� �ӵ� ����
    StaminaRegen,     // ���¹̳� ȸ�� �ӵ�
    InventorySize,    // �κ��丮 ũ��
    StealthBonus,     // ���� ȿ��
    NoiseReduction,   // ���� ����
    LuckBonus,        // ������ ��ġ ����
    DamageReduction   // �޴� ������ ����
}

/// <summary>
/// ���׷��̵� ȿ�� ���� ����
/// </summary>
public static class UpgradeEffects
{
    public static float GetHealthMultiplier()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.MaxHealth);
        return 1f + (level * 0.1f); // ������ 10% ����
    }

    public static float GetSpeedMultiplier()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.MoveSpeed);
        return 1f + (level * 0.05f); // ������ 5% ����
    }

    public static float GetStaminaRegenBonus()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.StaminaRegen);
        return level * 2f; // ������ +2
    }

    public static int GetInventorySizeBonus()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.InventorySize);
        return level * 2; // ������ +2 ����
    }

    public static float GetStealthMultiplier()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.StealthBonus);
        return 1f + (level * 0.1f); // ������ 10% ����
    }

    public static float GetNoiseReduction()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.NoiseReduction);
        return level * 0.15f; // ������ 15% ����
    }

    public static float GetLuckMultiplier()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.LuckBonus);
        return 1f + (level * 0.1f); // ������ 10% ����
    }

    public static float GetDamageReduction()
    {
        int level = ShopManager.Instance.GetUpgradeLevel(UpgradeType.DamageReduction);
        return level * 0.05f; // ������ 5% ����
    }
}

/// <summary>
/// ���� UI ��Ʈ�ѷ�
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
        // UI ������Ʈ
        if (goldText != null)
        {
            goldText.text = $"Gold: {GameManager.Instance.gold}";
        }

        if (dayText != null)
        {
            dayText.text = $"Day {GameManager.Instance.currentDay}";
        }

        // ���׷��̵� ��� ����
        RefreshUpgradeList();
    }

    private void RefreshUpgradeList()
    {
        // ���� ��ư ����
        foreach (Transform child in upgradeListContainer)
        {
            Destroy(child.gameObject);
        }

        // ���׷��̵� ��ư ����
        List<ShopUpgrade> upgrades = ShopManager.Instance.GetAvailableUpgrades();
        foreach (var upgrade in upgrades)
        {
            CreateUpgradeButton(upgrade);
        }
    }

    private void CreateUpgradeButton(ShopUpgrade upgrade)
    {
        GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeListContainer);

        // TODO: ��ư UI ����
        // - ���׷��̵� �̸�
        // - ���� ���� / �ִ� ����
        // - ���
        // - ����
        // - ���� ��ư
    }

    public void OnStartDayButtonClicked()
    {
        CloseShop();
        GameManager.Instance.StartDay();
    }

    public void OnSellAllItemsButtonClicked()
    {
        // TODO: �÷��̾� �κ��丮 ����
        // ShopManager.Instance.SellItems(playerInventory);
        RefreshShop();
    }
}