using UnityEngine;

public class Enemy_Run : StateMachineBehaviour
{
    public float speed = 2f;
    public int attackRange = 3;
    public int dashRange = 5;
    public float dashForce = 15f;
    public float dashCooldown = 2f;
    public float crouchDashInterval = 1.5f;

    Transform player;
    Rigidbody2D rb;
    Enemy enemy;
    Animator playerAnimator;

    float dashTimer;
    float crouchDashTimer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = animator.GetComponent<Rigidbody2D>();
        enemy = animator.GetComponent<Enemy>();
        playerAnimator = player.GetComponent<Animator>();
        rb.gravityScale = 9.81f;

        dashTimer = dashCooldown;
        crouchDashTimer = 0f;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemy.LookAtPlayer(player);

        Vector2 targetPos = new Vector2(player.position.x, rb.position.y);
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, speed * Time.deltaTime);
        rb.MovePosition(newPos);

        float distance = Vector2.Distance(player.position, rb.position);

        if (dashTimer > 0)
            dashTimer -= Time.deltaTime;
        if (crouchDashTimer > 0)
            crouchDashTimer -= Time.deltaTime;

        AnimatorStateInfo playerStateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        bool playerIsCrouching = playerStateInfo.IsName("Crouch") || playerStateInfo.IsName("CrouchAttack1") || playerStateInfo.IsName("CrouchAttack2");

        if (distance >= dashRange && dashTimer <= 0f)
        {
            animator.SetTrigger("Dash");
            float direction = Mathf.Sign(player.position.x - rb.position.x);
            Vector2 dashDir = new Vector2(direction, 0);
            rb.AddForce(dashDir * dashForce, ForceMode2D.Impulse);
            dashTimer = dashCooldown;
        }
        else if (playerIsCrouching && distance <= attackRange)
        {
            if (crouchDashTimer <= 0f)
            {
                animator.SetTrigger("Dash");
                float direction = Mathf.Sign(player.position.x - rb.position.x);
                Vector2 dashDir = new Vector2(direction, 0);
                rb.AddForce(dashDir * dashForce, ForceMode2D.Impulse);
                crouchDashTimer = crouchDashInterval;
            }
        }
        else if (distance <= attackRange && !playerIsCrouching)
        {
            animator.SetTrigger("Attack" + Random.Range(1, 3).ToString());
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Dash");
    }
}
