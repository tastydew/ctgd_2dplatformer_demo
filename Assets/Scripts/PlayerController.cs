using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private InputSystem_Actions inputSystem;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float xInput;
    private bool jumpInput;
    private bool isDamaged;
    private bool canJump;

    [SerializeField] private GameObject leftJumpCheck;
    [SerializeField] private GameObject rightJumpCheck;
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float knockbackForceX;
    [SerializeField] private float knockbackForceY;
    [SerializeField] private float damageFlashAmount;
    [SerializeField] private float damageFlashDuration;
    [SerializeField] private bool isGrounded;
    [SerializeField] private Transform startLocation;

    private void Awake()
    {
        inputSystem = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        HandleAnimation();
        HandleSpriteOrientation();
        jumpInput = inputSystem.Player.Jump.ReadValue<float>() > 0;
        xInput = inputSystem.Player.Move.ReadValue<Vector2>().x;

        isGrounded = Physics2D.Raycast(leftJumpCheck.transform.position, Vector2.down, 0.05f,
                         LayerMask.GetMask("Ground"))
                     || Physics2D.Raycast(rightJumpCheck.transform.position, Vector2.down, 0.05f,
                         LayerMask.GetMask("Ground"));

        canJump = isGrounded && jumpInput && rb.linearVelocityY <= 0.1f;


        Debug.DrawRay(this.leftJumpCheck.transform.position, Vector2.down * 0.05f, Color.red);
        Debug.DrawRay(this.rightJumpCheck.transform.position, Vector2.down * 0.05f, Color.red);
    }

    private void FixedUpdate()
    {
        if (!isDamaged)
        {
            HandleMove();
        }

        if (canJump)
        {
            HandleJump();
        }
    }

    private void HandleMove()
    {
        if (!isDamaged)
        {
            rb.linearVelocityX = xInput * speed;
        }

        if (isGrounded)
        {
            rb.linearVelocityY = 0.0f;
        }
    }

    private void HandleJump()
    {
        rb.linearVelocityY += jumpForce;
        canJump = false;
    }

    private void HandleSpriteOrientation()
    {
        if (xInput < 0)
        {
            spriteRenderer.flipX = true;
        }

        if (xInput > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    private void HandleAnimation()
    {
        if (Math.Abs(rb.linearVelocityX) > 0.1f && Math.Abs(rb.linearVelocityY) < 0.1f)
        {
            animator.Play("Run");
        }
        else if (rb.linearVelocityY > 0.1f && !isDamaged)
        {
            animator.Play("Jump");
        }
        else if (rb.linearVelocityY < -0.1f && !isDamaged)
        {
            animator.Play("Fall");
        }
        else if (isDamaged)
        {
            animator.Play("Hit");
            StartCoroutine(DamageFlash());
        }
        else
        {
            animator.Play("Idle");
        }
    }

    private IEnumerator DamageFlash()
    {

        float elapsedTime = 0f;
        while (!isGrounded && elapsedTime < damageFlashDuration)
        {
            elapsedTime += Time.deltaTime;
            var currentFlashAmount = Mathf.Lerp(1f, 0f, elapsedTime / damageFlashDuration);
            spriteRenderer.material.SetFloat("_FlashAmount", currentFlashAmount);
            yield return null;
        }

        isDamaged = false;
    }


    private void OnEnable()
    {
        // Enable input actions
        inputSystem.Enable();
    }


    private void OnDisable()
    {
        // Disable input actions
        inputSystem.Disable();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish"))
        {
            SceneManager.LoadScene("WinScreen");
        }
    }

    IEnumerator ResetPlayerOnDamage()
    {
        isDamaged = true;
        canJump = false;


        if (!spriteRenderer.flipX)
        {
            rb.linearVelocity = new Vector2(-knockbackForceX, knockbackForceY);
        }

        if (spriteRenderer.flipX)
        {
            rb.linearVelocity = new Vector2(knockbackForceX, knockbackForceY);
        }

        yield return new WaitForSeconds(0.5f);

        transform.position = startLocation.position;

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer.Equals(LayerMask.NameToLayer("Hazard")))
        {
                StartCoroutine(ResetPlayerOnDamage());
        }
    }
}