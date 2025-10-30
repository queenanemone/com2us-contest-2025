using UnityEngine;
using System.Collections;

/// <summary>
/// ���� ĳ���� AI
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class HeroAI : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private HeroClass heroClass;
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;
    private float lastAttackTime = 0f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chaseSpeed = 5f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float fieldOfViewAngle = 120f;
    [SerializeField] private LayerMask detectionLayers;

    [Header("Behavior")]
    [SerializeField] private HeroState currentState = HeroState.Patrol;
    [SerializeField] private Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    private Transform target; // ���� ��� (�÷��̾�)

    [Header("Loot Collection")]
    [SerializeField] private float lootCollectionRange = 2f;
    private bool isCarryingLoot = false;
    private ItemPickup targetLoot;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        IsDead = false;

        // ���� ��Ʈ�� ����Ʈ���� ����
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentPatrolIndex = Random.Range(0, patrolPoints.Length);
        }
    }

    private void Update()
    {
        if (IsDead) return;

        UpdateBehavior();
        DetectPlayer();
        DetectLoot();
    }

    private void UpdateBehavior()
    {
        switch (currentState)
        {
            case HeroState.Patrol:
                Patrol();
                break;
            case HeroState.Chase:
                ChaseTarget();
                break;
            case HeroState.Attack:
                AttackTarget();
                break;
            case HeroState.CollectLoot:
                CollectLoot();
                break;
            case HeroState.Retreat:
                Retreat();
                break;
        }
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            // ��Ʈ�� ����Ʈ�� ������ ���ڸ���
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // ��Ʈ�� ����Ʈ ���� üũ
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

            // ��� ���
            StartCoroutine(PauseAtPatrolPoint());
        }

        UpdateSpriteDirection(direction);
    }

    private IEnumerator PauseAtPatrolPoint()
    {
        HeroState previousState = currentState;
        currentState = HeroState.Idle;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(Random.Range(1f, 3f));

        if (currentState == HeroState.Idle)
        {
            currentState = previousState;
        }
    }

    private void ChaseTarget()
    {
        if (target == null)
        {
            currentState = HeroState.Patrol;
            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);

        // ���� ���� �ȿ� ������ ���� ���·�
        if (distance <= attackRange)
        {
            currentState = HeroState.Attack;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // �÷��̾� ����
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * chaseSpeed;

        UpdateSpriteDirection(direction);

        // �÷��̾ �������� ��Ʈ�ѷ� ����
        if (distance > detectionRange * 1.5f)
        {
            target = null;
            currentState = HeroState.Patrol;
            Debug.Log($"{heroClass} lost sight of player");
        }
    }

    private void AttackTarget()
    {
        if (target == null)
        {
            currentState = HeroState.Patrol;
            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);

        // ���� ������ ����� �ٽ� ����
        if (distance > attackRange)
        {
            currentState = HeroState.Chase;
            return;
        }

        // ���� ��ٿ� üũ
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }

        // �÷��̾� ���� �ٶ󺸱�
        Vector2 direction = (target.position - transform.position).normalized;
        UpdateSpriteDirection(direction);
    }

    private void PerformAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // �÷��̾�� ������
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(attackDamage);
            Debug.Log($"{heroClass} attacked player for {attackDamage} damage");
        }
    }

    private void CollectLoot()
    {
        if (targetLoot == null)
        {
            currentState = HeroState.Patrol;
            return;
        }

        // ��Ʈ�� �̵�
        Vector2 direction = (targetLoot.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, targetLoot.transform.position);

        if (distance <= lootCollectionRange)
        {
            // ��Ʈ ȹ��
            Destroy(targetLoot.gameObject);
            isCarryingLoot = true;
            targetLoot = null;

            // ���� �ⱸ�� �̵� (TODO: �ⱸ �ý��� ���� ��)
            currentState = HeroState.Retreat;
            Debug.Log($"{heroClass} collected loot!");
        }
        else
        {
            rb.linearVelocity = direction * moveSpeed;
        }

        UpdateSpriteDirection(direction);
    }

    private void Retreat()
    {
        // TODO: ���� �ⱸ�� �̵��ϴ� ����
        // ����� �����ϰ� ��Ʈ�ѷ� ����
        currentState = HeroState.Patrol;
    }

    private void DetectPlayer()
    {
        if (currentState == HeroState.Attack || currentState == HeroState.Chase)
        {
            return; // �̹� �÷��̾ ������
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, detectionLayers);

        foreach (var col in colliders)
        {
            PlayerController player = col.GetComponent<PlayerController>();
            if (player != null && !player.IsDead)
            {
                // �÷��̾ ���������� ���� �Ұ�
                if (!player.IsDetectable())
                {
                    continue;
                }

                // �þ߰� üũ
                Vector2 directionToPlayer = (col.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(GetForwardDirection(), directionToPlayer);

                if (angle <= fieldOfViewAngle / 2f)
                {
                    // �÷��̾� �߰�!
                    target = col.transform;
                    currentState = HeroState.Chase;
                    Debug.Log($"{heroClass} detected player!");
                    break;
                }
            }
        }
    }

    private void DetectLoot()
    {
        // �̹� ��Ʈ�� ��� �ְų� �÷��̾ �������̸� ��Ʈ ���� ����
        if (isCarryingLoot || currentState == HeroState.Chase || currentState == HeroState.Attack)
        {
            return;
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        foreach (var col in colliders)
        {
            ItemPickup item = col.GetComponent<ItemPickup>();
            if (item != null)
            {
                targetLoot = item;
                currentState = HeroState.CollectLoot;
                Debug.Log($"{heroClass} spotted loot!");
                break;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;
        Debug.Log($"{heroClass} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        currentState = HeroState.Dead;
        Debug.Log($"{heroClass} has been defeated!");

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        rb.linearVelocity = Vector2.zero;

        // ���� �ð� �� ����
        Destroy(gameObject, 2f);
    }

    private Vector2 GetForwardDirection()
    {
        // ��������Ʈ�� �ٶ󺸴� ���� ��ȯ
        return spriteRenderer.flipX ? Vector2.left : Vector2.right;
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // ���� ����
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // ���� ����
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // �þ߰�
        Vector3 forward = GetForwardDirection();
        Vector3 leftBoundary = Quaternion.Euler(0, 0, fieldOfViewAngle / 2f) * forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -fieldOfViewAngle / 2f) * forward * detectionRange;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}

public enum HeroClass
{
    Warrior,    // ���� - ���� ü��, ���� �̵�
    Rogue,      // ���� - ���� �̵�, ���� ü��
    Mage,       // ������ - ���Ÿ� ����, ���� ü��
    Paladin     // ����� - �������� ����, ȸ�� ����
}

public enum HeroState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    CollectLoot,
    Retreat,
    Dead
}