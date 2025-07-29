using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Params")]
    public int maxHealth = 2500;
    public int attackDamage = 45;
    public float attackRate = 1.2f;
    public float attackRange = 2.5f;
    public float dashAttackRange = 3.5f;
    public bool isFlipped = false;
    public Transform[] attackPoint;
    public Transform dashAttackPoint;
    public LayerMask playerLayers;
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip dashSound;

    public GameObject VictoryUI;
    public GameObject PlayerUI;

    private int currentHealth;
    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 9.81f;
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void LookAtPlayer(Transform player)
    {
        if (player == null) return;
        Vector3 scale = transform.localScale;
        if (transform.position.x < player.position.x)
        {
            scale.x = Mathf.Abs(scale.x);
            isFlipped = false;
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x);
            isFlipped = true;
        }
        transform.localScale = scale;
    }

    public void Attack(Transform player)
    {
        if (attackPoint == null || attackPoint.Length == 0) return;
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        foreach (Transform point in attackPoint)
        {
            if (point == null) continue;
            Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(point.position, attackRange, playerLayers);
            foreach (Collider2D hitPlayer in hitPlayers)
            {
                hitPlayer.GetComponent<PlayerCombat>()?.TakeDamage(attackDamage);
            }
        }
    }

    public void DashAttack(Transform player)
    {
        if (dashAttackPoint == null) return;
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(dashAttackPoint.position, dashAttackRange, playerLayers);
        if (dashSound != null)
        {
            audioSource.PlayOneShot(dashSound);
        }
        foreach (Collider2D hitPlayer in hitPlayers)
        {
            hitPlayer.GetComponent<PlayerCombat>()?.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"Enemy took {damage} damage. Current health: {currentHealth - damage}");
        currentHealth -= damage;
        if (hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        animator.SetTrigger("IsHurt");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        animator.SetBool("IsDead", true);

        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        PlayerUI.SetActive(false);
        VictoryUI.SetActive(true);
        this.enabled = false;
    }

    public void StopTime()
    {
        Time.timeScale = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in attackPoint)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, attackRange);
            }
        }

        if (dashAttackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(dashAttackPoint.position, dashAttackRange);
        }
    }
}
