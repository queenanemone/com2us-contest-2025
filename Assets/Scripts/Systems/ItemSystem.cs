using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ������ ������ ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Monster's Heist/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public string description;
    public Sprite icon;

    [Header("Value")]
    public ItemRarity rarity;
    public int baseValue;
    public float weight = 1f;

    [Header("Gameplay")]
    public bool makesNoise = false;
    public float noiseLevel = 0f;
    public bool isFragile = false;
}

public enum ItemRarity
{
    Common,      // ��� - 100~500���
    Uncommon,    // �ʷϻ� - 500~1500���
    Rare,        // �Ķ��� - 1500~3000���
    Epic,        // ����� - 3000~5000���
    Legendary    // ��Ȳ�� - 5000~10000���
}

/// <summary>
/// �κ��丮 �ý���
/// </summary>
[System.Serializable]
public class InventorySystem
{
    private List<ItemData> items;
    private int maxSize;

    public InventorySystem(int maxSize)
    {
        this.maxSize = maxSize;
        items = new List<ItemData>();
    }

    public bool AddItem(ItemData item)
    {
        if (items.Count >= maxSize)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        items.Add(item);
        Debug.Log($"Added {item.itemName} to inventory");
        return true;
    }

    public bool RemoveItem(ItemData item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            Debug.Log($"Removed {item.itemName} from inventory");
            return true;
        }
        return false;
    }

    public ItemData RemoveItemAt(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            ItemData item = items[index];
            items.RemoveAt(index);
            return item;
        }
        return null;
    }

    public void Clear()
    {
        items.Clear();
    }

    public int GetItemCount()
    {
        return items.Count;
    }

    public int GetMaxSize()
    {
        return maxSize;
    }

    public bool IsFull()
    {
        return items.Count >= maxSize;
    }

    public List<ItemData> GetAllItems()
    {
        return new List<ItemData>(items);
    }

    public int CalculateTotalValue()
    {
        int total = 0;
        foreach (var item in items)
        {
            total += item.baseValue;
        }
        return total;
    }

    public float CalculateTotalWeight()
    {
        float total = 0f;
        foreach (var item in items)
        {
            total += item.weight;
        }
        return total;
    }
}

/// <summary>
/// �ʵ忡 ��ġ�Ǵ� ������ �Ⱦ�
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;

    [Header("Visual")]
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (itemData != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = itemData.icon;

            // ����� ���� ���� ȿ��
            SetRarityColor();
        }
    }

    private void SetRarityColor()
    {
        Color rarityColor = Color.white;

        switch (itemData.rarity)
        {
            case ItemRarity.Common:
                rarityColor = Color.white;
                break;
            case ItemRarity.Uncommon:
                rarityColor = Color.green;
                break;
            case ItemRarity.Rare:
                rarityColor = Color.blue;
                break;
            case ItemRarity.Epic:
                rarityColor = new Color(0.6f, 0.2f, 0.8f); // �����
                break;
            case ItemRarity.Legendary:
                rarityColor = new Color(1f, 0.5f, 0f); // ��Ȳ��
                break;
        }

        // �ƿ������̳� �۷ο� ȿ���� ���
        // spriteRenderer.material.SetColor("_OutlineColor", rarityColor);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // �÷��̾ ������ ���� �� UI ǥ�� ��
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // UI �����
        }
    }
}

/// <summary>
/// ������ ������ - ������ ������ ����
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private List<ItemData> possibleItems;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject itemPickupPrefab;

    [Header("Spawn Rules")]
    [SerializeField] private int minItems = 5;
    [SerializeField] private int maxItems = 15;
    [SerializeField] private AnimationCurve rarityDistribution; // ��¥�� ���� ��� ����

    public void SpawnItems(int dayNumber)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        int itemCount = Random.Range(minItems, maxItems + 1);

        // ��� ������ ���� ����Ʈ ����
        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        ShuffleList(availablePoints);

        for (int i = 0; i < itemCount && i < availablePoints.Count; i++)
        {
            ItemData randomItem = GetRandomItem(dayNumber);
            if (randomItem != null)
            {
                SpawnItem(randomItem, availablePoints[i].position);
            }
        }
    }

    private void SpawnItem(ItemData itemData, Vector3 position)
    {
        GameObject itemObj = Instantiate(itemPickupPrefab, position, Quaternion.identity);
        ItemPickup pickup = itemObj.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            pickup.itemData = itemData;
        }
    }

    private ItemData GetRandomItem(int dayNumber)
    {
        if (possibleItems == null || possibleItems.Count == 0)
        {
            return null;
        }

        // ��¥�� �������� ���� �������� ���� Ȯ�� ����
        float rarityBonus = rarityDistribution.Evaluate(dayNumber / 30f);

        // ����ġ ��� ���� ���� (���� ����)
        return possibleItems[Random.Range(0, possibleItems.Count)];
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}