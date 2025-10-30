using UnityEngine;
using DG.Tweening;

/// <summary>
/// DOTween이 적용된 개선된 플레이어 컨트롤러
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    private Vector2 moveInput;
    private bool isSprinting = false;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    [SerializeField] private float maxStamina = 100f;
    private float currentStamina;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private float staminaDrainRate = 20f;

    [Header("Inventory")]
    [SerializeField] private int maxInventorySize = 5;
    private InventorySystem inventory;

    [Header("Stealth")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private bool isHidden = false;

    [Header("Animation")]
    [SerializeField] private float damageFlinchDuration = 0.2f;
    [SerializeField] private float pickupJumpHeight = 0.3f;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Status
    public bool IsDead { get; private set; }
    public bool IsCarryingItem { get; private set; }

    // DOTween tweeners (조작 가능하도록 저장)
    private Tween currentDamageTween;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        inventory = new InventorySystem(maxInventorySize);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        IsDead = false;
    }

    private void Update()
    {
        if (IsDead) return;

        HandleInput();
        UpdateStamina();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        HandleMovement();
    }

    private void HandleInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        isSprinting = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupItem();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryDropItem();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            TryHide();
        }
    }

    private void HandleMovement()
    {
        float currentSpeed = moveSpeed;

        if (isSprinting)
        {
            currentSpeed *= sprintMultiplier;
        }

        Vector2 velocity = moveInput * currentSpeed;
        rb.linearVelocity = velocity;

        if (moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x < 0;
        }
    }

    private void UpdateStamina()
    {
        if (isSprinting && moveInput.magnitude > 0)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(maxStamina, currentStamina);
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = moveInput.magnitude > 0;
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsSprinting", isSprinting);
        animator.SetBool("IsHidden", isHidden);
    }

    private void TryPickupItem()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var col in colliders)
        {
            ItemPickup item = col.GetComponent<ItemPickup>();
            if (item != null)
            {
                if (inventory.AddItem(item.itemData))
                {
                    // DOTween 픽업 애니메이션
                    AnimateItemPickup(col.transform);

                    // UI 알림
                    UIManager.Instance?.ShowNotification(
                        $"+{item.itemData.baseValue}G {item.itemData.itemName}",
                        NotificationType.Success
                    );

                    Debug.Log($"Picked up: {item.itemData.itemName}");
                    return;
                }
                else
                {
                    // 인벤토리 가득참 효과
                    AnimateInventoryFull();
                    Debug.Log("Inventory is full!");
                }
            }
        }
    }

    /// <summary>
    /// 아이템 픽업 애니메이션 (DOTween)
    /// </summary>
    private void AnimateItemPickup(Transform item)
    {
        Sequence pickupSeq = DOTween.Sequence();

        // 1. 아이템이 위로 약간 튐
        pickupSeq.Append(item.DOMoveY(item.position.y + pickupJumpHeight, 0.15f)
            .SetEase(Ease.OutQuad));

        // 2. 플레이어에게 빨려들어감
        pickupSeq.Append(item.DOMove(transform.position, 0.2f)
            .SetEase(Ease.InBack));

        // 3. 작아지면서 사라짐
        pickupSeq.Join(item.DOScale(0f, 0.2f)
            .SetEase(Ease.InBack));

        // 4. 완료 후 제거
        pickupSeq.OnComplete(() => {
            Destroy(item.gameObject);

            // 플레이어 살짝 튀는 효과
            transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f);
        });
    }

    /// <summary>
    /// 인벤토리 가득참 피드백 애니메이션
    /// </summary>
    private void AnimateInventoryFull()
    {
        // 플레이어가 좌우로 흔들림 (거부 느낌)
        transform.DOShakePosition(0.3f, strength: 0.2f, vibrato: 10);

        // 빨간색 깜빡임
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.DOColor(Color.red, 0.1f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => spriteRenderer.color = originalColor);
        }
    }

    private void TryDropItem()
    {
        if (inventory.GetItemCount() > 0)
        {
            // TODO: 아이템 드롭 로직
            Debug.Log("Item dropped");
        }
    }

    private void TryHide()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var col in colliders)
        {
            if (col.CompareTag("HidingSpot"))
            {
                ToggleHide();
                return;
            }
        }
        Debug.Log("No hiding spot nearby!");
    }

    /// <summary>
    /// 은신 토글 애니메이션 (DOTween)
    /// </summary>
    private void ToggleHide()
    {
        isHidden = !isHidden;

        if (isHidden)
        {
            // 숨을 때: 페이드 아웃 + 작아짐
            spriteRenderer.DOFade(0.3f, 0.3f).SetEase(Ease.OutQuad);
            transform.DOScale(0.7f, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            // 나타날 때: 페이드 인 + 커짐
            spriteRenderer.DOFade(1f, 0.3f).SetEase(Ease.OutQuad);
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        Debug.Log($"Hidden: {isHidden}");
    }

    /// <summary>
    /// 피해 입기 (DOTween 효과 추가)
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // 데미지 애니메이션
        AnimateDamage();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 데미지 받을 때 애니메이션 (DOTween)
    /// </summary>
    private void AnimateDamage()
    {
        // 진행중인 데미지 트윈 취소
        currentDamageTween?.Kill();

        // 짧은 흔들림
        transform.DOShakePosition(damageFlinchDuration, strength: 0.3f, vibrato: 10);

        // 빨간색 깜빡임 (더 드라마틱하게)
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            Sequence damageFlash = DOTween.Sequence();

            damageFlash.Append(spriteRenderer.DOColor(Color.red, 0.08f));
            damageFlash.Append(spriteRenderer.DOColor(originalColor, 0.08f));
            damageFlash.SetLoops(3);

            currentDamageTween = damageFlash;
        }

        // 카메라 쉐이크
        Camera.main?.transform.DOShakePosition(0.15f, strength: 0.2f);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"Player healed {amount}. Health: {currentHealth}/{maxHealth}");

        // 회복 애니메이션
        AnimateHeal();
    }

    /// <summary>
    /// 회복 애니메이션 (DOTween)
    /// </summary>
    private void AnimateHeal()
    {
        // 초록색 깜빡임
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.DOColor(Color.green, 0.2f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => spriteRenderer.color = originalColor);
        }

        // 살짝 커졌다 작아짐
        transform.DOPunchScale(Vector3.one * 0.15f, 0.4f, 5, 0.5f);
    }

    /// <summary>
    /// 사망 애니메이션 (DOTween)
    /// </summary>
    private void Die()
    {
        IsDead = true;
        Debug.Log("Player died!");

        // 사망 애니메이션 시퀀스
        Sequence deathSequence = DOTween.Sequence();

        // 1. 위로 약간 튀어오름
        deathSequence.Append(transform.DOMoveY(transform.position.y + 0.5f, 0.3f)
            .SetEase(Ease.OutQuad));

        // 2. 아래로 떨어지면서 회전
        deathSequence.Append(transform.DOMoveY(transform.position.y - 0.3f, 0.3f)
            .SetEase(Ease.InQuad));
        deathSequence.Join(transform.DORotate(new Vector3(0, 0, 90), 0.3f)
            .SetEase(Ease.InQuad));

        // 3. 페이드 아웃
        deathSequence.Join(spriteRenderer.DOFade(0f, 0.5f)
            .SetDelay(0.1f));

        // 4. 게임 오버 처리
        deathSequence.OnComplete(() => {
            GameManager.Instance.ChangeState(GameState.GameOver);
        });
    }

    public InventorySystem GetInventory()
    {
        return inventory;
    }

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }

    public bool IsDetectable()
    {
        return !isHidden;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }

    /// <summary>
    /// 씬 전환 시 DOTween 정리
    /// </summary>
    private void OnDestroy()
    {
        // 이 오브젝트의 모든 트윈 정리
        DOTween.Kill(transform);
        if (spriteRenderer != null)
        {
            DOTween.Kill(spriteRenderer);
        }
    }
}