using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private PlayerController PlayerController;

    [Header("Combat Params")]
    public Transform attackPoint;
    public Transform jumpAttackPoint;
    public float attackRange = 0.5f;
    public float jumpAttackRange = 1.0f;
    public int attackDamage = 20;
    public int jumpAttackDamage = 30;
    public int jumpAttackForce = 10;
    public LayerMask enemyLayers;

    [Header("Health and Stamina")]
    public int maxHealth = 200;
    public int maxStamina = 80;
    private int currentHealth;
    private int currentStamina;
    private float regenTimeStamina = 0f;

    [Header("Attack Settings")]
    public float attackRate = 2f;
    public int maxCombo = 4;
    public int maxComboCrouch = 2;
    public float comboMaxDelay = 0.7f;
    public float postComboDelay = 0.35f;
    private float nextAttackTime = 0f;
    private int currentCombo = 0;
    private float comboTimer = 0f;
    private float postComboTimer = 0f;

    [Header("Sound Effects")]
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip dieSound;

    [Header("UI Elements")]
    public GameObject GameOverUI;
    public GameObject PlayerUI;

    [SerializeField] private PauseMenu pauseMenu;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int MaxStamina => maxStamina;
    public int CurrentStamina => currentStamina;

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
            return;

        comboTimer += Time.deltaTime;
        if (postComboTimer > 0f)
            postComboTimer -= Time.deltaTime;

        bool canAttack = Time.time >= nextAttackTime && postComboTimer <= 0f;

        if (InputManager.GetInstance().GetAttackPressed())
        {
            if (!PlayerController.IsJumping)
            {
                if (!PlayerController.IsCrouching)
                {
                    if (canAttack && currentStamina >= 10)
                    {
                        if (comboTimer > comboMaxDelay)
                        {
                            currentCombo = 0;
                        }
                        currentCombo++;
                        if (currentCombo > maxCombo)
                            currentCombo = 1;

                        animator.SetTrigger("Attack" + currentCombo);
                        currentStamina -= 10;

                        nextAttackTime = Time.time + 1f / attackRate;
                        comboTimer = 0f;

                        if (currentCombo >= maxCombo)
                        {
                            postComboTimer = postComboDelay;
                            nextAttackTime = Time.time + postComboDelay;
                            currentCombo = 0;
                        }
                    }
                }
                else
                {
                    if (canAttack && currentStamina >= 5)
                    {
                        if (comboTimer > comboMaxDelay)
                        {
                            currentCombo = 0;
                        }
                        currentCombo++;
                        if (currentCombo > maxComboCrouch)
                            currentCombo = 1;

                        animator.SetTrigger("CrouchAttack" + currentCombo);
                        currentStamina -= 5;

                        nextAttackTime = Time.time + 1f / attackRate;
                        comboTimer = 0f;

                        if (currentCombo >= maxComboCrouch)
                        {
                            postComboTimer = postComboDelay;
                            nextAttackTime = Time.time + postComboDelay;
                            currentCombo = 0;
                        }
                    }
                }
            }
            else
            {
                if (canAttack && currentStamina >= 25)
                {
                    currentStamina -= 25;
                    nextAttackTime = Time.time + 1f / attackRate;
                    comboTimer = 0f;
                    PlayerController.DoJumpAttackFall();
                    postComboTimer = postComboDelay;
                    nextAttackTime = Time.time + postComboDelay;
                }
            }
        }

        if (comboTimer > comboMaxDelay && currentCombo != 0)
        {
            currentCombo = 0;
            postComboTimer = postComboDelay;
            nextAttackTime = Time.time + postComboDelay;
        }

        if (currentStamina < maxStamina)
            regenTimeStamina += Time.deltaTime;

        if (currentStamina <= maxStamina && regenTimeStamina >= 1f)
        {
            currentStamina += 3;
            regenTimeStamina = 0f;
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
        }
    }

    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound, 0.5f);
        }
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>()?.TakeDamage(attackDamage + currentCombo * 3);
        }
    }

    private void JumpAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(jumpAttackPoint.position, jumpAttackRange, enemyLayers);
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound, 0.5f);
        }
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>()?.TakeDamage(jumpAttackDamage);
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

    public void StopTime()
    {
        Time.timeScale = 0f;
    }
}
