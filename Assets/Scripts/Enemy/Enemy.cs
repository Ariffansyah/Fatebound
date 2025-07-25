using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Params")]
    public int maxHealth = 100;
    public int attackDamage = 40;
    public float attackRate = 1.5f;
    public float attackRange = 0.5f;

    public GameObject VictoryUI;
    public GameObject PlayerUI;

    private int currentHealth;
    private Rigidbody2D rb;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"Enemy took {damage} damage. Current health: {currentHealth - damage}");
        currentHealth -= damage;
        animator.SetTrigger("Hurt");
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
}
