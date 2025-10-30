using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 게임 전체 UI 관리
/// </summary>
public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance {
        get {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();
            }
            return instance;
        }
    }

    [Header("HUD Elements")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider staminaBar;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform inventorySlotContainer;
    [SerializeField] private GameObject inventorySlotPrefab;
    private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject dayEndPanel;
    [SerializeField] private GameObject tutorialPanel;

    [Header("Day End Stats")]
    [SerializeField] private TextMeshProUGUI itemsCollectedText;
    [SerializeField] private TextMeshProUGUI goldEarnedText;
    [SerializeField] private TextMeshProUGUI heroesDefeatedText;

    [Header("Notifications")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationContainer;
    [SerializeField] private float notificationDuration = 3f;

    private PlayerController player;
    private bool isPaused = false;

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
        InitializeUI();

        // 플레이어 찾기
        player = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        UpdateHUD();
        HandleInput();
    }

    private void InitializeUI()
    {
        // 시작 시 모든 패널 숨기기
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayEndPanel != null) dayEndPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    private void HandleInput()
    {
        // 일시정지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // 인벤토리
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    private void UpdateHUD()
    {
        if (player == null) return;

        // 체력바
        if (healthBar != null)
        {
            healthBar.value = player.GetHealthPercent();
        }

        // 스태미나바
        if (staminaBar != null)
        {
            staminaBar.value = player.GetStaminaPercent();
        }

        // 골드
        if (goldText != null)
        {
            goldText.text = $"{GameManager.Instance.gold}G";
        }

        // 날짜
        if (dayText != null)
        {
            dayText.text = $"Day {GameManager.Instance.currentDay}";
        }

        // 타이머
        if (timerText != null && DayManager.Instance != null)
        {
            float remainingTime = DayManager.Instance.GetRemainingTime();
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    public void UpdateInventoryUI(InventorySystem inventory)
    {
        // 인벤토리 슬롯이 없으면 생성
        if (inventorySlots.Count == 0)
        {
            CreateInventorySlots(inventory.GetMaxSize());
        }

        List<ItemData> items = inventory.GetAllItems();

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < items.Count)
            {
                inventorySlots[i].SetItem(items[i]);
            }
            else
            {
                inventorySlots[i].ClearSlot();
            }
        }
    }

    private void CreateInventorySlots(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, inventorySlotContainer);
            InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();
            if (slot != null)
            {
                inventorySlots.Add(slot);
            }
        }
    }

    public void ShowDayEndPanel(int itemsCollected, int goldEarned, int heroesDefeated)
    {
        if (dayEndPanel == null) return;

        dayEndPanel.SetActive(true);

        if (itemsCollectedText != null)
        {
            itemsCollectedText.text = $"Items Collected: {itemsCollected}";
        }

        if (goldEarnedText != null)
        {
            goldEarnedText.text = $"Gold Earned: {goldEarned}G";
        }

        if (heroesDefeatedText != null)
        {
            heroesDefeatedText.text = $"Heroes Defeated: {heroesDefeated}";
        }
    }

    public void HideDayEndPanel()
    {
        if (dayEndPanel != null)
        {
            dayEndPanel.SetActive(false);
        }
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        bool isActive = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!isActive);

        // 인벤토리 열 때 업데이트
        if (!isActive && player != null)
        {
            UpdateInventoryUI(player.GetInventory());
        }
    }

    public void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
        if (notificationPrefab == null || notificationContainer == null) return;

        GameObject notifObj = Instantiate(notificationPrefab, notificationContainer);
        NotificationUI notif = notifObj.GetComponent<NotificationUI>();

        if (notif != null)
        {
            notif.Initialize(message, type, notificationDuration);
        }
    }

    // 버튼 이벤트 핸들러들
    public void OnResumeButtonClicked()
    {
        TogglePause();
    }

    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1f;
        // TODO: 메인 메뉴 씬으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void OnContinueButtonClicked()
    {
        HideDayEndPanel();
        GameManager.Instance.GoToShop();
    }
}

/// <summary>
/// 인벤토리 슬롯 UI
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private GameObject emptySlotIndicator;

    private ItemData currentItem;

    public void SetItem(ItemData item)
    {
        currentItem = item;

        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
        }

        if (valueText != null)
        {
            valueText.text = $"{item.baseValue}G";
            valueText.enabled = true;
        }

        if (rarityBorder != null)
        {
            rarityBorder.color = GetRarityColor(item.rarity);
        }

        if (emptySlotIndicator != null)
        {
            emptySlotIndicator.SetActive(false);
        }
    }

    public void ClearSlot()
    {
        currentItem = null;

        if (itemIcon != null)
        {
            itemIcon.enabled = false;
        }

        if (valueText != null)
        {
            valueText.enabled = false;
        }

        if (emptySlotIndicator != null)
        {
            emptySlotIndicator.SetActive(true);
        }
    }

    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return Color.white;
            case ItemRarity.Uncommon:
                return Color.green;
            case ItemRarity.Rare:
                return Color.blue;
            case ItemRarity.Epic:
                return new Color(0.6f, 0.2f, 0.8f);
            case ItemRarity.Legendary:
                return new Color(1f, 0.5f, 0f);
            default:
                return Color.white;
        }
    }

    public ItemData GetItem()
    {
        return currentItem;
    }
}

/// <summary>
/// 알림 UI
/// </summary>
public class NotificationUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image background;
    [SerializeField] private CanvasGroup canvasGroup;

    private float duration;
    private float timer;

    public void Initialize(string message, NotificationType type, float duration)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        if (background != null)
        {
            background.color = GetTypeColor(type);
        }

        this.duration = duration;
        timer = 0f;

        StartCoroutine(FadeOut());
    }

    private System.Collections.IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(duration * 0.7f);

        float fadeTime = duration * 0.3f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - (elapsed / fadeTime);
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    private Color GetTypeColor(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Success:
                return new Color(0f, 0.8f, 0f, 0.8f);
            case NotificationType.Warning:
                return new Color(1f, 0.6f, 0f, 0.8f);
            case NotificationType.Error:
                return new Color(0.8f, 0f, 0f, 0.8f);
            case NotificationType.Info:
            default:
                return new Color(0.2f, 0.2f, 0.2f, 0.8f);
        }
    }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// 미니맵 시스템
/// </summary>
public class MinimapController : MonoBehaviour
{
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float zoomLevel = 10f;

    private void LateUpdate()
    {
        if (playerTransform != null && minimapCamera != null)
        {
            // 미니맵 카메라가 플레이어를 따라감
            Vector3 newPosition = playerTransform.position;
            newPosition.z = minimapCamera.transform.position.z;
            minimapCamera.transform.position = newPosition;
        }
    }

    public void SetZoom(float zoom)
    {
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = zoom;
        }
    }
}