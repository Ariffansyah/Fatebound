using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Params")]
    public float runSpeed = 6.0f;
    public float jumpForce = 15.0f;
    public float gravityScale = 3.0f;
    public float acceleration = 30.0f;
    public float deceleration = 40.0f;
    public int maxRolls = 3;

    private int currentRolls = 0;

    [Header("Sound Effects")]
    public AudioClip jumpSound;

    [Header("Combat Params")]
    public Transform attackPoint;

    private BoxCollider2D coll;
    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;

    private bool isFacingRight;
    private bool isGrounded = false;
    private Vector2 moveDirection;
    private bool isRolling;
    private float rollCooldown = 10f;
    private float rollTimer = 0f;
    private bool isRollingInvulnerable = false;
    private bool isRollingInProgress = false;
    private bool doubleJumped = false;

    public bool IsJumping => !isGrounded;
    public bool IsRollingInvulnerable => isRollingInvulnerable;
    public int CurrentRolls => currentRolls;
    public int MaxRolls => maxRolls;
    public float RollCooldown => rollCooldown;
    public float RollTimer => rollTimer;

    private bool isJumpAttacking = false;
    public float jumpAttackGravityScale = 6.0f;

    private bool isCrouching = false;
    public bool IsCrouching => isCrouching;
    private bool lastCrouchInput = false;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Vector2 crouchColliderSize;
    private Vector2 crouchColliderOffset;
    private Vector2 originalAttackPointPos;
    private Vector2 crouchAttackPointPos;

    [SerializeField] private PauseMenu pauseMenu;

    public void DoJumpAttackFall()
    {
        isJumpAttacking = true;
        rb.gravityScale = jumpAttackGravityScale;
        animator.SetTrigger("jumpAttack");
    }

    private void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        originalColliderSize = coll.size;
        originalColliderOffset = coll.offset;
        crouchColliderSize = new Vector2(coll.size.x, coll.size.y * 0.75f);
        crouchColliderOffset = originalColliderOffset + new Vector2(0, -(coll.size.y - crouchColliderSize.y) * 0.5f);
        originalAttackPointPos = attackPoint.localPosition;
        crouchAttackPointPos = new Vector2(attackPoint.localPosition.x, attackPoint.localPosition.y - 0.2f);

        currentRolls = maxRolls;

        rb.gravityScale = gravityScale;
        isFacingRight = true;
    }

    private void Update()
    {
        if (pauseMenu != null && pauseMenu.IsPaused)
        {
            return;
        }

        if (IsRollingInvulnerable)
        {
            isRollingInvulnerable = false;
        }


        isRollingInProgress = animator.GetCurrentAnimatorStateInfo(0).IsName("Rolling");

        AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (animatorStateInfo.IsName("Attack1") ||
            animatorStateInfo.IsName("Attack2") ||
            animatorStateInfo.IsName("Attack3") ||
            animatorStateInfo.IsName("Attack4") ||
            animatorStateInfo.IsName("CrouchAttack") ||
            animatorStateInfo.IsName("CrouchAttack2") ||
            animatorStateInfo.IsName("Hurt") ||
            animatorStateInfo.IsName("jumpAttack"))
        {
            moveDirection = Vector2.zero;
            isRolling = false;
        }
        else
        {
            moveDirection = InputManager.GetInstance().GetMoveDirection();

            if (!isRollingInProgress)
            {
                isRolling = InputManager.GetInstance().GetRollPressed();
            }
            else
            {
                isRolling = false;
            }
        }

        if (currentRolls < maxRolls)
        {
            rollTimer += Time.deltaTime;
        }
        if (rollTimer >= rollCooldown && currentRolls <= maxRolls)
        {
            currentRolls++;
            rollTimer = 0f;
        }

        FlipSprite();
    }

    private void FixedUpdate()
    {
        UpdateIsGrounded();

        HandleHorizontalMovement();

        HandleCrouching();

        HandleJumping();

        HandleRolling();

        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        float displayYVel = Mathf.Abs(rb.linearVelocity.y) < 0.01f ? 0f : rb.linearVelocity.y;
        animator.SetFloat("yVelocity", displayYVel);
        animator.SetBool("isJumping", !isGrounded);

        if (isJumpAttacking && isGrounded)
        {
            rb.gravityScale = gravityScale;
            isJumpAttacking = false;
        }
    }

    private void UpdateIsGrounded()
    {
        Bounds colliderBounds = coll.bounds;
        float colliderRadius = coll.size.x * 0.4f * Mathf.Abs(transform.localScale.x);
        Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, colliderRadius * 0.9f, 0);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckPos, colliderRadius);
        this.isGrounded = false;
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != coll)
                {
                    this.isGrounded = true;
                    break;
                }
            }
        }
    }

    private void HandleHorizontalMovement()
    {
        if (isJumpAttacking) return;
        if (isCrouching) return;
        float targetSpeed = moveDirection.x * runSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float movement = Mathf.Clamp(speedDiff, -accelRate * Time.fixedDeltaTime, accelRate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);
    }

    private void HandleCrouching()
    {
        bool crouchInput = InputManager.GetInstance().GetCrouchHeld();

        if (crouchInput != lastCrouchInput)
        {
            animator.SetBool("isCrouching", crouchInput);
            isCrouching = crouchInput;
            lastCrouchInput = crouchInput;

            if (crouchInput)
            {
                coll.size = crouchColliderSize;
                coll.offset = crouchColliderOffset;
                attackPoint.localPosition = crouchAttackPointPos;
            }
            else
            {
                coll.size = originalColliderSize;
                coll.offset = originalColliderOffset;
                attackPoint.localPosition = originalAttackPointPos;
            }
        }
    }

    private void HandleJumping()
    {
        if (isGrounded)
        {
            doubleJumped = false;
        }

        if (isJumpAttacking || isCrouching)
        {
            return;
        }

        bool jumpPressed = InputManager.GetInstance().GetJumpPressed();

        if (isGrounded && jumpPressed)
        {
            if (jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
            doubleJumped = false;
        }
        else if (!doubleJumped && !isGrounded && jumpPressed)
        {
            if (jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.8f);
            doubleJumped = true;
        }
    }

    private void HandleRolling()
    {
        if (isRolling && isGrounded && currentRolls > 0)
        {
            currentRolls--;
            animator.SetTrigger("Rolling");
            isRollingInvulnerable = true;
            if (moveDirection.x == 0f)
            {
                moveDirection.x = isFacingRight ? 1f : -1f;
            }
            rb.AddForce(new Vector2(moveDirection.x * runSpeed * 2.5f, 0), ForceMode2D.Impulse);
            isRolling = false;
        }
    }

    private void FlipSprite()
    {
        if (moveDirection.x < 0f && isFacingRight)
        {
            isFacingRight = false;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
        else if (moveDirection.x > 0f && !isFacingRight)
        {
            isFacingRight = true;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }
}
