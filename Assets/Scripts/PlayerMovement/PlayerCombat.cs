using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    [Header("Combat Params")]
    public Transform attackPoint;
    public Transform jumpAttackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 20;
    public int jumpAttackDamage = 30;
    public int jumpAttackForce = 10;
    public float jumpAttackRange = 1.0f;
    public LayerMask enemyLayers;
    [Header("Health and Stamina")]
    public int maxHealth = 100;
    public int maxStamina = 100;
    private int currentStamina;
    private int currentHealth;
    private float regenTimeStamina = 0f;
    [Header("Attack Settings")]
    public float attackRate = 2f;
    private float nextAttackTime = 0f;
    public int maxCombo = 4;
    private int currentCombo = 0;
    private float comboTimer = 0f;
    public float comboMaxDelay = 0.7f;
    [Header("Sound Effects")]
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip dieSound;
    private AudioSource audioSource;

    public GameObject GameOverUI;
    public GameObject PlayerUI;

    private PlayerController PlayerController;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int MaxStamina => maxStamina;
    public int CurrentStamina => currentStamina;

    [SerializeField] private PauseMenu pauseMenu;

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        PlayerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (pauseMenu != null && pauseMenu.IsPaused)
        {
            return;
        }

        comboTimer += Time.deltaTime;

        if (InputManager.GetInstance().GetAttackPressed())
        {
            if (!PlayerController.IsJumping)
            {
                if (Time.time >= nextAttackTime)
                {
                    if (comboTimer > comboMaxDelay)
                    {
                        currentCombo = 0;
                    }

                    currentCombo++;
                    if (currentCombo > maxCombo)
                        currentCombo = 1;

                    Attack(currentCombo);
                    nextAttackTime = Time.time + 1f / attackRate;
                    comboTimer = 0f;
                }
            }
            else
            {
                if (Time.time >= nextAttackTime)
                {
                    JumpAttack();
                    nextAttackTime = Time.time + 1f / attackRate;
                    comboTimer = 0f;
                }
            }
        }

        if (comboTimer > comboMaxDelay && currentCombo != 0)
        {
            currentCombo = 0;
        }

        if (currentStamina < maxStamina)
        {
            regenTimeStamina += Time.deltaTime;
        }

        if (currentStamina <= maxStamina && regenTimeStamina >= 1f)
        {
            currentStamina++;
            regenTimeStamina = 0f;
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
        }
    }

    private void Attack(int comboStep)
    {
        if (currentStamina < 5)
        {
            Debug.Log("Not enough stamina for attack!");
            return;
        }
        currentStamina -= 5;
        animator.SetTrigger("Attack" + comboStep);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound, 0.5f);
        }
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>()?.TakeDamage(attackDamage);
        }
    }

    private void JumpAttack()
    {
        if (currentStamina < 20)
        {
            Debug.Log("Not enough stamina for jump attack!");
            return;
        }
        currentStamina -= 20;
        animator.SetTrigger("jumpAttack");
        PlayerController.DoJumpAttackFall();

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(jumpAttackPoint.position, jumpAttackRange, enemyLayers);
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound, 0.5f);
        }
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>()?.TakeDamage(jumpAttackDamage);
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 forceDirection = (enemy.transform.position - jumpAttackPoint.position).normalized;
                enemyRb.AddForce(forceDirection * jumpAttackForce, ForceMode2D.Impulse);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        if (jumpAttackPoint == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(jumpAttackPoint.position, jumpAttackRange);
    }

    public void TakeDamage(int Damage)
    {
        if (PlayerController.IsRollingInvulnerable)
        {
            return;
        }
        animator.SetTrigger("Hurt");
        if (hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        currentHealth -= Damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        animator.SetBool("IsDead", true);
        if (dieSound != null)
        {
            audioSource.PlayOneShot(dieSound);
        }
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        PlayerUI.SetActive(false);
        GameOverUI.SetActive(true);
        this.enabled = false;
    }
}
